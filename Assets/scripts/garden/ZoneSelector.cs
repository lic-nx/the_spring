using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

/// <summary>
/// Defines placement zones for pots. Each zone is a set of world positions that pots may snap to.
/// Attach this script to an empty GameObject in the scene (e.g., "GardenZoneManager").
/// Add child empty GameObjects to represent individual allowed positions and assign them in the inspector.
/// At runtime the script will distribute those positions to all Pot instances that are children of this manager.
/// </summary>
public class ZoneSelector : MonoBehaviour
{
    // List of transforms representing allowed snap points for pots.
    [Tooltip("Assign empty child objects that mark valid pot positions.")]
    public Transform[] SnapPoints;

    // Optionally, you can group points into logical zones (e.g., left/right).
    // For simplicity we treat all points equally.

    private void Awake()
    {
        // Find all Pot components that are children of this manager.
        var pots = GetComponentsInChildren<Pot>(includeInactive: true);
        if (pots.Length == 0)
        {
            Debug.LogWarning($"ZoneSelector on '{name}' found no Pot components in its children.");
            return;
        }
        if (SnapPoints == null || SnapPoints.Length == 0)
        {
            Debug.LogWarning($"ZoneSelector on '{name}' has no SnapPoints assigned. Pots will have no allowed positions.");
            return;
        }

        // Distribute the snap points evenly among pots (or assign all points to each pot).
        // Here we assign the full list to every pot so each pot can snap to any zone.
        foreach (var pot in pots)
        {
            var positions = new Vector2[SnapPoints.Length];
            for (int i = 0; i < SnapPoints.Length; i++)
            {
                var p = SnapPoints[i];
                positions[i] = p.position;
            }
            pot.SetAllowedPositions(positions);
        }
    }
}
