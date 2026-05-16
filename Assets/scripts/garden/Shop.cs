using System.Collections.Generic;
using UnityEngine;

// Simple shop manager for purchasing seed items.
// Attach this script to a GameObject named "Shop" in the scene.
public class Shop : MonoBehaviour
{
    // List of seed items available for purchase. Populate in the inspector.
    [SerializeField] private List<SeedItem> seedItems = new List<SeedItem>();

    // Reference to the player’s inventory or currency manager (you can replace this
    // with your own implementation). For this example we just log the purchase.
    private void Awake()
    {
        // Ensure the list is not null.
        if (seedItems == null) seedItems = new List<SeedItem>();
    }

    // Called by UI buttons to buy a specific seed by index.
    public void PurchaseSeed(int index)
    {
        if (index < 0 || index >= seedItems.Count)
        {
            Debug.LogError($"Shop: Invalid seed index {index}");
            return;
        }

        SeedItem item = seedItems[index];
        if (item == null)
        {
            Debug.LogError($"Shop: Seed item at index {index} is null.");
            return;
        }
        if (item.flowerPrefab == null)
        {
            Debug.LogError($"Shop: Seed item '{item.name}' is missing a flower prefab.");
            return;
        }

        // TODO: deduct player currency here.
        Debug.Log($"Purchased seed for {item.price} coins.");

        // Instantiate the flower prefab in the scene – it will be placed in a pot later.
        GameObject flowerObj = Instantiate(item.flowerPrefab);
        // Add SeedFollower component so the prefab follows the cursor until placed.
        if (flowerObj.GetComponent<SeedFollower>() == null)
        {
            flowerObj.AddComponent<SeedFollower>();
        }
        // The flower component will handle its own initialization later (e.g., via Pot).
    }

    // Helper to get the number of available seeds (useful for UI).
    public int SeedCount => seedItems.Count;

    // Optional: expose the list for UI to display sprites.
    public List<SeedItem> SeedItems => seedItems;
}
