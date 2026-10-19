using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Plans a collision-checked continuation without changing the committed stem.</summary>
public sealed class FlowerGrowthRoutePlanner
{
    private sealed class Node
    {
        public Vector2Int key;
        public Vector2 point;
        public float distance;
        public Node parent;
        public bool closed;
    }

    private readonly struct Entry : IComparable<Entry>
    {
        public readonly Node node;
        private readonly float score;
        private readonly int order;
        public Entry(Node node, float score, int order) { this.node = node; this.score = score; this.order = order; }
        public int CompareTo(Entry other)
        {
            int comparison = score.CompareTo(other.score);
            return comparison != 0 ? comparison : order.CompareTo(other.order);
        }
    }

    private static readonly Vector2Int[] Neighbours =
    {
        Vector2Int.up, new Vector2Int(1, 1), Vector2Int.right, new Vector2Int(-1, 1),
        Vector2Int.left, new Vector2Int(1, -1), Vector2Int.down, new Vector2Int(-1, -1)
    };

    public IEnumerator Search(Vector2 start, Vector2 goal, Vector2 growthAxis, Rect bounds,
        Func<Vector2, Vector2, bool> canMove, List<Vector2> route, float cell = 0.25f)
    {
        route.Clear();
        var nodes = new Dictionary<Vector2Int, Node>();
        var open = new SortedSet<Entry>();
        var first = new Node { point = start, key = Vector2Int.zero };
        nodes.Add(first.key, first);
        int order = 0;
        long sliceStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        open.Add(new Entry(first, Vector2.Distance(start, goal), order++));
        for (int expanded = 0; open.Count > 0 && expanded < 2500;)
        {
            Entry entry = open.Min;
            open.Remove(entry);
            Node current = entry.node;
            if (current.closed) continue;
            current.closed = true;
            expanded++;
            Vector2 sunward = (goal - current.point).normalized;
            if (canMove(current.point, goal))
            {
                route.Add(goal);
                for (Node node = current; node.parent != null; node = node.parent) route.Add(node.point);
                route.Reverse();
                yield break;
            }
            foreach (Vector2Int neighbour in Neighbours)
            {
                Vector2 direction = ((Vector2)neighbour).normalized;
                // Sunward progress takes priority. A lateral/upward bypass is allowed;
                // walking down away from the sun is not a way out of a wrong branch.
                if (Vector2.Dot(direction, growthAxis) < -0.0001f &&
                    (direction.y < -0.0001f || Vector2.Dot(direction, sunward) < -0.8660254f)) continue;
                Vector2Int key = current.key + neighbour;
                Vector2 point = start + (Vector2)key * cell;
                if (!bounds.Contains(point)) continue;
                float length = Vector2.Distance(current.point, point);
                float distance = current.distance + length;
                if (nodes.TryGetValue(key, out Node next) && (next.closed || next.distance <= distance)) continue;
                if (!canMove(current.point, point)) continue;
                if (next == null) { next = new Node { point = point, key = key }; nodes.Add(key, next); }
                next.distance = distance;
                next.parent = current;
                // Equal-length alternatives prefer the upper route.
                float upperBias = Mathf.Max(0f, goal.y - point.y) * 0.001f;
                open.Add(new Entry(next, distance + Vector2.Distance(point, goal) * 1.05f + upperBias, order++));
            }
            if ((System.Diagnostics.Stopwatch.GetTimestamp() - sliceStarted) * 1000d /
                System.Diagnostics.Stopwatch.Frequency >= 12d)
            {
                yield return null;
                sliceStarted = System.Diagnostics.Stopwatch.GetTimestamp();
            }
        }
    }
}
