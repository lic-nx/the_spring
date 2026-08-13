using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YG;

/// <summary>
/// Компонент-обработчик кликов, добавляемый на призрак горшка.
/// Перемещение призрака идёт через Update() в PotDragManager —
/// призрак всегда преследует курсор, даже без нажатия.
/// Этот компонент отвечает только за обработку кликов (PointerEventData).
/// </summary>
public class PotGhostDragHandler : MonoBehaviour, IPointerDownHandler
{
    /// <summary>
    /// Флаг: первый клик после создания призрака игнорируется
    /// (это клик по кнопке «Купить» в магазине).
    /// </summary>
    private bool _ignoreFirstClick = true;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        // Игнорируем первый клик (клик по кнопке «Купить»)
        if (_ignoreFirstClick)
        {
            _ignoreFirstClick = false;
            return;
        }

        // Правый клик — отмена
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            PotDragManager.Instance.CancelDrag();
            return;
        }

        // Левый клик — попытка размещения
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PotDragManager.Instance.TryPlacePot();
        }
    }
}

/// <summary>
/// Менеджер перетаскивания горшка из магазина.
/// При покупке создаёт UI-призрак горшка на отдельном overlay-Canvas
/// без CanvasScaler (1:1 пиксели экрана), который:
/// - Перекрывает собой остальной UI (sortingOrder = 9999)
/// - Сразу появляется под курсором
/// - Всегда преследует курсор через Update() без отставаний и опережений
/// - При левом клике проверяет попадание в зону (iPotDropArea) и размещает горшок
/// - При правом клике отменяет размещение
/// 
/// ТРЕБОВАНИЯ:
/// 1. В инспекторе должен быть назначен potGhostPrefab (UI-префаб с SeedGhostUI и Image)
/// 2. На сцене должен быть EventSystem
/// </summary>
public class PotDragManager : MonoBehaviour
{
    public static PotDragManager Instance { get; private set; }
    
    [Header("Настройки")]
    [Tooltip("UI-префаб призрака горшка (должен иметь SeedGhostUI и Image)")]
    public GameObject potGhostPrefab;
    
    private GameObject _currentGhost;
    private Sprite _currentPotSprite;
    private Camera _mainCamera;
    private bool _isDragging = false;
    
    /// <summary>
    /// Отдельный Canvas для призраков — ScreenSpaceOverlay,
    /// ConstantPixelSize scaleFactor=1. Гарантирует 1:1 конвертацию
    /// экранных пикселей в локальные координаты без усиления.
    /// </summary>
    private Canvas _ghostCanvas;
    private RectTransform _ghostCanvasRect;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("[PotDragManager] Camera.main не найдена!");
        }
        
        // Создаём или находим отдельный overlay-Canvas для призраков.
        // Общий с SeedDragManager (по имени), чтобы не плодить канвасы.
        _ghostCanvas = GhostOverlayCanvas.GetOrCreate();
        _ghostCanvasRect = _ghostCanvas.transform as RectTransform;
        
        Debug.Log($"[PotDragManager] Ghost Canvas: {_ghostCanvas.name}, " +
            $"size: {_ghostCanvasRect.rect.size}");
    }
    
    /// <summary>
    /// Начало перетаскивания горшка из магазина.
    /// Вызывается из Shop.PurchasePot() после закрытия магазина.
    /// </summary>
    public void StartPotDrag(Sprite potSprite, int index)
    {
        if (_isDragging)
        {
            Debug.Log("[PotDragManager] Уже перетаскиваем горшок!");
            return;
        }
        
        if (potGhostPrefab == null)
        {
            Debug.LogError("[PotDragManager] potGhostPrefab не назначен в инспекторе!");
            return;
        }
        
        _currentPotSprite = potSprite;
        _isDragging = true;
        
        // Создаём UI-призрак как дочерний элемент overlay-Canvas
        _currentGhost = Instantiate(potGhostPrefab, _ghostCanvas.transform);
        _currentGhost.transform.SetAsLastSibling();
        
        // Устанавливаем спрайт
        SetSpriteOnGhost(potSprite);
        
        // Добавляем компонент для обработки кликов через EventSystem
        _currentGhost.AddComponent<PotGhostDragHandler>();
        
        // Убеждаемся, что Image на призраке принимает raycast
        Image ghostImage = _currentGhost.GetComponent<Image>();
        if (ghostImage != null && !ghostImage.raycastTarget)
        {
            ghostImage.raycastTarget = true;
        }
        
        // Сразу ставим призрак под курсор
        SetGhostPosition(Input.mousePosition);
        
        // Показываем зоны для горшков
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
        
        Debug.Log($"[PotDragManager] Начато перетаскивание горшка '{potSprite.name}'");
    }
    
    /// <summary>
    /// Призрак преследует курсор каждый кадр.
    /// </summary>
    private void Update()
    {
        if (!_isDragging || _currentGhost == null) return;

        SetGhostPosition(Input.mousePosition);
    }
    
    /// <summary>
    /// Устанавливает позицию призрака по экранным координатам.
    /// Конвертация идёт через overlay-Canvas с scaleFactor=1,
    /// поэтому пиксели экрана = локальные единицы Canvas = позиция призрака.
    /// Никакого усиления или отставания.
    /// </summary>
    private void SetGhostPosition(Vector2 screenPosition)
    {
        if (_currentGhost == null || _ghostCanvasRect == null) return;
        
        RectTransform ghostRect = _currentGhost.GetComponent<RectTransform>();
        if (ghostRect == null) return;
        
        // ScreenSpaceOverlay — камера всегда null, конвертация 1:1
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _ghostCanvasRect,
            screenPosition,
            null,
            out Vector2 localPoint);
        
        ghostRect.anchoredPosition = localPoint;
    }
    
    /// <summary>
    /// Пытается разместить горшок в зоне под курсором.
    /// Вызывается из PotGhostDragHandler при левом клике.
    /// Использует Input.mousePosition для надёжной проверки зон,
    /// как было в старом рабочем коде.
    /// </summary>
    public void TryPlacePot()
    {
        // Конвертируем экранные координаты в мировые для проверки коллайдеров зон
        // Используем Input.mousePosition напрямую (как в старом коде),
        // что гарантирует корректную конвертацию в мировые координаты
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        // Используем OverlapPointAll для надёжности — если зон несколько,
        // ищем именно iPotDropArea
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPos);
        
        foreach (Collider2D hit in hits)
        {
            var dropArea = hit.GetComponent<iPotDropArea>();
            if (dropArea != null)
            {
                // Создаём реальный горшок в мире
                GameObject potObj = Instantiate(Shop.Instance.potDragDropPrefab, worldPos, Quaternion.identity);
                
                // Устанавливаем спрайт горшка
                SpriteRenderer potSR = potObj.GetComponent<SpriteRenderer>();
                if (potSR != null && _currentPotSprite != null)
                {
                    potSR.sprite = _currentPotSprite;
                }
                
                // Пытаемся разместить в зоне
                bool placed = dropArea.OnPotDrop(potObj);
                
                if (placed)
                {
                    // Связываем горшок с зоной (для корректного освобождения при перетаскивании)
                    Pot pot = potObj.GetComponent<Pot>();
                    if (pot != null)
                    {
                        pot.SetCurrentZone(dropArea);
                    }
                    
                    Debug.Log($"[PotDragManager] Горшок '{_currentPotSprite.name}' успешно размещён в зоне!");
                    
                    // Сохраняем прогресс
                    YG2.SaveProgress();
                    
                    DestroyGhost();
                    return;
                }
                else
                {
                    Debug.LogWarning("[PotDragManager] Зона отклонила размещение. Горшок не поставлен.");
                    Destroy(potObj);
                    // Призрак остаётся — пользователь может попробовать другую зону
                    return;
                }
            }
        }
        
        Debug.Log("[PotDragManager] Не попали в зону для горшков. Попробуйте ещё раз.");
    }
    
    /// <summary>
    /// Отмена перетаскивания (правый клик).
    /// </summary>
    public void CancelDrag()
    {
        Debug.Log("[PotDragManager] Отмена размещения горшка.");
        DestroyGhost();
    }
    
    /// <summary>
    /// Устанавливает спрайт на призрак.
    /// </summary>
    private void SetSpriteOnGhost(Sprite sprite)
    {
        bool spriteSet = false;
        
        SeedGhostUI potGhost = _currentGhost.GetComponent<SeedGhostUI>();
        if (potGhost != null)
        {
            potGhost.Setup(sprite);
            spriteSet = true;
        }
        
        if (!spriteSet)
        {
            Image img = _currentGhost.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
                spriteSet = true;
            }
        }
        
        if (!spriteSet)
        {
            Debug.LogError("[PotDragManager] На префабе нет ни SeedGhostUI, ни Image!");
        }
    }
    
    /// <summary>
    /// Уничтожает призрак и сбрасывает состояние.
    /// </summary>
    private void DestroyGhost()
    {
        if (_currentGhost != null)
        {
            Destroy(_currentGhost);
            _currentGhost = null;
        }
        
        _isDragging = false;
        _currentPotSprite = null;
        
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(false);
        }
    }
}

/// <summary>
/// Отдельный overlay-Canvas для drag-призраков (горшки, семена и т.д.).
/// Создаётся один раз при первом обращении и живёт до конца сессии.
/// 
/// Ключевые свойства:
/// - ScreenSpaceOverlay: рендерится поверх всей сцены
/// - ConstantPixelSize с scaleFactor=1: пиксели экрана = локальные единицы
/// - sortingOrder=9999: поверх любого другого UI
/// - GraphicRaycaster: призраки могут получать клики
/// 
/// Благодаря scaleFactor=1, ScreenPointToLocalPointInRectangle
/// даёт координаты 1:1 с экраном — никакого усиления или отставания.
/// </summary>
public static class GhostOverlayCanvas
{
    private const string CanvasObjectName = "DragGhostOverlay";
    private static Canvas _canvas;
    
    /// <summary>
    /// Возвращает существующий или создаёт новый overlay-Canvas для призраков.
    /// Потокобезопасно для Unity (вызывается из Awake/Start).
    /// </summary>
    public static Canvas GetOrCreate()
    {
        if (_canvas != null && _canvas.gameObject != null) return _canvas;
        
        // Ищем существующий на сцене
        GameObject existing = GameObject.Find(CanvasObjectName);
        if (existing != null)
        {
            _canvas = existing.GetComponent<Canvas>();
            if (_canvas != null) return _canvas;
        }
        
        // Создаём новый
        GameObject obj = new GameObject(CanvasObjectName);
        
        _canvas = obj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 9999;
        
        CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        scaler.referencePixelsPerUnit = 100f;
        
        // Нужен для того, чтобы IPointerDownHandler на призраках работал
        obj.AddComponent<GraphicRaycaster>();
        
        Debug.Log($"[GhostOverlayCanvas] Создан overlay-Canvas для drag-призраков, " +
            $"size: {(_canvas.transform as RectTransform).rect.size}");
        
        return _canvas;
    }
}