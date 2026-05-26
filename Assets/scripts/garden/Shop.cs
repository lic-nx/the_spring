using System.Collections.Generic; // Подключаем generic коллекции
using UnityEngine; // Главное пространство имён Unity

// Simple shop manager for purchasing seed items.
// Attach this script to a GameObject named "Shop" in the scene.
public class Shop : MonoBehaviour // Класс магазина, наследует MonoBehaviour
{
    // List of seed items available for purchase. Populate in the inspector.
    [SerializeField] private List<SeedItem> seedItems = new List<SeedItem>(); // Список доступных семян, задаётся в инспекторе

    // Reference to the player’s inventory or currency manager (you can replace this
    // with your own implementation). For this example we just log the purchase.
    private void Awake()
    {
        // Ensure the list is not null.
        if (seedItems == null) seedItems = new List<SeedItem>(); // Инициализируем список, если он пустой
    }

    // Keep track of instantiated seed objects that have not yet been placed in a pot.
    private readonly List<GameObject> pendingSeeds = new List<GameObject>(); // Список семян, ожидающих посадки

    // Called by UI buttons to buy a specific seed by index.
    public void PurchaseSeed(int index)
    {
        if (index < 0 || index >= seedItems.Count)
        {
            Debug.LogError($"Shop: Invalid seed index {index}"); // Ошибка: неверный индекс семени
            return;
        }

        // Remove any previously purchased seeds that have not been placed yet.
        // This ensures a new purchase replaces the old pending seed.
        foreach (var oldSeed in pendingSeeds)
        {
            if (oldSeed != null)
            {
                Destroy(oldSeed); // Удаляем старый неиспользованный объект семени
            }
        }
        pendingSeeds.Clear(); // Очищаем список ожидающих семян

        SeedItem item = seedItems[index];
        if (item == null)
        {
            Debug.LogError($"Shop: Seed item at index {index} is null."); // Ошибка: объект SeedItem отсутствует
            return;
        }
        if (item.flowerPrefab == null)
        {
            Debug.LogError($"Shop: Seed item '{item.name}' is missing a flower prefab."); // Ошибка: отсутствует префаб цветка
            return;
        }

        // TODO: deduct player currency here.
        Debug.Log($"Purchased seed for {item.price} coins."); // Логируем покупку семени

        // Instantiate a seed sprite that follows the cursor.
        GameObject seedObj = new GameObject("SeedSprite"); // Создаём объект для визуала семени
        var sr = seedObj.AddComponent<SpriteRenderer>(); // Добавляем компонент SpriteRenderer
        sr.sprite = item.seedSprite; // Assign seed sprite. // Присваиваем спрайт семени
        // Add SeedFollower and assign the flower prefab to it.
        var follower = seedObj.AddComponent<SeedFollower>(); // Добавляем скрипт следования за курсором
        follower.FlowerPrefab = item.flowerPrefab; // We'll expose a property. // Устанавливаем префаб цветка
        // Track this instance as a pending seed.
        pendingSeeds.Add(seedObj); // Добавляем в список ожидающих семян
    }

    // Helper to get the number of available seeds (useful for UI).
    public int SeedCount => seedItems.Count; // Свойство: количество доступных семян

    // Optional: expose the list for UI to display sprites.
    public List<SeedItem> SeedItems => seedItems; // Свойство: список семян для UI
}