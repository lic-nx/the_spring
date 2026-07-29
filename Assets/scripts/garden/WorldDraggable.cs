using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Базовый класс для перетаскивания объектов в мировом пространстве.
/// Работает на ПК (мышь) и мобилках (тач) через EventSystem.
/// 
/// ТРЕБОВАНИЯ:
/// 1. На камере должен быть компонент Physics2DRaycaster
/// 2. На сцене должен быть EventSystem
/// 3. На объекте должен быть Collider2D
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WorldDraggable : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    [Header("Настройки перетаскивания")]
    [Tooltip("Возвращать ли объект на исходную позицию, если дроп не удался")]
    [SerializeField] protected bool returnOnFail = true;

    [Tooltip("Слой, на котором будет объект во время перетаскивания (чтобы не перекрывался)")]
    [SerializeField] protected int dragSortingOrder = 100;

    protected Vector3 _startPosition;
    protected Vector3 _dragOffset;
    protected bool _isDragging = false;
    protected Camera _mainCamera;
    protected Collider2D _collider;
    protected int _originalSortingOrder;

    protected virtual void Awake()
    {
        _mainCamera = Camera.main;
        _collider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        _startPosition = transform.position;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) _originalSortingOrder = sr.sortingOrder;
    }

    /// <summary>
    /// Вызывается при нажатии на объект (до начала перетаскивания).
    /// </summary>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        // Можно использовать для подсветки, звуков и т.д.
    }

    /// <summary>
    /// Вызывается, когда начинается перетаскивание.
    /// </summary>
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _startPosition = transform.position;

        // Считаем смещение между позицией объекта и точкой клика
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        _dragOffset = transform.position - worldPoint;

        // Поднимаем объект "ближе к камере" во время перетаскивания
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = dragSortingOrder;

        OnDragStarted();
    }

    /// <summary>
    /// Вызывается каждый кадр во время перетаскивания.
    /// </summary>
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        Vector3 worldPoint = ScreenToWorld(eventData.position);
        transform.position = new Vector3(
            worldPoint.x + _dragOffset.x,
            worldPoint.y + _dragOffset.y,
            0f
        );

        OnDragging();
    }

    /// <summary>
    /// Вызывается при отпускании.
    /// </summary>
    public virtual void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        // Возвращаем sorting order
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = _originalSortingOrder;

        // Проверяем, куда упал объект
        bool success = TryDrop(eventData.position);

        if (!success && returnOnFail)
        {
            transform.position = _startPosition;
        }

        OnDragEnded(success);
    }

    /// <summary>
    /// Проверка: куда упал объект? Переопределяй в дочерних классах.
    /// Возвращает true, если дроп успешен.
    /// </summary>
    protected virtual bool TryDrop(Vector2 screenPosition)
    {
        // Базовая реализация: проверяем, есть ли под курсором iPotDropArea
        Vector2 worldPoint = ScreenToWorld(screenPosition);
        
        // Временно отключаем свой коллайдер, чтобы не попасть в себя
        _collider.enabled = false;
        Collider2D hit = Physics2D.OverlapPoint(worldPoint);
        _collider.enabled = true;

        if (hit != null)
        {
            var dropArea = hit.GetComponent<iPotDropArea>();
            if (dropArea != null)
            {
                return dropArea.OnPotDrop(gameObject);
            }
        }

        return false;
    }

    /// <summary>
    /// Конвертирует экранные координаты в мировые (для 2D).
    /// </summary>
    protected Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(pos);
    }

    // ===== Виртуальные методы для переопределения в дочерних классах =====

    protected virtual void OnDragStarted() { }
    protected virtual void OnDragging() { }
    protected virtual void OnDragEnded(bool success) { }
}