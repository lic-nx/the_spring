using UnityEngine;
using UnityEngine.EventSystems;
using YG;

public class Pot : MonoBehaviour, IPointerClickHandler
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
    
    [Header("Меню действий")]
    [Tooltip("Объект меню действий (дочерний объект горшка)")]
    [SerializeField] private GameObject actionMenu;
    
    private SpriteRenderer _mySpriteRenderer;
    private Color _originalColor;
    private Camera _mainCamera;
    private Collider2D _collider;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _collider = GetComponent<Collider2D>();
        
        Debug.Log($"[Pot] Initialized. Collider: {_collider != null}, Camera: {_mainCamera != null}");
        
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
        if (_mySpriteRenderer != null)
        {
            _originalColor = _mySpriteRenderer.color;
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
        Debug.Log("clic on pot !!");
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("left clic on pot !!");
            ToggleActionMenu();
        }
    }

    private void ToggleActionMenu()
    {
        if (actionMenu == null)
        {
            Debug.LogError("[Pot] actionMenu не назначен!");
            return;
        }

        bool isActive = actionMenu.activeSelf;
        actionMenu.SetActive(!isActive);
        
        if (actionMenu.activeSelf)
        {
            var menu = actionMenu.GetComponent<PotActionMenu>();
            if (menu != null)
            {
                menu.Initialize();
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
        
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
        
        var dragManager = FindObjectOfType<PotDragManager>();
        if (dragManager != null)
        {
            dragManager.StartMovingExistingPot(this);
        }
    }

    public void HideActionMenu()
    {
        if (actionMenu != null)
        {
            actionMenu.SetActive(false);
        }
    }
}