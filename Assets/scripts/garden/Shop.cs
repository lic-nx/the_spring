using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("Настройки магазина")]
    public List<SeedItem> availableSeedsForSale; 

    [Header("Список горшков (Спрайты)")]
    [SerializeField] private List<Sprite> potItems = new List<Sprite>();
    [SerializeField] private GameObject potDragDropPrefab;

    [Header("UI ссылки")]
    public GameObject shopItemPrefab; 
    public Transform seedShopContainer;   
    public Transform potShopContainer;   

    private void Start()
    {
        GenerateShopUI();
    }

    private void GenerateShopUI()
    {
        // Очищаем контейнеры
        ClearContainer(seedShopContainer);
        ClearContainer(potShopContainer);

        // 1. Генерация карточек СЕМЯН
        foreach (SeedItem seed in availableSeedsForSale)
        {
            if (seed == null) continue;

            GameObject newItem = Instantiate(shopItemPrefab, seedShopContainer);
            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            
            if (itemUI != null)
            {
                // Передаем объект SeedItem и конкретное действие для семян
                itemUI.Setup(seed, () => PurchaseSeed(seed));
            }
        }

        // 2. Генерация карточек ГОРШКОВ
        for (int i = 0; i < potItems.Count; i++)
        {
            Sprite potSprite = potItems[i];
            if (potSprite == null) continue;

            GameObject newItem = Instantiate(shopItemPrefab, potShopContainer);
            ShopItemUI itemUI = newItem.GetComponent<ShopItemUI>();
            
            if (itemUI != null)
            {
                // Важно: сохраняем индекс в локальную переменную для корректной работы замыкания (closure)
                int capturedIndex = i; 
                
                // Передаем объект Sprite и конкретное действие для горшков
                itemUI.Setup(potSprite, () => PurchasePot(capturedIndex));
            }
        }
    }

    private void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // Логика покупки семени
    private void PurchaseSeed(SeedItem seed)
    {
        bool canAfford = true; // Заглушка для проверки валюты

        if (canAfford)
        {
            InventoryManager.Instance.AddItem(seed, 1);
            Debug.Log($"Успешно куплено: {seed.name}");
        }
        else
        {
            Debug.Log("Недостаточно средств!");
        }
    }

    // Логика покупки/создания горшка
    public void PurchasePot(int index)
    {
        if (index < 0 || index >= potItems.Count)
        {
            Debug.LogError($"Shop: Неверный индекс горшка {index}");
            return;
        }

        Sprite item = potItems[index];
        if (item == null) return;

        // Создаем горшок сразу в позиции курсора, чтобы избежать визуального скачка из Vector3.zero
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f; 

        GameObject potObj = Instantiate(potDragDropPrefab, mouseWorldPos, Quaternion.identity);

        // Подставляем спрайт
        SpriteRenderer potSpriteRenderer = potObj.GetComponent<SpriteRenderer>();
        if (potSpriteRenderer != null)
        {
            potSpriteRenderer.sprite = item;
        }
        else
        {
            Debug.LogError("Shop: У префаба горшка отсутствует компонент SpriteRenderer!");
        }

        // Примечание: Поскольку ваш Pot наследуется от DragDrop, 
        // он должен автоматически подхватывать перетаскивание через OnMouseDown в базовом классе.
        // Если в вашем DragDrop есть специальный метод для начала перетаскивания (например, StartDrag()), 
        // раскомментируйте строку ниже и вызовите его:
        // potObj.GetComponent<Pot>()?.StartDrag();
    }
}
// {
//     [SerializeField] private List<SeedItem> seedItems = new List<SeedItem>();
//     [SerializeField] private GameObject seedDragDropPrefab;
//     [SerializeField] private List<Sprite> potItems = new List<Sprite>();
//     [SerializeField] private GameObject potDragDropPrefab;

//     private void Awake()
//     {
//         if (seedItems == null)
//             seedItems = new List<SeedItem>();

//         if (potItems == null)
//             potItems = new List<Sprite>();
//     }

//     public void PurchaseSeed(int index)
//     {
//         if (index < 0 || index >= seedItems.Count)
//         {
//             Debug.LogError($"Shop: Invalid seed index {index}");
//             return;
//         }

//         SeedItem item = seedItems[index];
//         if (item == null)
//         {
//             Debug.LogError($"Shop: Seed item at index {index} is null.");
//             return;
//         }

//         if (item.flowerPrefab == null)
//         {
//             Debug.LogError($"Shop: Seed item '{item.name}' is missing a flower prefab.");
//             return;
//         }

//         // Проверяем, хватает ли денег у игрока
//         // if (!Player.HasEnoughMoney(item.price))
//         // {
//         //     Debug.LogError($"Shop: Not enough coins to buy {item.name}.");
//         //     return;
//         // }

//         // Списываем деньги
//         // Player.SpendCoins(item.price);
//         GameObject seedObj = Instantiate(seedDragDropPrefab, Vector3.zero, Quaternion.identity);

//         // Получаем компонент SeedDragDrop и передаём ему данные из SeedItem
//         SeedDragDrop seedDragDrop = seedObj.GetComponent<SeedDragDrop>();
//         if (seedDragDrop != null)
//         {
//             seedDragDrop.SetSeedItem(item);
//             seedDragDrop.on_mouse_follow(); // Запускаем перетаскивание
//         }
//     }

//     public void PurchasePot(int index)
//     {
//         if (index < 0 || index >= potItems.Count)
//         {
//             Debug.LogError($"Shop: Invalid pot index {index}");
//             return;
//         }

//         Sprite item = potItems[index];
//         if (item == null)
//         {
//             Debug.LogError($"Shop: Pot item at index {index} is null.");
//             return;
//         }

//         // Проверяем, хватает ли денег у игрока
//         // if (!Player.HasEnoughMoney(item.price))
//         // {
//         //     Debug.LogError($"Shop: Not enough coins to buy {item.name}.");
//         //     return;
//         // }

//         // Списываем деньги
//         // Player.SpendCoins(item.price);
//         GameObject potObj = Instantiate(potDragDropPrefab, Vector3.zero, Quaternion.identity);

//         // --- ДОБАВЛЕННЫЙ КОД: Подставляем спрайт в SpriteRenderer ---
//         SpriteRenderer potSpriteRenderer = potObj.GetComponent<SpriteRenderer>();
//         if (potSpriteRenderer != null)
//         {
//             potSpriteRenderer.sprite = item; // Меняем стандартный спрайт на спрайт из списка potItems
//         }
//         else
//         {
//             Debug.LogError("Shop: У префаба горшка отсутствует компонент SpriteRenderer!");
//         }
//         // -----------------------------------------------------------

//         // Получаем компонент Pot и запускаем перетаскивание
//         Pot potDragDrop = potObj.GetComponent<Pot>();
//         if (potDragDrop != null)
//         {
//             potDragDrop.on_mouse_follow(); // Запускаем перетаскивание
//         }
//     }

//     public int SeedCount => seedItems.Count;
//     public List<SeedItem> SeedItems => seedItems;
//     public int PotCount => potItems.Count;
//     public List<Sprite> PotItems => potItems;
// } 

