using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Temporarily hides flowers while a pot is being moved so the player can see and
/// reach the drop zone behind them. Colliders/bodies are disabled only for the
/// duration of the drag and are always restored afterwards. Reference counted,
/// because a shop drag and a world-pot drag can be started/stopped in any order.
/// </summary>
public static class FlowerDragPresentation
{
    private const float DragOpacity = 0.45f;
    private const int SortingOffsetWhileDragging = 200;

    private sealed class FlowerState
    {
        public Collider2D[] Colliders;
        public bool[] ColliderStates;
        public Rigidbody2D[] Bodies;
        public bool[] BodySimulatedStates;
        public SpriteRenderer[] Renderers;
        public Color[] Colors;
        public int[] SortingOrders;
    }

    private static readonly List<FlowerState> _states = new List<FlowerState>();
    private static int _sessions;

    public static void Begin()
    {
        if (_sessions++ > 0) return;

        Flower[] flowers = Object.FindObjectsByType<Flower>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Flower flower in flowers)
        {
            if (flower == null) continue;

            FlowerState state = new FlowerState();

            state.Colliders = flower.GetComponentsInChildren<Collider2D>(true);
            state.ColliderStates = new bool[state.Colliders.Length];
            for (int i = 0; i < state.Colliders.Length; i++)
            {
                state.ColliderStates[i] = state.Colliders[i].enabled;
                state.Colliders[i].enabled = false;
            }

            state.Bodies = flower.GetComponentsInChildren<Rigidbody2D>(true);
            state.BodySimulatedStates = new bool[state.Bodies.Length];
            for (int i = 0; i < state.Bodies.Length; i++)
            {
                state.BodySimulatedStates[i] = state.Bodies[i].simulated;
                state.Bodies[i].simulated = false;
            }

            state.Renderers = flower.GetComponentsInChildren<SpriteRenderer>(true);
            state.Colors = new Color[state.Renderers.Length];
            state.SortingOrders = new int[state.Renderers.Length];
            for (int i = 0; i < state.Renderers.Length; i++)
            {
                SpriteRenderer renderer = state.Renderers[i];
                state.Colors[i] = renderer.color;
                state.SortingOrders[i] = renderer.sortingOrder;

                Color dimmed = renderer.color;
                dimmed.a *= DragOpacity;
                renderer.color = dimmed;
                renderer.sortingOrder += SortingOffsetWhileDragging;
            }

            _states.Add(state);
        }
    }

    public static void End()
    {
        if (_sessions == 0) return;
        if (--_sessions > 0) return;

        foreach (FlowerState state in _states)
        {
            for (int i = 0; i < state.Renderers.Length; i++)
            {
                if (state.Renderers[i] == null) continue;
                state.Renderers[i].color = state.Colors[i];
                state.Renderers[i].sortingOrder = state.SortingOrders[i];
            }

            for (int i = 0; i < state.Bodies.Length; i++)
            {
                if (state.Bodies[i] != null) state.Bodies[i].simulated = state.BodySimulatedStates[i];
            }

            for (int i = 0; i < state.Colliders.Length; i++)
            {
                if (state.Colliders[i] != null) state.Colliders[i].enabled = state.ColliderStates[i];
            }
        }

        _states.Clear();
    }
}
