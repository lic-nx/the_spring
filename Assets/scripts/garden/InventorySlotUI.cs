using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Элементы")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text quantityText;
    
    private SeedItem _currentSeed;
    private int _currentQuantity;
    
    /// <summary>
    /// Полная настройка слота (используется при первичной отрисовке).
    /// </summary>
    public void Setup(SeedItem seed, int quantity)
    {
        _currentSeed = seed;
        _currentQuantity = quantity;
        
        if (nameText != null) nameText.text = seed.name;
        if (quantityText != null) quantityText.text = $"x{quantity}";
        
        if (iconImage != null && seed.seedSprite != null)
        {
            iconImage.sprite = seed.seedSprite;
        }
    }
    
    /// <summary>
    /// ТОЧЕЧНОЕ обновление: меняем только количество (без пересоздания всего слота).
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        _currentQuantity = newQuantity;
        if (quantityText != null)
        {
            quantityText.text = $"x{newQuantity}";
        }
    }
    
    /// <summary>
    /// Возвращает текущее семя (для поиска слота при точечном обновлении).
    /// </summary>
    public SeedItem GetCurrentSeed()
    {
        return _currentSeed;
    }
    
    // ===== Перетаскивание =====
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentQuantity > 0)
        {
            SeedDragManager.Instance.StartDraggingSeed(_currentSeed, this, eventData);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        SeedDragManager.Instance.UpdateDragPosition(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        SeedDragManager.Instance.EndDraggingSeed(eventData);
    }
}