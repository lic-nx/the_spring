using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventorySlotPrefab;
    public Transform inventoryContainer;

    private void OnEnable()
    {
        Debug.Log("🟢 [InventoryUIManager] Вызван OnEnable. Начинаем инициализацию...");

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("🔴 [InventoryUIManager] КРИТИЧЕСКАЯ ОШИБКА: InventoryManager.Instance равен null! Убедитесь, что объект с этим скриптом есть на сцене.");
            return;
        }

        Debug.Log("🟢 [InventoryUIManager] InventoryManager найден. Подписываемся на событие OnInventoryRefreshed...");
        InventoryManager.Instance.OnInventoryRefreshed += RenderInventory;
        
        Debug.Log("🟢 [InventoryUIManager] Подписка успешна. Запускаем первичную отрисовку инвентаря...");
        RenderInventory();
    }

    private void OnDisable()
    {
        Debug.Log("🟡 [InventoryUIManager] Вызван OnDisable. Отписываемся от событий для предотвращения утечек памяти...");
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryRefreshed -= RenderInventory;
            Debug.Log("🟡 [InventoryUIManager] Отписка успешна.");
        }
    }

    private void RenderInventory()
    {
        Debug.Log("🔄 [InventoryUIManager] === ЗАПУСК МЕТОДА RenderInventory ===");

        // Проверки на забытые ссылки в Инспекторе Unity
        if (inventoryContainer == null)
        {
            Debug.LogError("🔴 [InventoryUIManager] ОШИБКА: Поле 'Inventory Container' не назначено в Инспекторе!");
            return;
        }
        if (inventorySlotPrefab == null)
        {
            Debug.LogError("🔴 [InventoryUIManager] ОШИБКА: Поле 'Inventory Slot Prefab' не назначено в Инспекторе!");
            return;
        }

        // 1. Очистка старых слотов
        int destroyedCount = 0;
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
            destroyedCount++;
        }
        Debug.Log($"🧹 [InventoryUIManager] Очищено старых слотов: {destroyedCount}");

        // 2. Создание новых слотов
        var allItems = InventoryManager.Instance.GetAllItems();
        int createdCount = 0;

        if (allItems == null || !allItems.GetEnumerator().MoveNext())
        {
            Debug.Log("⚪ [InventoryUIManager] Инвентарь пуст. Слоты не создаются.");
            return;
        }

        foreach (var kvp in allItems)
        {
            if (kvp.Value <= 0)
            {
                Debug.Log($"⚪ [InventoryUIManager] Пропуск предмета '{kvp.Key.name}', так как количество <= 0 (Текущее: {kvp.Value})");
                continue;
            }

            Debug.Log($"✅ [InventoryUIManager] Создаем слот для: '{kvp.Key.name}' | Количество: {kvp.Value}");
            
            GameObject newSlot = Instantiate(inventorySlotPrefab, inventoryContainer);
            
            InventorySlotUI slotUI = newSlot.GetComponent<InventorySlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(kvp.Key, kvp.Value);
                createdCount++;
            }
            else
            {
                Debug.LogError($"🔴 [InventoryUIManager] ОШИБКА: На префабе '{inventorySlotPrefab.name}' отсутствует компонент InventorySlotUI!");
            }
        }

        Debug.Log($"🏁 [InventoryUIManager] === ОТРИСОВКА ЗАВЕРШЕНА. Всего создано новых слотов: {createdCount} ===\n");
    }
}