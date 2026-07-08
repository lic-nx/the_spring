using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private List<SeedItem> seedItems = new List<SeedItem>();
    [SerializeField] private GameObject seedDragDropPrefab;
    [SerializeField] private List<Sprite> potItems = new List<Sprite>();
    [SerializeField] private GameObject potDragDropPrefab;

    private void Awake()
    {
        if (seedItems == null)
            seedItems = new List<SeedItem>();

        if (potItems == null)
            potItems = new List<Sprite>();
    }

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

        // Проверяем, хватает ли денег у игрока
        // if (!Player.HasEnoughMoney(item.price))
        // {
        //     Debug.LogError($"Shop: Not enough coins to buy {item.name}.");
        //     return;
        // }

        // Списываем деньги
        // Player.SpendCoins(item.price);
        GameObject seedObj = Instantiate(seedDragDropPrefab, Vector3.zero, Quaternion.identity);

        // Получаем компонент SeedDragDrop и передаём ему данные из SeedItem
        SeedDragDrop seedDragDrop = seedObj.GetComponent<SeedDragDrop>();
        if (seedDragDrop != null)
        {
            seedDragDrop.SetSeedItem(item);
            seedDragDrop.on_mouse_follow(); // Запускаем перетаскивание
        }
    }

    public void PurchasePot(int index)
    {
        if (index < 0 || index >= potItems.Count)
        {
            Debug.LogError($"Shop: Invalid pot index {index}");
            return;
        }

        Sprite item = potItems[index];
        if (item == null)
        {
            Debug.LogError($"Shop: Pot item at index {index} is null.");
            return;
        }

        // Проверяем, хватает ли денег у игрока
        // if (!Player.HasEnoughMoney(item.price))
        // {
        //     Debug.LogError($"Shop: Not enough coins to buy {item.name}.");
        //     return;
        // }

        // Списываем деньги
        // Player.SpendCoins(item.price);
        GameObject potObj = Instantiate(potDragDropPrefab, Vector3.zero, Quaternion.identity);

        // --- ДОБАВЛЕННЫЙ КОД: Подставляем спрайт в SpriteRenderer ---
        SpriteRenderer potSpriteRenderer = potObj.GetComponent<SpriteRenderer>();
        if (potSpriteRenderer != null)
        {
            potSpriteRenderer.sprite = item; // Меняем стандартный спрайт на спрайт из списка potItems
        }
        else
        {
            Debug.LogError("Shop: У префаба горшка отсутствует компонент SpriteRenderer!");
        }
        // -----------------------------------------------------------

        // Получаем компонент Pot и запускаем перетаскивание
        Pot potDragDrop = potObj.GetComponent<Pot>();
        if (potDragDrop != null)
        {
            potDragDrop.on_mouse_follow(); // Запускаем перетаскивание
        }
    }

    public int SeedCount => seedItems.Count;
    public List<SeedItem> SeedItems => seedItems;
    public int PotCount => potItems.Count;
    public List<Sprite> PotItems => potItems;
}