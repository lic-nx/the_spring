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
    public float minLeafDistance = 1f;
    public float leafOffsetFromStem;
    public float leafGrowthDuration = 0.5f;
    [HideInInspector] public int smoothness = 10;

    // These are the actual swept tip positions. Never refit old segments through a spline:
    // doing so moves already-grown geometry and can cut across an obstacle's corner.
    private readonly List<Vector2> points = new List<Vector2>();
    private readonly List<GameObject> leaves = new List<GameObject>();
    private float distanceSinceLeaf;

    private void Awake() => CacheComponents();

    private void CacheComponents()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
        if (edgeCollider == null)
            edgeCollider = GetComponent<EdgeCollider2D>();
        if (lineRenderer != null)
            lineRenderer.useWorldSpace = true;
    }

    public void ResetGrowth(Vector2 root, Vector2 head)
    {
        CacheComponents();
        StopAllCoroutines();
        foreach (GameObject leaf in leaves)
        {
            if (leaf == null)
                continue;
            leaf.SetActive(false);
            if (Application.isPlaying)
                Destroy(leaf);
            else
                DestroyImmediate(leaf);
        }
        leaves.Clear();
        distanceSinceLeaf = 0f;
        points.Clear();
        points.Add(root);
        if ((head - root).sqrMagnitude > 0.000001f)
            points.Add(head);
        Apply();
    }

    public void AppendGrowthPoint(Vector2 point)
    {
        CacheComponents();
        if (points.Count == 0)
        {
            points.Add(point);
            Apply();
            return;
        }
        Vector2 previous = points[points.Count - 1];
        float distance = Vector2.Distance(previous, point);
        if (distance < 0.0001f)
            return;
        points.Add(point);
        Apply();
        distanceSinceLeaf += distance;
        if (growLeavesAlongStem && petalPrefab != null && distanceSinceLeaf >= Mathf.Max(0.1f, minLeafDistance))
        {
            distanceSinceLeaf = 0f;
            PlaceLeaf(point, (point - previous).normalized);
        }
    }

    // Keep old public entry points for scene scripts; committed geometry is append-only.
    public void AddPoint(Vector2 point) => AppendGrowthPoint(point);
    public void UpdateHead(Vector2 point) => AppendGrowthPoint(point);
    public void TryCommitSegment(Vector2 point) => AppendGrowthPoint(point);

    private void Apply()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
                lineRenderer.SetPosition(i, points[i]);
        }
        if (edgeCollider != null)
        {
            edgeCollider.enabled = points.Count >= 2;
            if (points.Count < 2)
                return;
            Vector2[] localPoints = new Vector2[points.Count];
            for (int i = 0; i < points.Count; i++)
                localPoints[i] = transform.InverseTransformPoint(points[i]);
            edgeCollider.points = localPoints;
        }
    }

    private void PlaceLeaf(Vector2 position, Vector2 forward)
    {
        Vector2 side = new Vector2(-forward.y, forward.x);
        if (Random.value > 0.5f)
            side = -side;
        GameObject leaf = Instantiate(petalPrefab, position + side * leafOffsetFromStem,
            Quaternion.identity, transform);
        leaves.Add(leaf);
        leaf.transform.localScale = Vector3.zero;
        float angle = Mathf.Clamp(Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg - 90f, -60f, 60f);
        leaf.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        StartCoroutine(ScaleLeaf(leaf.transform));
    }

    private IEnumerator ScaleLeaf(Transform leaf)
    {
        float elapsed = 0f;
        while (leaf != null && elapsed < leafGrowthDuration)
        {
            leaf.localScale = Vector3.one * (1.5f * elapsed / leafGrowthDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (leaf != null)
            leaf.localScale = Vector3.one * 1.5f;
    }
}
