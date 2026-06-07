using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private List<SeedItem> seedItems = new List<SeedItem>();
    [SerializeField] private GameObject seedDragDropPrefab;
    private void Awake()
    {
        if (seedItems == null)
            seedItems = new List<SeedItem>();
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
            // seedDragDrop.seedItem = item; // Передаём данные семени
            seedDragDrop.SetSeedItem(item);
            seedDragDrop.on_mouse_follow(); // Запускаем перетаскивание
        }
    }

    public int SeedCount => seedItems.Count;
    public List<SeedItem> SeedItems => seedItems;
}