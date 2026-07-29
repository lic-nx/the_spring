using UnityEngine;

public class Pot : WorldDraggable
{
    [SerializeField] private Transform flowerAttachment;
    [SerializeField] private Transform zoneAttachmentPoint;

    private Flower currentFlower;
    public Flower CurrentFlower => currentFlower;

    private iPotDropArea currentZone;

    [Header("Визуальная подсветка")]
    [Tooltip("Дочерний объект-спрайт (например, желтая обводка), который будет включаться при наведении лопатки")]
    [SerializeField] private GameObject highlightIndicator;

    private SpriteRenderer _mySpriteRenderer;
    private Color _originalColor;

    private void Awake()
    {
        // ⚠️ ВАЖНО: вызываем Awake базового класса, 
        // иначе не инициализируются _mainCamera и _collider
        base.Awake();

        _mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (_mySpriteRenderer != null)
        {
            _originalColor = _mySpriteRenderer.color;
        }
    }

    // ===== Логика работы с зоной =====

    public void SetCurrentZone(iPotDropArea zone)
    {
        currentZone = zone;
    }

    public void AlignToZone(Transform zoneRoot)
    {
        Transform zoneAttach = zoneRoot.childCount > 0 ? zoneRoot.GetChild(0) : zoneRoot;
        Transform potAttach = zoneAttachmentPoint != null
            ? zoneAttachmentPoint
            : (transform.childCount > 0 ? transform.GetChild(0) : transform);

        Vector3 originalOffset = potAttach.position - transform.position;
        transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
    }

    // ===== Логика цветка =====

    public bool PlantFlower(Flower flower)
    {
        if (currentFlower != null)
        {
            Debug.LogWarning("Attempted to plant a flower in an already occupied pot.");
            return false;
        }
        if (flower == null)
        {
            Debug.LogWarning("Cannot plant a null flower.");
            return false;
        }

        flower.transform.SetParent(transform);
        if (flowerAttachment != null)
        {
            flower.transform.position = flowerAttachment.position;
        }
        else
        {
            flower.transform.localPosition = Vector3.zero;
        }
        currentFlower = flower;
        return true;
    }

    public void RemoveFlower()
    {
        if (currentFlower != null)
        {
            Debug.Log($"Цветок {currentFlower.name} удален из горшка.");
            Destroy(currentFlower.gameObject);
            currentFlower = null;
        }
    }

    // ===== Подсветка (для лопатки) =====

    public void SetHighlight(bool isActive)
    {
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(isActive);
        }
        else if (_mySpriteRenderer != null)
        {
            _mySpriteRenderer.color = isActive
                ? new Color(1.3f, 1.3f, 1.3f, 1f)
                : _originalColor;
        }
    }

    // ===== Переопределение методов перетаскивания =====

    /// <summary>
    /// Вызывается в начале перетаскивания (вместо старого OnMouseDown).
    /// </summary>
    protected override void OnDragStarted()
    {
        // Освобождаем зону, в которой стоял горшок
        if (currentZone != null)
        {
            currentZone.FreeZone();
            currentZone = null;
        }

        // Показываем все свободные зоны как подсказку
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
    }

    /// <summary>
    /// Вызывается при отпускании (вместо старого OnMouseUp).
    /// Базовый класс УЖЕ проверил, куда упал горшок, через TryDrop().
    /// </summary>
    protected override void OnDragEnded(bool success)
    {
        // Скрываем зоны после завершения перетаскивания
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(false);
        }

        if (!success)
        {
            Debug.Log("Не удалось поставить горшок в зону.");
            // Возврат на исходную позицию уже обрабатывается в базовом классе
            // (благодаря returnOnFail = true в Inspector)
        }
    }
}