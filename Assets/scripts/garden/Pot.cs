using UnityEngine;
using UnityEngine.EventSystems;
using YG;

public class Pot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Transform flowerAttachment;
    [SerializeField] private Transform zoneAttachmentPoint;
    private Flower currentFlower;
    public Flower CurrentFlower => currentFlower;
    private iPotDropArea currentZone;
    public iPotDropArea CurrentZone => currentZone;

    [Header("Визуальная подсветка")]
    [Tooltip("Дочерний объект-спрайт (например, желтая обводка), который будет включаться при наведении лопатки")]
    [SerializeField] private GameObject highlightIndicator;
    

    
    private SpriteRenderer _mySpriteRenderer;
    private Color _originalColor;
    private Camera _mainCamera;
    private Collider2D _collider;
    private bool _isDragging = false;
    private Vector3 _dragOffset;
    private int _originalSortingOrder;
    private bool _wasMenuActiveBeforeDrag = false;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _collider = GetComponent<Collider2D>();
        
        Debug.Log($"[Pot] Initialized. Collider: {_collider != null}, Camera: {_mainCamera != null}");
        
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (_mySpriteRenderer != null)
        {
            _originalColor = _mySpriteRenderer.color;
            _originalSortingOrder = _mySpriteRenderer.sortingOrder;
        }
    }

    // Метод Start больше не нужен, удаляем его

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

    // ===== Save/Load Methods =====
    // public void SaveState()
    // {
    //     if (GameSaveManager.Instance != null)
    //     {
    //         GameSaveManager.Instance.SavePotState(this);
    //     }
    // }

    // public void LoadState(GameSaveManager.PotData potData)
    // {
    //     if (!string.IsNullOrEmpty(potData.spriteName))
    //     {
    //         var sprite = Shop.Instance?.GetPotSpriteByName(potData.spriteName);
    //         if (sprite != null && _mySpriteRenderer != null)
    //         {
    //             _mySpriteRenderer.sprite = sprite;
    //         }
    //     }
    //     transform.position = potData.position;
    // }

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

    // ===== Обработка кликов для меню действий =====
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging) return;
        
        Debug.Log("clic on pot !!");
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("left clic on pot !!");
            ToggleActionMenu();
        }
    }

    // ===== Drag and Drop for moving the pot directly =====
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentZone == null) return;
        
        _isDragging = true;
        
        // Track if menu was active before dragging
        _wasMenuActiveBeforeDrag = currentActionMenu != null && currentActionMenu.gameObject.activeSelf;
        
        // Hide action menu when starting to drag
        HideActionMenu();
        
        // Calculate drag offset
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        _dragOffset = transform.position - worldPoint;
        
        // Disable collider to avoid interference
        if (_collider != null)
        {
            _collider.enabled = false;
        }
        
        // Bring to front
        if (_mySpriteRenderer != null)
        {
            _mySpriteRenderer.sortingOrder = 100;
        }
        
        // Show drop zones
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
        
        Debug.Log($"[Pot] OnBeginDrag: {gameObject.name}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        transform.position = new Vector3(
            worldPoint.x + _dragOffset.x,
            worldPoint.y + _dragOffset.y,
            transform.position.z
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        
        // Re-enable collider
        if (_collider != null)
        {
            _collider.enabled = true;
        }
        
        // Restore sorting order
        if (_mySpriteRenderer != null)
        {
            _mySpriteRenderer.sortingOrder = _originalSortingOrder;
        }
        
        // Check if dropped in a valid zone
        Vector3 worldPoint = ScreenToWorld(eventData.position);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
        
        bool placed = false;
        foreach (Collider2D hit in hits)
        {
            var dropArea = hit.GetComponent<iPotDropArea>();
            if (dropArea != null)
            {
                if (dropArea == currentZone)
                {
                    Debug.Log("[Pot] Already in this zone!");
                    placed = true;
                    break;
                }
                
                if (dropArea.OnPotDrop(gameObject))
                {
                    currentZone.FreeZone();
                    SetCurrentZone(dropArea);
                    AlignToZone(hit.transform);
                    placed = true;
                    Debug.Log($"[Pot] Successfully moved to new zone!");
                    YG2.SaveProgress();
                    break;
                }
            }
        }
        
        if (!placed)
        {
            Debug.Log("[Pot] Not placed in any zone. Returning to original position.");
            // Return to original position if not placed
            if (currentZone != null)
            {
                currentZone.AlignPotToZone(this);
            }
        }
        
        // Hide drop zones
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(false);
        }
        
        // Restore action menu if it was active before dragging
        if (_wasMenuActiveBeforeDrag)
        {
            currentActionMenu = PotActionMenuPool.Instance.GetMenu();
            if (currentActionMenu != null)
            {
                currentActionMenu.SetTargetPot(this);
                Vector3 menuPosition = transform.position + new Vector3(1.5f, 0f, 0f);
                currentActionMenu.transform.position = menuPosition;
            }
        }
        _wasMenuActiveBeforeDrag = false;
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        Vector3 pos = new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z);
        return _mainCamera.ScreenToWorldPoint(pos);
    }

    private PotActionMenu currentActionMenu;

    private void ToggleActionMenu()
    {
        if (PotActionMenuPool.Instance == null)
        {
            Debug.LogError("[Pot] PotActionMenuPool is not available!");
            return;
        }

        if (currentActionMenu != null && currentActionMenu.gameObject.activeSelf)
        {
            currentActionMenu.ReturnToPool();
            currentActionMenu = null;
        }
        else
        {
            PotActionMenuPool.Instance.ReturnAllMenus();
            currentActionMenu = PotActionMenuPool.Instance.GetMenu();
            if (currentActionMenu != null)
            {
                currentActionMenu.SetTargetPot(this);
                Vector3 menuPosition = transform.position + new Vector3(1.5f, 0f, 0f);
                currentActionMenu.transform.position = menuPosition;
            }
        }
    }

    // ===== Методы для работы с меню =====
    public void DeletePot()
    {
        HideActionMenu();
        
        if (currentZone != null)
        {
            currentZone.FreeZone();
        }
        
        if (currentFlower != null)
        {
            RemoveFlower();
        }
        
        Debug.Log($"[Pot] Горшок {gameObject.name} удален.");
        Destroy(gameObject);
    }

    public void ReplaceSprite(Sprite newSprite)
    {
        HideActionMenu();
        
        if (_mySpriteRenderer != null && newSprite != null)
        {
            _mySpriteRenderer.sprite = newSprite;
            Debug.Log($"[Pot] Спрайт горшка заменен на {newSprite.name}");
            
            if (currentZone != null)
            {
                currentZone.FreeZone();
                currentZone.OnPotDrop(gameObject);
            }
        }
    }

    public void StartMoving()
    {
        HideActionMenu();
        Debug.Log($"[Pot] StartMoving called for {gameObject.name}");
    }

    public void HideActionMenu()
    {
        if (currentActionMenu != null)
        {
            currentActionMenu.ReturnToPool();
            currentActionMenu = null;
        }
    }
}