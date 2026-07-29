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
    
    // Пулы для переиспользования карточек
    private List<ShopItemUI> _seedCardPool = new List<ShopItemUI>();
    private List<ShopItemUI> _potCardPool = new List<ShopItemUI>();
    
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
            
            // Если карточки не хватает — создаём ОДИН раз
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
            
            // Обновляем данные в существующей карточке
            _seedCardPool[seedIndex].Setup(seed, () => PurchaseSeed(seed));
            _seedCardPool[seedIndex].gameObject.SetActive(true);
            
            Debug.Log($"✅ [Shop] Карточка семени {seedIndex}: {seed.name}");
            seedIndex++;
        }
        
        // Скрываем лишние карточки семян
        for (int i = seedIndex; i < _seedCardPool.Count; i++)
        {
            if (_seedCardPool[i] != null)
            {
                _seedCardPool[i].gameObject.SetActive(false);
                Debug.Log($"🗑️ [Shop] Скрыта лишняя карточка семени {i}");
            }
        }
        
        // 2. Генерация карточек ГОРШКОВ
        int potIndex = 0;
        for (int i = 0; i < potItems.Count; i++)
        {
            Sprite potSprite = potItems[i];
            if (potSprite == null) continue;
            
            // Если карточки не хватает — создаём ОДИН раз
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
            
            // Обновляем данные в существующей карточке
            int capturedIndex = i;
            _potCardPool[potIndex].Setup(potSprite, () => PurchasePot(capturedIndex));
            _potCardPool[potIndex].gameObject.SetActive(true);
            
            Debug.Log($"✅ [Shop] Карточка горшка {potIndex}: {potSprite.name}");
            potIndex++;
        }
        
        // Скрываем лишние карточки горшков
        for (int i = potIndex; i < _potCardPool.Count; i++)
        {
            if (_potCardPool[i] != null)
            {
                _potCardPool[i].gameObject.SetActive(false);
                Debug.Log($"🗑️ [Shop] Скрыта лишняя карточка горшка {i}");
            }
        }
        
        Debug.Log($"🏁 [Shop] === ГЕНЕРАЦИЯ ЗАВЕРШЕНА. Семян: {seedIndex}, Горшков: {potIndex} ===\n");
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

    // ✅ НОВОЕ: проверяем, хватает ли средств
    if (CurrencyManager.Instance == null)
    {
        Debug.LogError("[Shop] CurrencyManager.Instance равен null!");
        return;
    }

    if (!CurrencyManager.Instance.CanAfford(seed.price))
    {
        Debug.LogWarning($"⚠️ [Shop] Недостаточно средств для покупки '{seed.name}'! Нужно: {seed.price}");
        // TODO: Здесь можно показать UI-уведомление "Недостаточно средств"
        return;
    }

    // ✅ НОВОЕ: списываем средства
    if (!CurrencyManager.Instance.TrySpendCurrency(seed.price))
    {
        Debug.LogError($"[Shop] Не удалось списать {seed.price} солнышек!");
        return;
    }

    // Если всё хорошо — добавляем семя в инвентарь
    InventoryManager.Instance.AddItem(seed, 1);
    Debug.Log($"✅ [Shop] Успешно куплено: {seed.name} за {seed.price} солнышек.");
}
    
    /// <summary>
    /// Логика покупки/создания горшка.
    /// Горшок появляется в позиции курсора и следует за ним до установки в зону.
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
        
        // Создаём горшок в позиции курсора
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
            Debug.LogError("🔴 [Shop] У префаба горшка отсутствует компонент SpriteRenderer!");
        }
        
        Debug.Log($"✅ [Shop] Горшок '{item.name}' создан в позиции курсора.");
    }
}