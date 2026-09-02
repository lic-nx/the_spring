using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ForestGameplayPrefabFactory
{
    public const string LeavesPrefabPath = "Assets/prefabs/generated/LeavesGameplay.prefab";
    public const string ButterflyPrefabPath = "Assets/prefabs/generated/ButterflyGameplay.prefab";

    private const string GeneratedFolder = "Assets/prefabs/generated";
    private const string ButterflyTexturePath = "Assets/prefabs/sprite/Butterfly_Anim.png";
    private const string LeafOnePath = "Assets/prefabs/sprite/Leaf_1.png";
    private const string LeafTwoPath = "Assets/prefabs/sprite/Leaf_2.png";

    static ForestGameplayPrefabFactory()
    {
        EditorApplication.delayCall += EnsureGameplayPrefabs;
    }

    [MenuItem("Tools/Forest/Rebuild Gameplay Prefabs")]
    public static void RebuildGameplayPrefabs()
    {
        EnsureFolder();
        AssetDatabase.DeleteAsset(LeavesPrefabPath);
        AssetDatabase.DeleteAsset(ButterflyPrefabPath);
        BuildPrefabs();
    }

    private static void EnsureGameplayPrefabs()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureGameplayPrefabs;
            return;
        }

        EnsureFolder();
        if (AssetDatabase.LoadAssetAtPath<GameObject>(LeavesPrefabPath) == null ||
            AssetDatabase.LoadAssetAtPath<GameObject>(ButterflyPrefabPath) == null)
            BuildPrefabs();
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            AssetDatabase.CreateFolder("Assets/prefabs", "generated");
    }

    private static void BuildPrefabs()
    {
        GameObject butterflyPrefab = BuildButterflyPrefab();
        BuildLeavesPrefab(butterflyPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static GameObject BuildButterflyPrefab()
    {
        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(ButterflyTexturePath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();

        var root = new GameObject("ButterflyGameplay");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localPosition = new Vector3(-0.64f, -0.64f, 0f);
        visual.transform.localScale = Vector3.one * 0.25f;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = frames.FirstOrDefault();
        renderer.sortingOrder = 12;

        ButterflyFlyAway flyAway = root.AddComponent<ButterflyFlyAway>();
        flyAway.Configure(frames, renderer);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, ButterflyPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void BuildLeavesPrefab(GameObject butterflyPrefab)
    {
        var root = new GameObject("LeavesGameplay");
        Rigidbody2D body = root.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        var trigger = root.AddComponent<CircleCollider2D>();
        trigger.isTrigger = true;
        trigger.radius = 0.58f;
        LeafPowerup powerup = root.AddComponent<LeafPowerup>();
        powerup.Configure(butterflyPrefab);

        CreateLeafVisual(root.transform, "Leaf Left", LeafOnePath, new Vector2(-0.2f, 0f), 24f, false);
        CreateLeafVisual(root.transform, "Leaf Right", LeafTwoPath, new Vector2(0.2f, 0f), -24f, true);

        PrefabUtility.SaveAsPrefabAsset(root, LeavesPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateLeafVisual(
        Transform parent,
        string name,
        string spritePath,
        Vector2 localPosition,
        float angle,
        bool flipX)
    {
        var visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        visual.transform.localScale = Vector3.one * 0.72f;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        renderer.flipX = flipX;
        renderer.sortingOrder = 9;
    }
}
