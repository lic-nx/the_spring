using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
    public EdgeCollider2D edgeCollider;
    public LineRenderer lineRenderer;

    [Header("Листья вдоль стебля")]
    public GameObject petalPrefab;              // ← перетащи сюда префаб листа/лепестка!
    public bool growLeavesAlongStem = true;     // включить листья?
    public float minLeafDistance = 1.0f;        // минимальное расстояние между листьями
    public float leafOffsetFromStem = 0.3f;     // насколько лист выступает вбок
    public float leafGrowthDuration = 0.5f;     // время анимации появления листа

    [Tooltip("Чем выше — тем плавнее линия (но дороже по производительности)")]
    public int smoothness = 10;

    private List<Vector2> controlPoints = new List<Vector2>(); // исходные точки
    private List<Vector2> smoothPoints = new List<Vector2>();  // интерполированные точки
    private List<Vector2> placedLeafPositions = new List<Vector2>(); // где уже есть листья

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
        if (edgeCollider == null)
            edgeCollider = GetComponent<EdgeCollider2D>();
    }

    // Добавляет новую контрольную точку и пытается добавить лист
    public void AddPoint(Vector2 point)
    {
        controlPoints.Add(point);
        RebuildSmoothLine();
        TryPlaceNewLeaves();
    }

    // Обновляет последнюю контрольную точку и пытается добавить лист
    public void UpdateLastPoint(Vector2 point)
    {
        if (controlPoints.Count == 0) return;
        controlPoints[controlPoints.Count - 1] = point;
        RebuildSmoothLine();
        TryPlaceNewLeaves();
    }

    private void RebuildSmoothLine()
    {
        smoothPoints.Clear();

        if (controlPoints.Count == 0)
        {
            lineRenderer.positionCount = 0;
            edgeCollider.points = new Vector2[0];
            return;
        }

        if (controlPoints.Count == 1)
        {
            smoothPoints.Add(controlPoints[0]);
        }
        else if (controlPoints.Count == 2)
        {
            for (int i = 0; i <= smoothness; i++)
            {
                float t = (float)i / smoothness;
                smoothPoints.Add(Vector2.Lerp(controlPoints[0], controlPoints[1], t));
            }
        }
        else
        {
            List<Vector2> extended = new List<Vector2>(controlPoints);
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
                    float t = (float)j / smoothness;
                    smoothPoints.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }
            smoothPoints.Add(controlPoints[^1]);
        }

        lineRenderer.positionCount = smoothPoints.Count;
        for (int i = 0; i < smoothPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, smoothPoints[i]);
        }

        Vector2[] localPoints = new Vector2[smoothPoints.Count];
        for (int i = 0; i < smoothPoints.Count; i++)
        {
            localPoints[i] = transform.InverseTransformPoint(smoothPoints[i]);
        }
        edgeCollider.points = localPoints;
    }

    // Пытается разместить один новый лист при каждом обновлении
    private void TryPlaceNewLeaves()
    {
        if (!growLeavesAlongStem || petalPrefab == null || smoothPoints.Count < 3)
            return;

        Vector2 lastPoint = smoothPoints[0];

        for (int i = 1; i < smoothPoints.Count; i++)
        {
            Vector2 current = smoothPoints[i];
            float segmentLength = Vector2.Distance(lastPoint, current);

            // Проверяем несколько точек на сегменте
            int steps = Mathf.Max(1, Mathf.CeilToInt(segmentLength / (minLeafDistance * 0.6f)));
            for (int s = 1; s <= steps; s++)
            {
                float t = (float)s / steps;
                Vector2 candidate = Vector2.Lerp(lastPoint, current, t);

                // Проверка расстояния до уже существующих листьев
                bool canPlace = true;
                foreach (Vector2 placed in placedLeafPositions)
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
                    return; // только один лист за раз
                }
            }
            lastPoint = current;
        }
    }

    private void PlaceLeafAt(Vector2 stemPosition)
    {
        Vector2 forward = GetTangentAtPoint(stemPosition);
        Vector2 side = new Vector2(-forward.y, forward.x); // перпендикуляр

        // Случайная сторона (влево/вправо)
        if (Random.value > 0.5f)
            side = -side;

        Vector2 leafPosition = stemPosition + side * leafOffsetFromStem;

        GameObject leaf = Instantiate(petalPrefab, leafPosition, Quaternion.identity, transform);
        leaf.transform.localScale = Vector3.zero;

        // Поворот листа по направлению стебля
        float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
        leaf.transform.rotation = Quaternion.Euler(0, 0, angle + 90f); // +90 для естественного вида

        StartCoroutine(ScaleTo(leaf.transform, Vector3.one, leafGrowthDuration));
    }

    private Vector2 GetTangentAtPoint(Vector2 position)
    {
        if (smoothPoints.Count < 3) return Vector2.up;

        int bestIndex = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < smoothPoints.Count - 2; i++)
        {
            Vector2 a = smoothPoints[i];
            Vector2 b = smoothPoints[i + 1];
            Vector2 closest = ClosestPointOnSegment(a, b, position);
            float dist = Vector2.Distance(position, closest);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        return (smoothPoints[bestIndex + 1] - smoothPoints[bestIndex]).normalized;
    }

    private Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + ab * t;
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

    private IEnumerator ScaleTo(Transform t, Vector3 targetScale, float duration)
    {
        Vector3 startScale = t.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            t.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = targetScale;
    }

    // Вспомогательные методы
    public Vector2 GetLastPoint()
    {
        return controlPoints.Count < 2
            ? (controlPoints.Count == 1 ? controlPoints[0] : Vector2.zero)
            : controlPoints[^2];
    }

    public Vector2 GetLastControlPoint()
    {
        return controlPoints.Count > 0 ? controlPoints[^1] : Vector2.zero;
    }
} 