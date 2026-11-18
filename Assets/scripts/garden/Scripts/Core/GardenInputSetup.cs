using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Keeps flower colliders available for gameplay queries (the watering can needs
/// them on the "plant" layer) but prevents them from intercepting pointer events.
/// Without this, a flower growing in a foreground pot steals the click from a pot
/// that stands behind it, so that pot cannot be dragged or dropped into its zone.
/// Also ensures a Physics2DRaycaster exists for garden world-object pointer clicks.
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
        EnsurePhysics2DRaycaster();

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

    private static void EnsurePhysics2DRaycaster()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (mainCamera.GetComponent<Physics2DRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<Physics2DRaycaster>();
            Debug.Log($"[GardenInputSetup] Added Physics2DRaycaster to '{mainCamera.gameObject.name}'.");
        }
    }
}
