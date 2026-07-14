using System;
using System.Collections.Generic;
using UnityEngine;

// 👇 ВАЖНО: Оставьте эту строку, чтобы этот скрипт просыпался ПЕРВЫМ
[DefaultExecutionOrder(-100)]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // Событие для мгновенного обновления UI без проверки в Update()
    public event Action<SeedItem, int> OnItemQuantityChanged;
    public event Action OnInventoryRefreshed; // Для полной перерисовки инвентаря

    // Хранилище: Ссылка на ScriptableObject -> Количество
    private Dictionary<SeedItem, int> _inventory = new Dictionary<SeedItem, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("✅ [InventoryManager] Singleton инициализирован. Instance установлен.");
            // DontDestroyOnLoad(gameObject); // Раскомментируйте, если инвентарь должен жить между сценами
        }
        else
        {
            Debug.LogWarning("⚠️ [InventoryManager] Обнаружен дубликат объекта! Уничтожаю лишний.");
            Destroy(gameObject);
        }
    }

    // Добавление предмета (или увеличение количества)
    public void AddItem(SeedItem seed, int amount = 1)
    {
        if (seed == null)
        {
            Debug.LogError("🔴 [InventoryManager] Ошибка: Попытка добавить null предмет!");
            return;
        }
        if (amount <= 0)
        {
            Debug.LogWarning($"⚠️ [InventoryManager] Игнорировано добавление '{seed.name}', так как amount <= 0 ({amount})");
            return;
        }

        Debug.Log($"➕ [InventoryManager] Добавляем '{seed.name}' в количестве {amount} шт.");

        if (_inventory.ContainsKey(seed))
        {
            _inventory[seed] += amount;
            Debug.Log($"🔄 [InventoryManager] Предмет '{seed.name}' уже был в инвентаре. Новое количество: {_inventory[seed]}");
        }
        else
        {
            _inventory[seed] = amount;
            Debug.Log($"🆕 [InventoryManager] Предмет '{seed.name}' добавлен в инвентарь впервые. Количество: {_inventory[seed]}");
        }

        Debug.Log($"📢 [InventoryManager] Отправка событий обновления UI для '{seed.name}'...");
        OnItemQuantityChanged?.Invoke(seed, _inventory[seed]);
        OnInventoryRefreshed?.Invoke();
    }

    // Получение количества (быстрый поиск O(1))
    public int GetQuantity(SeedItem seed)
    {
        if (seed != null && _inventory.TryGetValue(seed, out int quantity))
        {
            // Раскомментируйте строку ниже, если хотите видеть каждый запрос количества (может засорять консоль)
            // Debug.Log($"🔍 [InventoryManager] Запрос количества '{seed.name}': {quantity}");
            return quantity;
        }
        return 0;
    }

    // Получение всех предметов для отрисовки инвентаря
    public IEnumerable<KeyValuePair<SeedItem, int>> GetAllItems()
    {
        Debug.Log($"📋 [InventoryManager] Запрошен полный список предметов. Уникальных типов в словаре: {_inventory.Count}");
        return _inventory;
    }

    // Удаление предмета (или уменьшение количества)
    public void RemoveItem(SeedItem seed, int amount = 1)
    {
        if (seed == null)
        {
            Debug.LogError("🔴 [InventoryManager] Ошибка: Попытка удалить null предмет!");
            return;
        }
        if (amount <= 0)
        {
            Debug.LogWarning($"⚠️ [InventoryManager] Игнорировано удаление '{seed.name}', так как amount <= 0 ({amount})");
            return;
        }

        if (!_inventory.ContainsKey(seed))
        {
            Debug.LogWarning($"⚠️ [InventoryManager] Попытка удалить '{seed.name}', но его НЕТ в инвентаре!");
            return;
        }

        Debug.Log($"➖ [InventoryManager] Удаляем '{seed.name}' в количестве {amount} шт.");
        _inventory[seed] -= amount;

        // Если количество стало 0 или меньше, удаляем ключ из словаря
        if (_inventory[seed] <= 0)
        {
            Debug.Log($"🗑️ [InventoryManager] Количество '{seed.name}' стало 0. Полное удаление из словаря.");
            _inventory.Remove(seed);
        }
        else
        {
            Debug.Log($"🔄 [InventoryManager] Новое количество '{seed.name}': {_inventory[seed]}");
        }

        // Уведомляем UI об изменении (передаем 0, если предмет удален полностью)
        int currentQty = GetQuantity(seed);
        Debug.Log($"📢 [InventoryManager] Отправка событий обновления UI для '{seed.name}' (текущее кол-во: {currentQty})...");
        OnItemQuantityChanged?.Invoke(seed, currentQty);
        OnInventoryRefreshed?.Invoke();
    }
}