using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public class ForestLevelGeneratorWindow : EditorWindow
{
    private const string GeneratedRootName = "[Generated Forest Level]";
    private const string UsualPrefabPath = "Assets/prefabs/usual_splineblock.prefab";
    private const string BreakablePrefabPath = "Assets/prefabs/crush_block_splineprefab.prefab";
    private const string MovablePrefabPath = "Assets/prefabs/moving_splineplatform.prefab";
    private const string LeafPrefabPath = "Assets/prefabs/generated/LeavesGameplay.prefab";
    private const string CaterpillarPrefabPath = "Assets/prefabs/enemy.prefab";
    private const float MinMovableWorldSize = 2.9f;
    private const float GrowthCorridorRadius = 0.72f;
    private static readonly Vector2 GrowthHeadHalfSize = new Vector2(0.29f, 0.59f);
    // Exact priority order from player_packet.prefab / player_move.Triggers.
    private static readonly Vector2[] GrowthTriggerOffsets =
    {
        new Vector2(0f, 1.76f),
        new Vector2(0.39f, 1.66f),
        new Vector2(-0.41f, 1.77f),
        new Vector2(0.65f, 1.15f),
        new Vector2(-0.7f, 1.2f),
        new Vector2(0.65f, 0.55f),
        new Vector2(-0.7f, 0.6f),
        new Vector2(0.65f, -0.06f),
        new Vector2(-0.7f, 0.05f)
    };
    private static readonly Vector3 AuthoredCaterpillarScale =
        new Vector3(0.6440086f, 0.6440086f, 0.25760344f);

    private enum EncounterTemplate
    {
        Random,
        ProtectionThenCaterpillar,
        MoveBushToCaterpillar,
        TwoPathsTwoCaterpillars,
        BreakForCaterpillar,
        MoveBushBlocksPath
    }

    private enum BlockType
    {
        Usual,
        Breakable,
        Movable
    }

    private enum ShapeFamily
    {
        Blob,
        Capsule,
        LongWall,
        FlatPlatform,
        Chunk,
        Crescent,
        LShape,
        Bean,
        Petal
    }

    [SerializeField] private int seed = 12345;
    [SerializeField] private EncounterTemplate encounter = EncounterTemplate.Random;
    [SerializeField, Range(8, 24)] private int obstacleCount = 16;
    [SerializeField, Range(0f, 0.65f)] private float splineChaos = 0.25f;
    [SerializeField] private float pathWidth = 2.8f;
    [SerializeField] private float playableHalfWidth = 5.2f;
    [SerializeField] private float minObstacleSize = 1.2f;
    [SerializeField] private float maxObstacleSize = 3.6f;
    [SerializeField, Range(0f, 1f)] private float usualWeight = 0.45f;
    [SerializeField, Range(0f, 1f)] private float breakableWeight = 0.35f;
    [SerializeField, Range(0f, 1f)] private float movableWeight = 0.2f;
    [SerializeField] private Transform flower;
    [SerializeField] private Transform sun;
    [SerializeField] private Vector2 fallbackFlowerPosition = new Vector2(0f, -8f);
    [SerializeField] private Vector2 fallbackSunPosition = new Vector2(0f, 8f);

    private GameObject usualPrefab;
    private GameObject breakablePrefab;
    private GameObject movablePrefab;
    private GameObject leafPrefab;
    private GameObject caterpillarPrefab;
    private Material[] usualMaterials;
    private Material[] breakableMaterials;
    private Vector2 scroll;

    private struct BlockPlan
    {
        public Vector2 center;
        public Vector2 size;
        public float angle;
        public BlockType type;
        public ShapeFamily shape;

        public BlockPlan(Vector2 center, Vector2 size, float angle, BlockType type, ShapeFamily shape)
        {
            this.center = center;
            this.size = size;
            this.angle = angle;
            this.type = type;
            this.shape = shape;
        }
    }

    private struct SafeZone
    {
        public Vector2 center;
        public float radius;

        public SafeZone(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }

    private sealed class GrowthRouteState
    {
        public Vector2 position;
        public Vector2 lastDirection;
        public List<Vector2> points;
        public int priorityCost;
        public float score;
    }

    [MenuItem("Tools/Forest Level Generator")]
    private static void Open()
    {
        GetWindow<ForestLevelGeneratorWindow>("Forest Generator");
    }

    private void OnEnable()
    {
        LoadPrefabs();
        AutoFindEndpoints(false);
    }

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Forest Level Generator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Генерирует редактируемую заготовку в открытой сцене. Runtime-код игры не изменяется.",
            MessageType.Info);

        DrawEndpoints();
        EditorGUILayout.Space();
        DrawRecipe();
        EditorGUILayout.Space();
        DrawActions();
        EditorGUILayout.Space();
        DrawPrefabStatus();
        EditorGUILayout.EndScrollView();
    }

    private void DrawEndpoints()
    {
        EditorGUILayout.LabelField("Путь цветка", EditorStyles.boldLabel);
        flower = (Transform)EditorGUILayout.ObjectField("Flower", flower, typeof(Transform), true);
        sun = (Transform)EditorGUILayout.ObjectField("Sun", sun, typeof(Transform), true);

        if (flower == null)
            fallbackFlowerPosition = EditorGUILayout.Vector2Field("Flower position", fallbackFlowerPosition);
        if (sun == null)
            fallbackSunPosition = EditorGUILayout.Vector2Field("Sun position", fallbackSunPosition);

        if (GUILayout.Button("Найти Flower и Sun в сцене"))
            AutoFindEndpoints(true);
    }

    private void DrawRecipe()
    {
        EditorGUILayout.LabelField("Recipe", EditorStyles.boldLabel);
        seed = EditorGUILayout.IntField("Seed", seed);
        encounter = (EncounterTemplate)EditorGUILayout.EnumPopup("Encounter", encounter);
        EditorGUILayout.HelpBox(GetScenarioDescription(encounter), MessageType.None);
        obstacleCount = EditorGUILayout.IntSlider("Spline blocks", obstacleCount, 8, 24);
        splineChaos = EditorGUILayout.Slider("Spline chaos", splineChaos, 0f, 0.65f);
        pathWidth = Mathf.Max(1f, EditorGUILayout.FloatField("Encounter width", pathWidth));
        playableHalfWidth = Mathf.Max(pathWidth, EditorGUILayout.FloatField("Playable half width", playableHalfWidth));
        minObstacleSize = Mathf.Max(0.5f, EditorGUILayout.FloatField("Min block size", minObstacleSize));
        maxObstacleSize = Mathf.Max(minObstacleSize, EditorGUILayout.FloatField("Max block size", maxObstacleSize));

        EditorGUILayout.Space(3f);
        EditorGUILayout.LabelField("Типы блоков");
        usualWeight = EditorGUILayout.Slider("Usual", usualWeight, 0f, 1f);
        breakableWeight = EditorGUILayout.Slider("Breakable", breakableWeight, 0f, 1f);
        movableWeight = EditorGUILayout.Slider("Movable", movableWeight, 0f, 1f);
    }

    private static string GetScenarioDescription(EncounterTemplate scenario)
    {
        switch (scenario)
        {
            case EncounterTemplate.ProtectionThenCaterpillar:
                return "Куст перед гусеницей: сначала листья-защита, затем встреча с гусеницей.";
            case EncounterTemplate.MoveBushToCaterpillar:
                return "Куст закреплён на подвижном блоке; его нужно доставить к гусенице.";
            case EncounterTemplate.TwoPathsTwoCaterpillars:
                return "Две ветви и две гусеницы; куст находится только на одной ветви.";
            case EncounterTemplate.BreakForCaterpillar:
                return "Разрушаемый блок разделяет гусеницу и куст.";
            case EncounterTemplate.MoveBushBlocksPath:
                return "Куст на подвижном блоке: доставить к гусенице и затем освободить маршрут.";
            default:
                return "Случайно выбирает один из пяти сценариев.";
        }
    }

    private void DrawActions()
    {
        EditorGUILayout.LabelField("Действия", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!PrefabsAreReady()))
        {
            if (GUILayout.Button("Generate", GUILayout.Height(32f)))
                Generate();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Randomize seed"))
            {
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                Repaint();
            }
            if (GUILayout.Button("Regenerate shapes"))
                RegenerateAllShapes();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Regenerate selected block shapes"))
                RegenerateSelectedShapes();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bake to editable objects"))
            BakeGeneratedLevel();
        if (GUILayout.Button("Clear generated"))
            ClearGeneratedLevel();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPrefabStatus()
    {
        if (PrefabsAreReady())
        {
            EditorGUILayout.HelpBox("Все штатные префабы найдены.", MessageType.None);
            return;
        }

        EditorGUILayout.HelpBox(
            "Не найдены один или несколько префабов. Проверьте пути в Assets/prefabs.",
            MessageType.Error);
        if (GUILayout.Button("Reload prefabs"))
            LoadPrefabs();
    }

    private void LoadPrefabs()
    {
        usualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UsualPrefabPath);
        breakablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BreakablePrefabPath);
        movablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MovablePrefabPath);
        leafPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeafPrefabPath);
        if (leafPrefab == null)
            leafPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabs/Leaf.prefab");
        caterpillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CaterpillarPrefabPath);
        usualMaterials = new[]
        {
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Pink_Crush.mat"),
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Orange_Crush.mat"),
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Yellow_Crush.mat")
        };
        breakableMaterials = new[]
        {
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Pink_Crush_1.mat"),
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Orange_Crush_1.mat"),
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/Yellow_Crush_1.mat")
        };
    }

    private bool PrefabsAreReady()
    {
        return usualPrefab != null && breakablePrefab != null && movablePrefab != null &&
               leafPrefab != null && caterpillarPrefab != null && MaterialsAreReady(usualMaterials) &&
               MaterialsAreReady(breakableMaterials);
    }

    private static bool MaterialsAreReady(Material[] materials)
    {
        if (materials == null || materials.Length == 0)
            return false;
        foreach (Material material in materials)
        {
            if (material == null)
                return false;
        }
        return true;
    }

    private void AutoFindEndpoints(bool reportFailure)
    {
        if (flower == null)
            flower = FindSceneTransform("Flower", null);
        if (sun == null)
            sun = FindSceneTransform("sun", "sun");

        if (reportFailure && (flower == null || sun == null))
        {
            EditorUtility.DisplayDialog(
                "Forest Level Generator",
                "Flower или Sun не найдены. Можно указать их вручную либо использовать координаты в окне.",
                "OK");
        }
    }

    private static Transform FindSceneTransform(string objectName, string tag)
    {
        if (!string.IsNullOrEmpty(tag))
        {
            try
            {
                GameObject tagged = GameObject.FindGameObjectWithTag(tag);
                if (tagged != null)
                    return tagged.transform;
            }
            catch (UnityException)
            {
                // The tag may not exist in an early prototype scene.
            }
        }

        GameObject named = GameObject.Find(objectName);
        return named != null ? named.transform : null;
    }

    private void Generate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Forest Level Generator", "Остановите Play Mode перед генерацией.", "OK");
            return;
        }

        Vector2 flowerPosition = flower != null ? (Vector2)flower.position : fallbackFlowerPosition;
        Vector2 sunPosition = sun != null ? (Vector2)sun.position : fallbackSunPosition;
        if (Mathf.Abs(sunPosition.y - flowerPosition.y) < 7f)
        {
            EditorUtility.DisplayDialog(
                "Forest Level Generator",
                "Между Flower и Sun должно быть хотя бы 7 world units.",
                "OK");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Generate forest level");
        ClearGeneratedLevelInternal();

        var rng = new System.Random(seed);
        var root = new GameObject(GeneratedRootName);
        Undo.RegisterCreatedObjectUndo(root, "Create generated forest level");

        // Authored levels are composed in a vertical portrait field. Flower and Sun may sit in
        // opposite horizontal corners, but that must not rotate the entire level layout.
        float layoutCenterX = ResolveLayoutCenterX(flowerPosition, sunPosition);
        Vector2 start = new Vector2(layoutCenterX, flowerPosition.y);
        Vector2 goal = new Vector2(layoutCenterX, sunPosition.y);
        Vector2 direction = sunPosition.y >= flowerPosition.y ? Vector2.up : Vector2.down;
        Vector2 side = new Vector2(-direction.y, direction.x);
        float pathAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        EncounterTemplate chosenEncounter = ResolveEncounter(rng);

        Vector2 leafPosition;
        Vector2 caterpillarPosition;
        var plans = BuildEncounter(
            chosenEncounter,
            start,
            goal,
            direction,
            side,
            pathAngle,
            rng,
            out leafPosition,
            out caterpillarPosition);

        var safeZones = new List<SafeZone>
        {
            new SafeZone(flowerPosition, 1.8f),
            new SafeZone(sunPosition, 1.8f),
            new SafeZone(leafPosition, 1.15f),
            new SafeZone(caterpillarPosition, 1.45f)
        };

        List<Vector2> growthRoute;
        if (!TryBuildGrowthRoute(plans, flowerPosition, sunPosition, out growthRoute))
        {
            growthRoute = BuildFallbackGrowthRoute(flowerPosition, sunPosition);
            RemovePlansBlockingRoute(plans, growthRoute);
        }
        AddInteractionGates(plans, growthRoute, safeZones, rng);
        AddGrowthRouteSafeZones(safeZones, growthRoute);

        AddFillerBlocks(plans, safeZones, start, goal, direction, side, pathAngle, rng);

        var createdBlocks = new List<GameObject>(plans.Count);
        for (int i = 0; i < plans.Count; i++)
            createdBlocks.Add(CreateBlock(plans[i], root.transform, rng, i));

        PlaceScenarioContent(
            chosenEncounter,
            plans,
            createdBlocks,
            leafPosition,
            caterpillarPosition,
            root.transform,
            rng);

        root.name = GeneratedRootName + " - " + chosenEncounter + " - seed " + seed;
        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    private static float ResolveLayoutCenterX(Vector2 flowerPosition, Vector2 sunPosition)
    {
        Camera sceneCamera = Camera.main;
        if (sceneCamera == null)
            sceneCamera = FindObjectOfType<Camera>();
        return sceneCamera != null
            ? sceneCamera.transform.position.x
            : (flowerPosition.x + sunPosition.x) * 0.5f;
    }

    private EncounterTemplate ResolveEncounter(System.Random rng)
    {
        if (encounter != EncounterTemplate.Random)
            return encounter;
        return (EncounterTemplate)rng.Next(1, Enum.GetValues(typeof(EncounterTemplate)).Length);
    }

    private List<BlockPlan> BuildEncounter(
        EncounterTemplate template,
        Vector2 start,
        Vector2 goal,
        Vector2 direction,
        Vector2 side,
        float pathAngle,
        System.Random rng,
        out Vector2 leafPosition,
        out Vector2 caterpillarPosition)
    {
        var plans = new List<BlockPlan>();
        float sideSign = rng.NextDouble() < 0.5 ? -1f : 1f;
        Func<float, float, Vector2> onPath = (t, offset) => Vector2.Lerp(start, goal, t) + side * offset;

        leafPosition = onPath(
            RandomRange(rng, 0.24f, 0.54f),
            sideSign * pathWidth * RandomRange(rng, 0.52f, 1.02f));
        caterpillarPosition = onPath(
            RandomRange(rng, 0.5f, 0.79f),
            pathWidth * RandomRange(rng, -0.48f, 0.48f));

        switch (template)
        {
            case EncounterTemplate.ProtectionThenCaterpillar:
            {
                // A readable three-part situation: the caterpillar sits on a central island,
                // while two side blocks make the route around it visible.
                float encounterT = RandomRange(rng, 0.57f, 0.74f);
                float encounterOffset = pathWidth * RandomRange(rng, -0.32f, 0.32f);
                caterpillarPosition = onPath(encounterT, encounterOffset);
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.045f, encounterOffset),
                    new Vector2(2.7f, 1.05f),
                    pathAngle + 90f,
                    BlockType.Usual,
                    ShapeFamily.FlatPlatform));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.105f, encounterOffset + pathWidth * 1.08f),
                    new Vector2(2.4f, 1.15f),
                    pathAngle + 90f,
                    BlockType.Breakable,
                    ShapeFamily.Chunk));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.105f, encounterOffset - pathWidth * 1.08f),
                    new Vector2(2.4f, 1.15f),
                    pathAngle + 90f,
                    BlockType.Usual,
                    ShapeFamily.Blob));
                break;
            }

            case EncounterTemplate.MoveBushToCaterpillar:
            {
                float encounterT = RandomRange(rng, 0.27f, 0.55f);
                float encounterOffset = sideSign * pathWidth * RandomRange(rng, 0.68f, 0.98f);
                leafPosition = onPath(encounterT, encounterOffset);
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.04f, encounterOffset - sideSign * pathWidth * 0.54f),
                    new Vector2(MinMovableWorldSize, MinMovableWorldSize),
                    pathAngle + 90f,
                    BlockType.Movable,
                    ShapeFamily.Bean));
                plans.Add(new BlockPlan(
                    onPath(encounterT + 0.05f, encounterOffset + sideSign * pathWidth * 0.4f),
                    new Vector2(2.25f, 1.2f),
                    pathAngle + 90f,
                    BlockType.Usual,
                    ShapeFamily.Blob));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.07f, encounterOffset - sideSign * pathWidth * 1.58f),
                    new Vector2(2.1f, 1.05f),
                    pathAngle + 90f,
                    BlockType.Breakable,
                    ShapeFamily.Capsule));
                break;
            }

            case EncounterTemplate.TwoPathsTwoCaterpillars:
            {
                float encounterT = RandomRange(rng, 0.25f, 0.53f);
                float encounterOffset = sideSign * pathWidth * RandomRange(rng, 0.72f, 1f);
                leafPosition = onPath(encounterT, encounterOffset);
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.03f, encounterOffset - sideSign * pathWidth * 0.55f),
                    new Vector2(2.5f, 1.2f),
                    pathAngle + 90f,
                    BlockType.Movable,
                    ShapeFamily.Capsule));
                plans.Add(new BlockPlan(
                    onPath(encounterT + 0.06f, encounterOffset + sideSign * pathWidth * 0.57f),
                    new Vector2(2.35f, 1.15f),
                    pathAngle + 90f,
                    BlockType.Usual,
                    ShapeFamily.Blob));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.09f, encounterOffset - sideSign * pathWidth * 1.67f),
                    new Vector2(2.15f, 1.1f),
                    pathAngle + 90f,
                    BlockType.Usual,
                    ShapeFamily.FlatPlatform));
                break;
            }

            case EncounterTemplate.BreakForCaterpillar:
            {
                float encounterT = RandomRange(rng, 0.38f, 0.58f);
                leafPosition = onPath(encounterT, sideSign * pathWidth * RandomRange(rng, 0.68f, 1f));
                caterpillarPosition = onPath(
                    encounterT + RandomRange(rng, 0.14f, 0.24f),
                    -sideSign * pathWidth * RandomRange(rng, 0.48f, 0.82f));
                plans.Add(new BlockPlan(
                    onPath(encounterT + 0.07f, 0f),
                    new Vector2(Vector2.Distance(start, goal) * 0.28f, 1.15f),
                    pathAngle,
                    BlockType.Usual,
                    ShapeFamily.LongWall));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.04f, -sideSign * pathWidth * 1.05f),
                    new Vector2(2.2f, 1.15f),
                    pathAngle + 90f,
                    BlockType.Breakable,
                    ShapeFamily.Chunk));
                break;
            }

            case EncounterTemplate.MoveBushBlocksPath:
            {
                float encounterT = RandomRange(rng, 0.54f, 0.76f);
                float encounterOffset = pathWidth * RandomRange(rng, -0.24f, 0.24f);
                caterpillarPosition = onPath(encounterT, encounterOffset);
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.02f, encounterOffset + pathWidth * 0.7f),
                    new Vector2(5.2f, 1.05f),
                    pathAngle,
                    BlockType.Usual,
                    ShapeFamily.LongWall));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.02f, encounterOffset - pathWidth * 0.7f),
                    new Vector2(5.2f, 1.05f),
                    pathAngle,
                    BlockType.Usual,
                    ShapeFamily.LongWall));
                plans.Add(new BlockPlan(
                    onPath(encounterT - 0.25f, encounterOffset),
                    new Vector2(MinMovableWorldSize, MinMovableWorldSize),
                    pathAngle + 90f,
                    BlockType.Movable,
                    ShapeFamily.Bean));
                break;
            }
        }

        for (int i = 0; i < plans.Count; i++)
            plans[i] = EnsureBlockTypeSize(plans[i]);
        return plans;
    }

    private void AddFillerBlocks(
        List<BlockPlan> plans,
        List<SafeZone> safeZones,
        Vector2 start,
        Vector2 goal,
        Vector2 direction,
        Vector2 side,
        float pathAngle,
        System.Random rng)
    {
        int needed = Mathf.Max(0, obstacleCount - plans.Count);
        int bandCount = Mathf.Clamp(Mathf.CeilToInt((needed + 2) / 3f), 4, 7);
        var slots = new List<Vector2>(bandCount * 3);
        for (int row = 0; row < bandCount; row++)
        {
            float baseT = (row + 0.5f) / bandCount;
            for (int lane = -1; lane <= 1; lane++)
            {
                float t = Mathf.Clamp(
                    baseT + RandomRange(rng, -0.18f, 0.18f) / bandCount,
                    0.07f,
                    0.93f);
                float offset = lane * playableHalfWidth * 0.62f +
                               RandomRange(rng, -0.32f, 0.32f);
                slots.Add(new Vector2(t, offset));
            }
        }
        Shuffle(slots, rng);

        int attempts = 0;
        int slotIndex = 0;
        while (plans.Count < obstacleCount && attempts++ < 400)
        {
            float t;
            float offset;
            if (slotIndex < slots.Count)
            {
                Vector2 slot = slots[slotIndex++];
                t = slot.x;
                offset = slot.y;
            }
            else
            {
                t = RandomRange(rng, 0.08f, 0.92f);
                offset = RandomRange(rng, -playableHalfWidth * 0.82f, playableHalfWidth * 0.82f);
            }

            Vector2 size = PickFillerSize(rng);
            ShapeFamily shape = PickFillerShape(rng);

            if (shape == ShapeFamily.LongWall || shape == ShapeFamily.FlatPlatform)
                size.x = Mathf.Max(size.x, size.y * RandomRange(rng, 2.1f, 3f));

            Vector2 center = Vector2.Lerp(start, goal, t) + side * offset;
            float angle = shape == ShapeFamily.LongWall || shape == ShapeFamily.FlatPlatform
                ? pathAngle + 90f + RandomRange(rng, -28f, 28f)
                : RandomRange(rng, -180f, 180f);
            BlockType type = PickBlockType(rng);
            BlockPlan candidate = EnsureBlockTypeSize(new BlockPlan(center, size, angle, type, shape));
            if (!FitsPlayableField(center, candidate.size, angle, start, goal, direction, side) ||
                IntersectsSafeZone(candidate, safeZones) ||
                IntersectsPlan(candidate, plans))
                continue;

            plans.Add(candidate);
        }
    }

    private static BlockPlan EnsureBlockTypeSize(BlockPlan plan)
    {
        if (plan.type == BlockType.Movable)
        {
            plan.size.x = Mathf.Max(plan.size.x, MinMovableWorldSize);
            plan.size.y = Mathf.Max(plan.size.y, MinMovableWorldSize);
        }
        return plan;
    }

    private static void AddInteractionGates(
        List<BlockPlan> plans,
        List<Vector2> route,
        List<SafeZone> criticalSafeZones,
        System.Random rng)
    {
        if (route == null || route.Count < 6)
            return;

        int gateCount = rng.NextDouble() < 0.48 ? 1 : 2;
        BlockType firstType = rng.NextDouble() < 0.5 ? BlockType.Breakable : BlockType.Movable;
        var gateTypes = new BlockType[gateCount];
        gateTypes[0] = firstType;
        if (gateCount > 1)
        {
            gateTypes[1] = rng.NextDouble() < 0.72
                ? (firstType == BlockType.Breakable ? BlockType.Movable : BlockType.Breakable)
                : firstType;
        }
        float[] preferredFractions = gateCount == 1
            ? new[] { RandomRange(rng, 0.4f, 0.64f) }
            : new[] { 0.36f, 0.66f };

        for (int gate = 0; gate < gateTypes.Length; gate++)
        {
            int preferredIndex = Mathf.Clamp(
                Mathf.RoundToInt((route.Count - 1) * preferredFractions[gate]),
                2,
                route.Count - 3);
            for (int offset = 0; offset < route.Count; offset++)
            {
                int signedOffset = offset == 0 ? 0 : ((offset + 1) / 2) * (offset % 2 == 0 ? -1 : 1);
                int index = Mathf.Clamp(preferredIndex + signedOffset, 2, route.Count - 3);
                Vector2 tangent = (route[index + 1] - route[index - 1]).normalized;
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
                BlockType type = gateTypes[gate];
                Vector2 size = type == BlockType.Movable
                    ? new Vector2(MinMovableWorldSize, MinMovableWorldSize)
                    : new Vector2(RandomRange(rng, 1.9f, 2.35f), RandomRange(rng, 0.72f, 0.95f));
                ShapeFamily shape = type == BlockType.Movable
                    ? ShapeFamily.Bean
                    : ShapeFamily.Capsule;
                BlockPlan candidate = EnsureBlockTypeSize(new BlockPlan(
                    route[index],
                    size,
                    angle,
                    type,
                    shape));

                if (IntersectsSafeZone(candidate, criticalSafeZones) || IntersectsPlan(candidate, plans))
                    continue;

                plans.Add(candidate);
                if (type == BlockType.Movable)
                {
                    Vector2 normal = new Vector2(-tangent.y, tangent.x);
                    float sideSign = rng.NextDouble() < 0.5 ? -1f : 1f;
                    // Leave one compact pocket beside the gate so the player has somewhere
                    // meaningful to drag the block instead of merely shifting it inside the path.
                    criticalSafeZones.Add(new SafeZone(
                        route[index] + normal * sideSign * 2.15f,
                        1.15f));
                }
                break;
            }
        }
    }

    private static bool TryBuildGrowthRoute(
        List<BlockPlan> plans,
        Vector2 flowerPosition,
        Vector2 sunPosition,
        out List<Vector2> route)
    {
        route = null;
        float verticalSign = sunPosition.y >= flowerPosition.y ? 1f : -1f;
        float totalRise = Mathf.Abs(sunPosition.y - flowerPosition.y);
        var frontier = new List<GrowthRouteState>
        {
            new GrowthRouteState
            {
                position = flowerPosition,
                lastDirection = Vector2.zero,
                points = new List<Vector2> { flowerPosition },
                priorityCost = 0,
                score = 0f
            }
        };

        int maxSteps = Mathf.Clamp(Mathf.CeilToInt(totalRise / 0.55f) + 3, 8, 34);
        for (int step = 0; step < maxSteps; step++)
        {
            var nextFrontier = new List<GrowthRouteState>();
            var occupiedCells = new HashSet<Vector2Int>();
            foreach (GrowthRouteState state in frontier)
            {
                float progress = (state.position.y - flowerPosition.y) * verticalSign;
                if (progress >= totalRise - 1.15f && Mathf.Abs(state.position.x - sunPosition.x) <= 1.6f)
                {
                    route = state.points;
                    return true;
                }

                // The first seven checks are upward or upward-sideways. The final two checks
                // are almost horizontal/downward and are deliberately excluded from generated
                // critical routes, because they easily produce unrecoverable pockets.
                for (int priority = 0; priority < 7; priority++)
                {
                    Vector2 offset = GrowthTriggerOffsets[priority];
                    offset.y *= verticalSign;
                    Vector2 candidate = state.position + offset;
                    Vector2 newDirection = offset.normalized;
                    if (state.lastDirection != Vector2.zero &&
                        Vector2.Angle(state.lastDirection, newDirection) > 120f)
                        continue;
                    if (!GrowthSegmentIsClear(state.position, candidate, plans))
                        continue;

                    float candidateProgress = (candidate.y - flowerPosition.y) * verticalSign;
                    float lineT = Mathf.Clamp01(candidateProgress / Mathf.Max(0.001f, totalRise));
                    float desiredX = Mathf.Lerp(flowerPosition.x, sunPosition.x, lineT);
                    float remaining = Mathf.Max(0f, totalRise - candidateProgress);
                    int priorityCost = state.priorityCost + priority;
                    float score = Mathf.Abs(candidate.x - desiredX) * 1.25f +
                                  remaining * 0.035f + priorityCost * 0.12f;
                    var cell = new Vector2Int(
                        Mathf.RoundToInt(candidate.x * 2f),
                        Mathf.RoundToInt(candidate.y * 2f));
                    if (!occupiedCells.Add(cell))
                        continue;

                    var points = new List<Vector2>(state.points) { candidate };
                    nextFrontier.Add(new GrowthRouteState
                    {
                        position = candidate,
                        lastDirection = newDirection,
                        points = points,
                        priorityCost = priorityCost,
                        score = score
                    });
                }
            }

            if (nextFrontier.Count == 0)
                break;
            nextFrontier.Sort((a, b) => a.score.CompareTo(b.score));
            if (nextFrontier.Count > 36)
                nextFrontier.RemoveRange(36, nextFrontier.Count - 36);
            frontier = nextFrontier;
        }

        return false;
    }

    private static bool GrowthSegmentIsClear(Vector2 from, Vector2 to, List<BlockPlan> plans)
    {
        float distance = Vector2.Distance(from, to);
        int samples = Mathf.Max(2, Mathf.CeilToInt(distance / 0.28f));
        for (int sample = 1; sample <= samples; sample++)
        {
            Vector2 point = Vector2.Lerp(from, to, sample / (float)samples);
            foreach (BlockPlan plan in plans)
            {
                // Breakable and movable blocks are valid conditional passages: the player can
                // open them. Only an ordinary block is an unconditional wall for solvability.
                if (plan.type != BlockType.Usual)
                    continue;
                if (GrowthHeadOverlapsPlan(point, plan))
                    return false;
            }
        }
        return true;
    }

    private static bool GrowthHeadOverlapsPlan(Vector2 point, BlockPlan plan)
    {
        float radians = plan.angle * Mathf.Deg2Rad;
        Vector2 localX = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 localY = new Vector2(-localX.y, localX.x);
        Vector2 delta = point - plan.center;
        float localPointX = Mathf.Abs(Vector2.Dot(delta, localX));
        float localPointY = Mathf.Abs(Vector2.Dot(delta, localY));
        float headOnX = Mathf.Abs(localX.x) * GrowthHeadHalfSize.x +
                        Mathf.Abs(localX.y) * GrowthHeadHalfSize.y;
        float headOnY = Mathf.Abs(localY.x) * GrowthHeadHalfSize.x +
                        Mathf.Abs(localY.y) * GrowthHeadHalfSize.y;
        const float safety = 0.12f;
        return localPointX < plan.size.x * 0.5f + headOnX + safety &&
               localPointY < plan.size.y * 0.5f + headOnY + safety;
    }

    private static List<Vector2> BuildFallbackGrowthRoute(Vector2 flowerPosition, Vector2 sunPosition)
    {
        float distance = Vector2.Distance(flowerPosition, sunPosition);
        int samples = Mathf.Max(2, Mathf.CeilToInt(distance / 0.65f));
        var route = new List<Vector2>(samples + 1);
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            float easedT = t * t * (3f - 2f * t);
            route.Add(new Vector2(
                Mathf.Lerp(flowerPosition.x, sunPosition.x, easedT),
                Mathf.Lerp(flowerPosition.y, sunPosition.y, t)));
        }
        return route;
    }

    private static void RemovePlansBlockingRoute(List<BlockPlan> plans, List<Vector2> route)
    {
        plans.RemoveAll(plan =>
        {
            if (plan.type != BlockType.Usual)
                return false;
            foreach (Vector2 point in route)
            {
                if (PlanIntersectsCircle(plan, point, GrowthCorridorRadius))
                    return true;
            }
            return false;
        });
    }

    private static void AddGrowthRouteSafeZones(List<SafeZone> safeZones, List<Vector2> route)
    {
        if (route == null || route.Count == 0)
            return;

        safeZones.Add(new SafeZone(route[0], GrowthCorridorRadius));
        for (int i = 1; i < route.Count; i++)
        {
            float distance = Vector2.Distance(route[i - 1], route[i]);
            int samples = Mathf.Max(1, Mathf.CeilToInt(distance / 0.55f));
            for (int sample = 1; sample <= samples; sample++)
            {
                Vector2 point = Vector2.Lerp(route[i - 1], route[i], sample / (float)samples);
                safeZones.Add(new SafeZone(point, GrowthCorridorRadius));
            }
        }
    }

    private Vector2 PickFillerSize(System.Random rng)
    {
        double roll = rng.NextDouble();
        if (roll < 0.16)
        {
            return new Vector2(
                RandomRange(rng, minObstacleSize, Mathf.Min(2.05f, maxObstacleSize)),
                RandomRange(rng, Mathf.Max(1.05f, minObstacleSize * 0.85f), 1.65f));
        }

        if (roll > 0.86)
        {
            return new Vector2(
                RandomRange(rng, Mathf.Max(3f, maxObstacleSize * 0.82f), maxObstacleSize * 1.3f),
                RandomRange(rng, 1.75f, Mathf.Max(2.1f, maxObstacleSize * 0.78f)));
        }

        return new Vector2(
            RandomRange(rng, Mathf.Max(1.8f, minObstacleSize), maxObstacleSize),
            RandomRange(rng, Mathf.Max(1.3f, minObstacleSize), Mathf.Max(1.7f, maxObstacleSize * 0.72f)));
    }

    private static ShapeFamily PickFillerShape(System.Random rng)
    {
        int roll = rng.Next(100);
        if (roll < 27) return ShapeFamily.Blob;
        if (roll < 48) return ShapeFamily.Bean;
        if (roll < 66) return ShapeFamily.Chunk;
        if (roll < 76) return ShapeFamily.Capsule;
        if (roll < 84) return ShapeFamily.LongWall;
        if (roll < 90) return ShapeFamily.FlatPlatform;
        if (roll < 94) return ShapeFamily.Crescent;
        if (roll < 97) return ShapeFamily.Petal;
        return ShapeFamily.LShape;
    }

    private bool FitsPlayableField(
        Vector2 center,
        Vector2 size,
        float angle,
        Vector2 start,
        Vector2 goal,
        Vector2 direction,
        Vector2 side)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector2 localX = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 localY = new Vector2(-localX.y, localX.x);
        float lateralExtent = Mathf.Abs(Vector2.Dot(localX, side)) * size.x * 0.5f +
                              Mathf.Abs(Vector2.Dot(localY, side)) * size.y * 0.5f;
        float alongExtent = Mathf.Abs(Vector2.Dot(localX, direction)) * size.x * 0.5f +
                            Mathf.Abs(Vector2.Dot(localY, direction)) * size.y * 0.5f;
        float lateral = Mathf.Abs(Vector2.Dot(center - start, side));
        float along = Vector2.Dot(center - start, direction);
        float length = Vector2.Distance(start, goal);
        return lateral + lateralExtent <= playableHalfWidth &&
               along - alongExtent * 0.55f >= 0f &&
               along + alongExtent * 0.55f <= length;
    }

    private static void Shuffle<T>(List<T> items, System.Random rng)
    {
        for (int i = items.Count - 1; i > 0; i--)
        {
            int swapIndex = rng.Next(i + 1);
            T value = items[i];
            items[i] = items[swapIndex];
            items[swapIndex] = value;
        }
    }

    private static bool IntersectsSafeZone(BlockPlan candidate, List<SafeZone> safeZones)
    {
        foreach (SafeZone zone in safeZones)
        {
            if (PlanIntersectsCircle(candidate, zone.center, zone.radius))
                return true;
        }
        return false;
    }

    private static bool PlanIntersectsCircle(BlockPlan plan, Vector2 center, float radius)
    {
        float radians = plan.angle * Mathf.Deg2Rad;
        Vector2 localX = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 localY = new Vector2(-localX.y, localX.x);
        Vector2 delta = center - plan.center;
        Vector2 localPoint = new Vector2(
            Vector2.Dot(delta, localX),
            Vector2.Dot(delta, localY));
        Vector2 half = plan.size * 0.5f;
        Vector2 closest = new Vector2(
            Mathf.Clamp(localPoint.x, -half.x, half.x),
            Mathf.Clamp(localPoint.y, -half.y, half.y));
        return (localPoint - closest).sqrMagnitude < radius * radius;
    }

    private static bool IntersectsPlan(BlockPlan candidate, List<BlockPlan> plans)
    {
        foreach (BlockPlan plan in plans)
        {
            if (OrientedBoundsOverlap(candidate, plan, 0.08f))
                return true;
        }
        return false;
    }

    private static bool OrientedBoundsOverlap(BlockPlan a, BlockPlan b, float padding)
    {
        float aRadians = a.angle * Mathf.Deg2Rad;
        float bRadians = b.angle * Mathf.Deg2Rad;
        Vector2 aX = new Vector2(Mathf.Cos(aRadians), Mathf.Sin(aRadians));
        Vector2 aY = new Vector2(-aX.y, aX.x);
        Vector2 bX = new Vector2(Mathf.Cos(bRadians), Mathf.Sin(bRadians));
        Vector2 bY = new Vector2(-bX.y, bX.x);
        Vector2 delta = b.center - a.center;
        Vector2 aHalf = a.size * 0.5f + Vector2.one * padding;
        Vector2 bHalf = b.size * 0.5f + Vector2.one * padding;

        return OverlapsOnAxis(delta, aX, aX, aY, aHalf, bX, bY, bHalf) &&
               OverlapsOnAxis(delta, aY, aX, aY, aHalf, bX, bY, bHalf) &&
               OverlapsOnAxis(delta, bX, aX, aY, aHalf, bX, bY, bHalf) &&
               OverlapsOnAxis(delta, bY, aX, aY, aHalf, bX, bY, bHalf);
    }

    private static bool OverlapsOnAxis(
        Vector2 delta,
        Vector2 axis,
        Vector2 aX,
        Vector2 aY,
        Vector2 aHalf,
        Vector2 bX,
        Vector2 bY,
        Vector2 bHalf)
    {
        float distance = Mathf.Abs(Vector2.Dot(delta, axis));
        float aRadius = Mathf.Abs(Vector2.Dot(aX, axis)) * aHalf.x +
                        Mathf.Abs(Vector2.Dot(aY, axis)) * aHalf.y;
        float bRadius = Mathf.Abs(Vector2.Dot(bX, axis)) * bHalf.x +
                        Mathf.Abs(Vector2.Dot(bY, axis)) * bHalf.y;
        return distance < aRadius + bRadius;
    }

    private BlockType PickBlockType(System.Random rng)
    {
        float total = usualWeight + breakableWeight + movableWeight;
        if (total <= 0.0001f)
            return BlockType.Usual;

        float value = RandomRange(rng, 0f, total);
        if (value < usualWeight)
            return BlockType.Usual;
        if (value < usualWeight + breakableWeight)
            return BlockType.Breakable;
        return BlockType.Movable;
    }

    private void PlaceScenarioContent(
        EncounterTemplate scenario,
        List<BlockPlan> plans,
        List<GameObject> blocks,
        Vector2 preferredLeafPosition,
        Vector2 preferredCaterpillarPosition,
        Transform root,
        System.Random rng)
    {
        bool leafNeedsMovableHost = scenario == EncounterTemplate.MoveBushToCaterpillar ||
                                    scenario == EncounterTemplate.MoveBushBlocksPath;
        int leafHost = FindHostIndex(
            plans,
            blocks,
            preferredLeafPosition,
            leafNeedsMovableHost ? (BlockType?)BlockType.Movable : null,
            null,
            rng);
        if (leafHost < 0)
            leafHost = FindHostIndex(plans, blocks, preferredLeafPosition, null, null, rng);

        if (leafHost >= 0)
        {
            PlacePrefabOnBlock(
                leafPrefab,
                blocks[leafHost],
                root,
                "Bush / leaves (scenario)",
                null,
                rng);
        }
        else
        {
            CreatePrefabInstance(
                leafPrefab,
                preferredLeafPosition,
                Quaternion.identity,
                root,
                "Bush / leaves (scenario)");
        }

        int caterpillarCount = scenario == EncounterTemplate.TwoPathsTwoCaterpillars ? 2 : 1;
        var usedHosts = new HashSet<int>();
        if (leafHost >= 0)
            usedHosts.Add(leafHost);

        for (int i = 0; i < caterpillarCount; i++)
        {
            Vector2 preferred = preferredCaterpillarPosition;
            if (caterpillarCount == 2)
            {
                float sideSign = i == 0 ? -1f : 1f;
                preferred.x += sideSign * pathWidth * 0.82f;
                preferred.y += (i == 0 ? -0.035f : 0.035f) *
                               Mathf.Abs(fallbackSunPosition.y - fallbackFlowerPosition.y);
            }

            int hostIndex = FindHostIndex(
                plans,
                blocks,
                preferred,
                BlockType.Usual,
                usedHosts,
                rng);
            if (hostIndex < 0)
                hostIndex = FindHostIndex(plans, blocks, preferred, null, usedHosts, rng);
            if (hostIndex < 0)
                continue;

            usedHosts.Add(hostIndex);
            GameObject caterpillar = PlacePrefabOnBlock(
                caterpillarPrefab,
                blocks[hostIndex],
                root,
                caterpillarCount == 1
                    ? "Caterpillar (scenario)"
                    : "Caterpillar path " + (i + 1),
                AuthoredCaterpillarScale,
                rng);
            ConfigureCaterpillarMovement(caterpillar, blocks[hostIndex], rng);
        }
    }

    private static void ConfigureCaterpillarMovement(
        GameObject caterpillar,
        GameObject host,
        System.Random rng)
    {
        if (caterpillar == null || host == null || !TryGetWorldBounds(host, out Bounds hostBounds))
            return;

        Rigidbody2D body = caterpillar.GetComponent<Rigidbody2D>();
        if (body == null)
            body = Undo.AddComponent<Rigidbody2D>(caterpillar);
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        if (rng.NextDouble() < 0.36)
        {
            var pivot = new GameObject("Caterpillar circular patrol pivot");
            Undo.RegisterCreatedObjectUndo(pivot, "Create caterpillar patrol pivot");
            pivot.transform.position = caterpillar.transform.position;
            Undo.SetTransformParent(pivot.transform, host.transform, "Attach patrol pivot");
            Undo.SetTransformParent(caterpillar.transform, pivot.transform, "Attach caterpillar to patrol pivot");

            CirclePatrol2D patrol = Undo.AddComponent<CirclePatrol2D>(caterpillar);
            var serializedPatrol = new SerializedObject(patrol);
            serializedPatrol.FindProperty("radius").floatValue = RandomRange(rng, 0.24f, 0.44f);
            serializedPatrol.FindProperty("angularSpeed").floatValue = RandomRange(rng, 38f, 68f);
            serializedPatrol.ApplyModifiedPropertiesWithoutUndo();
            return;
        }

        float halfSpan = Mathf.Clamp(hostBounds.size.x * 0.24f, 0.28f, 0.82f);
        Transform pointA = CreatePatrolPoint(
            host.transform,
            caterpillar.transform.position + Vector3.left * halfSpan,
            "Caterpillar patrol A");
        Transform pointB = CreatePatrolPoint(
            host.transform,
            caterpillar.transform.position + Vector3.right * halfSpan,
            "Caterpillar patrol B");
        PatrolBetweenPoints2D linePatrol = Undo.AddComponent<PatrolBetweenPoints2D>(caterpillar);
        var serializedLinePatrol = new SerializedObject(linePatrol);
        serializedLinePatrol.FindProperty("pointA").objectReferenceValue = pointA;
        serializedLinePatrol.FindProperty("pointB").objectReferenceValue = pointB;
        serializedLinePatrol.FindProperty("speed").floatValue = RandomRange(rng, 0.5f, 0.95f);
        serializedLinePatrol.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform CreatePatrolPoint(Transform parent, Vector3 worldPosition, string name)
    {
        var point = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(point, "Create caterpillar patrol point");
        point.transform.position = worldPosition;
        Undo.SetTransformParent(point.transform, parent, "Attach caterpillar patrol point");
        return point.transform;
    }

    private static int FindHostIndex(
        List<BlockPlan> plans,
        List<GameObject> blocks,
        Vector2 preferredPosition,
        BlockType? requiredType,
        HashSet<int> excluded,
        System.Random rng)
    {
        int bestIndex = -1;
        float bestScore = float.PositiveInfinity;
        for (int i = 0; i < plans.Count && i < blocks.Count; i++)
        {
            if (blocks[i] == null || (excluded != null && excluded.Contains(i)))
                continue;
            if (requiredType.HasValue && plans[i].type != requiredType.Value)
                continue;
            if (plans[i].size.x < 1.55f || plans[i].size.y < 0.72f)
                continue;

            float score = Vector2.Distance(plans[i].center, preferredPosition) +
                          RandomRange(rng, 0f, 0.45f);
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private static GameObject PlacePrefabOnBlock(
        GameObject prefab,
        GameObject host,
        Transform root,
        string objectName,
        Vector3? worldScale,
        System.Random rng)
    {
        if (prefab == null || host == null || !TryGetWorldBounds(host, out Bounds hostBounds))
            return null;

        float usableHalfWidth = Mathf.Max(0f, hostBounds.extents.x * 0.48f);
        float xOffset = RandomRange(rng, -usableHalfWidth, usableHalfWidth);
        Vector2 initialPosition = new Vector2(
            hostBounds.center.x + xOffset,
            hostBounds.max.y + 1f);
        GameObject instance = CreatePrefabInstance(
            prefab,
            initialPosition,
            Quaternion.identity,
            root,
            objectName);
        if (instance == null)
            return null;

        if (worldScale.HasValue)
        {
            Undo.RecordObject(instance.transform, "Apply authored actor scale");
            instance.transform.localScale = worldScale.Value;
        }

        if (TryGetVisualBounds(instance, out Bounds actorBounds))
        {
            Vector3 position = instance.transform.position;
            position.y += hostBounds.max.y + 0.035f - actorBounds.min.y;
            instance.transform.position = position;
        }

        Undo.SetTransformParent(instance.transform, host.transform, "Attach scenario content to block");
        return instance;
    }

    private static bool TryGetWorldBounds(GameObject target, out Bounds bounds)
    {
        Collider2D[] colliders = target.GetComponentsInChildren<Collider2D>(true);
        bool found = false;
        bounds = default;
        foreach (Collider2D collider in colliders)
        {
            if (!collider.enabled || collider.isTrigger)
                continue;
            if (!found)
            {
                bounds = collider.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }
        return found || TryGetVisualBounds(target, out bounds);
    }

    private static bool TryGetVisualBounds(GameObject target, out Bounds bounds)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        bounds = default;
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
                continue;
            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        return found;
    }

    private GameObject CreateBlock(BlockPlan plan, Transform parent, System.Random rng, int index)
    {
        ShapeFamily shape = plan.type == BlockType.Movable
            ? PickMovingShape(plan.shape, rng)
            : plan.shape;
        GameObject prefab = plan.type == BlockType.Breakable
            ? breakablePrefab
            : plan.type == BlockType.Movable ? movablePrefab : usualPrefab;

        GameObject instance = CreatePrefabInstance(
            prefab,
            plan.center,
            Quaternion.Euler(0f, 0f, plan.angle),
            parent,
            "Block " + (index + 1) + " - " + plan.type + " - " + shape);
        if (instance == null)
            return null;

        SpriteShapeController controller = instance.GetComponentInChildren<SpriteShapeController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[Forest Generator] SpriteShapeController not found in " + instance.name, instance);
            return instance;
        }

        if (controller.transform != instance.transform)
        {
            // moving_splineplatform wraps the actual SpriteShape in a child with an authored
            // prefab offset. Remove only that offset, but preserve the authored 0.4 spline scale:
            // the Move shader's hole radius is defined in those original local coordinates.
            controller.transform.localPosition = Vector3.zero;
            controller.transform.localRotation = Quaternion.identity;
        }

        Vector3 scale = controller.transform.lossyScale;
        Vector2 localSize = new Vector2(
            plan.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)),
            plan.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)));
        if (plan.type == BlockType.Movable)
        {
            // Move.mat cuts a fixed-radius (2.06 local units) circle. Authored movable splines
            // are large in local space and then displayed at scale 0.4. Keep enough material
            // around that mask so the hole cannot consume almost the whole generated block.
            localSize.x = Mathf.Max(localSize.x, 7.2f);
            localSize.y = Mathf.Max(localSize.y, 7.2f);
        }
        Vector3[] points = GenerateShape(shape, localSize, splineChaos, rng);
        FitPointsToSize(points, localSize);
        ApplySpline(controller, points);
        ApplyAuthoredMaterial(controller, plan.type, rng);
        return instance;
    }

    private static ShapeFamily PickMovingShape(ShapeFamily requested, System.Random rng)
    {
        // The fixed circular mask of Move.mat needs a shape that surrounds the object origin.
        if (requested == ShapeFamily.Blob || requested == ShapeFamily.Capsule ||
            requested == ShapeFamily.Chunk || requested == ShapeFamily.Bean)
            return requested;

        ShapeFamily[] compatible =
        {
            ShapeFamily.Blob,
            ShapeFamily.Capsule,
            ShapeFamily.Chunk,
            ShapeFamily.Bean
        };
        return compatible[rng.Next(compatible.Length)];
    }

    private void ApplyAuthoredMaterial(SpriteShapeController controller, BlockType type, System.Random rng)
    {
        SpriteShapeRenderer renderer = controller.GetComponent<SpriteShapeRenderer>();
        if (renderer == null || type == BlockType.Movable)
            return;

        Material selected = type == BlockType.Breakable
            ? breakableMaterials[rng.Next(breakableMaterials.Length)]
            : usualMaterials[rng.Next(usualMaterials.Length)];
        Undo.RecordObject(renderer, "Apply generated block material");

        if (type == BlockType.Usual)
        {
            // This matches the overrides used by the authored scenes.
            renderer.sharedMaterials = new[] { selected, selected };
        }
        else
        {
            Material[] materials = renderer.sharedMaterials;
            if (materials == null || materials.Length == 0)
                materials = new Material[1];
            materials[0] = selected;
            renderer.sharedMaterials = materials;
        }

        renderer.color = Color.white;
        EditorUtility.SetDirty(renderer);
    }

    private static GameObject CreatePrefabInstance(
        GameObject prefab,
        Vector2 position,
        Quaternion rotation,
        Transform parent,
        string objectName)
    {
        if (prefab == null)
            return null;

        var instance = PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene()) as GameObject;
        if (instance == null)
            return null;

        Undo.RegisterCreatedObjectUndo(instance, "Create " + objectName);
        instance.name = objectName;
        instance.transform.SetParent(parent, true);
        instance.transform.SetPositionAndRotation(new Vector3(position.x, position.y, 0f), rotation);
        return instance;
    }

    private static Vector3[] GenerateShape(ShapeFamily family, Vector2 size, float chaos, System.Random rng)
    {
        switch (family)
        {
            case ShapeFamily.Capsule:
                return RoundedRectangle(size, 0.48f, chaos * 0.35f, rng);
            case ShapeFamily.LongWall:
                return RoundedRectangle(size, 0.28f, chaos * 0.5f, rng);
            case ShapeFamily.FlatPlatform:
                return Platform(size, chaos * 0.35f, rng);
            case ShapeFamily.Chunk:
                return RadialShape(5, size, Mathf.Max(chaos, 0.16f), rng);
            case ShapeFamily.Crescent:
                return TemplateShape(CrescentTemplate(), size, chaos * 0.45f, rng, true);
            case ShapeFamily.LShape:
                return TemplateShape(LShapeTemplate(), size, chaos * 0.25f, rng, true);
            case ShapeFamily.Bean:
                return BeanShape(size, chaos, rng);
            case ShapeFamily.Petal:
                return TemplateShape(PetalTemplate(), size, chaos * 0.5f, rng, true);
            default:
                return RadialShape(6, size, chaos * 0.8f, rng);
        }
    }

    private static Vector3[] RadialShape(int pointCount, Vector2 size, float chaos, System.Random rng)
    {
        var points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float angle = Mathf.PI * 2f * (1f - i / (float)pointCount);
            float noise = 1f + RandomRange(rng, -chaos, chaos);
            points[i] = new Vector3(
                Mathf.Cos(angle) * size.x * 0.5f * noise,
                Mathf.Sin(angle) * size.y * 0.5f * noise,
                0f);
        }
        return points;
    }

    private static Vector3[] RoundedRectangle(Vector2 size, float corner, float chaos, System.Random rng)
    {
        float x = size.x * 0.5f;
        float y = size.y * 0.5f;
        float insetX = Mathf.Min(x * corner, y * 0.8f);
        var points = new[]
        {
            new Vector3(-x + insetX, -y, 0f),
            new Vector3(-x, 0f, 0f),
            new Vector3(-x + insetX, y, 0f),
            new Vector3(x - insetX, y, 0f),
            new Vector3(x, 0f, 0f),
            new Vector3(x - insetX, -y, 0f)
        };
        Jitter(points, size, chaos, rng, true);
        return points;
    }

    private static Vector3[] Platform(Vector2 size, float chaos, System.Random rng)
    {
        float x = size.x * 0.5f;
        float y = size.y * 0.5f;
        var points = new[]
        {
            new Vector3(-x * 0.72f, -y, 0f),
            new Vector3(-x, y * 0.1f, 0f),
            new Vector3(-x * 0.55f, y, 0f),
            new Vector3(x * 0.55f, y, 0f),
            new Vector3(x, y * 0.1f, 0f),
            new Vector3(x * 0.72f, -y, 0f)
        };
        Jitter(points, size, chaos, rng, false);
        return points;
    }

    private static Vector3[] BeanShape(Vector2 size, float chaos, System.Random rng)
    {
        const int pointCount = 7;
        var points = new Vector3[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            float angle = Mathf.PI * 2f * (1f - i / (float)pointCount);
            float wave = 1f + Mathf.Sin(angle * 2f + 0.65f) * 0.18f;
            float noise = 1f + RandomRange(rng, -chaos, chaos) * 0.45f;
            float x = Mathf.Cos(angle) * size.x * 0.5f * wave * noise;
            float y = Mathf.Sin(angle) * size.y * 0.5f * noise;
            x += Mathf.Sin(angle) * size.x * 0.09f;
            points[i] = new Vector3(x, y, 0f);
        }
        return points;
    }

    private static Vector3[] TemplateShape(
        Vector2[] template,
        Vector2 size,
        float chaos,
        System.Random rng,
        bool bothAxes)
    {
        var points = new Vector3[template.Length];
        for (int i = 0; i < template.Length; i++)
            points[i] = new Vector3(template[i].x * size.x, template[i].y * size.y, 0f);
        Jitter(points, size, chaos, rng, bothAxes);
        return points;
    }

    private static Vector2[] CrescentTemplate()
    {
        return new[]
        {
            new Vector2(-0.12f, -0.5f),
            new Vector2(-0.5f, -0.08f),
            new Vector2(-0.44f, 0.25f),
            new Vector2(0.18f, 0.5f),
            new Vector2(0.48f, 0.3f),
            new Vector2(-0.04f, 0.03f),
            new Vector2(0.16f, -0.3f),
            new Vector2(0.42f, -0.38f)
        };
    }

    private static Vector2[] LShapeTemplate()
    {
        return new[]
        {
            new Vector2(-0.34f, -0.5f),
            new Vector2(-0.5f, 0.34f),
            new Vector2(0.34f, 0.5f),
            new Vector2(0.5f, 0.18f),
            new Vector2(0.04f, 0.06f),
            new Vector2(-0.06f, -0.05f),
            new Vector2(-0.06f, -0.36f),
            new Vector2(-0.18f, -0.5f)
        };
    }

    private static Vector2[] PetalTemplate()
    {
        return new[]
        {
            new Vector2(-0.48f, -0.12f),
            new Vector2(-0.12f, 0.5f),
            new Vector2(0.23f, 0.43f),
            new Vector2(0.5f, 0.12f),
            new Vector2(0.08f, -0.5f),
            new Vector2(-0.25f, -0.4f)
        };
    }

    private static void Jitter(Vector3[] points, Vector2 size, float chaos, System.Random rng, bool bothAxes)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float dx = RandomRange(rng, -chaos, chaos) * size.x * 0.12f;
            float dy = RandomRange(rng, -chaos, chaos) * size.y * 0.12f;
            if (!bothAxes && i >= 1 && i <= 4)
                dy = 0f; // Keep the playable top of a platform flat.
            points[i] += new Vector3(dx, dy, 0f);
        }
    }

    private static void FitPointsToSize(Vector3[] points, Vector2 targetSize)
    {
        if (points == null || points.Length == 0)
            return;

        Vector3 min = points[0];
        Vector3 max = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            min = Vector3.Min(min, points[i]);
            max = Vector3.Max(max, points[i]);
        }

        Vector3 center = (min + max) * 0.5f;
        float width = Mathf.Max(0.001f, max.x - min.x);
        float height = Mathf.Max(0.001f, max.y - min.y);
        float scaleX = targetSize.x / width;
        float scaleY = targetSize.y / height;
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 centered = points[i] - center;
            points[i] = new Vector3(centered.x * scaleX, centered.y * scaleY, 0f);
        }
    }

    private static void ApplySpline(SpriteShapeController controller, Vector3[] points)
    {
        Undo.RecordObject(controller, "Generate spline shape");
        Spline spline = controller.spline;
        spline.Clear();

        for (int i = 0; i < points.Length; i++)
            spline.InsertPointAt(i, points[i]);

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 previous = points[(i - 1 + points.Length) % points.Length];
            Vector3 next = points[(i + 1) % points.Length];
            float handleLength = Mathf.Min(
                Vector3.Distance(points[i], previous),
                Vector3.Distance(points[i], next)) * 0.4f;
            Vector3 tangent = (next - previous).normalized * handleLength;
            spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            spline.SetLeftTangent(i, -tangent);
            spline.SetRightTangent(i, tangent);
        }

        controller.RefreshSpriteShape();
        controller.BakeCollider();
        EditorUtility.SetDirty(controller);
        if (controller.TryGetComponent(out PolygonCollider2D polygonCollider))
            EditorUtility.SetDirty(polygonCollider);
    }

    private void RegenerateAllShapes()
    {
        if (!CanEditGeneratedShapes())
            return;

        GameObject root = FindGeneratedRoot();
        if (root == null)
        {
            EditorUtility.DisplayDialog("Forest Level Generator", "Сгенерированный уровень не найден.", "OK");
            return;
        }

        SpriteShapeController[] controllers = root.GetComponentsInChildren<SpriteShapeController>(true);
        var rng = new System.Random(seed ^ 0x51F15EED);
        for (int i = 0; i < controllers.Length; i++)
            RegenerateController(controllers[i], rng);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void RegenerateSelectedShapes()
    {
        if (!CanEditGeneratedShapes())
            return;

        var controllers = new HashSet<SpriteShapeController>();
        foreach (GameObject selected in Selection.gameObjects)
        {
            SpriteShapeController controller = selected.GetComponentInChildren<SpriteShapeController>(true);
            if (controller != null)
                controllers.Add(controller);
        }

        if (controllers.Count == 0)
        {
            EditorUtility.DisplayDialog("Forest Level Generator", "Выберите один или несколько spline-блоков.", "OK");
            return;
        }

        var rng = new System.Random(seed ^ Selection.activeInstanceID);
        foreach (SpriteShapeController controller in controllers)
            RegenerateController(controller, rng);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private void RegenerateController(SpriteShapeController controller, System.Random rng)
    {
        Vector2 size = GetSplineSize(controller.spline);
        ShapeFamily family = (ShapeFamily)rng.Next(0, Enum.GetValues(typeof(ShapeFamily)).Length);
        if (controller.GetComponentInParent<DragBlock2D>() != null)
            family = PickMovingShape(family, rng);

        // Preserve the current local bounding box exactly. In the previous implementation a
        // LongWall could enlarge the current size; using that enlarged result on the next click
        // made repeated regeneration drift toward absurdly long blocks.
        Vector3[] points = GenerateShape(family, size, splineChaos, rng);
        FitPointsToSize(points, size);
        ApplySpline(controller, points);
    }

    private static bool CanEditGeneratedShapes()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
            return true;

        EditorUtility.DisplayDialog(
            "Forest Level Generator",
            "Остановите Play Mode перед изменением spline-форм.",
            "OK");
        return false;
    }

    private static Vector2 GetSplineSize(Spline spline)
    {
        if (spline.GetPointCount() == 0)
            return Vector2.one * 2f;

        Vector3 min = spline.GetPosition(0);
        Vector3 max = min;
        for (int i = 1; i < spline.GetPointCount(); i++)
        {
            Vector3 point = spline.GetPosition(i);
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }
        return new Vector2(Mathf.Max(0.8f, max.x - min.x), Mathf.Max(0.8f, max.y - min.y));
    }

    private void BakeGeneratedLevel()
    {
        GameObject root = FindGeneratedRoot();
        if (root == null)
        {
            EditorUtility.DisplayDialog("Forest Level Generator", "Сгенерированный уровень не найден.", "OK");
            return;
        }

        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Bake generated forest level");
        var directChildren = new List<GameObject>();
        foreach (Transform child in root.transform)
            directChildren.Add(child.gameObject);

        foreach (GameObject child in directChildren)
        {
            if (PrefabUtility.IsOutermostPrefabInstanceRoot(child))
            {
                PrefabUtility.UnpackPrefabInstance(
                    child,
                    PrefabUnpackMode.Completely,
                    InteractionMode.UserAction);
            }
        }

        Undo.RecordObject(root, "Bake generated forest level");
        root.name = "Forest Level - baked seed " + seed;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = root;
    }

    private void ClearGeneratedLevel()
    {
        if (!ClearGeneratedLevelInternal())
            EditorUtility.DisplayDialog("Forest Level Generator", "Сгенерированный уровень не найден.", "OK");
    }

    private static bool ClearGeneratedLevelInternal()
    {
        GameObject root = FindGeneratedRoot();
        if (root == null)
            return false;
        Undo.DestroyObjectImmediate(root);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        return true;
    }

    private static GameObject FindGeneratedRoot()
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name.StartsWith(GeneratedRootName, StringComparison.Ordinal))
                return root;
        }
        return null;
    }

    private static float RandomRange(System.Random rng, float min, float max)
    {
        if (max < min)
        {
            float value = min;
            min = max;
            max = value;
        }
        return min + (float)rng.NextDouble() * (max - min);
    }
}
