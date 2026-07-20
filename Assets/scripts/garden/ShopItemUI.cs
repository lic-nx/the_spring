using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Элементы (назначаются в префабе)")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text quantityText;
    public Button buyButton;

    private object _currentItem;
    private Action _onBuyAction;

    /// <summary>
    /// Универсальная настройка карточки. Принимает любой объект и действие при покупке.
    /// </summary>
    public void Setup(object item, Action onBuyCallback)
    {
        Debug.Log($"[ShopItemUI] Вызван Setup. Тип переданного объекта: {item?.GetType().Name ?? "null"}");
        
        _currentItem = item;
        _onBuyAction = onBuyCallback;

        // 1. Обработка ИКОНКИ (Спрайта)
        Sprite targetSprite = null;
        if (item is SeedItem seed) 
        {
            targetSprite = seed.seedSprite;
            Debug.Log($"[ShopItemUI] Распознан SeedItem. Спрайт: {(targetSprite != null ? targetSprite.name : "null")}");
        }
        else if (item is Sprite sprite) 
        {
            targetSprite = sprite;
            Debug.Log($"[ShopItemUI] Распознан Sprite (горшок). Спрайт: {(targetSprite != null ? targetSprite.name : "null")}");
        }
        else
        {
            Debug.LogWarning($"[ShopItemUI] Неизвестный тип объекта: {item?.GetType().Name}. Спрайт не будет установлен.");
        }

        if (iconImage != null)
        {
            iconImage.sprite = targetSprite;
            iconImage.enabled = (targetSprite != null);
            Debug.Log($"[ShopItemUI] IconImage обновлён. Включён: {iconImage.enabled}");
        }
        else
        {
            Debug.LogWarning("[ShopItemUI] Ссылка на iconImage не назначена в инспекторе!");
        }

        // 2. Обработка ИМЕНИ
        string itemName = "Неизвестно";
        if (item is SeedItem seedName) itemName = seedName.name;
        else if (item is Sprite spriteName) itemName = spriteName.name;

        if (nameText != null)
        {
            nameText.text = itemName;
            nameText.enabled = !string.IsNullOrEmpty(itemName);
            Debug.Log($"[ShopItemUI] NameText обновлён: '{itemName}', Включён: {nameText.enabled}");
        }

        // 3. Обработка ЦЕНЫ
        int itemPrice = 0;
        if (item is SeedItem seedPrice) 
        {
            itemPrice = seedPrice.price;
        }
        // Если это горшок (Sprite), цена остаётся 0 (или можно добавить логику для цены горшка)

        if (priceText != null)
        {
            priceText.text = itemPrice > 0 ? $"{itemPrice}" : "Бесплатно";
            priceText.enabled = true;
            Debug.Log($"[ShopItemUI] PriceText обновлён: '{priceText.text}'");
        }

        // 4. Обработка КОЛИЧЕСТВА (Есть только у семян в инвентаре)
        if (quantityText != null)
        {
            if (item is SeedItem seedQty)
            {
                quantityText.enabled = true;
                InventoryManager.Instance.OnItemQuantityChanged += HandleInventoryChange;
                UpdateQuantityText(seedQty);
                Debug.Log($"[ShopItemUI] QuantityText включён. Подписка на OnItemQuantityChanged выполнена.");
            }
            else
            {
                // Если параметра нет (например, это горшок), зануляем и скрываем
                quantityText.text = "";
                quantityText.enabled = false;
                Debug.Log($"[ShopItemUI] Объект не является SeedItem. QuantityText скрыт и очищен.");
            }
        }

        // 5. Назначение действия покупки
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners(); // Защита от дублирования при перезапуске
            buyButton.onClick.AddListener(() => 
            {
                Debug.Log($"[ShopItemUI] Нажата кнопка покупки для: {itemName}");
                _onBuyAction?.Invoke();
            });
            Debug.Log("[ShopItemUI] Слушатель события onClick добавлен на кнопку покупки.");
        }
        else
        {
            Debug.LogWarning("[ShopItemUI] Ссылка на buyButton не назначена в инспекторе!");
        }
        
        Debug.Log($"[ShopItemUI] Setup завершён успешно для: {itemName}");
    }

    private void HandleInventoryChange(SeedItem changedSeed, int newQuantity)
    {
        Debug.Log($"[ShopItemUI] Событие OnItemQuantityChanged: изменён {changedSeed.name}, новое кол-во: {newQuantity}");
        
        if (_currentItem is SeedItem currentSeed && changedSeed == currentSeed)
        {
            Debug.Log($"[ShopItemUI] Изменение касается текущей карточки. Обновляем текст.");
            UpdateQuantityText(currentSeed);
        }
    }

    private void UpdateQuantityText(SeedItem seed)
    {
        if (quantityText != null)
        {
            int owned = InventoryManager.Instance.GetQuantity(seed);
            quantityText.text = $"{owned}";
            Debug.Log($"[ShopItemUI] QuantityText обновлён до: {owned} для {seed.name}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[ShopItemUI] OnDestroy вызван для объекта: {gameObject.name}");
        
        // Отписываемся от событий только если это был SeedItem
        if (_currentItem is SeedItem seed && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemQuantityChanged -= HandleInventoryChange;
            Debug.Log($"[ShopItemUI] Отписка от OnItemQuantityChanged выполнена для {seed.name}");
        }
    }
}