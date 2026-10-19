using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Real Physics2D regression checks. Run via menu or -executeMethod FlowerGrowthValidation.Run.</summary>
public static class FlowerGrowthValidation
{
    private static readonly BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int passed;
    private static readonly List<string> failures = new List<string>();

    [MenuItem("Tools/Flower/Validate growth")]
    public static void Run()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        var previousScenes = EditorSceneManager.GetSceneManagerSetup();
        float previousTimeScale = Time.timeScale;
        SimulationMode2D previousSimulation = Physics2D.simulationMode;
        passed = 0;
        failures.Clear();
        Exception failure = null;
        try
        {
            Time.timeScale = 1f;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Check("Capsule fits a narrow corridor", NarrowCorridor);
            Check("Rounded corners do not behave like a box", RoundedCorner);
            Check("Collider offset, scale and rotation are respected", TransformedCapsule);
            Check("Growth aims directly at the sun", DirectSunlight);
            Check("Upper bypass is stable and collision-free", UpperBypass);
            Check("A sharp sunward turn does not require a restart", SharpSunwardTurn);
            Check("Returning along the recent stem is forbidden", RecentStemReturn);
            Check("Fast growth cannot tunnel through a thin wall", ThinWall);
            Check("Dead end waits and resumes when opened", DeadEndAndResume);
            Check("A block dragged over the tip causes safe waiting", CoveredTip);
            Check("Opening a passed branch offers a soft restart", PassedBranchAndRestart);
            Check("A short grown stem also gets a restart hint", ShortPassedBranch);
            Check("Grown stem is immutable and resets cleanly", ImmutableStem);
            Check("Unsynchronized spawn cannot create a ghost stem", UnsynchronizedSpawn);
            Check("Legacy sensors are disabled", LegacySensors);
            Check("Sun completion is terminal and idempotent", SunCompletion);
            Check("Authored level starts and masks", AuthoredLevels);
            Check("Level 2 reaches the sun after clearing breakable blocks", SecondLevelOpenRoute);
            Check("Level 2 distinguishes a true dead end and retains soft restart", SecondLevelStagedRoute);
            Check("Level 3 reaches the sun after clearing breakable blocks", () => ValidateAuthoredLevel(3, false));
            Check("Opening animation prepares immediate growth on levels 1–3", () => AnimationPreparesGrowth(false));
            Check("A late opening starts sunward growth on the first tick", () => AnimationPreparesGrowth(true));
            Assert(failures.Count == 0, string.Join("\n", failures));
            Debug.Log($"FLOWER_GROWTH_VALIDATION: PASS ({passed} checks)");
        }
        catch (Exception exception)
        {
            Debug.LogError("FLOWER_GROWTH_VALIDATION: FAIL\n" + exception);
            failure = exception;
        }
        finally
        {
            Time.timeScale = previousTimeScale;
            Physics2D.simulationMode = previousSimulation;
            if (previousScenes.Length > 0 && !Application.isBatchMode)
                EditorSceneManager.RestoreSceneManagerSetup(previousScenes);
        }
        System.IO.Directory.CreateDirectory(OutputDirectory);
        System.IO.File.WriteAllText(OutputDirectory + "/geometry-validation.txt",
            failure == null ? "PASS " + passed + " checks; 31 authored scenes" : "FAIL " + failure);
        if (failure != null)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            else
                throw failure;
        }
    }

    public static string OutputDirectory => Application.isBatchMode ? "Logs/FlowerGrowth" : "Temp/FlowerGrowth";

    private static void Check(string name, Action test)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        try
        {
            test();
            passed++;
            Debug.Log("FLOWER_GROWTH_CHECK: PASS " + name);
        }
        catch (Exception exception)
        {
            failures.Add(name + ": " + exception.Message);
            Debug.LogError("FLOWER_GROWTH_CHECK: FAIL " + name + "\n" + exception);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static BoxCollider2D Block(Vector2 position, Vector2 size)
    {
        var block = new GameObject("Validation block");
        block.transform.position = position;
        BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
        collider.size = size;
        Physics2D.SyncTransforms();
        return collider;
    }

    private static CapsuleCollider2D Capsule()
    {
        var tip = new GameObject("Validation tip");
        tip.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        CapsuleCollider2D capsule = tip.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.6f, 0.8f);
        capsule.isTrigger = true;
        return capsule;
    }

    private static FlowerGrowthCollision Space(CapsuleCollider2D capsule)
    {
        Physics2D.SyncTransforms();
        return new FlowerGrowthCollision(capsule, null, 65, 0.02f);
    }

    private static player_move Player(Vector2 sunPosition, Action<GameObject> beforeAwake = null)
    {
        CapsuleCollider2D capsule = Capsule();
        var sun = new GameObject("Validation sun");
        sun.tag = "sun";
        sun.transform.position = sunPosition;
        player_move player = capsule.gameObject.AddComponent<player_move>();
        player.Sun = sun;
        beforeAwake?.Invoke(player.gameObject);
        Invoke(player, "Awake");
        Invoke(player, "Start");
        player.enabled = true;
        return player;
    }

    private static void Invoke(player_move player, string method, params object[] arguments)
    {
        try { typeof(player_move).GetMethod(method, PrivateInstance).Invoke(player, arguments); }
        catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
    }

    private static void Tick(player_move player, int count, float deltaTime = 0.02f)
    {
        var space = Space(player.GetComponent<CapsuleCollider2D>());
        for (int i = 0; i < count; i++)
        {
            Invoke(player, "TickGrowth", deltaTime);
            Physics2D.Simulate(deltaTime);
            Physics2D.SyncTransforms();
            Assert(space.CanOccupy(player.GetComponent<Rigidbody2D>().position),
                "Tip penetrated a solid collider at " + player.transform.position);
        }
    }

    private static void NarrowCorridor()
    {
        CapsuleCollider2D capsule = Capsule();
        BoxCollider2D left = Block(new Vector2(-0.675f, 2f), new Vector2(0.6f, 6f));
        BoxCollider2D right = Block(new Vector2(0.675f, 2f), new Vector2(0.6f, 6f));
        FlowerGrowthCollision space = Space(capsule);
        Assert(space.FreeDistance(Vector2.zero, Vector2.up, 4f) > 3.99f,
            "A 0.6-wide capsule should fit a 0.75-wide corridor.");
        left.transform.position += Vector3.right * 0.1f;
        right.transform.position += Vector3.left * 0.1f;
        Physics2D.SyncTransforms();
        Assert(!space.CanOccupy(Vector2.zero), "A 0.6-wide capsule must not fit a 0.55-wide corridor.");
    }

    private static void RoundedCorner()
    {
        CapsuleCollider2D capsule = Capsule();
        Block(new Vector2(0.29f, 0.39f), Vector2.one * 0.04f);
        Assert(Space(capsule).CanOccupy(Vector2.zero), "A box corner outside the capsule falsely blocked it.");
    }

    private static void TransformedCapsule()
    {
        CapsuleCollider2D capsule = Capsule();
        capsule.offset = new Vector2(0.2f, 0.4f);
        capsule.transform.localScale = new Vector3(1.5f, 2f, 1f);
        capsule.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
        FlowerGrowthCollision space = Space(capsule);
        Assert(Vector2.Distance(space.CenterAt(Vector2.zero), capsule.bounds.center) < 0.001f,
            "Query center differs from the actual capsule center.");
        Assert(Vector2.Distance(space.Size, new Vector2(0.9f, 1.6f)) < 0.001f, "Scale was ignored.");
        Block(space.CenterAt(Vector2.zero) + Vector2.right * 3f, new Vector2(0.05f, 10f));
        float distance = space.FreeDistance(Vector2.zero, Vector2.right, 5f);
        Assert(distance > 1f && distance < 3f, "Transformed sweep missed a wall.");
    }

    private static void DirectSunlight()
    {
        player_move player = Player(new Vector2(10f, 5f));
        Tick(player, 20);
        Vector2 direction = ((Vector2)player.Sun.transform.position).normalized;
        Assert(Vector2.Dot(player.GrowthDirection, direction) > 0.999f, "Growth does not point at the sun.");
        Assert(player.transform.position.magnitude > 0.9f, "The physics body did not advance.");
        Assert(!player.RestartSuggested, "An unchanged open field must not suggest restarting.");
    }

    private static void UpperBypass()
    {
        Block(new Vector2(2f, 0f), new Vector2(1f, 2f));
        player_move player = Player(new Vector2(10f, 0f));
        float maxHeight = 0f;
        for (int i = 0; i < 220; i++)
        {
            Tick(player, 1);
            maxHeight = Mathf.Max(maxHeight, player.transform.position.y);
            Assert(player.transform.position.y > -0.01f, "Flower chose the lower bypass first.");
        }
        var space = Space(player.GetComponent<CapsuleCollider2D>());
        Vector2 bodyPosition = player.GetComponent<Rigidbody2D>().position;
        space.TryGetBlockingNormal(bodyPosition, ((Vector2)player.Sun.transform.position - bodyPosition).normalized,
            0.7f, out Vector2 normal);
        bool upAllowed = (bool)typeof(player_move).GetMethod("CanGrow", PrivateInstance).Invoke(player,
            new object[] { bodyPosition, Vector2.up, 0.35f });
        bool found = player.TryFindNextPosition(out Vector3 target, out Trigger_checker unused);
        Vector2 chosen = ((Vector2)target - bodyPosition).normalized;
        Assert(player.transform.position.x > 4f && maxHeight > 1.35f,
            "Flower failed to finish an upper bypass: " + player.transform.position + " body=" +
            bodyPosition + " state=" + player.State + " heading=" + player.GrowthDirection +
            " normal=" + normal.ToString("F6") + " up=" + space.FreeDistance(bodyPosition, Vector2.up, 0.35f) +
            " upAllowed=" + upAllowed + " reason=" + player.WaitingReason + " found=" + found +
            " chosen=" + chosen.ToString("F6") + " stepFree=" + space.FreeDistance(bodyPosition, chosen, 0.05f) +
            " stepFit=" + space.CanOccupy(bodyPosition + chosen * 0.05f));
    }

    private static void ThinWall()
    {
        Block(new Vector2(2f, 0f), new Vector2(0.02f, 100f));
        player_move player = Player(new Vector2(100f, 0f));
        player.moveSpeed = 100f;
        Tick(player, 1, 0.5f);
        Assert(player.transform.position.x < 1.71f, "Fast growth crossed a thin wall.");
    }

    private static void SharpSunwardTurn()
    {
        player_move player = Player(new Vector2(20f, 20f));
        Vector2 previous = new Vector2(-0.93f, 0.37f).normalized;
        typeof(player_move).GetField("heading", PrivateInstance).SetValue(player, previous);
        var history = (List<Vector2>)typeof(player_move).GetField("history", PrivateInstance).GetValue(player);
        history.Clear();
        history.Add(-previous);
        history.Add(Vector2.zero);
        // Reproduce the remembered upper-left bypass after its blocker disappears.
        typeof(player_move).GetField("bypassSide", PrivateInstance).SetValue(player, 1);
        player.OnWorldChanged();
        Tick(player, 20);
        Assert(player.transform.position.x > 0.6f && player.transform.position.y > 0.6f,
            "A free sunward route was rejected because the preceding step pointed left.");
        Assert(history[0] == -previous && history[1] == Vector2.zero,
            "Resuming growth rewrote the grown stem.");
    }

    private static void RecentStemReturn()
    {
        player_move player = Player(new Vector2(0f, 20f));
        var history = (List<Vector2>)typeof(player_move).GetField("history", PrivateInstance).GetValue(player);
        history.Clear();
        for (int i = 0; i <= 20; i++)
            history.Add(new Vector2(-1f + i * 0.05f, 0f));
        bool allowed = (bool)typeof(player_move).GetMethod("CanGrow", PrivateInstance).Invoke(player,
            new object[] { Vector2.zero, Vector2.left, 0.35f });
        Assert(!allowed, "Removing the angular limit allowed retracing the recent stem.");
        history.Clear();
        history.Add(Vector2.left * 0.05f);
        history.Add(Vector2.zero);
        allowed = (bool)typeof(player_move).GetMethod("CanGrow", PrivateInstance).Invoke(player,
            new object[] { Vector2.zero, Vector2.left, 0.35f });
        Assert(!allowed, "A short initial stem could be retraced inside the junction allowance.");
        Assert(FlowerGrowthCollision.SegmentDistance(new Vector2(-1f, 0f), Vector2.right,
            Vector2.down, Vector2.up) == 0f, "A proposed crossing missed the grown stem.");
    }

    private static void DeadEndAndResume()
    {
        Block(new Vector2(1f, 1f), new Vector2(8f, 0.3f));
        Block(new Vector2(1f, -1f), new Vector2(8f, 0.3f));
        Block(new Vector2(-3f, 0f), new Vector2(0.3f, 2f));
        BoxCollider2D end = Block(new Vector2(3f, 0f), new Vector2(0.3f, 2f));
        player_move player = Player(new Vector2(20f, 0f));
        Tick(player, 150);
        Assert(player.State == player_move.GrowthState.Waiting, "A closed corridor must stop growth.");
        Vector3 stopped = player.transform.position;
        Tick(player, 40);
        Assert(Vector3.Distance(stopped, player.transform.position) < 0.001f, "Stopped tip jittered or retreated.");
        UnityEngine.Object.DestroyImmediate(end.gameObject);
        player.OnWorldChanged();
        Tick(player, 1);
        Assert(player.transform.position.x > stopped.x + 0.01f,
            "Opening the front still waits for the complete sun route before its first step.");
        Tick(player, 19);
        Assert(player.transform.position.x > stopped.x + 0.5f, "Growth did not resume after opening the front.");
        Assert(!player.RestartSuggested, "Opening directly ahead should not require a restart.");
    }

    private static void CoveredTip()
    {
        player_move player = Player(new Vector2(20f, 0f));
        Tick(player, 10);
        Vector3 stopped = player.transform.position;
        BoxCollider2D block = Block(stopped, Vector2.one * 2f);
        Invoke(player, "TickGrowth", 0.02f);
        Assert(player.State == player_move.GrowthState.Waiting && player.transform.position == stopped,
            "An overlapping tip must wait without depenetration or teleportation.");
        block.transform.position = new Vector2(0f, -5f);
        player.OnWorldChanged();
        Tick(player, 10);
        Assert(player.transform.position.x > stopped.x + 0.2f, "Tip did not resume when uncovered.");
    }

    private static void PassedBranchAndRestart()
    {
        BoxCollider2D block = Block(new Vector2(0.9f, 0f), new Vector2(0.3f, 1.4f));
        player_move player = Player(new Vector2(20f, 0f));
        Tick(player, 100);
        Assert(!player.RestartSuggested, "Original geometry should not suggest a restart.");
        UnityEngine.Object.DestroyImmediate(block.gameObject);
        player.OnWorldChanged();
        Tick(player, 15);
        Assert(player.RestartSuggested, "Opening a passed exit should offer a restart.");
        BoxCollider2D movedBlock = Block(new Vector2(8f, 4f), Vector2.one);
        player.RestartGrowth();
        Assert(player.transform.position.sqrMagnitude < 0.0001f && !player.RestartSuggested,
            "Soft restart did not reset the tip and hint.");
        Assert(movedBlock != null && movedBlock.transform.position == new Vector3(8f, 4f),
            "Soft restart changed the world.");
        Tick(player, 20);
        Assert(Mathf.Abs(player.transform.position.y) < 0.001f,
            "Restart did not use the newly opened direct path.");
    }

    private static void ShortPassedBranch()
    {
        BoxCollider2D block = Block(new Vector2(0.9f, 0f), new Vector2(0.3f, 1.4f));
        player_move player = Player(new Vector2(20f, 0f));
        Tick(player, 16); // 0.8 units: shorter than the capsule's old 1.0-unit threshold.
        Assert(!player.RestartSuggested, "Unchanged geometry suggested restarting.");
        player.moveSpeed = 0f; // Keep the existing stem short during the deferred world check.
        UnityEngine.Object.DestroyImmediate(block.gameObject);
        player.OnWorldChanged();
        Tick(player, 15);
        Assert(player.RestartSuggested, "An opened passed branch was ignored because the stem was short.");
    }

    private static void ImmutableStem()
    {
        var stemObject = new GameObject("Validation stem");
        LineRenderer renderer = stemObject.AddComponent<LineRenderer>();
        EdgeCollider2D edge = stemObject.AddComponent<EdgeCollider2D>();
        Line_rendered stem = stemObject.AddComponent<Line_rendered>();
        stem.ResetGrowth(Vector2.zero, Vector2.right);
        stem.AppendGrowthPoint(Vector2.one);
        Vector3 fixedCorner = renderer.GetPosition(1);
        stem.AppendGrowthPoint(new Vector2(2f, 1f));
        Assert(renderer.GetPosition(1) == fixedCorner && edge.pointCount == 4,
            "Growing the tip changed an older stem segment.");
        stem.ResetGrowth(Vector2.zero, Vector2.zero);
        Assert(renderer.positionCount == 1 && !edge.enabled, "Stem was not reset cleanly.");
    }

    private static void UnsynchronizedSpawn()
    {
        var stemObject = new GameObject("Spawn test stem", typeof(LineRenderer), typeof(EdgeCollider2D), typeof(Line_rendered));
        stemObject.tag = "stem";
        Line_rendered stem = stemObject.GetComponent<Line_rendered>();
        Vector2 spawn = new Vector2(-4.1f, -9.63f);
        player_move player = Player(spawn + Vector2.one * 10f, root =>
        {
            root.transform.position = spawn;
            root.GetComponent<player_move>().line_render = stem;
            var seed = new GameObject("Spawn test seed");
            seed.transform.position = spawn - Vector2.up;
            root.GetComponent<player_move>().first_dot = seed;
        });
        Assert(Vector2.Distance(stem.lineRenderer.GetPosition(1), spawn) < 0.001f,
            "The initial stem point used a stale physics position instead of the spawn.");
        Tick(player, 5);
        for (int i = 2; i < stem.lineRenderer.positionCount; i++)
            Assert(Vector2.Distance(stem.lineRenderer.GetPosition(i - 1), stem.lineRenderer.GetPosition(i)) <= 0.081f,
                "A ghost segment crosses outside the actual growth path.");
    }

    private static void LegacySensors()
    {
        Collider2D sensorCollider = null;
        Player(new Vector2(10f, 0f), root =>
        {
            var sensor = new GameObject("Legacy sensor");
            sensor.transform.SetParent(root.transform, false);
            sensor.AddComponent<Trigger_checker>();
            sensorCollider = sensor.AddComponent<CircleCollider2D>();
        });
        Assert(sensorCollider != null && !sensorCollider.enabled, "Legacy sensor collider is still active.");
    }

    private static void SunCompletion()
    {
        player_move player = Player(new Vector2(3f, 0f));
        player.Sun.AddComponent<CircleCollider2D>().radius = 0.4f;
        Tick(player, 100);
        Assert(player.State == player_move.GrowthState.ReachedSun, "The actual tip did not detect the sun.");
        Vector3 position = player.transform.position;
        player.is_sun();
        player.RestartGrowth();
        Tick(player, 10);
        Assert(player.transform.position == position && !player.enabled, "Completion was not terminal.");
    }

    private static void AuthoredLevels()
    {
        var levels = new List<string>();
        for (int i = 1; i <= 27; i++)
            levels.Add("level_" + i);
        levels.AddRange(new[] { "tutorial_level_1", "tutorial_level_4", "tutorial_level_11", "tutorial_21" });
        var invalidStarts = new List<string>();
        foreach (string level in levels)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/" + level + ".unity", OpenSceneMode.Single);
            player_move player = UnityEngine.Object.FindObjectOfType<player_move>();
            Assert(player != null && player.Sun != null && player.line_render != null,
                level + ": missing serialized growth references.");
            var serialized = new SerializedObject(player);
            int mask = serialized.FindProperty("obstacleMask").intValue;
            Assert((mask & 65) == 65, level + ": legacy blocks are missing from the obstacle mask.");
            Invoke(player, "Awake");
            Invoke(player, "Start");
            var space = new FlowerGrowthCollision(player.GetComponent<CapsuleCollider2D>(), player.line_render, mask, 0.02f);
            if (!space.CanOccupy(player.transform.position))
            {
                invalidStarts.Add(level);
                foreach (Collider2D hit in Physics2D.OverlapCapsuleAll(space.CenterAt(player.transform.position),
                    space.Size, player.GetComponent<CapsuleCollider2D>().direction, player.transform.eulerAngles.z))
                    if (space.IsObstacle(hit))
                        Debug.LogError("FLOWER_INVALID_START: " + level + " tip=" + player.transform.position +
                            " obstacle=" + hit.name + " bounds=" + hit.bounds + " separation=" +
                            player.GetComponent<CapsuleCollider2D>().Distance(hit).distance);
                continue;
            }
            player.enabled = true;
            Tick(player, 40);
            Debug.Log("FLOWER_AUTHORED_LEVEL: " + level + " " + player.State + " " + player.transform.position);
        }
        Assert(invalidStarts.Count == 0, "Authored starts overlap: " + string.Join(", ", invalidStarts));
    }

    private static void SecondLevelOpenRoute()
    {
        ValidateSecondLevel(false);
    }

    private static void AnimationPreparesGrowth(bool lateChange)
    {
        for (int level = 1; level <= 3; level++)
            foreach (bool cleared in new[] { false, true })
            {
                if (lateChange && !cleared) continue;
                EditorSceneManager.OpenScene("Assets/Scenes/level_" + level + ".unity", OpenSceneMode.Single);
                if (Camera.main != null) Camera.main.aspect = 9f / 16f;
                player_move player = UnityEngine.Object.FindObjectOfType<player_move>();
                player.line_render.growLeavesAlongStem = false;
                Invoke(player, "Awake");
                Invoke(player, "Start");
                player.enabled = false;
                if (cleared && !lateChange)
                {
                    foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
                        if (block.canCrush)
                            foreach (Collider2D part in block.GetComponentsInChildren<Collider2D>()) part.enabled = false;
                    player.OnWorldChanged();
                }
                Vector2 spawn = player.transform.position;
                // The authored opening animation enables growth at one second.
                for (int frame = 0; frame < 50; frame++) Invoke(player, "FixedUpdate");
                if (lateChange)
                {
                    foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
                        if (block.canCrush)
                            foreach (Collider2D part in block.GetComponentsInChildren<Collider2D>()) part.enabled = false;
                    player.OnWorldChanged();
                }
                Assert(Vector2.Distance(spawn, player.transform.position) < 0.0001f,
                    "Navigation moved the seed before its animation enabled growth.");
                player.change_enabled();
                Tick(player, 1);
                Assert(Vector2.Distance(spawn, player.transform.position) > 0.001f,
                    "Level " + level + " waits for planning after the opening animation; cleared=" + cleared);
            }
    }

    private static void SecondLevelStagedRoute()
    {
        ValidateSecondLevel(true);
    }

    private static void ValidateSecondLevel(bool staged)
    {
        ValidateAuthoredLevel(2, staged);
    }

    private static void ValidateAuthoredLevel(int level, bool staged)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/level_" + level + ".unity", OpenSceneMode.Single);
        if (Camera.main != null) Camera.main.aspect = 9f / 16f;
        foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
            if (block.canCrush && !staged)
                foreach (Collider2D collider in block.GetComponentsInChildren<Collider2D>())
                    collider.enabled = false;
        player_move player = UnityEngine.Object.FindObjectOfType<player_move>();
        Invoke(player, "Awake");
        Invoke(player, "Start");
        player.enabled = true;
        int opened = 0;
        int restarts = 0;
        for (int frame = 0; frame < (staged ? 1600 : 800) && player.State != player_move.GrowthState.ReachedSun; frame++)
        {
            Tick(player, 1);
            if (!staged || player.State != player_move.GrowthState.Waiting)
                continue;
            Crush_block closest = null;
            float nearest = float.PositiveInfinity;
            foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
            {
                Collider2D collider = block.GetComponent<Collider2D>();
                if (!block.canCrush || collider == null || !collider.enabled)
                    continue;
                float distance = Vector2.Distance(collider.ClosestPoint(player.transform.position), player.transform.position);
                if (distance < nearest)
                {
                    nearest = distance;
                    closest = block;
                }
            }
            if (closest == null)
            {
                // The staged route may legitimately enter a pocket whose only exits
                // retreat downward. A restart must never hide an available forward step.
                Tick(player, 15);
                Assert(!player.TryFindNextPosition(out Vector3 unusedPosition, out Trigger_checker unusedTrigger),
                    "Staged level stopped despite an available continuation.");
                var currentSpace = new FlowerGrowthCollision(player.GetComponent<CapsuleCollider2D>(),
                    player.line_render, 65, 0.02f);
                Vector2 tip = player.GetComponent<Rigidbody2D>().position;
                Vector2 originalAxis = (Vector2)typeof(player_move).GetField("growthAxis", PrivateInstance).GetValue(player);
                for (int angle = 0; angle < 360; angle += 5)
                {
                    Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    if (direction.y < -0.0001f && Vector2.Dot(direction, originalAxis) < -0.0001f)
                        continue;
                    if (currentSpace.FreeDistance(tip, direction, 0.05f) < 0.0499f)
                        continue;
                    bool revisits = (bool)typeof(player_move).GetMethod("RevisitsStem", PrivateInstance).Invoke(player,
                        new object[] { tip, tip + direction * 0.05f });
                    Assert(revisits, "Restart cannot replace a free forward step at angle " + angle);
                }
                Assert(player.RestartSuggested, "A true dead end after opening passed routes lost its restart hint.");
                Assert(restarts == 0, "The cleared level remained blocked after its soft restart.");
                player.RestartGrowth();
                restarts++;
                continue;
            }
            foreach (Collider2D collider in closest.GetComponentsInChildren<Collider2D>())
                collider.enabled = false;
            opened++;
            player.OnWorldChanged();
        }
        string diagnostics = "position=" + player.transform.position.ToString("F5") +
            " state=" + player.State + " reason=" + player.WaitingReason + " heading=" + player.GrowthDirection +
            " staged=" + staged + " opened=" + opened + " restarts=" + restarts;
        var space = new FlowerGrowthCollision(player.GetComponent<CapsuleCollider2D>(), player.line_render, 65, 0.02f);
        Vector2 position = player.GetComponent<Rigidbody2D>().position;
        Vector2 axis = (Vector2)typeof(player_move).GetField("growthAxis", PrivateInstance).GetValue(player);
        diagnostics += " axis=" + axis;
        for (int angle = 0; angle < 360; angle += 15)
        {
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float free = space.FreeDistance(position, direction, 0.35f);
            float shortFree = space.FreeDistance(position, direction, 0.05f);
            if (shortFree > 0.049f)
                diagnostics += " [freeAngle=" + angle + " longFree=" + free + " progress=" + Vector2.Dot(axis, direction) +
                    " revisits=" + typeof(player_move).GetMethod("RevisitsStem", PrivateInstance).Invoke(player,
                        new object[] { position, position + direction * 0.35f }) + "]";
        }
        System.IO.Directory.CreateDirectory(OutputDirectory);
        System.IO.File.WriteAllText(OutputDirectory + "/level-" + level +
            (staged ? "-staged-validation.txt" : "-validation.txt"), diagnostics);
        Assert(player.State == player_move.GrowthState.ReachedSun, "Cleared level " + level + " stopped: " + diagnostics);
    }
}
