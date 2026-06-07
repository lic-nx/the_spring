using UnityEngine;

// ScriptableObject representing a purchasable seed.
// It stores the prefab of the flower that will be instantiated
// and an optional sprite that can be shown in the shop UI.
[CreateAssetMenu(fileName = "SeedItem", menuName = "Garden/Seed Item")]
public class SeedItem : ScriptableObject
{
    public string name = "Ромашка";
    // The flower prefab that will be instantiated when the seed is bought.
    public GameObject flowerPrefab;// Префаб цветка, который будет создан при покупке семени

    // Optional sprite for UI representation of the seed.
    public Sprite seedSprite;// Спрайт, отображаемый в UI магазина для представления семени (опционально)

    // Price of the seed in whatever currency system you use.
    public int price = 10; // Стоимость семени в текущей валютной системе (по умолчанию 10)

}
