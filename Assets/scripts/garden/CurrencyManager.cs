using System;
using UnityEngine;
using YG;

/// <summary>
/// Менеджер валюты (солнышек). 
/// Отвечает за начисление, списание и уведомление UI.
/// </summary>
[DefaultExecutionOrder(-50)] // Инициализируется рано, но после InventoryManager
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }


    /// <summary>
    /// Событие: количество валюты изменилось.
    /// Подписывай UI-счётчик солнышек на это событие.
    /// </summary>
    public event Action<int> OnCurrencyChanged;


    /// <summary>
    /// Текущее количество валюты (только для чтения).
    /// </summary>

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Раскомментируй, если нужна жизнь между сценами
            Debug.Log("✅ [CurrencyManager] Singleton инициализирован.");
        }
        else
        {
            Debug.LogWarning("⚠️ [CurrencyManager] Обнаружен дубликат! Уничтожаю лишний.");
            Destroy(gameObject);
            return;
        }

        Debug.Log($"💰 [CurrencyManager] Стартовая валюта: {YG2.saves.Coins}");
    }

    private void Start()
    {
        // Уведомляем всех подписчиков о начальном значении
        OnCurrencyChanged?.Invoke(YG2.saves.Coins);
    }

    /// <summary>
    /// Начислить валюту (например, при сборе солнышка).
    /// </summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"⚠️ [CurrencyManager] Попытка начислить некорректное количество: {amount}");
            return;
        }

        YG2.saves.Coins += amount;
        YG2.SaveProgress();
        Debug.Log($"💰 [CurrencyManager] +{amount} солнышек. Итого: {YG2.saves.Coins}");
        OnCurrencyChanged?.Invoke(YG2.saves.Coins);
    }

    /// <summary>
    /// Проверить, хватает ли валюты для покупки.
    /// </summary>
    public bool CanAfford(int amount)
    {
        return YG2.saves.Coins >= amount;
    }

    /// <summary>
    /// Попытаться списать валюту. Возвращает true, если успешно.
    /// </summary>
    public bool TrySpendCurrency(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"⚠️ [CurrencyManager] Попытка списать некорректное количество: {amount}");
            return false;
        }

        if (!CanAfford(amount))
        {
            Debug.LogWarning($"⚠️ [CurrencyManager] Недостаточно средств! Нужно: {amount}, есть: {YG2.saves.Coins}");
            return false;
        }

        YG2.saves.Coins -= amount;
        YG2.SaveProgress();
        Debug.Log($"💰 [CurrencyManager] -{amount} солнышек. Итого: {YG2.saves.Coins}");
        OnCurrencyChanged?.Invoke(YG2.saves.Coins);
        return true;
    }

    /// <summary>
    /// Принудительно установить значение (для загрузок сохранения).
    /// </summary>
    // public void SetCurrency(int amount)
    // {
    //     YG2.saves.Coins = Mathf.Max(0, amount);
    //     YG2.SaveProgress();
    //     Debug.Log($"💰 [CurrencyManager] Установлено значение: {YG2.saves.Coins}");
    //     OnCurrencyChanged?.Invoke(YG2.saves.Coins);
    // }
}