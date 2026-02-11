using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
    public EdgeCollider2D edgeCollider;
    public LineRenderer lineRenderer;

    [Header("Листья вдоль стебля")]
    public GameObject petalPrefab;
    public bool growLeavesAlongStem = true;
    public float minLeafDistance = 1.0f;
    public float leafOffsetFromStem = 0.0f;
    public float leafGrowthDuration = 0.5f;

    [Tooltip("Чем выше — тем плавнее линия")]
    public int smoothness = 10;

    [SerializeField] private float minSegmentLength = 0.3f;

    private List<Vector2> controlPoints = new();
    private List<Vector2> smoothPoints = new();
    private List<Vector2> placedLeafPositions = new();

    private bool dirtyFullRebuild = true;

    private void Awake()
    {
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();
        if (!edgeCollider)
            edgeCollider = GetComponent<EdgeCollider2D>();
    }

    /* =========================
       ПУБЛИЧНЫЕ МЕТОДЫ РОСТА
       ========================= */

    // Обновляется каждый кадр — двигает только голову
    public void AddPoint(Vector2 point)
{
        controlPoints.Add(point);
    
    if (controlPoints.Count >= 2)
    {
        FullRebuild();
    }
}
    public void UpdateHead(Vector2 headPosition)
    {
        if (controlPoints.Count == 0)
        {
            controlPoints.Add(headPosition);
            dirtyFullRebuild = true;
            FullRebuild();
            return;
        }

        controlPoints[^1] = headPosition;

        if (dirtyFullRebuild || controlPoints.Count < 4)
            FullRebuild();
        else
            FullRebuild(); //здесь надо заменить на апдейт только хвоста, но я не понимаю, почему он косячит с этим
    }

    // Фиксирует сегмент (добавляет новую точку ПЕРЕД головой)
    public void TryCommitSegment(Vector2 headPosition)
    {
        if (controlPoints.Count < 2)
            return;

        Vector2 prev = controlPoints[^2];

        if (Vector2.Distance(prev, headPosition) >= minSegmentLength)
        {
            controlPoints.Insert(controlPoints.Count - 1, headPosition);
            dirtyFullRebuild = true;
            FullRebuild();
            TryPlaceNewLeaves();
        }
    }

    /* =========================
       ПЕРЕСТРОЕНИЕ ЛИНИИ
       ========================= */

    private void FullRebuild()
    {
        dirtyFullRebuild = false;
        smoothPoints.Clear();

        if (controlPoints.Count == 1)
        {
            smoothPoints.Add(controlPoints[0]);
        }
        else if (controlPoints.Count == 2)
        {
            for (int i = 0; i <= smoothness; i++)
            {
                float t = i / (float)smoothness;
                smoothPoints.Add(Vector2.Lerp(controlPoints[0], controlPoints[1], t));
            }
        }
        else
        {
            List<Vector2> extended = new(controlPoints);
            extended.Insert(0, controlPoints[0]);
            extended.Add(controlPoints[^1]);

            for (int i = 1; i < controlPoints.Count; i++)
            {
                Vector2 p0 = extended[i - 1];
                Vector2 p1 = extended[i];
                Vector2 p2 = extended[i + 1];
                Vector2 p3 = extended[i + 2];

                for (int j = 0; j < smoothness; j++)
                {
                    float t = j / (float)smoothness;
                    smoothPoints.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            smoothPoints.Add(controlPoints[^1]);
        }

        ApplyToRenderer();
    }

    // Обновляет ТОЛЬКО последний сегмент
    private void UpdateTailOnly()
    {
        int cp = controlPoints.Count;

        Vector2 p0 = controlPoints[cp - 4];
        Vector2 p1 = controlPoints[cp - 3];
        Vector2 p2 = controlPoints[cp - 2];
        Vector2 p3 = controlPoints[cp - 1];

        int removeCount = smoothness;
        smoothPoints.RemoveRange(smoothPoints.Count - removeCount, removeCount);

        for (int i = 0; i < smoothness; i++)
        {
            float t = i / (float)smoothness;
            smoothPoints.Add(CatmullRom(p0, p1, p2, p3, t));
        }

        ApplyToRenderer();
    }

    private void ApplyToRenderer()
    {
        lineRenderer.positionCount = smoothPoints.Count;
        lineRenderer.SetPositions(smoothPoints.ConvertAll(p => (Vector3)p).ToArray());

        Vector2[] colliderPoints = new Vector2[smoothPoints.Count];
        for (int i = 0; i < smoothPoints.Count; i++)
            colliderPoints[i] = transform.InverseTransformPoint(smoothPoints[i]);

        edgeCollider.points = colliderPoints;
    }

    /* =========================
       ЛИСТЬЯ
       ========================= */

    private void TryPlaceNewLeaves()
    {
        if (!growLeavesAlongStem || !petalPrefab || smoothPoints.Count < 3)
            return;

        Vector2 last = smoothPoints[0];

        for (int i = 1; i < smoothPoints.Count; i++)
        {
            Vector2 current = smoothPoints[i];
            float dist = Vector2.Distance(last, current);

            int steps = Mathf.Max(1, Mathf.CeilToInt(dist / (minLeafDistance * 0.6f)));

            for (int s = 1; s <= steps; s++)
            {
                Vector2 candidate = Vector2.Lerp(last, current, s / (float)steps);

                bool canPlace = true;
                foreach (var placed in placedLeafPositions)
                {
                    if (Vector2.Distance(candidate, placed) < minLeafDistance)
                    {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace)
                {
                    PlaceLeafAt(candidate);
                    placedLeafPositions.Add(candidate);
                    return;
                }
            }
            last = current;
        }
    }

    private void PlaceLeafAt(Vector2 pos)
    {
        Vector2 forward = GetTangentAtPoint(pos);
        Vector2 side = new(-forward.y, forward.x);
        if (Random.value > 0.5f) side = -side;

        GameObject leaf = Instantiate(petalPrefab, pos + side * leafOffsetFromStem, Quaternion.identity, transform);
        leaf.transform.localScale = Vector3.zero;

        float angle = Mathf.Clamp(Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f, -60f, 60f);
        leaf.transform.rotation = Quaternion.Euler(0, 0, angle);

        StartCoroutine(ScaleTo(leaf.transform, Vector3.one * 1.5f, leafGrowthDuration));
    }

    /* =========================
       ВСПОМОГАТЕЛЬНОЕ
       ========================= */

    private Vector2 GetTangentAtPoint(Vector2 pos)
    {
        for (int i = 0; i < smoothPoints.Count - 1; i++)
            if (Vector2.Distance(pos, smoothPoints[i]) < 0.2f)
                return (smoothPoints[i + 1] - smoothPoints[i]).normalized;

        return Vector2.up;
    }

    private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * (
            (2 * p1) +
            (-p0 + p2) * t +
            (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
            (-p0 + 3 * p1 - 3 * p2 + p3) * t3
        );
    }

    private IEnumerator ScaleTo(Transform t, Vector3 target, float duration)
    {
        Vector3 start = t.localScale;
        float time = 0f;
        while (time < duration)
        {
            t.localScale = Vector3.Lerp(start, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        t.localScale = target;
    }
}
