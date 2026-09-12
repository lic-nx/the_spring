using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Скрипт лопатки. При перетаскивании подсвечивает горшки с цветами под курсором.
/// При отпускании на подсвеченный горшок открывает окно подтверждения удаления.
/// 
/// ВАЖНО: Лопатка ВСЕГДА возвращается на исходную позицию, 
/// так как это инструмент, а не размещаемый объект.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Shovel : WorldDraggable
{
    [Header("Настройки лопатки")]
    [Tooltip("Слой, на котором находятся горшки (для поиска при подсветке)")]
    [SerializeField] private LayerMask potLayerMask;

    // Ссылка на горшок, который подсвечен прямо сейчас
    private Pot _currentlyHighlightedPot;

    // Исходная позиция лопатки на сцене
    private Vector3 _startPosition;

    private void Awake()
    {
        // ⚠️ ВАЖНО: вызываем Awake базового класса
        base.Awake();

        // Если слой не задан в инспекторе — берём стандартный "pot"
        if (potLayerMask.value == 0)
        {
            potLayerMask = LayerMask.GetMask("pot");
        }
    }

    private void Start()
    {
        // Сохраняем исходную позицию лопатки (её "дом")
        _startPosition = transform.position;
    }

    // ===== Переопределение методов перетаскивания =====

    /// <summary>
    /// Начало перетаскивания.
    /// </summary>
    protected override void OnDragStarted()
    {
        // Ничего особенного не делаем
    }

    /// <summary>
    /// КЛЮЧЕВОЕ ПЕРЕОПРЕДЕЛЕНИЕ:
    /// Каждый кадр во время перетаскивания проверяем, есть ли под курсором горшок с цветком.
    /// Если есть — подсвечиваем его.
    /// </summary>
    protected override void OnDragging()
    {
        // Получаем текущую позицию лопатки (она уже обновлена в базовом классе)
        Vector2 shovelWorldPos = transform.position;

        // Временно отключаем свой коллайдер, чтобы не попасть в себя
        _collider.enabled = false;

        // Ищем коллайдер под лопаткой
        Collider2D hitCollider = Physics2D.OverlapPoint(shovelWorldPos, potLayerMask);

        _collider.enabled = true;

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
            if (targetPot != _currentlyHighlightedPot)
            {
                ClearHighlight(); // Снимаем подсветку со старого
                _currentlyHighlightedPot = targetPot;
                _currentlyHighlightedPot.SetHighlight(true); // Включаем на новом
            }
        }
        else
        {
            // Если курсор ушел с горшка или горшок пустой
            ClearHighlight();
        }
    }

    /// <summary>
    /// КЛЮЧЕВОЕ ПЕРЕОПРЕДЕЛЕНИЕ:
    /// Лопатка ВСЕГДА возвращается на исходную позицию.
    /// Если был подсвечен горшок — открываем окно подтверждения удаления.
    /// </summary>
    protected override void OnDragEnded(bool success)
    {
        // 1. ВСЕГДА возвращаем лопатку на её "домашнюю" позицию
        transform.position = _startPosition;

        // 2. Если на момент отпускания у нас был подсвечен горшок с цветком
        if (_currentlyHighlightedPot != null)
        {
            if (FlowerRemovalManager.Instance != null)
            {
                FlowerRemovalManager.Instance.ShowConfirmation(_currentlyHighlightedPot);
            }
            else
            {
                Debug.LogError("FlowerRemovalManager не найден на сцене!");
            }

            // Снимаем подсветку после инициации действия
            ClearHighlight();
        }
    }

    /// <summary>
    /// Переопределяем базовую логику TryDrop.
    /// Лопатка не "садится" никуда — она всегда возвращается.
    /// Поэтому TryDrop всегда возвращает false (чтобы базовый класс не пытался "посадить" лопатку).
    /// </summary>
    protected override bool TryDrop(Vector2 screenPosition)
    {
        // Лопатка не "садится" никуда — она всегда возвращается
        return false;
    }

    /// <summary>
    /// Гарантированно снимает подсветку и очищает ссылку.
    /// </summary>
    private void ClearHighlight()
    {
        if (_currentlyHighlightedPot != null)
        {
            _currentlyHighlightedPot.SetHighlight(false);
            _currentlyHighlightedPot = null;
        }
    }

    /// <summary>
    /// На случай, если объект лопатки будет деактивирован во время перетаскивания.
    /// </summary>
    private void OnDisable()
    {
        ClearHighlight();
    }
}