using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Keeps flower colliders available for gameplay queries (the watering can needs
/// them on the "plant" layer) but prevents them from intercepting pointer events.
/// Without this, a flower growing in a foreground pot steals the click from a pot
/// that stands behind it, so that pot cannot be dragged or dropped into its zone.
/// </summary>
public static class GardenInputSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        EnsureConfigured();
    }

    public static void EnsureConfigured()
    {
        int plantLayer = LayerMask.NameToLayer("plant");
        if (plantLayer < 0)
        {
            Debug.LogWarning("[GardenInputSetup] Layer 'plant' is missing; flower clicks will not be filtered.");
            return;
        }

        int plantBit = 1 << plantLayer;
        Physics2DRaycaster[] raycasters = Object.FindObjectsByType<Physics2DRaycaster>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Physics2DRaycaster raycaster in raycasters)
        {
            int mask = raycaster.eventMask;
            if ((mask & plantBit) == 0) continue;

            raycaster.eventMask = mask & ~plantBit;
            Debug.Log($"[GardenInputSetup] Excluded 'plant' layer from raycaster on '{raycaster.gameObject.name}'.");
        }
    }
}
