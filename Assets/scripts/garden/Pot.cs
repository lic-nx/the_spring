using UnityEngine;

public class Pot : DragDrop
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
        // Кэшируем спрайт-рендерер для запасного варианта подсветки (изменение яркости)
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (_mySpriteRenderer != null)
        {
            _originalColor = _mySpriteRenderer.color;
        }
    }

    public void SetCurrentZone(iPotDropArea zone)
    {
        currentZone = zone;
    }

    public void AlignToZone(Transform zoneRoot)
    {
        Transform zoneAttach = zoneRoot.childCount > 0 ? zoneRoot.GetChild(0) : zoneRoot;
        Transform potAttach = zoneAttachmentPoint != null ? zoneAttachmentPoint : (this.transform.childCount > 0 ? this.transform.GetChild(0) : this.transform);
        Vector3 originalOffset = potAttach.position - this.transform.position;
        this.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
    }

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
        
        flower.transform.SetParent(this.transform);
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

    /// <summary>
    /// Удаляет цветок из горшка и уничтожает его объект.
    /// </summary>
    public void RemoveFlower()
    {
        if (currentFlower != null)
        {
            Debug.Log($"Цветок {currentFlower.name} удален из горшка.");
            
            // Если нужно возвращать цветок в инвентарь, замените Destroy на вашу логику.
            Destroy(currentFlower.gameObject);
            currentFlower = null;
        }
    }

    /// <summary>
    /// Включает или выключает визуальную подсветку горшка.
    /// </summary>
    public void SetHighlight(bool isActive)
    {
        // Вариант 1: Если назначен специальный объект-обводка (рекомендуется)
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(isActive);
        }
        // Вариант 2: Запасной вариант - делаем основной спрайт ярче
        else if (_mySpriteRenderer != null)
        {
            _mySpriteRenderer.color = isActive ? new Color(1.3f, 1.3f, 1.3f, 1f) : _originalColor;
        }
    }

    private void OnMouseDown()
    {
        base.OnMouseDown(); 

        if (currentZone != null)
        {
            currentZone.FreeZone();
            currentZone = null;
        }

        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
    }

    private void OnMouseUp()
    {
        _collider.enabled = false;
        Collider2D dropArea = Physics2D.OverlapPoint(transform.position);
        _collider.enabled = true;

        bool isPlacedSuccessfully = false;

        if (dropArea != null && dropArea.GetComponent<iPotDropArea>() != null)
        {
            iPotDropArea area = dropArea.GetComponent<iPotDropArea>();
            isPlacedSuccessfully = area.OnPotDrop(this.gameObject);
        }

        if (!isPlacedSuccessfully)
        {
            Debug.Log("Не удалось поставить горшок, возвращаем на исходную позицию.");
            transform.position = _startDragPosition;
        }

        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(false);
        }
    }
}