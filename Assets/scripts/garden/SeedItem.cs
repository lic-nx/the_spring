using UnityEngine;

// ScriptableObject representing a purchasable seed.
// It stores the prefab of the flower that will be instantiated
// and an optional sprite that can be shown in the shop UI.
[CreateAssetMenu(fileName = "SeedItem", menuName = "Garden/Seed Item")]
public class SeedItem : ScriptableObject
{
    // The flower prefab that will be instantiated when the seed is bought.
    public GameObject flowerPrefab;

    // Optional sprite for UI representation of the seed.
    public Sprite seedSprite;

    // Price of the seed in whatever currency system you use.
    public int price = 10;
}
