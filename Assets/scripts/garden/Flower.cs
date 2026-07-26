using UnityEngine;
using UnityEngine.UI;

public class Flower : MonoBehaviour
{
    [SerializeField] private GrowthConditions _conditions;

    [SerializeField] private GameObject wateringIconObj;
    [SerializeField] private GameObject fertilizingIconObj;
    [SerializeField] private GameObject sunIconObj; 

    public GrowthConditions Conditions => _conditions;
    public System.Action<int> OnSunCollected;

    private float _timeSinceLastWatering;
    // private float _timeSinceLastFertilizing;
    private float _timeSinceLastSunGeneration;

    private bool _needWater;
    private bool _needFertilize;
    private bool _isFullyGrown;

    private int _careEventCount;
    private int _currentStageIndex;

    private void Awake()
    {
        if (_conditions == null)
        {
            _conditions = Resources.Load<GrowthConditions>("DefaultGrowthConditions");
            if (_conditions == null) _conditions = ScriptableObject.CreateInstance<GrowthConditions>();
        }
    }

    public void Initialize(GrowthConditions conditions)
    {
        _conditions = conditions;
        _currentStageIndex = 0;
        _careEventCount = 0;
        _isFullyGrown = false;
        
        ResetNeedsOnly(); 

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && _conditions.StageSprites != null && _conditions.StageSprites.Length > 0)
        {
            sr.sprite = _conditions.StageSprites[0];
            ResetColliderToCurrentSprite(); 
        }

        UpdateNeedIcons();
    }

    private void Update()
    {
        // 1. Сначала всегда проверяем и обновляем потребности (и для роста, и для взрослого цветка)
        if (!_needWater && !_needFertilize)
        {
            _timeSinceLastWatering += Time.deltaTime;
            // _timeSinceLastFertilizing += Time.deltaTime;

            bool needStateChanged = false;

            if (_timeSinceLastWatering >= Conditions.TimeBetweenWatering)
            {
                _needWater = true;
                needStateChanged = true;
            }
            
            // if (_timeSinceLastFertilizing >= Conditions.TimeBetweenFertilizing)
            // {
            //     _needFertilize = true;
            //     needStateChanged = true;
            // }

            if (needStateChanged)
            {
                UpdateNeedIcons();
            }
        }

        // 2. ГЛАВНОЕ: Если есть потребность в уходе, мы БЛОКИРУЕМ всё остальное, включая генерацию солнца
        if (_needWater || _needFertilize)
        {
            return; 
        }

        // 3. Если цветок вырос и потребностей НЕТ, генерируем солнце
        if (_isFullyGrown)
        {
            _timeSinceLastSunGeneration += Time.deltaTime;
            if (_timeSinceLastSunGeneration >= Conditions.SunGenerationInterval)
            {
                _timeSinceLastSunGeneration = 0f;
                SpawnSunIcon();
            }
            return; // Дальше логика роста не нужна
        }
    }

    public void Water()
    {
        if (!_needWater) return;
        _needWater = false;
        _timeSinceLastWatering = 0f; 
        RegisterCareEvent();
    }

    // public void Fertilize()
    // {
    //     if (!_needFertilize) return;
    //     _needFertilize = false;
    //     _timeSinceLastFertilizing = 0f; 
    //     RegisterCareEvent();
    // }

    private void RegisterCareEvent()
    {
        _careEventCount++;
        UpdateNeedIcons(); 
        TryAdvanceStage();
    }

    private void TryAdvanceStage()
    {
        // Если цветок уже полностью вырос, нам не нужно пытаться продвинуть его дальше
        if (_isFullyGrown) return;

        int required = Conditions.GetRequiredEventsForStage(_currentStageIndex + 1);
        
        if (required > 0 && _careEventCount >= required)
        {
            if (Conditions.StageSprites != null && _currentStageIndex + 1 < Conditions.StageSprites.Length)
            {
                _currentStageIndex++;
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null) 
                {
                    sr.sprite = Conditions.StageSprites[_currentStageIndex];
                    ResetColliderToCurrentSprite();
                }
            }
            else
            {
                _isFullyGrown = true;
                Debug.Log("🌸 Цветок полностью вырос! Активирована генерация солнца (при отсутствии потребностей).");
            }
            
            _careEventCount = 0;
            _needWater = false;
            _needFertilize = false;
            UpdateNeedIcons();
        }
    }

    private void ResetColliderToCurrentSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Vector3 worldCenter = sr.bounds.center;
        Vector3 worldSize = sr.bounds.size;

        // Преобразуем мировые координаты в локальные для корректного offset
        Vector2 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector2 localSize = new Vector2(
            worldSize.x / transform.lossyScale.x,
            worldSize.y / transform.lossyScale.y
        );

        var polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            bool wasTrigger = polyCollider.isTrigger;
            DestroyImmediate(polyCollider);
            var newPoly = gameObject.AddComponent<PolygonCollider2D>();
            newPoly.isTrigger = wasTrigger;
            return;
        }

        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            bool wasTrigger = boxCollider.isTrigger;
            DestroyImmediate(boxCollider);
            var newBox = gameObject.AddComponent<BoxCollider2D>();
            newBox.isTrigger = wasTrigger;
            newBox.size = localSize;
            newBox.offset = localCenter;
            return;
        }

        var circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            bool wasTrigger = circleCollider.isTrigger;
            DestroyImmediate(circleCollider);
            var newCircle = gameObject.AddComponent<CircleCollider2D>();
            newCircle.isTrigger = wasTrigger;
            float radius = Mathf.Max(localSize.x, localSize.y) / 2f;
            newCircle.radius = radius;
            newCircle.offset = localCenter;
            return;
        }
    }

    private void SpawnSunIcon()
    {
        if (sunIconObj != null)
        {
            sunIconObj.SetActive(true);
        }
    }

    public void CollectSun()
    {
        if (sunIconObj != null && sunIconObj.activeSelf)
        {
            sunIconObj.SetActive(false);
            Debug.Log($"☀️ Собрано солнце! +{Conditions.SunValue} валюты.");
            OnSunCollected?.Invoke(Conditions.SunValue);
        }
    }

    private void ResetNeedsOnly()
    {
        _needWater = false;
        _needFertilize = false;
    }

    private void CreateIconIfNull(ref GameObject iconObj, string name, Sprite sprite, Vector3 offset, bool isClickable)
    {
        if (iconObj != null)
        {
            iconObj.SetActive(false);
            return;
        }

        if (sprite != null)
        {
            iconObj = new GameObject(name);
            iconObj.transform.SetParent(transform);
            iconObj.transform.localPosition = offset;
            
            var renderer = iconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = isClickable ? 20 : 10;
            
            if (isClickable)
            {
                iconObj.AddComponent<CircleCollider2D>().isTrigger = true;
                var button = iconObj.AddComponent<Button>();
                button.onClick.AddListener(CollectSun);
                button.transition = Selectable.Transition.None;
            }
            
            iconObj.SetActive(false);
        }
    }

    private void UpdateNeedIcons()
    {
        // УБРАНО: условие if (_isFullyGrown), которое раньше принудительно скрывало иконки.
        // Теперь иконки просто отражают реальное состояние потребностей, независимо от стадии роста.

        if (wateringIconObj != null) wateringIconObj.SetActive(_needWater);
        if (fertilizingIconObj != null) fertilizingIconObj.SetActive(_needFertilize);
    }
}