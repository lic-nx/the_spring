using UnityEngine;

/// <summary>
/// Simple drag‑and‑drop contract for objects that can be dragged in the garden.
/// Implementations should handle visual feedback elsewhere (e.g., via FlowerCareSystem).
/// </summary>
public interface IDraggable
{
    /// <summary>Called when the drag operation starts.</summary>
    void OnDragStart();

    /// <summary>Called every frame while the object is being dragged.</summary>
    void OnDrag();

    /// <summary>Called when the drag operation ends.</summary>
    void OnDragEnd();
}
