using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // <-- Добавляем для работы с событиями мыши

// Добавляем интерфейсы IPointerDownHandler и IPointerUpHandler
public class InventorySlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("UI Элементы")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text quantityText;

    private SeedItem _currentSeed;
    private int _currentQuantity;

    public void Setup(SeedItem seed, int quantity)
    {
        _currentSeed = seed;
        _currentQuantity = quantity;

        nameText.text = seed.name;
        quantityText.text = $"x{quantity}";
        
        if (seed.seedSprite != null)
        {
            iconImage.sprite = seed.seedSprite;
        }
    }

    // Вызывается, когда игрок нажимает левую кнопку мыши на этом слоте
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (_currentQuantity > 0)
            {
                // Запускаем процесс перетаскивания через менеджер
                SeedDragManager.Instance.StartDraggingSeed(_currentSeed, this);
            }
        }
    }

    // Вызывается, когда игрок отпускает кнопку мыши
    public void OnPointerUp(PointerEventData eventData)
    {
        // Здесь можно добавить логику, если нужно, но пока SeedDragManager 
        // сам уничтожит объект при клике в мире или при правом клике.
    }
}