using System;
using UnityEngine;

/// <summary>
/// Менеджер валюты (солнышек). 
/// Отвечает за начисление, списание и уведомление UI.
/// </summary>
[DefaultExecutionOrder(-50)] // Инициализируется рано, но после InventoryManager
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Начальные настройки")]
    [Tooltip("Количество солнышек при старте новой игры")]
    [SerializeField] private int startingCurrency = 50;

    /// <summary>
    /// Событие: количество валюты изменилось.
    /// Подписывай UI-счётчик солнышек на это событие.
    /// </summary>
    public event Action<int> OnCurrencyChanged;

    private int _currentCurrency;

    /// <summary>
    /// Текущее количество валюты (только для чтения).
    /// </summary>
    public int CurrentCurrency => _currentCurrency;

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

        // Инициализируем начальную валюту
        _currentCurrency = startingCurrency;
        Debug.Log($"💰 [CurrencyManager] Стартовая валюта: {_currentCurrency}");
    }

    private void Start()
    {
        // Уведомляем всех подписчиков о начальном значении
        OnCurrencyChanged?.Invoke(_currentCurrency);
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

        _currentCurrency += amount;
        Debug.Log($"💰 [CurrencyManager] +{amount} солнышек. Итого: {_currentCurrency}");
        OnCurrencyChanged?.Invoke(_currentCurrency);
    }

    /// <summary>
    /// Проверить, хватает ли валюты для покупки.
    /// </summary>
    public bool CanAfford(int amount)
    {
        return _currentCurrency >= amount;
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
            Debug.LogWarning($"⚠️ [CurrencyManager] Недостаточно средств! Нужно: {amount}, есть: {_currentCurrency}");
            return false;
        }

        _currentCurrency -= amount;
        Debug.Log($"💰 [CurrencyManager] -{amount} солнышек. Итого: {_currentCurrency}");
        OnCurrencyChanged?.Invoke(_currentCurrency);
        return true;
    }

    /// <summary>
    /// Принудительно установить значение (для загрузок сохранения).
    /// </summary>
    public void SetCurrency(int amount)
    {
        _currentCurrency = Mathf.Max(0, amount);
        Debug.Log($"💰 [CurrencyManager] Установлено значение: {_currentCurrency}");
        OnCurrencyChanged?.Invoke(_currentCurrency);
    }
}