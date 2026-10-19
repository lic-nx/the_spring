using System.Collections.Generic;
using UnityEngine;

/// <summary>The same world-space capsule and filtering for movement and restart hints.</summary>
public sealed class FlowerGrowthCollision
{
    private readonly CapsuleCollider2D capsule;
    private readonly Line_rendered stem;
    private readonly ContactFilter2D filter;
    private readonly float clearance;
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(16);
    private readonly List<Collider2D> overlaps = new List<Collider2D>(16);
    private readonly Dictionary<Collider2D, bool> obstacleKinds = new Dictionary<Collider2D, bool>();
    private Vector2 size, offset;
    private float angle;
    private CapsuleDirection2D direction;

    public FlowerGrowthCollision(CapsuleCollider2D capsule, Line_rendered stem, LayerMask mask,
        float clearance)
    {
        this.capsule = capsule;
        this.stem = stem;
        this.clearance = Mathf.Max(0.001f, clearance);
        filter = new ContactFilter2D();
        filter.SetLayerMask(mask.value == 0 ? (LayerMask)65 : mask);
        filter.useTriggers = false;
        RefreshGeometry();
    }

    public Vector2 Size => size;

    // Shape/transform values are stable throughout one growth tick or planning slice.
    public void RefreshGeometry()
    {
        Vector3 scale = capsule.transform.lossyScale;
        size = Vector2.Scale(capsule.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        offset = capsule.transform.TransformVector(capsule.offset);
        angle = capsule.transform.eulerAngles.z;
        direction = capsule.direction;
    }

    public Vector2 CenterAt(Vector2 position)
    {
        return position + offset;
    }

    public void InvalidateObstacles() => obstacleKinds.Clear();

    public bool IsObstacle(Collider2D other)
    {
        if (other == null || other.isTrigger) return false;
        if (obstacleKinds.TryGetValue(other, out bool obstacle)) return obstacle;
        obstacle = other != capsule && !other.transform.IsChildOf(capsule.transform) &&
               (stem == null || !other.transform.IsChildOf(stem.transform)) &&
               !other.CompareTag("Player") && !other.CompareTag("sun") && !other.CompareTag("stem") &&
               other.GetComponentInParent<enemy>() == null &&
               other.GetComponentInParent<LeafPowerup>() == null;
        obstacleKinds.Add(other, obstacle);
        return obstacle;
    }

    public bool CanOccupy(Vector2 position)
    {
        overlaps.Clear();
        Physics2D.OverlapCapsule(CenterAt(position), Size, direction, angle, filter, overlaps);
        foreach (Collider2D other in overlaps)
            if (IsObstacle(other))
                return false;
        return true;
    }

    public float FreeDistance(Vector2 position, Vector2 direction, float distance)
    {
        if (distance <= 0f || direction.sqrMagnitude < 0.0001f)
            return 0f;
        hits.Clear();
        Physics2D.CapsuleCast(CenterAt(position), Size, this.direction, angle, direction,
            filter, hits, distance + clearance);
        float result = distance;
        foreach (RaycastHit2D hit in hits)
            if (IsObstacle(hit.collider))
                result = Mathf.Min(result, Mathf.Max(0f, hit.distance - clearance));
        return result;
    }

    public bool TryGetBlockingNormal(Vector2 position, Vector2 direction, float distance, out Vector2 normal)
    {
        hits.Clear();
        Physics2D.CapsuleCast(CenterAt(position), Size, this.direction, angle, direction,
            filter, hits, distance + clearance);
        float closest = float.PositiveInfinity;
        normal = Vector2.zero;
        foreach (RaycastHit2D hit in hits)
        {
            if (!IsObstacle(hit.collider) || hit.distance >= closest)
                continue;
            closest = hit.distance;
            normal = hit.normal;
        }
        return normal.sqrMagnitude > 0.01f;
    }

    public static float SegmentDistance(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Vector2 ab = b - a;
        Vector2 cd = d - c;
        float cross = ab.x * cd.y - ab.y * cd.x;
        if (Mathf.Abs(cross) > 0.000001f)
        {
            Vector2 ac = c - a;
            float t = (ac.x * cd.y - ac.y * cd.x) / cross;
            float u = (ac.x * ab.y - ac.y * ab.x) / cross;
            if (t >= 0f && t <= 1f && u >= 0f && u <= 1f)
                return 0f;
        }
        return Mathf.Min(Mathf.Min(DistanceToSegment(a, c, d), DistanceToSegment(b, c, d)),
            Mathf.Min(DistanceToSegment(c, a, b), DistanceToSegment(d, a, b)));
    }

    public static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float t = segment.sqrMagnitude > 0.000001f
            ? Mathf.Clamp01(Vector2.Dot(point - start, segment) / segment.sqrMagnitude)
            : 0f;
        return Vector2.Distance(point, start + segment * t);
    }
}
