using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Headless, long-running navigation checks against every authored scene.</summary>
public static class FlowerGrowthAudit
{
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    [Serializable] private class Report { public List<Result> results = new List<Result>(); }
    [Serializable] private class Result
    {
        public string level, mode, state, reason, error;
        public Vector2 position, heading, sunward;
        public int opened, steps, physicalExits, legalExits;
        public bool restart, growthEnabled, stemAtStart;
        public string exits;
        public string navigation;
        public double maximumTickMilliseconds;
        public int startupIdleSteps, maximumResumeIdleSteps, planningSteps;
    }

    public static void Run() => RunLevels(27);
    public static void RunReportedLevels() => RunLevels(3);
    public static void RunAll()
    {
        Run();
        FlowerGrowthPlayValidation.RunAll();
    }

    private static void RunLevels(int count)
    {
        var report = new Report();
        float previousTimeScale = Time.timeScale;
        SimulationMode2D previousSimulation = Physics2D.simulationMode;
        try
        {
            Time.timeScale = 1f;
            Physics2D.simulationMode = SimulationMode2D.Script;
            var levels = new List<string>();
            for (int i = 1; i <= count; i++) levels.Add("level_" + i);
            if (count == 27) levels.AddRange(new[] { "tutorial_level_1", "tutorial_level_4", "tutorial_level_11", "tutorial_21" });
            foreach (string level in levels)
                foreach (string mode in new[] { "intact", "cleared", "staged", "front-opened" })
                {
                    var result = new Result { level = level, mode = mode };
                    report.results.Add(result);
                    try { Check(result); }
                    catch (Exception exception) { result.error = exception.ToString(); }
                    Directory.CreateDirectory("Logs/FlowerGrowth");
                    File.WriteAllText("Logs/FlowerGrowth/audit.json", JsonUtility.ToJson(report, true));
                    Debug.Log("FLOWER_AUDIT " + JsonUtility.ToJson(result));
                }
        }
        finally
        {
            Time.timeScale = previousTimeScale;
            Physics2D.simulationMode = previousSimulation;
        }
        int failures = report.results.FindAll(result => !string.IsNullOrEmpty(result.error)).Count;
        Debug.Log("FLOWER_AUDIT_COMPLETE: " + report.results.Count + " scenarios; failures=" + failures);
        if (Application.isBatchMode && failures > 0) EditorApplication.Exit(1);
    }

    private static void Invoke(player_move player, string method, params object[] args)
    {
        try { typeof(player_move).GetMethod(method, Private).Invoke(player, args); }
        catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
    }

    private static bool OpenBlock(player_move player, bool all)
    {
        Crush_block nearest = null;
        float distance = float.PositiveInfinity;
        bool opened = false;
        foreach (Crush_block block in UnityEngine.Object.FindObjectsOfType<Crush_block>())
        {
            Collider2D collider = block.GetComponent<Collider2D>();
            if (!block.canCrush || collider == null || !collider.enabled) continue;
            if (all)
            {
                foreach (Collider2D part in block.GetComponentsInChildren<Collider2D>()) part.enabled = false;
                opened = true;
                continue;
            }
            float candidate = Vector2.Distance(collider.ClosestPoint(player.transform.position), player.transform.position);
            if (candidate < distance) { distance = candidate; nearest = block; }
        }
        if (!all && nearest != null)
        {
            foreach (Collider2D part in nearest.GetComponentsInChildren<Collider2D>()) part.enabled = false;
            opened = true;
        }
        if (opened) player.OnWorldChanged();
        return opened;
    }

    private static void Check(Result result)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/" + result.level + ".unity", OpenSceneMode.Single);
        if (Camera.main != null) Camera.main.aspect = 9f / 16f;
        player_move player = UnityEngine.Object.FindObjectOfType<player_move>();
        player.line_render.growLeavesAlongStem = false;
        Invoke(player, "Awake");
        player.enabled = false;
        Invoke(player, "Start");
        if (result.mode == "cleared" && OpenBlock(player, true)) result.opened++;
        if (result.mode == "front-opened" && OpenBlock(player, false)) result.opened++;
        // Match the one-second opening animation, including navigation work done
        // before its animation event enables movement.
        for (int frame = 0; frame < 50; frame++) Invoke(player, "FixedUpdate");
        player.change_enabled();
        var space = new FlowerGrowthCollision(player.GetComponent<CapsuleCollider2D>(), player.line_render, 65, 0.02f);
        int waiting = 0;
        int idleSteps = 0;
        bool awaitingMovement = true;
        bool startup = true;
        for (int i = 0; i < 2400; i++)
        {
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            Vector2 previousPosition = player.GetComponent<Rigidbody2D>().position;
            Invoke(player, "TickGrowth", 0.02f);
            result.maximumTickMilliseconds = Math.Max(result.maximumTickMilliseconds,
                (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000d / System.Diagnostics.Stopwatch.Frequency);
            Physics2D.Simulate(0.02f);
            Physics2D.SyncTransforms();
            if (player.State == player_move.GrowthState.Planning) result.planningSteps++;
            if (awaitingMovement)
            {
                if (Vector2.Distance(previousPosition, player.GetComponent<Rigidbody2D>().position) > 0.0001f)
                {
                    if (startup) result.startupIdleSteps = idleSteps;
                    else result.maximumResumeIdleSteps = Math.Max(result.maximumResumeIdleSteps, idleSteps);
                    awaitingMovement = false;
                    startup = false;
                }
                else idleSteps++;
            }
            if (!space.CanOccupy(player.GetComponent<Rigidbody2D>().position))
                throw new Exception("Tip penetrated a block at step " + i);
            result.steps++;
            if (player.State == player_move.GrowthState.ReachedSun) break;
            if (player.State != player_move.GrowthState.Waiting) { waiting = 0; continue; }
            if (++waiting < 15) continue;
            if (result.mode == "front-opened" && result.opened == 1 && OpenBlock(player, true))
            {
                result.opened++;
                waiting = 0;
                awaitingMovement = true;
                idleSteps = 0;
                continue;
            }
            if (result.mode == "staged" && OpenBlock(player, false))
            {
                result.opened++;
                waiting = 0;
                awaitingMovement = true;
                idleSteps = 0;
                continue;
            }
            break;
        }
        result.state = player.State.ToString();
        result.reason = player.WaitingReason.ToString();
        result.position = player.transform.position;
        result.heading = player.GrowthDirection;
        var route = (List<Vector2>)typeof(player_move).GetField("route", Private).GetValue(player);
        int routeIndex = (int)typeof(player_move).GetField("routeIndex", Private).GetValue(player);
        result.navigation = "step=" + typeof(player_move).GetField("chosenStep", Private).GetValue(player) +
            " route=" + routeIndex + "/" + route.Count +
            (routeIndex < route.Count ? " target=" + route[routeIndex].ToString("F6") : "") +
            " free=" + space.FreeDistance(player.transform.position, player.GrowthDirection, 0.05f);
        result.restart = player.RestartSuggested;
        result.growthEnabled = player.enabled;
        result.stemAtStart = (bool)typeof(player_move).GetMethod("RevisitsStem", Private).Invoke(player,
            new object[] { result.position, result.position });
        result.sunward = ((Vector2)player.Sun.transform.position - space.CenterAt(result.position)).normalized;
        Vector2 axis = (Vector2)typeof(player_move).GetField("growthAxis", Private).GetValue(player);
        for (int angle = 0; angle < 360; angle += 2)
        {
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            if (!space.CanOccupy(result.position + direction * 0.05f) ||
                space.FreeDistance(result.position, direction, 0.05f) < 0.0499f) continue;
            result.physicalExits++;
            bool stem = (bool)typeof(player_move).GetMethod("RevisitsStem", Private).Invoke(player,
                new object[] { result.position, result.position + direction * 0.05f });
            bool forward = Vector2.Dot(direction, axis) >= -0.0001f ||
                (direction.y >= -0.0001f && Vector2.Dot(direction, result.sunward) >= -0.8660254f);
            if (!stem && forward) result.legalExits++;
            result.exits += angle + (stem ? "S" : "") + (forward ? "F" : "") + " ";
        }
        if (player.State == player_move.GrowthState.Waiting && result.legalExits > 0)
            throw new Exception("Stopped with " + result.legalExits + " physically clear legal directions: " + result.navigation);
        if (player.State != player_move.GrowthState.Waiting && player.State != player_move.GrowthState.ReachedSun)
            throw new Exception("Growth did not settle within 2400 steps.");
        if (result.startupIdleSteps > 0)
            throw new Exception("Growth waited " + result.startupIdleSteps + " ticks after its opening animation.");
        if ((result.level == "level_1" || result.level == "level_2" || result.level == "level_3") &&
            (result.mode == "cleared" || result.mode == "front-opened") &&
            player.State != player_move.GrowthState.ReachedSun)
            throw new Exception("The previously working opened route no longer reaches the sun.");
    }
}
