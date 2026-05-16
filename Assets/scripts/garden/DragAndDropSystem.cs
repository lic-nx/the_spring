using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Event‑driven drag‑and‑drop system for garden pots.
/// Replaces the previous per‑frame polling implementation with Unity UI event callbacks.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DragAndDropSystem : MonoBehaviour,
                                 IPointerDownHandler,
                                 IDragHandler,
                                 IPointerUpHandler
{
    // ---------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------
    private Pot _currentDraggedPot;          // The pot currently being moved.
    private Camera _camera;                  // Cached main camera.
    private static bool _isDragging = false; // Ensures only one pot is dragged at a time.

    // ---------------------------------------------------------------------
    // Unity callbacks
    // ---------------------------------------------------------------------
    private void Awake()
    {
        _camera = Camera.main;
    }

    /// <summary>
    /// Called when the primary mouse button is pressed over this collider.
    /// Starts a drag operation if a Pot is hit.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging) return;

        var ray = _camera.ScreenPointToRay(eventData.position);
        // Perform raycast only against the Pot layer to avoid hitting unrelated colliders.
        int potLayerMask = LayerMask.GetMask("Pot");
        var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, potLayerMask).collider;
        if (hit == null) return;

        var pot = hit.GetComponent<Pot>();
        if (pot == null) return;

        _currentDraggedPot = pot;
        _isDragging = true;
        pot.OnDragStart(); // currently a no‑op but kept for symmetry.
    }

    /// <summary>
    /// Called each frame while the mouse button is held down after OnPointerDown.
    /// Moves the pot to follow the cursor and shows drop‑zone feedback.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (_currentDraggedPot == null) return;

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(eventData.position);
        _currentDraggedPot.transform.position = mouseWorld;
        FlowerCareSystem.Instance.ShowDropZoneFeedback(_currentDraggedPot, mouseWorld);
    }

    /// <summary>
    /// Called when the mouse button is released. Snaps the pot to the nearest
    /// allowed position and hides visual feedback.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (_currentDraggedPot == null) return;

        Vector2 mouseWorld = _camera.ScreenToWorldPoint(eventData.position);
        Vector2 closest = FindClosestAllowedPosition(_currentDraggedPot, mouseWorld);
        _currentDraggedPot.transform.position = closest;
        FlowerCareSystem.Instance.HideDropZoneFeedback();
        _currentDraggedPot.OnDragEnd();

        _currentDraggedPot = null;
        _isDragging = false;
    }

    // ---------------------------------------------------------------------
    // Helper
    // ---------------------------------------------------------------------
    /// <summary>
    /// Returns the allowed position of the given pot that is closest to the target.
    /// </summary>
    private static Vector2 FindClosestAllowedPosition(Pot pot, Vector2 target)
    {
        if (pot.AllowedPositions == null || pot.AllowedPositions.Length == 0)
            return pot.transform.position;

        Vector2 best = pot.AllowedPositions[0];
        float min = float.MaxValue;
        foreach (var pos in pot.AllowedPositions)
        {
            float d = Vector2.Distance(target, pos);
            if (d < min)
            {
                min = d;
                best = pos;
            }
        }
        return best;
    }
}
