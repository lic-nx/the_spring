using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Генератор уровней для игры про цветок, растущий к солнцу.
///
/// Логика:
///  - На сцене всегда есть Цветок (старт) и Солнце (цель).
///  - Триггеры-направления являются частью префаба цветка,
///    генератор их НЕ создаёт — он лишь расставляет блоки так,
///    чтобы путь от цветка к солнцу был проходимым.
///  - Цветок растёт в сторону триггера, если он свободен и угол
///    между прошлым направлением и новым не превышает maxTurnAngle (120°).
///  - Три вида блоков:
///      1) Solid   — статичные препятствия (не двигаются, не ломаются)
///      2) Breakable — ломаются после нескольких нажатий (открывают путь)
///      3) Movable  — игрок может их двигать
///
/// Генератор строит «гарантированно проходимый» путь от цветка к солнцу
/// по сетке, а затем заполняет остальную карту одиночными блоками,
/// сохраняя путь.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    public enum BlockType
    {
        Empty = 0,
        Solid = 1,
        Breakable = 2,
        Movable = 3
    }

    [Header("Шаблоны блоков (объекты со скриптами на них)")]
    [Tooltip("Объект со скриптом статичного блока — нельзя двигать и ломать")]
    public GameObject solidBlockTemplate;
    [Tooltip("Объект со скриптом ломаемого блока — разрушается после нажатий")]
    public GameObject breakableBlockTemplate;
    [Tooltip("Объект со скриптом перемещаемого блока — игрок может его двигать")]
    public GameObject movableBlockTemplate;
    [Tooltip("Цветок — стартовая точка роста")]
    public GameObject flowerPrefab;
    [Tooltip("Солнце — цель, к которой растёт цветок")]
    public GameObject sunPrefab;

    [Header("Размеры поля генерации")]
    [Tooltip("Ширина поля в клетках")]
    [Min(8)] public int width = 16;
    [Tooltip("Высота поля в клетках")]
    [Min(10)] public int height = 24;
    [Tooltip("Размер одной клетки в юнитах")]
    public float cellSize = 5f;
    [Tooltip("Если включено — размеры поля подгоняются под камеру автоматически")]
    public bool fitToCamera = true;
    [Tooltip("Камера, под которую подгоняется поле (пусто = Camera.main)")]
    public Camera targetCamera;
    [Tooltip("Максимальный угол поворота от прошлого направления к новому (градусы)")]
    public float maxTurnAngle = 120f;
   [Header("Интерактивные блоки на пути")]
    [Tooltip("Минимальное количество ломаемых блоков на пути")]
    [Min(0)] public int breakableOnPathMin = 1;
    [Tooltip("Максимальное количество ломаемых блоков на пути")]
    [Min(0)] public int breakableOnPathMax = 9;

    [Tooltip("Минимальное количество перемещаемых блоков на пути")]
    [Min(0)] public int movableOnPathMin = 0;
    [Tooltip("Максимальное количество перемещаемых блоков на пути")]
    [Min(0)] public int movableOnPathMax = 5;

    [Header("Плотность блоков вне пути")]
    [Range(0f, 1f)] public float solidDensity = 0.5f;
    [Range(0f, 1f)] public float breakableDensity = 0.15f;
    [Range(0f, 1f)] public float movableDensity = 0.1f;

    [Tooltip("Seed. Одинаковый seed = одинаковый уровень")]
    public int seed = 12345;

    [Header("Форма блока (сплайн внутри одной клетки)")]
    [Tooltip("Насколько форма может «съёживаться» внутрь (0 — ровный, 0.5 — сильно неровный)")]
    [Range(0f, 0.5f)] public float shapeRoughness = 0.25f;
    [Tooltip("Сколько контрольных точек у сплайна блока")]
    [Range(3, 12)] public int shapeControlPoints = 7;
    [Tooltip("Отступ формы от границ клетки — защита от пересечений")]
    [Range(0.02f, 0.4f)] public float shapeMargin = 0.15f;
    [Tooltip("Сглаживание сплайна: сегментов между контрольными точками")]
    [Range(1, 12)] public int shapeSplineSamples = 6;

    // --- Ссылки для других скриптов ---
    public GameObject FlowerInstance { get; private set; }
    public GameObject SunInstance { get; private set; }
    public Vector2Int FlowerCell { get; private set; }
    public Vector2Int SunCell { get; private set; }

    /// <summary>Карта клеток: [x, y] — тип блока.</summary>
    private BlockType[,] grid;

    private System.Random rng;

    private void Awake()
    {
        Generate();
    }

    /// <summary>
    /// Подгоняет width/height под видимую ортографическую область камеры
    /// и центрирует поле на камере.
    /// </summary>
    private void FitFieldToCamera()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null || !cam.orthographic)
        {
            Debug.LogWarning("LevelGenerator: камера не найдена или не ортографическая — используем размеры из инспектора.");
            return;
        }

        float worldWidth = cam.orthographicSize * 2f * cam.aspect;
        float worldHeight = cam.orthographicSize * 2f;

        width = Mathf.Max(8, Mathf.RoundToInt(worldWidth / cellSize));
        height = Mathf.Max(10, Mathf.RoundToInt(worldHeight / cellSize));

        transform.position = new Vector3(
            cam.transform.position.x,
            cam.transform.position.y,
            transform.position.z);
    }

    /// <summary>
    /// Публичная точка входа: генерировать новый уровень.
    /// </summary>
    public void Generate(int? newSeed = null)
    {
        if (newSeed.HasValue) seed = newSeed.Value;
        rng = new System.Random(seed);

        if (fitToCamera) FitFieldToCamera();

        ClearOldLevel();

        grid = new BlockType[width, height];
        BuildPath();
        FillObstacles();
        SpawnBlocks();
        SpawnFlowerAndSun();
    }

    private void OnValidate()
    {
        if (maxTurnAngle > 180f) maxTurnAngle = 180f;
    }

    private void ClearOldLevel()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        FlowerInstance = null;
        SunInstance = null;
    }

    // -------------------------------------------------------------------
    //  1. ПУТЬ
    // -------------------------------------------------------------------

    /// <summary>
    /// Строит путь от цветка (низ карты) к солнцу (верх карты),
    /// соблюдая правило: угол поворота между шагами <= maxTurnAngle.
    /// </summary>
    private void BuildPath()
    {
        FlowerCell = new Vector2Int(width / 2, 1);
        SunCell = new Vector2Int(rng.Next(width / 4, 3 * width / 4), height - 2);

        var path = new List<Vector2Int> { FlowerCell };
        var current = FlowerCell;
        var direction = Vector2Int.up;

        int safety = width * height * 4;
        while (current != SunCell && safety-- > 0)
        {
            var next = ChooseNextStep(current, direction, path);
            if (next == current) break;
            direction = next - current;
            current = next;
            path.Add(current);
        }

        foreach (var cell in path)
            grid[cell.x, cell.y] = BlockType.Empty;

        PathCells = path;
    }

    /// <summary>Путь целиком — другим скриптам.</summary>
    public List<Vector2Int> PathCells { get; private set; }

    /// <summary>
    /// Выбирает следующий шаг пути.
    /// </summary>
    private Vector2Int ChooseNextStep(Vector2Int from, Vector2Int lastDir, List<Vector2Int> path)
    {
        var candidates = new List<Vector2Int>
        {
            from + Vector2Int.up,
            from + Vector2Int.left,
            from + Vector2Int.right
        };

        Vector2 lastDirF = ((Vector2)lastDir).normalized;
        float bestScore = float.MinValue;
        Vector2Int best = from;

        foreach (var candidate in candidates)
        {
            if (candidate.x < 0 || candidate.x >= width || candidate.y < 0 || candidate.y >= height)
                continue;
            if (path.Contains(candidate))
                continue;
            if (candidate.y > SunCell.y)
                continue;

            Vector2 newDirF = ((Vector2)(candidate - from)).normalized;
            float angle = Vector2.Angle(lastDirF, newDirF);
            if (angle > maxTurnAngle)
                continue;

            float dist = Vector2Int.Distance(candidate, SunCell);
            float score = -dist + (float)rng.NextDouble() * 1.5f;

            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }
        return best;
    }

    // -------------------------------------------------------------------
    //  2. ПРЕПЯТСТВИЯ (одиночные блоки, по одной клетке)
    // -------------------------------------------------------------------

    /// <summary>
    /// Заполняет карту одиночными блоками: каждая клетка вне пути
    /// с некоторой вероятностью получает свой тип блока.
    /// </summary>
    private void FillObstacles()
    {
        // Случайное количество интерактивных блоков на пути в заданном диапазоне.
        int breakableCount = rng.Next(breakableOnPathMin, breakableOnPathMax + 1);
        int movableCount = rng.Next(movableOnPathMin, movableOnPathMax + 1);

        // Обязательные головоломки на пути: ломаемые и перемещаемые блоки.
        PlaceInteractiveOnPath(BlockType.Breakable, breakableCount);
        PlaceInteractiveOnPath(BlockType.Movable, movableCount);

        // Остальная карта — одиночные клетки-блоки.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new Vector2Int(x, y);
                if (grid[x, y] != BlockType.Empty) continue;
                if (IsOnPath(cell)) continue;
                if (cell == SunCell || cell == FlowerCell) continue;

                float r = (float)rng.NextDouble();
                BlockType type;
                if (r < solidDensity) type = BlockType.Solid;
                else if (r < solidDensity + breakableDensity) type = BlockType.Breakable;
                else if (r < solidDensity + breakableDensity + movableDensity) type = BlockType.Movable;
                else continue;

                grid[x, y] = type;
            }
        }
    }

    /// <summary>
    /// Ставит count блоков заданного типа на клетки пути.
    /// </summary>
    private void PlaceInteractiveOnPath(BlockType type, int count)
    {
        var candidates = new List<Vector2Int>(PathCells);
        candidates.Remove(FlowerCell);
        candidates.Remove(SunCell);

        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = rng.Next(candidates.Count);
            var cell = candidates[index];
            candidates.RemoveAt(index);

            if (HasInteractiveNeighbour(cell)) { i--; continue; }

            grid[cell.x, cell.y] = type;
        }
    }

    private bool HasInteractiveNeighbour(Vector2Int cell)
    {
        foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            var n = cell + dir;
            if (n.x < 0 || n.x >= width || n.y < 0 || n.y >= height) continue;
            if (grid[n.x, n.y] == BlockType.Breakable || grid[n.x, n.y] == BlockType.Movable)
                return true;
        }
        return false;
    }

    private bool IsOnPath(Vector2Int cell) => PathCells != null && PathCells.Contains(cell);

    // -------------------------------------------------------------------
    //  3. СПАВН
    // -------------------------------------------------------------------

    /// <summary>
    /// Спавнит по одному блоку на каждую непустую клетку.
    /// </summary>
    private void SpawnBlocks()
    {
        var parent = new GameObject("Blocks");
        parent.transform.SetParent(transform, false);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var type = grid[x, y];
                if (type == BlockType.Empty) continue;

                GameObject template = type switch
                {
                    BlockType.Solid => solidBlockTemplate,
                    BlockType.Breakable => breakableBlockTemplate,
                    BlockType.Movable => movableBlockTemplate,
                    _ => null
                };
                if (template == null)
                {
                    Debug.LogWarning($"LevelGenerator: не назначен шаблон для блока типа {type} — блок пропущен.");
                    continue;
                }

                SpawnSingleBlock(new Vector2Int(x, y), type, template, parent.transform);
            }
        }
    }

    /// <summary>
    /// Спавнит один блок в центре клетки. Форма — сплайн внутри одной клетки.
    /// </summary>
    private void SpawnSingleBlock(Vector2Int cell, BlockType type, GameObject template, Transform parent)
    {
        Vector3 worldPos = CellToWorld(cell.x, cell.y);

        var block = new GameObject($"Block_{type}_{cell.x}_{cell.y}");
        block.transform.SetParent(parent, false);
        block.transform.position = worldPos;

        var shape = block.AddComponent<SplineBlockShape>();
        shape.BuildShape(cellSize, rng, shapeControlPoints, shapeMargin, shapeSplineSamples, shapeRoughness);

        CopyScriptsFromTemplate(block, template);

        var breakable = block.GetComponent<BreakableBlock>();
        if (breakable != null)
            breakable.hitsToBreak = rng.Next(2, 4);
    }

    /// <summary>
    /// Копирует все скрипты (MonoBehaviour) с объекта-шаблона на целевой блок.
    /// </summary>
    private static void CopyScriptsFromTemplate(GameObject target, GameObject template)
    {
        foreach (var source in template.GetComponents<MonoBehaviour>())
        {
            var comp = (MonoBehaviour)target.AddComponent(source.GetType());
            string json = JsonUtility.ToJson(source);
            JsonUtility.FromJsonOverwrite(json, comp);
        }
    }

    private void SpawnFlowerAndSun()
    {
        FlowerInstance = Instantiate(flowerPrefab, CellToWorld(FlowerCell.x, FlowerCell.y),
            Quaternion.identity, transform);
        FlowerInstance.name = "Flower";

        SunInstance = Instantiate(sunPrefab, CellToWorld(SunCell.x, SunCell.y) + Vector3.up * cellSize,
            Quaternion.identity, transform);
        SunInstance.name = "Sun";
    }

    /// <summary>Мировые координаты центра клетки.</summary>
    public Vector3 CellToWorld(int x, int y)
    {
        return new Vector3(
            (x - width / 2) * cellSize,
            (y - height / 2) * cellSize,
            0f);
    }

    /// <summary>Обратное преобразование: клетка по мировой точке.</summary>
    public Vector2Int WorldToCell(Vector3 world)
    {
        return new Vector2Int(
            Mathf.RoundToInt(world.x / cellSize + width / 2f),
            Mathf.RoundToInt(world.y / cellSize + height / 2f));
    }

    /// <summary>Свободна ли клетка (для логики роста цветка).</summary>
    public bool IsCellFree(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return false;
        return grid[cell.x, cell.y] == BlockType.Empty;
    }

    /// <summary>Отметить клетку пустой (когда блок ломается/уносится).</summary>
    public void SetCellEmpty(Vector2Int cell)
    {
        if (cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height)
            grid[cell.x, cell.y] = BlockType.Empty;
    }

#if UNITY_EDITOR
    [ContextMenu("Generate Level")]
    private void ContextGenerate()
    {
        UnityEditor.EditorApplication.delayCall += () => Generate();
    }
#endif
}

// =====================================================================
//  ВСПОМОГАТЕЛЬНЫЕ КОМПОНЕНТЫ
// =====================================================================

/// <summary>
/// Триггер-направление: задаёт, куда цветок может расти.
/// </summary>
public class DirectionTrigger : MonoBehaviour
{
    [Tooltip("Направление роста, которое задаёт этот триггер")]
    public Vector2 direction = Vector2.up;

    [Tooltip("Максимальный угол от прошлого направления к этому")]
    public float maxTurnAngle = 120f;

    [HideInInspector] public bool isFree = true;

    private void OnDrawGizmos()
    {
        Gizmos.color = isFree ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 1.5f);
    }
}

/// <summary>
/// Генерирует случайную форму одиночного блока замкнутым сплайном
/// (Catmull-Rom) внутри одной клетки.
/// </summary>
public class SplineBlockShape : MonoBehaviour
{
    /// <summary>
    /// Строит случайную сплайновую форму внутри клетки размером cellSize.
    /// Создаёт MeshFilter + MeshRenderer + PolygonCollider2D.
    /// </summary>
   public void BuildShape(float cellSize, System.Random rng,
    int controlPoints, float margin, int samplesPerSegment, float roughness)
{
    // Радиус должен доходить почти до края клетки
    float maxR = cellSize * 0.8f - margin;
    
    // Сплайн "срезает" углы, поэтому компенсируем это расширением.
    // При roughness=0 форма ровная, при 0.5 — рваная, но всё равно крупная.
    float shrinkFromSpline = 1.15f; // компенсация Catmull-Rom
    maxR *= shrinkFromSpline;
    
    // minR ближе к maxR — блоки крупные, но с неровным краем
    float minR = maxR * Mathf.Lerp(0.85f, 0.6f, Mathf.Clamp01(roughness / 0.5f));

        // 1. Контрольные точки: случайные углы по кругу (сортировка ->
        //    кривая не самопересекается) со случайным радиусом.
        var angles = new List<float>();
        for (int i = 0; i < controlPoints; i++)
            angles.Add((float)rng.NextDouble() * Mathf.PI * 2f);
        angles.Sort();

        var ctrl = new List<Vector2>(controlPoints);
        foreach (var a in angles)
        {
            float r = Mathf.Lerp(minR, maxR, (float)rng.NextDouble());
            ctrl.Add(new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r));
        }

        // 2. Catmull-Rom сплайн по замкнутому контуру.
        var outline = new List<Vector2>();
        for (int i = 0; i < ctrl.Count; i++)
        {
            Vector2 p0 = ctrl[(i - 1 + ctrl.Count) % ctrl.Count];
            Vector2 p1 = ctrl[i];
            Vector2 p2 = ctrl[(i + 1) % ctrl.Count];
            Vector2 p3 = ctrl[(i + 2) % ctrl.Count];

            for (int s = 0; s < samplesPerSegment; s++)
            {
                float t = s / (float)samplesPerSegment;
                outline.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }

        // Страховка: не выпускаем точки за границы клетки.
        for (int i = 0; i < outline.Count; i++)
            outline[i] = Vector2.ClampMagnitude(outline[i], maxR);

        BuildMesh(outline);
        BuildCollider(outline);
    }

    /// <summary>Catmull-Rom интерполяция между p1 и p2.</summary>
    private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }

    private void BuildMesh(List<Vector2> outline)
    {
        var verts = new List<Vector3> { Vector3.zero };
        var tris = new List<int>();
        for (int i = 0; i < outline.Count; i++)
        {
            verts.Add(outline[i]);
            int a = 1 + i;
            int b = 1 + (i + 1) % outline.Count;
            tris.Add(0); tris.Add(b); tris.Add(a);
        }

        var mesh = new Mesh { name = "SplineBlockMesh" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mf = GetComponent<MeshFilter>();
        if (mf == null) mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        if (mr == null)
        {
            mr = gameObject.AddComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        else
        {
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }
    }

    private void BuildCollider(List<Vector2> outline)
    {
        var col = GetComponent<PolygonCollider2D>();
        if (col == null) col = gameObject.AddComponent<PolygonCollider2D>();
        col.points = outline.ToArray();
    }
}

/// <summary>
/// Ломаемый блок: разрушается после hitsToBreak нажатий.
/// </summary>
public class BreakableBlock : MonoBehaviour
{
    [Tooltip("Сколько нажатий нужно, чтобы сломать блок")]
    public int hitsToBreak = 3;
    private int hitsLeft;

    private void Awake() => hitsLeft = hitsToBreak;

    private void OnMouseDown()
    {
        Hit();
    }

    public void Hit()
    {
        hitsLeft--;
        float t = Mathf.Clamp01(1f - (float)hitsLeft / hitsToBreak);
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.enabled)
        {
            var c = sr.color;
            c.a = Mathf.Lerp(1f, 0.4f, t);
            sr.color = c;
        }
        transform.localScale = Vector3.one * Mathf.Lerp(1f, 0.7f, t);

        if (hitsLeft <= 0)
        {
            var gen = FindObjectOfType<LevelGenerator>();
            if (gen != null)
            {
                var cell = gen.WorldToCell(transform.position);
                gen.SetCellEmpty(cell);
            }
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Простая логика роста цветка.
/// </summary>
public class FlowerGrowth : MonoBehaviour
{
    public float stepInterval = 0.5f;
    public float stepLength = 1f;

    private Vector2 lastDirection = Vector2.up;
    private LevelGenerator generator;
    private float timer;

    private void Start()
    {
        generator = FindObjectOfType<LevelGenerator>();
        lastDirection = Vector2.up;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < stepInterval) return;
        timer = 0f;

        if (TryGrow()) { }
    }

    private bool TryGrow()
    {
        if (generator == null) return false;

        Vector3 toSun = generator.SunInstance.transform.position - transform.position;
        Vector2 toSun2D = new Vector2(toSun.x, toSun.y).normalized;

        var candidates = Object.FindObjectsOfType<DirectionTrigger>();

        DirectionTrigger best = null;
        float bestScore = float.MinValue;

        foreach (var trigger in candidates)
        {
            Vector3 aheadWorld = transform.position +
                new Vector3(trigger.direction.x, trigger.direction.y, 0f) * stepLength;
            var aheadCell = generator.WorldToCell(aheadWorld);
            bool free = generator.IsCellFree(aheadCell);

            float angle = Vector2.Angle(lastDirection, trigger.direction);
            bool angleOk = angle <= trigger.maxTurnAngle;

            if (!free || !angleOk) continue;

            float score = Vector2.Dot(trigger.direction.normalized, toSun2D);
            if (score > bestScore)
            {
                bestScore = score;
                best = trigger;
            }
        }

        if (best == null) return false;

        lastDirection = best.direction.normalized;
        transform.position += new Vector3(best.direction.x, best.direction.y, 0f).normalized * stepLength;
        return true;
    }
}