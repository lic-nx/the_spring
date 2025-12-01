using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
    [Header("References")]
    public EdgeCollider2D edgeCollider;
    public LineRenderer lineRenderer;

    [Header("Leaves Settings")]
    public GameObject petalPrefab;
    public bool growLeavesAlongStem = true;
    public float minLeafDistance = 1.0f;
    public float leafOffsetFromStem = 0.3f;
    public float leafGrowthDuration = 0.5f;
    public int smoothness = 10;
    public float maxLeafSize = 2f;
    public float minLeafSize = 0.5f;

    private List<Vector2> controlPoints = new();
    private List<Vector2> smoothPoints = new();
    private List<LeafData> leavesData = new();
    private float currentStemLength;

    [System.Serializable]
    private class LeafData
    {
        public GameObject leafObject;
        public Vector2 position;
        public float creationTime;
        public float initialStemLength;
    }

    private void Awake()
    {
        lineRenderer ??= GetComponent<LineRenderer>();
        edgeCollider ??= GetComponent<EdgeCollider2D>();
    }

    public void AddPoint(Vector2 point)
    {
        controlPoints.Add(point);
        RebuildLine();
    }

    public void UpdateLastPoint(Vector2 point)
    {
        if (controlPoints.Count == 0) return;
        controlPoints[^1] = point;
        RebuildLine();
    }

    private void RebuildLine()
    {
        if (controlPoints.Count == 0)
        {
            lineRenderer.positionCount = 0;
            edgeCollider.points = new Vector2[0];
            return;
        }

        smoothPoints = GenerateSmoothPoints();
        lineRenderer.positionCount = smoothPoints.Count;
        for (int i = 0; i < smoothPoints.Count; i++)
            lineRenderer.SetPosition(i, smoothPoints[i]);

        edgeCollider.points = smoothPoints.ToArray();
        currentStemLength = CalculateStemLength();
        UpdateAllLeavesScale();

        if (growLeavesAlongStem && petalPrefab != null)
            TryPlaceNewLeaves();
    }

    private List<Vector2> GenerateSmoothPoints()
    {
        List<Vector2> points = new();
        if (controlPoints.Count == 1)
        {
            points.Add(controlPoints[0]);
            return points;
        }

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
                float t = (float)j / smoothness;
                points.Add(CatmullRom(p0, p1, p2, p3, t));
            }
        }
        points.Add(controlPoints[^1]);
        return points;
    }

    private void TryPlaceNewLeaves()
    {
        if (smoothPoints.Count < 3) return;

        Vector2 lastPoint = smoothPoints[0];
        for (int i = 1; i < smoothPoints.Count; i++)
        {
            Vector2 current = smoothPoints[i];
            float segmentLength = Vector2.Distance(lastPoint, current);
            int steps = Mathf.Max(1, Mathf.CeilToInt(segmentLength / (minLeafDistance * 0.6f)));

            for (int s = 1; s <= steps; s++)
            {
                Vector2 candidate = Vector2.Lerp(lastPoint, current, (float)s / steps);
                if (IsLeafTooClose(candidate)) continue;

                PlaceLeafAt(candidate);
                return;
            }
            lastPoint = current;
        }
    }

    private bool IsLeafTooClose(Vector2 position)
    {
        foreach (LeafData leaf in leavesData)
            if (Vector2.Distance(position, leaf.position) < minLeafDistance)
                return true;
        return false;
    }

    private void PlaceLeafAt(Vector2 stemPosition)
    {
        Vector2 tangent = GetTangentAtPoint(stemPosition);
        Vector2 side = new(-tangent.y, tangent.x);
        Vector2 leafPosition = stemPosition + side * leafOffsetFromStem;

        GameObject leaf = Instantiate(petalPrefab, leafPosition, Quaternion.identity, transform);
        leaf.transform.localScale = Vector3.zero;
        leaf.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg + 90f);

        LeafData newLeaf = new()
        {
            leafObject = leaf,
            position = stemPosition,
            creationTime = Time.time,
            initialStemLength = currentStemLength
        };
        leavesData.Add(newLeaf);
        StartCoroutine(GrowLeaf(newLeaf));
    }

    private IEnumerator GrowLeaf(LeafData leafData)
    {
        float elapsed = 0f;
        while (elapsed < leafGrowthDuration)
        {
            leafData.leafObject.transform.localScale = Vector3.one * Mathf.Lerp(0, GetLeafTargetScale(leafData), elapsed / leafGrowthDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        leafData.leafObject.transform.localScale = Vector3.one * GetLeafTargetScale(leafData);
    }

    private float GetLeafTargetScale(LeafData leafData)
    {
        float ageFactor = 1f - (leafData.creationTime / Time.time);
        float size = minLeafSize + (maxLeafSize - minLeafSize) * ageFactor;
        size *= currentStemLength / leafData.initialStemLength;
        return Mathf.Clamp(size, minLeafSize, maxLeafSize);
    }

    private void UpdateAllLeavesScale()
    {
        foreach (LeafData leaf in leavesData)
            if (leaf.leafObject != null)
                leaf.leafObject.transform.localScale = Vector3.one * GetLeafTargetScale(leaf);
    }

    private float CalculateStemLength()
    {
        float length = 0f;
        for (int i = 1; i < smoothPoints.Count; i++)
            length += Vector2.Distance(smoothPoints[i - 1], smoothPoints[i]);
        return length;
    }

    private Vector2 GetTangentAtPoint(Vector2 position)
    {
        if (smoothPoints.Count < 2) return Vector2.up;

        int bestIndex = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < smoothPoints.Count - 1; i++)
        {
            Vector2 closest = ClosestPointOnSegment(smoothPoints[i], smoothPoints[i + 1], position);
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
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab));
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

    public Vector2 GetLastPoint() => controlPoints.Count switch
    {
        0 => Vector2.zero,
        1 => controlPoints[0],
        _ => controlPoints[^2]
    };

    public Vector2 GetLastControlPoint() => controlPoints.Count > 0 ? controlPoints[^1] : Vector2.zero;

    private void OnDestroy()
    {
        foreach (LeafData leaf in leavesData)
            if (leaf.leafObject != null)
                Destroy(leaf.leafObject);
        leavesData.Clear();
    }
}
