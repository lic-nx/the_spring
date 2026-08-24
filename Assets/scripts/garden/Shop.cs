using System.Collections.Generic;
using UnityEngine;
using YG;

public class Shop : MonoBehaviour
{
    public static Shop Instance { get; private set; }

    [Header("Настройки магазина")]
    public List<SeedItem> availableSeedsForSale;
    
    [Header("Список горшков (Спрайты)")]
    [SerializeField] private List<Sprite> potItems = new List<Sprite>();
    [SerializeField] public GameObject potDragDropPrefab; // Сделал public для доступа из PotZoneArea
    
    [Header("UI ссылки")]
    public GameObject shopItemPrefab;
    public Transform seedShopContainer;
    public Transform potShopContainer;

    // Пулы для переиспользования карточек
    private List<ShopItemUI> _seedCardPool = new List<ShopItemUI>();
    private List<ShopItemUI> _potCardPool = new List<ShopItemUI>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateShopUI();
    }

    /// <summary>
    /// Генерация UI магазина с переиспользованием карточек (пулинг).
    /// </summary>
    private void GenerateShopUI()
    {
        Debug.Log("🔄 [Shop] === ЗАПУСК GenerateShopUI ===");
        
        // 1. Генерация карточек СЕМЯН
        int seedIndex = 0;
        foreach (SeedItem seed in availableSeedsForSale)
        {
            if (seed == null) continue;
            
            if (seedIndex >= _seedCardPool.Count)
            {
                Debug.Log($"🆕 [Shop] Создаём новую карточку семени (индекс {seedIndex})...");
                GameObject newCard = Instantiate(shopItemPrefab, seedShopContainer);
                ShopItemUI cardUI = newCard.GetComponent<ShopItemUI>();
                if (cardUI == null)
                {
                    Debug.LogError($"🔴 [Shop] На префабе отсутствует компонент ShopItemUI!");
                    Destroy(newCard);
                    continue;
                }
                _seedCardPool.Add(cardUI);
            }
            
            _seedCardPool[seedIndex].Setup(seed, () => PurchaseSeed(seed));
            _seedCardPool[seedIndex].gameObject.SetActive(true);
            Debug.Log($"✅ [Shop] Карточка семени {seedIndex}: {seed.name}");
            seedIndex++;
        }
        
        for (int i = seedIndex; i < _seedCardPool.Count; i++)
        {
            if (_seedCardPool[i] != null)
            {
                _seedCardPool[i].gameObject.SetActive(false);
            }
        }

        // 2. Генерация карточек ГОРШКОВ
        int potIndex = 0;
        for (int i = 0; i < potItems.Count; i++)
        {
            Sprite potSprite = potItems[i];
            if (potSprite == null) continue;
            
            if (potIndex >= _potCardPool.Count)
            {
                Debug.Log($"🆕 [Shop] Создаём новую карточку горшка (индекс {potIndex})...");
                GameObject newCard = Instantiate(shopItemPrefab, potShopContainer);
                ShopItemUI cardUI = newCard.GetComponent<ShopItemUI>();
                if (cardUI == null)
                {
                    Debug.LogError($"🔴 [Shop] На префабе отсутствует компонент ShopItemUI!");
                    Destroy(newCard);
                    continue;
                }
                _potCardPool.Add(cardUI);
            }
            
            int capturedIndex = i;
            _potCardPool[potIndex].Setup(potSprite, () => PurchasePot(capturedIndex));
            _potCardPool[potIndex].gameObject.SetActive(true);
            Debug.Log($"✅ [Shop] Карточка горшка {potIndex}: {potSprite.name}");
            potIndex++;
        }
        
        for (int i = potIndex; i < _potCardPool.Count; i++)
        {
            if (_potCardPool[i] != null)
            {
                _potCardPool[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"🏁 [Shop] === ГЕНЕРАЦИЯ ЗАВЕРШЕНА. Семян: {seedIndex}, Горшков: {potIndex} ===\n");
    }

    /// <summary>
    /// Поиск спрайта горшка по его имени для загрузки из сохранений.
    /// </summary>
    public Sprite GetPotSpriteByName(string spriteName)
    {
        return potItems.Find(s => s != null && s.name == spriteName);
    }

    /// <summary>
    /// Логика покупки семени.
    /// </summary>
    private void PurchaseSeed(SeedItem seed)
    {
        if (seed == null)
        {
            Debug.LogError("[Shop] Попытка купить null семя!");
            return;
        }
        
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[Shop] CurrencyManager.Instance равен null!");
            return;
        }
        
        if (!CurrencyManager.Instance.CanAfford(seed.price))
        {
            Debug.LogWarning($"⚠️ [Shop] Недостаточно средств для покупки '{seed.name}'! Нужно: {seed.price}");
            return;
        }
        
        if (!CurrencyManager.Instance.TrySpendCurrency(seed.price))
        {
            Debug.LogError($"[Shop] Не удалось списать {seed.price} солнышек!");
            return;
        }
        
        InventoryManager.Instance.AddItem(seed, 1);
        Debug.Log($"✅ [Shop] Успешно куплено: {seed.name} за {seed.price} солнышек.");
        
        YG2.SaveProgress();
    }

    /// <summary>
    /// Логика покупки/создания горшка.
    /// Закрывает магазин и передаёт управление PotDragManager,
    /// который создаёт UI-призрак горшка на Canvas.
    /// Горшок в мире создаётся только при клике в зону.
    /// </summary>
    public void PurchasePot(int index)
    {
        if (index < 0 || index >= potItems.Count)
        {
            Debug.LogError($"🔴 [Shop] Неверный индекс горшка {index}");
            return;
        }
        Sprite item = potItems[index];
        if (item == null) return;
        
        // Закрываем магазин, чтобы его Canvas не блокировал клики
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        
        // Передаём управление PotDragManager — он создаст UI-призрак
        if (PotDragManager.Instance != null)
        {
            PotDragManager.Instance.StartPotDrag(item, index);
            Debug.Log($"✅ [Shop] UI-призрак горшка '{item.name}' создан. Ожидание размещения в зоне.");
        }
        else
        {
            Debug.LogError("[Shop] PotDragManager.Instance равен null! Убедитесь, что PotDragManager есть на сцене и является дочерним элементом Canvas.");
        }
    }
}