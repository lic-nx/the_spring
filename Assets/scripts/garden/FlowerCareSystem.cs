using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class FlowerCareSystem : MonoBehaviour
{
    // Singleton instance for easy access from other systems (e.g., DragAndDropSystem).
    public static FlowerCareSystem Instance { get; private set; }

    public List<Flower> Flowers { get; private set; } = new List<Flower>();

    // Visual feedback components
    private SpriteRenderer _zoneHighlightRenderer;
    [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.3f);   // semi‑transparent green
    [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.3f); // semi‑transparent red

    private void Awake()
    {
        // Ensure a single instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);

        // Ensure DropZoneHighlight exists – use existing child if present, otherwise create it.
        Transform highlightTransform = transform.Find("DropZoneHighlight");
        GameObject highlightObj;
        if (highlightTransform != null)
        {
            highlightObj = highlightTransform.gameObject;
        }
        else
        {
            highlightObj = new GameObject("DropZoneHighlight");
            highlightObj.transform.SetParent(transform);
        }
        _zoneHighlightRenderer = highlightObj.GetComponent<SpriteRenderer>();
        if (_zoneHighlightRenderer == null)
        {
            _zoneHighlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
        }
        _zoneHighlightRenderer.enabled = false;
        // Use a simple square sprite; if none is assigned, the renderer will stay invisible until a sprite is set in the editor.
    }

    public void RegisterFlower(Flower flower)
    {
        if (!Flowers.Contains(flower))
            Flowers.Add(flower);
    }

    public void UnregisterFlower(Flower flower)
    {
        Flowers.Remove(flower);
    }

    /// <summary>
    /// Called by the drag‑and‑drop system while a pot is being dragged.
    /// Shows a semi‑transparent square at the closest allowed position.
    /// </summary>
    /// <param name="pot">The pot currently being dragged.</param>
    /// <param name="mouseWorldPos">Current mouse position in world coordinates.</param>
    public void ShowDropZoneFeedback(Pot pot, Vector2 mouseWorldPos)
    {
        if (pot == null || pot.AllowedPositions == null || pot.AllowedPositions.Length == 0)
        {
            _zoneHighlightRenderer.enabled = false;
            return;
        }

        // Find closest allowed position.
        Vector2 closest = pot.AllowedPositions[0];
        float minDist = Vector2.Distance(mouseWorldPos, closest);
        foreach (var pos in pot.AllowedPositions)
        {
            float d = Vector2.Distance(mouseWorldPos, pos);
            if (d < minDist)
            {
                minDist = d;
                closest = pos;
            }
        }

        // Update renderer position and colour.
        _zoneHighlightRenderer.transform.position = closest;
        _zoneHighlightRenderer.color = minDist < 1.0f ? _validColor : _invalidColor; // within 1 unit = valid
        _zoneHighlightRenderer.enabled = true;
    }

    /// <summary>
    /// Hide the visual feedback when the drag operation ends.
    /// </summary>
    public void HideDropZoneFeedback()
    {
        _zoneHighlightRenderer.enabled = false;
    }
}
