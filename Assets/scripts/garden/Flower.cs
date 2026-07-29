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
            UpdateColliderToCurrentSprite();
        }
        UpdateNeedIcons();
    }

    private void Update()
    {
        // 1. Проверяем потребности (всегда, независимо от стадии)
        if (!_needWater && !_needFertilize)
        {
            _timeSinceLastWatering += Time.deltaTime;
            bool needStateChanged = false;

            if (_timeSinceLastWatering >= Conditions.TimeBetweenWatering)
            {
                _needWater = true;
                needStateChanged = true;
            }

            if (needStateChanged)
            {
                UpdateNeedIcons();
            }
        }

        // 2. Генерация солнца (работает НЕЗАВИСИМО от потребностей!)
        if (_isFullyGrown)
        {
            _timeSinceLastSunGeneration += Time.deltaTime;
            if (_timeSinceLastSunGeneration >= Conditions.SunGenerationInterval)
            {
                _timeSinceLastSunGeneration = 0f;
                SpawnSunIcon();
            }
        }

        // 3. Рост (блокируется потребностями)
        if (_needWater || _needFertilize)
        {
            return; // Блокируем только РОСТ, но не генерацию солнца
        }
    }

    public void Water()
    {
        if (!_needWater) return;
        _needWater = false;
        _timeSinceLastWatering = 0f;
        RegisterCareEvent();
    }

    private void RegisterCareEvent()
    {
        _careEventCount++;
        UpdateNeedIcons();
        TryAdvanceStage();
    }

    private void TryAdvanceStage()
    {
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
                    UpdateColliderToCurrentSprite();
                }
            }
            else
            {
                _isFullyGrown = true;
                Debug.Log("🌸 Цветок полностью вырос! Активирована генерация солнца.");
            }
            _careEventCount = 0;
            _needWater = false;
            _needFertilize = false;
            UpdateNeedIcons();
        }
    }

    private void UpdateColliderToCurrentSprite()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Vector3 worldCenter = sr.bounds.center;
        Vector3 worldSize = sr.bounds.size;

        Vector2 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector2 localSize = new Vector2(
            worldSize.x / transform.lossyScale.x,
            worldSize.y / transform.lossyScale.y
        );

        var boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.size = localSize;
            boxCollider.offset = localCenter;
            return;
        }

        var circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            float radius = Mathf.Max(localSize.x, localSize.y) / 2f;
            circleCollider.radius = radius;
            circleCollider.offset = localCenter;
            return;
        }

        var polyCollider = GetComponent<PolygonCollider2D>();
        if (polyCollider != null)
        {
            polyCollider.pathCount = 0;
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

    /// <summary>
    /// ИСПРАВЛЕНО: Солнце можно собирать НЕЗАВИСИМО от потребностей цветка.
    /// Это поощряет игрока собирать солнце, даже если цветок просит полив.
    /// </summary>
    public void CollectSun()
    {
        if (sunIconObj != null && sunIconObj.activeSelf)
        {
            sunIconObj.SetActive(false);
            Debug.Log($"☀️ Собрано солнце! +{Conditions.SunValue} валюты.");
            
            // ✅ НОВОЕ: начисляем валюту через CurrencyManager
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.AddCurrency(Conditions.SunValue);
            }
            else
            {
                Debug.LogError("[Flower] CurrencyManager.Instance равен null!");
            }
            
            // Старое событие можно оставить для обратной совместимости, 
            // но теперь оно не обязательно
            OnSunCollected?.Invoke(Conditions.SunValue);
        }
    }

    private void ResetNeedsOnly()
    {
        _needWater = false;
        _needFertilize = false;
    }

    private void UpdateNeedIcons()
    {
        if (wateringIconObj != null) wateringIconObj.SetActive(_needWater);
        if (fertilizingIconObj != null) fertilizingIconObj.SetActive(_needFertilize);
    }
}