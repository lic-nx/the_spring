using UnityEngine;
using UnityEngine.UI;
using TMPro; // <-- 1. Добавляем пространство имен TextMeshPro

public class ShopItemUI : MonoBehaviour
{
    [Header("UI Элементы (назначаются в префабе)")]
    public Image iconImage;
    
    // <-- 2. Заменяем Text на TMP_Text
    // public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text quantityText;
    
    public Button buyButton;

    private SeedItem _currentSeed;

    public void Setup(SeedItem seed)
    {
        _currentSeed = seed;

        // Заполняем данные
        // nameText.text = seed.name;
        priceText.text = $"{seed.price}";
        
        if (seed.seedSprite != null)
        {
            iconImage.sprite = seed.seedSprite;
        }

        UpdateQuantityText();

        buyButton.onClick.AddListener(OnBuyButtonClicked);
        InventoryManager.Instance.OnItemQuantityChanged += HandleInventoryChange;
    }

    private void HandleInventoryChange(SeedItem changedSeed, int newQuantity)
    {
        if (changedSeed == _currentSeed)
        {
            UpdateQuantityText();
        }
    }

    private void UpdateQuantityText()
    {
        int owned = InventoryManager.Instance.GetQuantity(_currentSeed);
        quantityText.text = $"В инвентаре: {owned}";
    }

    private void OnBuyButtonClicked()
    {
        bool canAfford = true; // Заглушка для проверки валюты

        if (canAfford)
        {
            InventoryManager.Instance.AddItem(_currentSeed, 1);
            Debug.Log($"Успешно куплено: {_currentSeed.name}");
        }
        else
        {
            Debug.Log("Недостаточно средств!");
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnItemQuantityChanged -= HandleInventoryChange;
        }
    }
}