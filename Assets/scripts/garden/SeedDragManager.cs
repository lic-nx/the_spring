using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Менеджер перетаскивания семян из инвентаря на сцену.
/// Создаёт UI-призрак на Canvas, который следует за курсором/пальцем.
/// При отпускании проверяет попадание в горшок и сажает цветок.
/// 
/// ТРЕБОВАНИЯ:
/// 1. Этот объект должен быть дочерним элементом Canvas
/// 2. В инспекторе должен быть назначен seedGhostPrefab (UI-префаб призрака)
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
        
        // Создаём UI-призрак
        _currentGhost = Instantiate(seedGhostPrefab, _canvas.transform);
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
    /// </summary>
    public void UpdateDragPosition(PointerEventData eventData)
{
    if (_currentGhost != null)
    {
        // 1. Берем экранные координаты курсора/касания
        Vector3 screenPos = eventData.position;
        
        // 2. Указываем Z как расстояние от камеры до мировой плоскости Z = 0.
        // Для стандартной 2D-камеры это модуль её координаты Z (например, если камера в -10, расстояние = 10).
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        
        // 3. Конвертируем в мировые координаты
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        
        // 4. Явно фиксируем Z = 0 для страховки от погрешностей float
        worldPos.z = 0f;
        
        // 5. Применяем позицию к объекту
        _currentGhost.transform.position = worldPos;
    }
}
    
    /// <summary>
    /// Завершение перетаскивания: проверяет попадание в горшок и сажает цветок.
    /// </summary>
    public void EndDraggingSeed(PointerEventData eventData)
    {
        if (_currentGhost == null) return;
    
    // 1. Получаем мировые координаты так же, как и при перетаскивании
        Vector3 screenPos = eventData.position;
        screenPos.z = Mathf.Abs(_mainCamera.transform.position.z);
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        
        // 2. ПРОВЕРКА: Для проверки коллайдера в конкретной точке в 2D 
        // лучше использовать Physics2D.OverlapPoint, а не Raycast с нулевым направлением.
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