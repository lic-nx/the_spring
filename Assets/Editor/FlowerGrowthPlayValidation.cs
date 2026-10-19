using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>Checks actual FixedUpdate, trigger delivery, UI and soft restart in Play Mode.</summary>
[InitializeOnLoad]
public static class FlowerGrowthPlayValidation
{
    private const string RunningKey = "FlowerGrowthPlayValidation.Running";
    private const string PhaseKey = "FlowerGrowthPlayValidation.Phase";
    private const string StartedKey = "FlowerGrowthPlayValidation.Started";
    private const string RestoreKey = "FlowerGrowthPlayValidation.Restore";
    private const string ScenesKey = "FlowerGrowthPlayValidation.Scenes";
    private static int loggedPhase = -1;
    private static readonly Vector3 Spawn = new Vector3(-4.1f, -9.63f, 0f);

    [Serializable] private sealed class SavedScenes { public SavedScene[] scenes; }
    [Serializable] private sealed class SavedScene { public string path; public bool loaded; public bool active; }

    static FlowerGrowthPlayValidation()
    {
        EditorApplication.playModeStateChanged += RestoreScenes;
        if (SessionState.GetBool(RunningKey, false))
            EditorApplication.update += Update;
        if (SessionState.GetBool(RestoreKey, false) && !EditorApplication.isPlayingOrWillChangePlaymode)
            EditorApplication.delayCall += () => RestoreScenes(PlayModeStateChange.EnteredEditMode);
    }

    // Batch entry point; intentionally no -quit, since the check spans Play Mode frames.
    [MenuItem("Tools/Flower/Validate all growth checks")]
    public static void RunAll()
    {
        FlowerGrowthValidation.Run();
        Run();
    }

    [MenuItem("Tools/Flower/Validate growth in Play Mode")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before starting validation.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;
        SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
        var saved = new SavedScenes { scenes = new SavedScene[setup.Length] };
        for (int i = 0; i < setup.Length; i++)
            saved.scenes[i] = new SavedScene { path = setup[i].path, loaded = setup[i].isLoaded, active = setup[i].isActive };
        SessionState.SetString(ScenesKey, JsonUtility.ToJson(saved));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var sun = new GameObject("Validation sun");
        sun.tag = "sun";
        sun.transform.position = Spawn + new Vector3(10f, 0.67f, 0f);
        sun.AddComponent<CircleCollider2D>().radius = 0.4f;

        var block = new GameObject("Validation branch block");
        block.transform.position = Spawn + new Vector3(0.9f, 0.67f, 0f);
        block.AddComponent<BoxCollider2D>().size = new Vector2(0.3f, 1.4f);

        // Use the actual nested prefab and startup animation away from the world origin.
        // An origin-only fixture hid an unsynchronized Rigidbody2D point in the original test.
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/player_packet.prefab");
        var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        root.SetActive(false);
        root.transform.position = Spawn;
        player_move player = root.GetComponentInChildren<player_move>(true);
        player.Sun = sun;
        player.line_render.growLeavesAlongStem = false;
        player.enabled = false; // The real flower animation must start growth.
        root.SetActive(true);

        var canvasObject = new GameObject("Validation UI", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var buttons = new GameObject("Gameplay buttons", typeof(RectTransform));
        buttons.transform.SetParent(canvasObject.transform, false);
        RectTransform rect = (RectTransform)buttons.transform;
        // Match the real prefab's small, centered group instead of an idealized full-screen one.
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100f, 100f);
        UIController ui = canvasObject.AddComponent<UIController>();
        var serialized = new SerializedObject(ui);
        serialized.FindProperty("gameplayButtons").objectReferenceValue = buttons;
        serialized.FindProperty("growthRestartFont").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/fonts/DRAgu 4 SDF 1.asset");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        new GameObject("Validation events", typeof(EventSystem), typeof(StandaloneInputModule));

        SessionState.SetBool(RunningKey, true);
        SessionState.SetInt(PhaseKey, 0);
        SessionState.SetFloat(StartedKey, (float)EditorApplication.timeSinceStartup);
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
        EditorApplication.EnterPlaymode();
    }

    private static void Update()
    {
        if (EditorApplication.timeSinceStartup - SessionState.GetFloat(StartedKey, 0f) > 45f)
        {
            player_move current = player_move._instance;
            Finish(false, "Timed out at phase " + SessionState.GetInt(PhaseKey, 0) + "; " +
                (current == null ? "no player" : current.gameObject.scene.name + " " + current.State + " " +
                current.transform.position + " hint=" + current.RestartSuggested));
            return;
        }
        if (!EditorApplication.isPlaying)
            return;
        try
        {
            player_move player = player_move._instance;
            if (player == null)
                return;
            int phase = SessionState.GetInt(PhaseKey, 0);
            if (phase != loggedPhase)
            {
                Debug.Log("FLOWER_PLAY_PHASE: " + phase + " " + player.State + " " + player.transform.position);
                loggedPhase = phase;
            }
            LineRenderer stem = player.line_render.lineRenderer;
            if (stem.positionCount < 2)
                return; // Start has not run yet.
            Vector3 expectedSpawn = phase >= 3
                ? (Vector3)typeof(player_move).GetField("initialPosition",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(player)
                : Spawn;
            if ((phase == 0 || phase == 6 || phase == 8) && player.enabled &&
                player.State == player_move.GrowthState.Planning &&
                Vector2.Distance(player.transform.position, expectedSpawn) < 0.001f)
                throw new InvalidOperationException("Growth still waits for planning after its opening animation.");
            if (Vector2.Distance(stem.GetPosition(1), expectedSpawn) > 0.001f)
                throw new InvalidOperationException("Initial stem point differs from the actual spawn: " +
                    stem.GetPosition(1) + " expected " + expectedSpawn + " scene=" + player.gameObject.scene.path);
            for (int i = 2; i < stem.positionCount; i++)
                if (Vector2.Distance(stem.GetPosition(i - 1), stem.GetPosition(i)) > 0.081f)
                    throw new InvalidOperationException("The stem contains a jump outside the swept growth path at point " + i);
            if (phase == 0 && player.transform.position.x > Spawn.x + 3f)
            {
                GameObject block = GameObject.Find("Validation branch block");
                UnityEngine.Object.Destroy(block);
                player.OnWorldChanged();
                SessionState.SetInt(PhaseKey, 1);
            }
            else if (phase == 1 && player.RestartSuggested)
            {
                GameObject restart = GameObject.Find("Restart growth");
                if (restart == null)
                    return; // UI updates in LateUpdate.
                Button button = restart.GetComponent<Button>();
                TextMeshProUGUI label = restart.GetComponentInChildren<TextMeshProUGUI>();
                if (button == null || label == null || label.font == null || string.IsNullOrEmpty(label.text))
                    throw new InvalidOperationException("Restart UI has no button or localized label.");
                Canvas.ForceUpdateCanvases();
                RectTransform canvasRect = (RectTransform)button.GetComponentInParent<Canvas>().transform;
                if (canvasRect.rect.height <= 0f)
                    throw new InvalidOperationException("Restart UI canvas has no screen area.");
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, button.transform);
                float screenFraction = (bounds.center.y - canvasRect.rect.yMin) / canvasRect.rect.height;
                if (screenFraction < 0f || screenFraction > 0.2f)
                    throw new InvalidOperationException("Restart control is outside the bottom screen area: " + screenFraction);
                button.onClick.Invoke();
                if (Vector2.Distance(player.transform.position, Spawn) > 0.001f || player.RestartSuggested)
                    throw new InvalidOperationException("Click did not reset growth.");
                if (GameObject.Find("Validation branch block") != null)
                    throw new InvalidOperationException("Soft restart restored a destroyed block.");
                SessionState.SetInt(PhaseKey, 2);
            }
            else if (phase == 2 && player.State == player_move.GrowthState.ReachedSun)
            {
                if (player.enabled || player.RestartSuggested)
                    throw new InvalidOperationException("The flower continued after reaching the sun.");
                if (player.line_render.lineRenderer.positionCount < 10)
                    throw new InvalidOperationException("The stem did not follow actual physics movement.");
                SessionState.SetInt(PhaseKey, 3);
                // A scene loaded by the level/restart buttons has a different physics startup
                // order than an editor-instantiated fixture. Validate the real loading path too.
                SceneManager.LoadScene("level_1", LoadSceneMode.Single);
            }
            else if (phase == 3 && player.gameObject.scene.name == "level_1" &&
                     player.State == player_move.GrowthState.Waiting)
            {
                Crush_block closest = null;
                float nearest = float.PositiveInfinity;
                foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
                {
                    Collider2D collider = block.GetComponent<Collider2D>();
                    if (!block.canCrush || collider == null)
                        continue;
                    float distance = Vector2.Distance(collider.ClosestPoint(player.transform.position), player.transform.position);
                    if (distance < nearest)
                    {
                        closest = block;
                        nearest = distance;
                    }
                }
                if (closest == null)
                    throw new InvalidOperationException("The authored level has no nearby breakable block to test.");
                SessionState.SetInt("FlowerGrowthPlayValidation.OpenedBlock", closest.GetInstanceID());
                SessionState.SetFloat("FlowerGrowthPlayValidation.StopX", player.transform.position.x);
                SessionState.SetFloat("FlowerGrowthPlayValidation.StopY", player.transform.position.y);
                SessionState.SetInt("FlowerGrowthPlayValidation.StopPoints", stem.positionCount);
                SessionState.SetFloat("FlowerGrowthPlayValidation.OpenedAt", (float)EditorApplication.timeSinceStartup);
                int hits = Mathf.Max(1, closest.max_count - closest.count);
                for (int i = 0; i < hits; i++)
                    closest.OnPointerClick(null);
                SessionState.SetInt(PhaseKey, 4);
            }
            else if (phase == 4)
            {
                Vector2 stopped = new Vector2(SessionState.GetFloat("FlowerGrowthPlayValidation.StopX", 0f),
                    SessionState.GetFloat("FlowerGrowthPlayValidation.StopY", 0f));
                int committedPoints = SessionState.GetInt("FlowerGrowthPlayValidation.StopPoints", 0);
                if (stem.positionCount < committedPoints ||
                    Vector2.Distance(stem.GetPosition(committedPoints - 1), stopped) > 0.001f)
                    throw new InvalidOperationException("Opening the real block reset or rewrote the existing stem.");
                if (Vector2.Distance(player.transform.position, stopped) > 1f &&
                    player.transform.position.x > stopped.x + 0.5f)
                {
                    SessionState.SetInt(PhaseKey, 5);
                    SceneManager.LoadScene("level_2", LoadSceneMode.Single);
                }
                else if (EditorApplication.timeSinceStartup - SessionState.GetFloat("FlowerGrowthPlayValidation.OpenedAt", 0f) > 3f)
                    throw new InvalidOperationException("Real block opening did not resume growth from the current tip. " +
                        DescribeCheckpoints(player));
            }
            else if (phase == 5 && player.gameObject.scene.name == "level_2")
            {
                foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
                {
                    if (!block.canCrush)
                        continue;
                    int hits = Mathf.Max(1, block.max_count - block.count);
                    for (int i = 0; i < hits; i++)
                        block.OnPointerClick(null);
                }
                SessionState.SetInt(PhaseKey, 6);
            }
            else if (phase == 6 && player.State == player_move.GrowthState.ReachedSun)
            {
                SessionState.SetInt(PhaseKey, 7);
                SceneManager.LoadScene("level_3", LoadSceneMode.Single);
            }
            else if (phase == 7 && player.gameObject.scene.name == "level_3")
            {
                foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
                {
                    if (!block.canCrush) continue;
                    int hits = Mathf.Max(1, block.max_count - block.count);
                    for (int i = 0; i < hits; i++) block.OnPointerClick(null);
                }
                SessionState.SetInt(PhaseKey, 8);
            }
            else if (phase == 8 && player.State == player_move.GrowthState.ReachedSun)
                Finish(true, "Fixtures, runtime level_1 continuation without restarting, and cleared runtime levels 2 and 3 reaching the sun with an unbroken swept stem.");
        }
        catch (Exception exception) { Finish(false, exception.ToString()); }
    }

    private static void Finish(bool success, string detail)
    {
        SessionState.SetBool(RunningKey, false);
        EditorApplication.update -= Update;
        if (success)
            Debug.Log("FLOWER_GROWTH_PLAY_VALIDATION: PASS " + detail);
        else
            Debug.LogError("FLOWER_GROWTH_PLAY_VALIDATION: FAIL " + detail);
        System.IO.Directory.CreateDirectory(FlowerGrowthValidation.OutputDirectory);
        System.IO.File.WriteAllText(FlowerGrowthValidation.OutputDirectory + "/play-validation.txt", (success ? "PASS " : "FAIL ") + detail);
        if (Application.isBatchMode)
            EditorApplication.Exit(success ? 0 : 1);
        else
        {
            SessionState.SetBool(RestoreKey, true);
            EditorApplication.ExitPlaymode();
        }
    }

    private static string DescribeCheckpoints(player_move player)
    {
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var points = (System.Collections.IEnumerable)typeof(player_move).GetField("checkpoints", flags).GetValue(player);
        var exits = typeof(player_move).GetMethod("AvailableExits", flags);
        string result = "state=" + player.State + " reason=" + player.WaitingReason + " travelled=" +
            typeof(player_move).GetField("travelled", flags).GetValue(player);
        foreach (object point in points)
        {
            Type type = point.GetType();
            Vector2 position = (Vector2)type.GetField("position").GetValue(point);
            Vector2 heading = (Vector2)type.GetField("heading").GetValue(point);
            ulong before = (ulong)type.GetField("exits").GetValue(point);
            ulong after = (ulong)exits.Invoke(player, new object[] { position, heading });
            result += " [distance=" + type.GetField("distance").GetValue(point) + " newExits=" + (after & ~before).ToString("X") + "]";
        }
        return result;
    }

    private static void RestoreScenes(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(RestoreKey, false))
            return;
        SessionState.SetBool(RestoreKey, false);
        SavedScenes saved = JsonUtility.FromJson<SavedScenes>(SessionState.GetString(ScenesKey, ""));
        if (saved == null || saved.scenes == null || saved.scenes.Length == 0)
            return;
        var setup = new SceneSetup[saved.scenes.Length];
        for (int i = 0; i < setup.Length; i++)
            setup[i] = new SceneSetup { path = saved.scenes[i].path, isLoaded = saved.scenes[i].loaded, isActive = saved.scenes[i].active };
        EditorSceneManager.RestoreSceneManagerSetup(setup);
    }
}
