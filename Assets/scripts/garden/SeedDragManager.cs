using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Менеджер перетаскивания семян из инвентаря.
/// Создаёт UI-призрак на Canvas, который следует за курсором/пальцем
/// и перекрывает собой остальной UI (SetAsLastSibling).
/// При отпускании проверяет попадание в горшок и сажает цветок.
/// 
/// ТРЕБОВАНИЯ:
/// 1. Этот объект должен быть дочерним элементом Canvas
/// 2. В инспекторе должен быть назначен seedGhostPrefab (UI-префаб с SeedGhostUI и Image)
/// </summary>
public class SeedDragManager : MonoBehaviour
{
    public static SeedDragManager Instance { get; private set; }
    
    [Header("Настройки")]
    [Tooltip("UI-префаб призрака семени (должен иметь компонент SeedGhostUI и Image)")]
    public GameObject seedGhostPrefab;
    
    private GameObject _currentGhost;
    private SeedItem _currentSeed;
    private InventorySlotUI _sourceSlot;
    private Camera _mainCamera;
    private Canvas _canvas;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("[SeedDragManager] Camera.main не найдена!");
        }
        
        _canvas = GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("[SeedDragManager] Этот объект должен быть дочерним элементом Canvas!");
        }
    }
    
    /// <summary>
    /// Начало перетаскивания: создаёт UI-призрак семени.
    /// </summary>
    public void StartDraggingSeed(SeedItem seed, InventorySlotUI sourceSlot, PointerEventData eventData)
    {
        if (_currentGhost != null)
        {
            Debug.Log("[SeedDragManager] Уже что-то перетаскивается!");
            return;
        }
        
        _currentSeed = seed;
        _sourceSlot = sourceSlot;
        
        // Создаём UI-призрак как последний дочерний элемент Canvas (поверх всего UI)
        _currentGhost = Instantiate(seedGhostPrefab, _canvas.transform);
        _currentGhost.transform.SetAsLastSibling();
        
        SeedGhostUI ghostUI = _currentGhost.GetComponent<SeedGhostUI>();
        if (ghostUI != null)
        {
            ghostUI.Setup(seed.seedSprite);
        }
        else
        {
            Debug.LogError("[SeedDragManager] На префабе призрака отсутствует компонент SeedGhostUI!");
        }
        
        // Устанавливаем начальную позицию
        UpdateDragPosition(eventData);
    }
    
    /// <summary>
    /// Обновление позиции призрака во время перетаскивания.
    /// Использует RectTransformUtility для корректного позиционирования
    /// внутри Canvas (независимо от режима рендеринга Canvas).
    /// </summary>
    public void UpdateDragPosition(PointerEventData eventData)
    {
        if (_currentGhost == null) return;
        
        RectTransform canvasRect = _canvas.transform as RectTransform;
        RectTransform ghostRect = _currentGhost.GetComponent<RectTransform>();
        
        if (canvasRect == null || ghostRect == null) return;
        
        // Для ScreenSpaceOverlay камера не нужна (null),
        // для ScreenSpaceCamera / WorldSpace — используем основную камеру
        Camera canvasCamera = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) 
            ? null 
            : _mainCamera;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            canvasCamera,
            out Vector2 localPoint);
        
        ghostRect.anchoredPosition = localPoint;
    }
    
    /// <summary>
    /// Завершение перетаскивания: проверяет попадание в горшок и сажает цветок.
    /// </summary>
    public void EndDraggingSeed(PointerEventData eventData)
    {
        if (_currentGhost == null) return;
    
        // Конвертируем экранные координаты в мировые для проверки коллайдеров
        Vector3 screenPos = eventData.position;
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        // Проверяем, попали ли в горшок
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        bool planted = false;
        
        if (hit.collider != null && hit.collider.CompareTag("Pot"))
        {
            planted = TryPlantInPot(hit.collider.transform);
        }
        else
        {
            Debug.Log("[SeedDragManager] Не попали в горшок. Семя возвращается в инвентарь.");
        }
        
        // Уничтожаем призрак
        Destroy(_currentGhost);
        _currentGhost = null;
        _currentSeed = null;
        _sourceSlot = null;
    }
    
    /// <summary>
    /// Попытка посадить цветок в горшок.
    /// </summary>
    private bool TryPlantInPot(Transform potTransform)
    {
        if (_currentSeed.flowerPrefab == null)
        {
            Debug.LogError("[SeedDragManager] В ScriptableObject семени не назначен Flower Prefab!");
            return false;
        }
        
        Pot pot = potTransform.GetComponent<Pot>();
        if (pot == null)
        {
            Debug.LogError($"[SeedDragManager] На объекте '{potTransform.name}' отсутствует компонент Pot!");
            return false;
        }
        
        if (pot.CurrentFlower != null)
        {
            Debug.Log("[SeedDragManager] Горшок уже занят.");
            return false;
        }
        
        // Создаём цветок
        GameObject flowerObj = Instantiate(_currentSeed.flowerPrefab, Vector3.zero, Quaternion.identity);
        Flower flowerComp = flowerObj.GetComponent<Flower>();
        if (flowerComp == null)
        {
            Debug.LogError($"[SeedDragManager] На префабе '{_currentSeed.flowerPrefab.name}' отсутствует компонент Flower!");
            Destroy(flowerObj);
            return false;
        }
        
        // Выбираем случайное условие роста
        GrowthConditions selectedCondition = SelectRandomCondition();
        flowerComp.Initialize(selectedCondition);
        
        // Сажаем в горшок
        bool placed = pot.PlantFlower(flowerComp);
        if (placed)
        {
            Debug.Log($"[SeedDragManager] Цветок '{_currentSeed.name}' успешно посажен!");
            InventoryManager.Instance.RemoveItem(_currentSeed, 1);
            return true;
        }
        else
        {
            Debug.LogWarning("[SeedDragManager] pot.PlantFlower вернул FALSE. Уничтожаем созданный цветок.");
            Destroy(flowerObj);
            return false;
        }
    }
    
    /// <summary>
    /// Выбор случайного условия роста на основе весов.
    /// </summary>
    private GrowthConditions SelectRandomCondition()
    {
        if (_currentSeed.growthConditionsList?.Count > 0 && 
            _currentSeed.weights?.Count == _currentSeed.growthConditionsList.Count)
        {
            int totalWeight = 0;
            foreach (int weight in _currentSeed.weights)
            {
                totalWeight += weight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            for (int i = 0; i < _currentSeed.growthConditionsList.Count; i++)
            {
                currentWeight += _currentSeed.weights[i];
                if (randomValue < currentWeight)
                {
                    return _currentSeed.growthConditionsList[i];
                }
            }
        }
        
        // Fallback: возвращаем первое условие
        return _currentSeed.growthConditionsList[0];
    }
}