using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Keep the class and serialized entry points used by existing scenes/animation events.
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class player_move : MonoBehaviour
{
    public enum GrowthState { Dormant, Growing, Avoiding, Waiting, ReachedSun, Planning }
    public enum StopCause { None, TipCovered, NoForwardRoute, MotionBlocked }

    public GameObject Sun;
    public static player_move _instance;
    public Line_rendered line_render;
    [HideInInspector] public Trigger_checker[] Triggers;
    public new bool enabled = false;
    public GameObject first_dot;
    [Min(0f)] public float moveSpeed = 2.5f;
    [Range(0.1f, 5f)] public float speedMultiplier = 1f;
    [HideInInspector] public float duration_of_move = 0.4f;
    [HideInInspector] public float max_angle = 120f;

    [Header("Growth navigation")]
    [SerializeField] private LayerMask obstacleMask = 65; // Default + Block; old blocks use both.
    [SerializeField, Min(0.001f)] private float clearance = 0.02f;
    [SerializeField, Min(0.05f)] private float probeDistance = 0.35f;
    [SerializeField, Min(0.02f)] private float maxStepLength = 0.08f;
    [SerializeField, Min(0.1f)] private float checkpointSpacing = 0.5f;

    public GrowthState State { get; private set; } = GrowthState.Dormant;
    public bool RestartSuggested { get; private set; }
    public StopCause WaitingReason { get; private set; }
    public Vector2 GrowthDirection => heading;

    private Rigidbody2D body;
    private CapsuleCollider2D capsule;
    private FlowerGrowthCollision space;
    private flower visual;
    private Vector3 initialPosition;
    private Quaternion initialVisualRotation;
    private Vector2 growthAxis;
    private Vector2 heading;
    private int bypassSide;
    private bool initialized;
    private bool navigationDirty = true;
    private bool worldDirty = true;
    private bool changedAfterGrowth;
    private float nextBranchCheck;
    private float travelled;
    private float planningStep;
    private float nextRouteSearch;
    private float chosenStep;
    private IEnumerator routeSearch;
    private bool immediateSunward;
    private int routeIndex;
    private readonly FlowerGrowthRoutePlanner routePlanner = new FlowerGrowthRoutePlanner();
    private readonly List<Vector2> route = new List<Vector2>();
    private readonly List<Vector2> history = new List<Vector2>();
    private readonly List<Checkpoint> checkpoints = new List<Checkpoint>();

    private sealed class Checkpoint
    {
        public Vector2 position;
        public Vector2 heading;
        public float distance;
        public ulong exits;
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        visual = GetComponentInChildren<flower>();
        space = new FlowerGrowthCollision(capsule, line_render, obstacleMask, clearance);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        capsule.isTrigger = true;
        _instance = this;
        // Large moving sensors no longer control navigation or victory.
        foreach (Trigger_checker sensor in GetComponentsInChildren<Trigger_checker>(true))
        {
            sensor.enabled = false;
            foreach (Collider2D sensorCollider in sensor.GetComponents<Collider2D>())
                sensorCollider.enabled = false;
        }
    }

    private void Start()
    {
        initialPosition = transform.position;
        if (visual != null)
            initialVisualRotation = visual.transform.localRotation;
        growthAxis = Sun != null
            ? ((Vector2)Sun.transform.position - space.CenterAt(initialPosition)).normalized
            : new Vector2(1f, 1f).normalized;
        if (growthAxis.sqrMagnitude < 0.01f)
            growthAxis = new Vector2(1f, 1f).normalized;
        initialized = true;
        ResetPath();
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void change_enabled()
    {
        if (State == GrowthState.ReachedSun)
            return;
        enabled = !enabled;
        if (!enabled)
            State = GrowthState.Dormant;
        else if (navigationDirty || routeSearch != null)
            immediateSunward = true;
        worldDirty = true;
    }

    public void OnWorldChanged()
    {
        // Never discard an update while growing; query after transforms are synchronized.
        worldDirty = true;
        navigationDirty = true;
        immediateSunward = initialized && enabled;
        space?.InvalidateObstacles();
        if (initialized && travelled > 0.05f)
            changedAfterGrowth = true;
    }

    private void FixedUpdate()
    {
        PrewarmNavigation();
        TickGrowth(Time.fixedDeltaTime);
    }

    private void PrewarmNavigation()
    {
        // Use the seed's opening animation to prepare navigation. Starting growth
        // must retain this result instead of discarding it and making the player wait.
        if (!initialized || enabled || State != GrowthState.Dormant ||
            (!navigationDirty && routeSearch == null)) return;
        Physics2D.SyncTransforms();
        space.RefreshGeometry();
        if (space.CanOccupy(body.position))
            TryChooseDirection(body.position, Mathf.Min(maxStepLength, moveSpeed * Time.fixedDeltaTime), out _);
    }

    private void TickGrowth(float deltaTime)
    {
        if (!initialized || !enabled || State == GrowthState.ReachedSun || deltaTime <= 0f)
            return;
        Physics2D.SyncTransforms();
        space.RefreshGeometry();
        if (TouchesSun())
        {
            is_sun();
            return;
        }
        // A dragged block may cover the tip. Wait instead of teleporting out.
        if (!space.CanOccupy(body.position))
        {
            State = GrowthState.Waiting;
            WaitingReason = StopCause.TipCovered;
            CheckOpenedBranches(deltaTime);
            return;
        }

        bool wasWaiting = State == GrowthState.Waiting || State == GrowthState.Planning;
        float remaining = Mathf.Max(0f, moveSpeed * speedMultiplier) * deltaTime;
        while (remaining > 0.0001f)
        {
            float step = Mathf.Min(remaining, Mathf.Max(0.02f, maxStepLength));
            Vector2 position = body.position;
            if (!TryChooseDirection(position, step, out Vector2 direction))
            {
                State = routeSearch != null ? GrowthState.Planning : GrowthState.Waiting;
                WaitingReason = routeSearch != null ? StopCause.None : StopCause.NoForwardRoute;
                break;
            }
            float distance = space.FreeDistance(position, direction, Mathf.Min(step, chosenStep));
            if (distance < 0.0001f)
            {
                State = GrowthState.Waiting;
                WaitingReason = StopCause.MotionBlocked;
                break;
            }
            Vector2 next = position + direction * distance;
            if (!space.CanOccupy(next))
            {
                State = GrowthState.Waiting;
                WaitingReason = StopCause.MotionBlocked;
                break;
            }
            // Explicitly swept kinematic placement; forces cannot bend the grown stem.
            body.position = next;
            heading = direction;
            travelled += distance;
            history.Add(next);
            line_render?.AppendGrowthPoint(next);
            State = bypassSide == 0 ? GrowthState.Growing : GrowthState.Avoiding;
            WaitingReason = StopCause.None;
            remaining -= distance;
            if (TouchesSun())
            {
                is_sun();
                return;
            }
            if (travelled - checkpoints[checkpoints.Count - 1].distance >= checkpointSpacing)
                RecordCheckpoint();
        }
        if (wasWaiting && (State == GrowthState.Growing || State == GrowthState.Avoiding))
        {
            // The opening is usable from this tip. Do not advertise the same change
            // as a missed branch at an older checkpoint along the approach.
            foreach (Checkpoint checkpoint in checkpoints)
                checkpoint.exits = AvailableExits(checkpoint.position, checkpoint.heading);
            RestartSuggested = false;
            worldDirty = false;
        }
        else
            CheckOpenedBranches(deltaTime);
    }

    private Vector2 SunDirection(Vector2 position)
    {
        Vector2 direction = Sun != null
            ? (Vector2)Sun.transform.position - space.CenterAt(position)
            : growthAxis;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : growthAxis;
    }

    private bool TryChooseDirection(Vector2 position, float step, out Vector2 direction)
    {
        planningStep = step;
        chosenStep = step;
        if (navigationDirty)
        {
            route.Clear();
            routeSearch = null;
            routeIndex = 0;
            nextRouteSearch = travelled;
            bypassSide = 0;
            navigationDirty = false;
        }
        if (Sun != null)
        {
            direction = SunDirection(position);
            if (immediateSunward)
            {
                if (CanGrow(position, direction, Mathf.Max(step, probeDistance)))
                {
                    // React to an opening now. A straight, swept sunward approach
                    // does not require exploring every still-closed route to the sun.
                    route.Clear();
                    routeIndex = 0;
                    routeSearch = null;
                    nextRouteSearch = travelled;
                    bypassSide = 0;
                    return true;
                }
                immediateSunward = false;
            }
            Vector2 goal = (Vector2)Sun.transform.position - (space.CenterAt(position) - position);
            Vector2 goalDirection = (goal - position).normalized;
            if (CanGrow(position, goalDirection, Mathf.Min(step, Vector2.Distance(position, goal))) &&
                CanPlanSegment(position, goal))
            {
                route.Clear();
                routeIndex = 0;
                routeSearch = null;
                bypassSide = 0;
                direction = (goal - position).normalized;
                chosenStep = Mathf.Min(step, Vector2.Distance(position, goal));
                return true;
            }
            while (routeIndex < route.Count && Vector2.Distance(position, route[routeIndex]) < 0.001f)
                routeIndex++;
            if (routeIndex < route.Count && !CanPlanSegment(position, route[routeIndex]))
            {
                route.Clear();
                routeIndex = 0;
                nextRouteSearch = travelled;
            }
            if (routeIndex >= route.Count && travelled >= nextRouteSearch)
            {
                routeIndex = 0;
                if (routeSearch == null) routeSearch = SearchRoute(position, goal);
                if (routeSearch.MoveNext())
                {
                    direction = Vector2.zero;
                    return false;
                }
                routeSearch = null;
                // With unchanged obstacles a failed complete search cannot become
                // reachable by adding more stem. Retry when the world changes.
                nextRouteSearch = float.PositiveInfinity;
            }
            if (routeIndex < route.Count)
            {
                Vector2 delta = route[routeIndex] - position;
                direction = delta.normalized;
                chosenStep = Mathf.Min(step, delta.magnitude);
                if (CanGrow(position, direction, chosenStep))
                {
                    bypassSide = 1;
                    return true;
                }
                // The real step is authoritative, including at tight polygon corners.
                route.Clear();
                routeIndex = 0;
                nextRouteSearch = float.PositiveInfinity;
            }
        }
        chosenStep = step;
        return TryChooseLocalDirection(position, step, out direction);
    }

    private IEnumerator SearchRoute(Vector2 start, Vector2 goal)
    {
        Rect bounds = RouteBounds(start, goal);
        // The tip stays still during one search and world changes cancel it. Reuse
        // occupancy results for shared grid points instead of querying every incoming edge.
        var occupancy = new Dictionary<Vector2, bool>();
        bool CanMove(Vector2 from, Vector2 to) => ValidatePlanSegment(from, to, occupancy);
        IEnumerator search = routePlanner.Search(start, goal, growthAxis, bounds, CanMove, route);
        while (search.MoveNext()) yield return null;
        if (route.Count == 0)
        {
            search = routePlanner.Search(start, goal, growthAxis, bounds, CanMove, route, 0.125f);
            while (search.MoveNext()) yield return null;
        }
    }

    private bool CanPlanSegment(Vector2 start, Vector2 end) => ValidatePlanSegment(start, end, null);

    private bool ValidatePlanSegment(Vector2 start, Vector2 end, Dictionary<Vector2, bool> occupancy)
    {
        float distance = Vector2.Distance(start, end);
        if (distance < 0.0001f) return true;
        bool CanOccupy(Vector2 point)
        {
            if (occupancy == null) return space.CanOccupy(point);
            if (!occupancy.TryGetValue(point, out bool clear))
            {
                clear = space.CanOccupy(point);
                occupancy.Add(point, clear);
            }
            return clear;
        }
        if (!CanOccupy(end)) return false;
        if (space.FreeDistance(start, (end - start) / distance, distance) < distance - 0.0001f ||
            RevisitsStem(start, end)) return false;
        // Complex authored polygons can disagree between a long shape cast and an
        // overlap inside that segment. Validate the same short placements used by growth.
        int samples = Mathf.CeilToInt(distance / Mathf.Min(0.05f, maxStepLength));
        for (int i = 1; i < samples; i++)
            if (!CanOccupy(Vector2.Lerp(start, end, (float)i / samples))) return false;
        return true;
    }

    private Rect RouteBounds(Vector2 start, Vector2 goal)
    {
        Camera camera = Camera.main;
        if (camera != null && camera.orthographic)
        {
            Vector2 half = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
            Vector2 offset = space.CenterAt(start) - start;
            Vector2 minimum = (Vector2)camera.transform.position - half - offset + space.Size * 0.5f;
            Vector2 maximum = (Vector2)camera.transform.position + half - offset - space.Size * 0.5f;
            // The authored seed and sun may sit partly outside the camera's safe inset.
            minimum = Vector2.Min(minimum, Vector2.Min(start, goal));
            maximum = Vector2.Max(maximum, Vector2.Max(start, goal));
            return Rect.MinMaxRect(minimum.x - 0.001f, minimum.y - 0.001f,
                maximum.x + 0.001f, maximum.y + 0.001f);
        }
        Vector2 low = Vector2.Min(start, goal) - Vector2.one * 3f;
        Vector2 high = Vector2.Max(start, goal) + Vector2.one * 3f;
        return Rect.MinMaxRect(low.x, low.y, high.x, high.y);
    }

    private bool TryChooseLocalDirection(Vector2 position, float step, out Vector2 direction)
    {
        planningStep = step;
        if (navigationDirty)
        {
            bypassSide = 0;
            navigationDirty = false;
        }
        direction = SunDirection(position);
        float probe = Mathf.Max(step, probeDistance);
        // A longer clear exit avoids oscillating between sunlight and wall following.
        float exitProbe = bypassSide == 0 ? probe : probe * 1.75f;
        if (CanGrow(position, direction, exitProbe))
        {
            bypassSide = 0;
            return true;
        }
        Vector2 sunward = direction;
        int upperSide = Rotate(sunward, 5f).y >= Rotate(sunward, -5f).y ? 1 : -1;
        int preferredSide = bypassSide == 0 ? upperSide : bypassSide;
        // Prefer detours that still advance toward the sun, on either side. Only
        // when both are blocked may following the edge temporarily lose projection.
        // Otherwise an early upper search can choose a long detour back toward the seed.
        if (TryBypassPair(position, sunward, preferredSide, probe, true, out direction))
            return true;
        // A curved passage may fit the next swept step without fitting an entire
        // straight probe. Replan at the real step length before declaring it blocked.
        if (CanGrow(position, sunward, step))
        {
            direction = sunward;
            bypassSide = 0;
            return true;
        }
        if (TryBypassPair(position, sunward, preferredSide, step, true, out direction))
            return true;
        return TryBypassPair(position, sunward, preferredSide, probe, false, out direction) ||
               TryBypassPair(position, sunward, preferredSide, step, false, out direction) ||
               TryFineBypass(position, sunward, step, out direction);
    }

    private bool TryFineBypass(Vector2 position, Vector2 sunward, float step, out Vector2 direction)
    {
        direction = Vector2.zero;
        float best = float.NegativeInfinity;
        // A very narrow opening can fall between the coarse fan's directions.
        // Resolve it before classifying the tip as trapped.
        for (int angle = 0; angle < 360; angle++)
        {
            Vector2 candidate = Rotate(Vector2.right, angle);
            bool progress = AdvancesDetour(candidate, sunward, true);
            if (!progress && !AdvancesDetour(candidate, sunward, false)) continue;
            float score = (progress ? 3f : 0f) + Vector2.Dot(candidate, sunward) + candidate.y * 0.001f;
            if (score <= best || !CanGrow(position, candidate, step)) continue;
            best = score;
            direction = candidate;
        }
        if (float.IsNegativeInfinity(best)) return false;
        bypassSide = sunward.x * direction.y - sunward.y * direction.x >= 0f ? 1 : -1;
        return true;
    }

    private bool TryBypassPair(Vector2 position, Vector2 sunward, int preferredSide, float probe,
        bool preferProgress, out Vector2 direction)
    {
        if (TryBypass(position, sunward, preferredSide, probe, preferProgress, out direction))
        {
            bypassSide = preferredSide;
            return true;
        }
        if (TryBypass(position, sunward, -preferredSide, probe, preferProgress, out direction))
        {
            bypassSide = -preferredSide;
            return true;
        }
        return false;
    }

    private bool TryBypass(Vector2 position, Vector2 sunward, int side, float probe, bool preferProgress,
        out Vector2 direction)
    {
        for (int angle = 5; angle <= 175; angle += 5)
        {
            Vector2 candidate = Rotate(sunward, angle * side);
            if (AdvancesDetour(candidate, sunward, preferProgress) &&
                CanGrow(position, candidate, probe))
            {
                direction = candidate;
                return true;
            }
        }
        // A finite fan can miss the exact tangent of a flat wall. Include the real surface
        // tangent, otherwise a near-vertical ray hits the wall and its neighbour points backward.
        if (space.TryGetBlockingNormal(position, sunward, probe * 2f, out Vector2 normal))
        {
            Vector2 tangent = new Vector2(-normal.y, normal.x);
            if ((sunward.x * tangent.y - sunward.y * tangent.x) * side < 0f)
                tangent = -tangent;
            if (AdvancesDetour(tangent, sunward, preferProgress) &&
                CanGrow(position, tangent, probe))
            {
                direction = tangent;
                return true;
            }
        }
        direction = Vector2.zero;
        return false;
    }

    private bool AdvancesDetour(Vector2 direction, Vector2 sunward, bool preferProgress)
    {
        // A sideways/upward wall-following step may lose sun-axis projection.
        // Do not extend this fallback downward toward the seed.
        return preferProgress ? Vector2.Dot(direction, growthAxis) >= -0.0001f :
            direction.y >= -0.0001f && Vector2.Dot(direction, sunward) >= -0.8660254f;
    }

    private bool CanGrow(Vector2 position, Vector2 direction, float distance)
    {
        return !RevisitsStem(position, position + direction * distance) &&
               space.CanOccupy(position + direction * Mathf.Min(planningStep, distance)) &&
               space.FreeDistance(position, direction, distance) >= distance - 0.0001f;
    }

    private bool RevisitsStem(Vector2 start, Vector2 end)
    {
        if (history.Count >= 2)
        {
            Vector2 previous = history[history.Count - 2];
            // Even a stem shorter than the junction allowance must not be retraced.
            if (Vector2.Dot(end - start, previous - start) > 0f &&
                FlowerGrowthCollision.DistanceToSegment(previous, start, end) < 0.002f)
                return true;
        }
        float tailLength = 0f;
        float recentTail = Mathf.Max(probeDistance * 3f, space.Size.magnitude * 1.5f);
        Vector2 minimum = Vector2.Min(start, end);
        Vector2 maximum = Vector2.Max(start, end);
        for (int i = history.Count - 1; i > 0; i--)
        {
            float previousTail = tailLength;
            float length = Vector2.Distance(history[i], history[i - 1]);
            tailLength += length;
            // Adjacent segments share the current tip. Skip only this tiny junction,
            // then check the whole proposed segment, including the recent stem.
            if (tailLength <= 0.15f)
                continue;
            Vector2 segmentEnd = previousTail < 0.15f
                ? Vector2.Lerp(history[i], history[i - 1], (0.15f - previousTail) / length)
                : history[i];
            float radius = previousTail > recentTail ? 0.15f : 0.04f;
            Vector2 otherStart = history[i - 1];
            if (Mathf.Max(otherStart.x, segmentEnd.x) < minimum.x - radius ||
                Mathf.Min(otherStart.x, segmentEnd.x) > maximum.x + radius ||
                Mathf.Max(otherStart.y, segmentEnd.y) < minimum.y - radius ||
                Mathf.Min(otherStart.y, segmentEnd.y) > maximum.y + radius) continue;
            if (FlowerGrowthCollision.SegmentDistance(start, end, history[i - 1], segmentEnd) < radius)
                return true;
        }
        return false;
    }

    private static Vector2 Rotate(Vector2 direction, float degrees)
    {
        float angle = degrees * Mathf.Deg2Rad;
        return new Vector2(direction.x * Mathf.Cos(angle) - direction.y * Mathf.Sin(angle),
            direction.x * Mathf.Sin(angle) + direction.y * Mathf.Cos(angle));
    }

    private ulong AvailableExits(Vector2 position, Vector2 previous)
    {
        if (!space.CanOccupy(position))
            return 0;
        ulong exits = 0;
        // Fixed world directions make snapshots comparable after the tip has moved.
        for (int i = 0; i < 64; i++)
        {
            Vector2 direction = Rotate(growthAxis, i * (360f / 64f));
            if (space.FreeDistance(position, direction, probeDistance * 2f) >= probeDistance * 2f - 0.0001f)
                exits |= 1UL << i;
        }
        return exits;
    }

    private void RecordCheckpoint()
    {
        checkpoints.Add(new Checkpoint
        {
            position = body.position, heading = heading, distance = travelled,
            exits = AvailableExits(body.position, heading)
        });
    }

    private void CheckOpenedBranches(float deltaTime)
    {
        if (State == GrowthState.Planning)
        {
            RestartSuggested = false;
            return;
        }
        nextBranchCheck -= deltaTime;
        if (nextBranchCheck > 0f || (!worldDirty && State != GrowthState.Waiting))
            return;
        nextBranchCheck = 0.25f;
        worldDirty = false;
        // A usable opening may have appeared during an earlier automatic resume.
        // If a later stop really requires retreating, keep the soft restart available.
        RestartSuggested = State == GrowthState.Waiting &&
            WaitingReason == StopCause.NoForwardRoute && changedAfterGrowth;
        if (RestartSuggested)
            return;
        // A branch is behind us once another navigation checkpoint has been committed.
        // Using the full capsule diagonal hid restart hints for short, already-trapped stems.
        float passedDistance = State == GrowthState.Waiting ? 0.05f : Mathf.Max(0.1f, checkpointSpacing);
        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (travelled - checkpoint.distance < passedDistance)
                break;
            if ((AvailableExits(checkpoint.position, checkpoint.heading) & ~checkpoint.exits) != 0)
            {
                RestartSuggested = true;
                break;
            }
        }
    }

    private bool TouchesSun()
    {
        if (Sun == null)
            return false;
        foreach (Collider2D target in Sun.GetComponentsInChildren<Collider2D>())
            if (target.enabled && target.gameObject.activeInHierarchy && capsule.Distance(target).isOverlapped)
                return true;
        return false;
    }

    public void is_sun()
    {
        if (State == GrowthState.ReachedSun)
            return;
        State = GrowthState.ReachedSun;
        enabled = false;
        RestartSuggested = false;
        WaitingReason = StopCause.None;
        if (Application.isPlaying)
            StartCoroutine(FinishAtSun());
    }

    private IEnumerator FinishAtSun()
    {
        if (visual != null && Sun != null)
            yield return visual.rotate_flower_to_sun(Sun.transform.position);
        if (visual != null)
            visual.anim_flower(true);
        if (Sun != null)
            Destroy(Sun);
    }

    public void RestartGrowth()
    {
        if (!initialized || State == GrowthState.ReachedSun || Time.timeScale <= 0f)
            return;
        body.position = initialPosition;
        transform.position = initialPosition;
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        if (visual != null)
            visual.transform.localRotation = initialVisualRotation;
        // Blocks, collected powerups and caterpillars keep their current state.
        enabled = true;
        ResetPath();
        immediateSunward = true;
        Physics2D.SyncTransforms();
    }

    private void ResetPath()
    {
        // On scene load, the Transform already has its authored world position while
        // Rigidbody2D.position may still contain the previous, unsynchronized physics pose.
        // Initialize both physics and the path from the same authoritative spawn position.
        Vector2 headPosition = transform.position;
        body.position = headPosition;
        Physics2D.SyncTransforms();
        space.RefreshGeometry();
        heading = Vector2.zero;
        bypassSide = 0;
        travelled = 0f;
        route.Clear();
        routeSearch = null;
        immediateSunward = false;
        routeIndex = 0;
        nextRouteSearch = 0f;
        history.Clear();
        history.Add(headPosition);
        checkpoints.Clear();
        RestartSuggested = false;
        worldDirty = true;
        navigationDirty = true;
        changedAfterGrowth = false;
        WaitingReason = StopCause.None;
        nextBranchCheck = 0f;
        State = enabled ? GrowthState.Growing : GrowthState.Dormant;
        line_render?.ResetGrowth(first_dot != null ? (Vector2)first_dot.transform.position : headPosition,
            headPosition);
        RecordCheckpoint();
    }

    private void LateUpdate()
    {
        if (enabled && (State == GrowthState.Growing || State == GrowthState.Avoiding))
            visual?.RotateByDirection(heading);
    }

    // Legacy query: do not commit a direction until movement actually happens.
    public bool TryFindNextPosition(out Vector3 targetPosition, out Trigger_checker targetTrigger)
    {
        targetPosition = transform.position;
        targetTrigger = null;
        if (!initialized)
            return false;
        int savedSide = bypassSide;
        bool savedDirty = navigationDirty;
        Vector2 direction;
        int index = routeIndex;
        while (index < route.Count && Vector2.Distance(body.position, route[index]) < 0.001f) index++;
        bool found;
        float distance = maxStepLength;
        if (!navigationDirty && index < route.Count && CanPlanSegment(body.position, route[index]))
        {
            Vector2 delta = route[index] - body.position;
            direction = delta.normalized;
            distance = Mathf.Min(distance, delta.magnitude);
            found = true;
        }
        else found = TryChooseLocalDirection(body.position, maxStepLength, out direction);
        bypassSide = savedSide;
        navigationDirty = savedDirty;
        if (found)
            targetPosition += (Vector3)(direction * distance);
        return found;
    }

    private void OnDrawGizmosSelected()
    {
        if (capsule == null)
            capsule = GetComponent<CapsuleCollider2D>();
        if (capsule == null)
            return;
        Gizmos.color = State == GrowthState.Waiting ? Color.yellow : Color.green;
        Gizmos.DrawWireCube(capsule.bounds.center, capsule.bounds.size);
        Gizmos.DrawRay(capsule.bounds.center, (Vector3)heading * probeDistance);
    }
}
