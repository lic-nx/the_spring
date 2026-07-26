using UnityEngine;

public class Shovel : DragDrop
{
    // Ссылка на горшок, который подсвечен прямо сейчас
    private Pot currentlyHighlightedPot;
    
    // Флаг для отслеживания состояния перетаскивания (если в базовом классе его нет)
    private bool _isDragging;

    private void OnMouseDown()
    {
        base.OnMouseDown();
        _isDragging = true;
    }

    private void OnMouseUp()
    {
        _isDragging = false;
        
        // 1. Стандартная логика возврата лопатки на место (использует protected поля из DragDrop)
        _collider.enabled = false;
        // Здесь можно добавить проверку Physics2D.OverlapPoint для возврата, если нужно, 
        // но обычно лопатка просто возвращается на базу.
        transform.position = _startDragPosition;
        _collider.enabled = true;

        // 2. Если на момент отпускания мыши у нас был подсвечен горшок с цветком
        if (currentlyHighlightedPot != null)
        {
            if (FlowerRemovalManager.Instance != null)
            {
                FlowerRemovalManager.Instance.ShowConfirmation(currentlyHighlightedPot);
            }
            else
            {
                Debug.LogError("FlowerRemovalManager не найден на сцене!");
            }
            
            // Снимаем подсветку после инициации действия
            ClearHighlight();
        }
    }

    private void Update()
    {
        // Если мы не перетаскиваем лопатку, подсветка не нужна
        if (!_isDragging)
        {
            ClearHighlight();
            return;
        }

        // Получаем позицию мыши в мировых координатах
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Проверяем, есть ли под курсором какой-либо коллайдер
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);
        Pot targetPot = null;

        if (hitCollider != null)
        {
            // Ищем компонент Pot на самом объекте или на его родителе (если кликнули по цветку)
            targetPot = hitCollider.GetComponentInParent<Pot>();
        }

        // Логика переключения подсветки
        if (targetPot != null && targetPot.CurrentFlower != null)
        {
            // Если навели на НОВЫЙ горшок с цветком
            if (targetPot != currentlyHighlightedPot)
            {
                ClearHighlight(); // Снимаем подсветку со старого
                currentlyHighlightedPot = targetPot;
                currentlyHighlightedPot.SetHighlight(true); // Включаем на новом
            }
        }
        else
        {
            // Если курсор ушел с горшка или горшок пустой
            ClearHighlight();
        }
    }

    /// <summary>
    /// Гарантированно снимает подсветку и очищает ссылку
    /// </summary>
    private void ClearHighlight()
    {
        if (currentlyHighlightedPot != null)
        {
            currentlyHighlightedPot.SetHighlight(false);
            currentlyHighlightedPot = null;
        }
    }

    private void OnDisable()
    {
        // На случай, если объект лопатки будет деактивирован во время перетаскивания
        ClearHighlight();
        _isDragging = false;
    }
}