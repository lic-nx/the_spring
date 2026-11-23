using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Элементы (назначаются в префабе)")]
    public Image iconImage;
    public Image stickerImage;      // Наклейка с видом семени на мешке
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text quantityText;
    public Button buyButton;
    public GameObject seedCountBadge; // Фон счётчика семян

    // Имена используются только как запасной вариант для префабов, где поля выше не назначены
    private const string StickerObjectName = "FlowerSticker";
    private const string CountBadgeObjectName = "SeedCountBadge";

    private object _currentItem;
    private Action _onBuyAction;

    /// <summary>
    /// Универсальная настройка карточки. Принимает любой объект и действие при покупке.
    /// </summary>
    public void Setup(object item, Action onBuyCallback)
    {
        Debug.Log($"[ShopItemUI] Вызван Setup. Тип переданного объекта: {item?.GetType().Name ?? "null"}");
        
        if (_currentItem is SeedItem && InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemQuantityChanged -= HandleInventoryChange;

        _currentItem = item;
        _onBuyAction = onBuyCallback;

        // 1. Обработка ИКОНКИ И НАКЛЕЙКИ
        // Для семян спрайт мешка остаётся из префаба, а seedSprite уходит на наклейку.
        Sprite stickerSprite = null;
        bool isSeed = item is SeedItem;

        if (item is SeedItem seed)
        {
            stickerSprite = seed.seedSprite;
            Debug.Log($"[ShopItemUI] Распознан SeedItem. Спрайт наклейки: {(stickerSprite != null ? stickerSprite.name : "null")}");
        }
        else if (item is Sprite sprite)
        {
            if (iconImage != null) iconImage.sprite = sprite;
            Debug.Log($"[ShopItemUI] Распознан Sprite (горшок). Спрайт: {(sprite != null ? sprite.name : "null")}");
        }
        else
        {
            Debug.LogWarning($"[ShopItemUI] Неизвестный тип объекта: {item?.GetType().Name}. Спрайт не будет установлен.");
        }

        if (iconImage != null)
        {
            iconImage.enabled = iconImage.sprite != null;
            Debug.Log($"[ShopItemUI] IconImage обновлён. Включён: {iconImage.enabled}");
        }
        else
        {
            Debug.LogWarning("[ShopItemUI] Ссылка на iconImage не назначена в инспекторе!");
        }

        ApplySticker(stickerSprite, isSeed);
        ApplyCountBadge(isSeed);

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

    /// <summary>
    /// Ставит спрайт семени на наклейку префаба. Размер и положение наклейки задаются в префабе.
    /// </summary>
    private void ApplySticker(Sprite stickerSprite, bool showSticker)
    {
        Image sticker = stickerImage;
        if (sticker == null && iconImage != null)
        {
            Transform stickerTransform = iconImage.transform.Find(StickerObjectName);
            if (stickerTransform != null) sticker = stickerTransform.GetComponent<Image>();
        }

        if (sticker == null)
        {
            if (showSticker)
                Debug.LogWarning("[ShopItemUI] В префабе не найдена наклейка (stickerImage). Спрайт семени не будет показан.");
            return;
        }

        if (showSticker)
        {
            sticker.sprite = stickerSprite;
            sticker.enabled = stickerSprite != null;
        }
        else
        {
            sticker.enabled = false;
        }
    }

    /// <summary>
    /// Бейдж счётчика показываем только у семян; у горшков он скрыт.
    /// </summary>
    private void ApplyCountBadge(bool showBadge)
    {
        GameObject badge = seedCountBadge;
        if (badge == null && quantityText != null && quantityText.transform.parent != null)
        {
            Transform badgeTransform = quantityText.transform.parent.Find(CountBadgeObjectName);
            if (badgeTransform != null) badge = badgeTransform.gameObject;
        }

        if (badge != null) badge.SetActive(showBadge);
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
