using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles pointer events for a Pot. This component works together with DragAndDropSystem
/// (which contains the shared snapping logic) but can also operate standalone.
/// Attach this script to the same GameObject that has the Pot component and a Collider2D.
/// </summary>
[RequireComponent(typeof(Pot))]
[RequireComponent(typeof(Collider2D))]
public class PotDragHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Pot _pot;
    private Camera _camera;
    private static bool _isDragging = false;

    private void Awake()
    {
        _pot = GetComponent<Pot>();
        _camera = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDragging) return;
        // Start dragging regardless of flower presence.
        _isDragging = true;
        _pot.OnDragStart();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        Vector2 worldPos = _camera.ScreenToWorldPoint(eventData.position);
        _pot.transform.position = worldPos;
        FlowerCareSystem.Instance.ShowDropZoneFeedback(_pot, worldPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging) return;
        Vector2 worldPos = _camera.ScreenToWorldPoint(eventData.position);
        // Use the same snapping logic defined in Pot.
        Vector2 closest = FindClosestAllowedPosition(_pot, worldPos);
        _pot.transform.position = closest;
        FlowerCareSystem.Instance.HideDropZoneFeedback();
        _pot.OnDragEnd();
        _isDragging = false;
    }

    private static Vector2 FindClosestAllowedPosition(Pot pot, Vector2 target)
    {
        if (pot.AllowedPositions == null || pot.AllowedPositions.Length == 0)
            return pot.transform.position;
        Vector2 best = pot.AllowedPositions[0];
        float min = float.MaxValue;
        foreach (var p in pot.AllowedPositions)
        {
            float d = Vector2.Distance(target, p);
            if (d < min)
            {
                min = d;
                best = p;
            }
        }
        return best;
    }
}
