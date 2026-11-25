using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Элементы (назначаются в префабе)")]
    public Image iconImage;
    public GameObject stickerImage;      // Наклейка с видом семени на мешке
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text quantityText;
    public Button buyButton;
    public GameObject seedCountBadge; // Фон счётчика семян

    // Имена используются только как запасной вариант для префабов, где поля выше не назначены
    private const string StickerObjectName = "FlowerSticker";
    private const string CountBadgeObjectName = "CountImage";

    private object _currentItem;
    private Action _onBuyAction;

    /// <summary>
    /// Универсальная настройка карточки. Принимает любой объект и действие при покупке.
    /// </summary>
    public void Setup(object item, Action onBuyCallback)
    {
        Debug.Log($"[ShopItemUI] Вызван Setup. Тип переданного объекта: {item?.GetType().Name ?? "null"}");
        
        if (_currentItem is SeedItem && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemQuantityChanged -= HandleInventoryChange;
            InventoryManager.Instance.OnInventoryRefreshed -= HandleInventoryRefreshed;
        }

        _currentItem = item;
        _onBuyAction = onBuyCallback;

        // 1. Обработка ИКОНКИ И НАКЛЕЙКИ
        // Для семян спрайт мешка остаётся из префаба, а наклейка берётся из переданного SeedItem.seedSprite.
        bool isSeed = item is SeedItem;

        if (item is SeedItem seed)
        {
            Debug.Log($"[ShopItemUI] Распознан SeedItem. Спрайт наклейки: {(seed.seedSprite != null ? seed.seedSprite.name : "null")}");
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

        ApplySticker(item, isSeed);

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
        if (item is SeedItem seedQty)
        {
            if (quantityText != null)
            {
                quantityText.enabled = true;
                Debug.Log("[ShopItemUI] QuantityText включён.");
            }

            if (InventoryManager.Instance != null)
            {
                // OnItemQuantityChanged — при покупке/трате,
                // OnInventoryRefreshed — после загрузки сохранения (GameSaveManager).
                InventoryManager.Instance.OnItemQuantityChanged += HandleInventoryChange;
                InventoryManager.Instance.OnInventoryRefreshed += HandleInventoryRefreshed;
                Debug.Log("[ShopItemUI] Подписка на события инвентаря выполнена.");
                UpdateQuantityText(seedQty);
            }
            else
            {
                Debug.LogWarning("[ShopItemUI] InventoryManager.Instance равен null. Количество будет обновлено позже.");
                UpdateCountBadge(false);
            }
        }
        else
        {
            // Если это горшок, скрываем и текст, и бейдж счётчика.
            if (quantityText != null)
            {
                quantityText.text = "";
                quantityText.enabled = false;
                Debug.Log($"[ShopItemUI] Объект не является SeedItem. QuantityText скрыт и очищен.");
            }
            UpdateCountBadge(false);
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
    /// Ставит на мешок наклейку с спрайтом из переданного SeedItem.seedSprite.
    /// </summary>
    private void ApplySticker(object item, bool showSticker)
    {
        Sprite stickerSprite = (item as SeedItem)?.seedSprite;

        Image sticker = stickerImage != null ? stickerImage.GetComponent<Image>() : null;
        if (sticker == null && iconImage != null)
        {
            Transform stickerTransform = iconImage.transform.Find(StickerObjectName);
            if (stickerTransform != null) sticker = stickerTransform.GetComponent<Image>();
        }

        // Если в префабе наклейки нет, создаём её прямо на мешке.
        if (sticker == null && showSticker && iconImage != null)
        {
            GameObject stickerObject = new GameObject(StickerObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            stickerObject.layer = iconImage.gameObject.layer;
            stickerObject.transform.SetParent(iconImage.transform, false);

            RectTransform stickerRect = stickerObject.GetComponent<RectTransform>();
            stickerRect.anchorMin = new Vector2(0.27f, 0.34f);
            stickerRect.anchorMax = new Vector2(0.73f, 0.70f);
            stickerRect.offsetMin = stickerRect.offsetMax = Vector2.zero;

            sticker = stickerObject.GetComponent<Image>();
            sticker.preserveAspect = true;
            sticker.raycastTarget = false;
        }

        if (sticker == null)
        {
            if (showSticker)
                Debug.LogWarning("[ShopItemUI] Не удалось создать наклейку: спрайт семени не будет показан.");
            return;
        }

        if (showSticker)
        {
            sticker.sprite = stickerSprite;
            sticker.enabled = stickerSprite != null;
            sticker.gameObject.SetActive(stickerSprite != null);
            Debug.Log($"[ShopItemUI] Наклейка на мешке: {(stickerSprite != null ? stickerSprite.name : "null")}");
        }
        else
        {
            sticker.enabled = false;
            sticker.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Находит фон счётчика в префабе seed (поле инспектора может быть не назначено).
    /// </summary>
    private GameObject ResolveCountBadge()
    {
        if (seedCountBadge != null) return seedCountBadge;

        // В префабе CountImage является родителем quantityText.
        Transform parent = quantityText != null ? quantityText.transform.parent : null;
        if (parent != null && parent.name == CountBadgeObjectName)
            return parent.gameObject;

        Transform found = transform.Find(CountBadgeObjectName);
        return found != null ? found.gameObject : null;
    }

    /// <summary>
    /// Бейдж счётчика показываем только у семян, и только когда их больше 0.
    /// </summary>
    private void UpdateCountBadge(bool showBadge)
    {
        GameObject badge = ResolveCountBadge();
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

    private void HandleInventoryRefreshed()
    {
        // GameSaveManager при загрузке вызывает ApplyInventory(), который не шлёт
        // OnItemQuantityChanged, но шлёт OnInventoryRefreshed. Поэтому обновляемся здесь.
        if (_currentItem is SeedItem seed)
        {
            Debug.Log($"[ShopItemUI] Событие OnInventoryRefreshed. Обновляем {seed.name}.");
            UpdateQuantityText(seed);
        }
    }

    private void UpdateQuantityText(SeedItem seed)
    {
        if (seed == null || InventoryManager.Instance == null) return;

        int owned = InventoryManager.Instance.GetQuantity(seed);
        if (quantityText != null)
        {
            quantityText.text = $"{owned}";
            Debug.Log($"[ShopItemUI] QuantityText обновлён до: {owned} для {seed.name}");
        }

        // Бейдж виден только если семян действительно больше 0.
        UpdateCountBadge(owned > 0);
    }

    private void OnDestroy()
    {
        Debug.Log($"[ShopItemUI] OnDestroy вызван для объекта: {gameObject.name}");
        
        // Отписываемся от событий только если это был SeedItem
        if (_currentItem is SeedItem seed && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemQuantityChanged -= HandleInventoryChange;
            InventoryManager.Instance.OnInventoryRefreshed -= HandleInventoryRefreshed;
            Debug.Log($"[ShopItemUI] Отписка от событий инвентаря выполнена для {seed.name}");
        }
    }
}
