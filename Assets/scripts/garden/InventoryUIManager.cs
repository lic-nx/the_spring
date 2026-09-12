using UnityEngine;
using System.Collections.Generic;

public class InventoryUIManager : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject inventorySlotPrefab;
    public Transform inventoryContainer;
    
    // Пул активных слотов (переиспользуем их вместо создания новых)
    private List<InventorySlotUI> _activeSlots = new List<InventorySlotUI>();
    
    private void OnEnable()
    {
        Debug.Log("🟢 [InventoryUIManager] Инициализация...");
        
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("🔴 [InventoryUIManager] InventoryManager.Instance равен null!");
            return;
        }
        
        // Подписываемся на точечное обновление (без полной перерисовки)
        InventoryManager.Instance.OnItemQuantityChanged += UpdateSlotQuantity;
        
        // Подписываемся на полную перерисовку (только при первом запуске)
        InventoryManager.Instance.OnInventoryRefreshed += RenderInventory;
        
        Debug.Log("🟢 [InventoryUIManager] Подписка на события успешна. Запускаем первичную отрисовку...");
        RenderInventory();
    }
    
    private void OnDisable()
    {
        Debug.Log("🟡 [InventoryUIManager] Отписка от событий...");
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemQuantityChanged -= UpdateSlotQuantity;
            InventoryManager.Instance.OnInventoryRefreshed -= RenderInventory;
        }
    }
    
    /// <summary>
    /// ТОЧЕЧНОЕ обновление: меняем только количество в нужном слоте, не пересоздавая всё.
    /// </summary>
    private void UpdateSlotQuantity(SeedItem seed, int newQuantity)
    {
        Debug.Log($"🔄 [InventoryUIManager] Точечное обновление: {seed.name} → {newQuantity}");
        
        // Ищем слот с этим семенем
        foreach (var slot in _activeSlots)
        {
            if (slot != null && slot.GetCurrentSeed() == seed)
            {
                if (newQuantity > 0)
                {
                    // Обновляем количество
                    slot.UpdateQuantity(newQuantity);
                    Debug.Log($"✅ [InventoryUIManager] Слот обновлён: {seed.name} x{newQuantity}");
                }
                else
                {
                    // Количество стало 0 — скрываем слот
                    slot.gameObject.SetActive(false);
                    Debug.Log($"🗑️ [InventoryUIManager] Слот скрыт: {seed.name} (количество = 0)");
                }
                return;
            }
        }
        
        // Если слот не найден, но количество > 0 — значит, это новый предмет
        if (newQuantity > 0)
        {
            Debug.Log($"🆕 [InventoryUIManager] Новый предмет: {seed.name}. Запускаем полную перерисовку...");
            RenderInventory();
        }
    }
    
    /// <summary>
    /// Полная перерисовка инвентаря (используется только при первом запуске или добавлении нового предмета).
    /// НЕ уничтожает слоты, а переиспользует существующие.
    /// </summary>
    private void RenderInventory()
    {
        Debug.Log("🔄 [InventoryUIManager] === ЗАПУСК RenderInventory ===");
        
        if (inventoryContainer == null || inventorySlotPrefab == null)
        {
            Debug.LogError("🔴 [InventoryUIManager] Не назначены поля в инспекторе!");
            return;
        }
        
        var allItems = InventoryManager.Instance.GetAllItems();
        int index = 0;
        
        // Перебираем все предметы в инвентаре
        foreach (var kvp in allItems)
        {
            if (kvp.Value <= 0) continue;
            
            // Если слота не хватает — создаём ОДИН раз и сохраняем в пул
            if (index >= _activeSlots.Count)
            {
                Debug.Log($"🆕 [InventoryUIManager] Создаём новый слот (индекс {index})...");
                GameObject newSlotObj = Instantiate(inventorySlotPrefab, inventoryContainer);
                InventorySlotUI newSlot = newSlotObj.GetComponent<InventorySlotUI>();
                
                if (newSlot == null)
                {
                    Debug.LogError($"🔴 [InventoryUIManager] На префабе отсутствует компонент InventorySlotUI!");
                    Destroy(newSlotObj);
                    continue;
                }
                
                _activeSlots.Add(newSlot);
            }
            
            // Обновляем данные в существующем слоте (без уничтожения!)
            _activeSlots[index].Setup(kvp.Key, kvp.Value);
            _activeSlots[index].gameObject.SetActive(true);
            
            Debug.Log($"✅ [InventoryUIManager] Слот {index}: {kvp.Key.name} x{kvp.Value}");
            index++;
        }
        
        // Скрываем лишние слоты, если предметов стало меньше
        for (int i = index; i < _activeSlots.Count; i++)
        {
            if (_activeSlots[i] != null)
            {
                _activeSlots[i].gameObject.SetActive(false);
                Debug.Log($"🗑️ [InventoryUIManager] Скрыт лишний слот {i}");
            }
        }
        
        Debug.Log($"🏁 [InventoryUIManager] === ОТРИСОВКА ЗАВЕРШЕНА. Активных слотов: {index} ===\n");
    }
}