using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using YG;

public class Flower : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GrowthConditions _conditions;
    [SerializeField] private GameObject wateringIconObj;
    [SerializeField] private GameObject fertilizingIconObj;
    [SerializeField] private GameObject sunIconObj;

    [Header("Настройки сохранения")]
    [Tooltip("Уникальный идентификатор цветка. Если оставить пустым, будет сгенерирован автоматически.")]
    [SerializeField] private string flowerId;

    public GrowthConditions Conditions => _conditions;
    public System.Action<int> OnSunCollected;

    private float _timeSinceLastWatering;
    private float _timeSinceLastSunGeneration;
    private bool _needWater;
    private bool _needFertilize;
    private bool _isFullyGrown;
    private int _careEventCount;
    private int _currentStageIndex;
    private bool _hasGivenSun;
    private string _prefabName;
    private GameObject _maturityHighlight;
    private GameObject _flowerNamePlate;

    private void Awake()
    {
        if (_conditions == null)
        {
            _conditions = Resources.Load<GrowthConditions>("DefaultGrowthConditions");
            if (_conditions == null) _conditions = ScriptableObject.CreateInstance<GrowthConditions>();
        }

        EnsureMaturityPresentation();
        ApplySunSprite();
    }

    public void Initialize(GrowthConditions conditions)
    {
        if (conditions == null)
        {
            Debug.LogError("[Flower] Нельзя инициализировать цветок без GrowthConditions.");
            return;
        }

        _conditions = conditions;
        _currentStageIndex = 0;
        _careEventCount = 0;
        _isFullyGrown = _conditions.StageSprites == null || _conditions.StageSprites.Length <= 1;
        _timeSinceLastWatering = 0f;
        _timeSinceLastSunGeneration = 0f;
        _hasGivenSun = false;
        ResetNeedsOnly();

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && _conditions.StageSprites != null && _conditions.StageSprites.Length > 0)
        {
            sr.sprite = _conditions.StageSprites[0];
            UpdateColliderToCurrentSprite();
        }
        UpdateNeedIcons();
        UpdateMaturityPresentation();
    }

    private void Update()
    {
        // 1. Проверяем потребности (всегда, независимо от стадии)
        // Таймер полива ставится на паузу, пока на экране есть неубранная иконка солнца
        bool hasActiveSun = sunIconObj != null && (sunIconObj.activeSelf || _hasGivenSun);
        if (!_needWater && !_needFertilize && !hasActiveSun)
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
                SaveState();
            }
        }

        // 2. Генерация солнца (таймер не тикает, пока у цветка есть нерешённая потребность)
        if (_isFullyGrown && !_needWater && !_needFertilize)
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
        SaveState();
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

        if (Conditions.StageSprites == null || Conditions.StageSprites.Length == 0)
        {
            _isFullyGrown = true;
            return;
        }

        int required = Conditions.GetRequiredEventsForStage(_currentStageIndex + 1);
        if (required > 0 && _careEventCount >= required)
        {
            if (_currentStageIndex + 1 < Conditions.StageSprites.Length)
            {
                _currentStageIndex++;
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = Conditions.StageSprites[_currentStageIndex];
                    UpdateColliderToCurrentSprite();
                }

                // Последний спрайт и есть взрослая стадия. Дополнительный цикл ухода не нужен.
                _isFullyGrown = _currentStageIndex >= Conditions.StageSprites.Length - 1;
            }
            else
            {
                _isFullyGrown = true;
            }

            if (_isFullyGrown)
            {
                Debug.Log("🌸 Цветок полностью вырос! Активирована генерация солнца.");
                UpdateMaturityPresentation();
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
        _hasGivenSun = true;
        SaveState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !_isFullyGrown) return;

        // Acknowledging a grown flower dismisses only the rotating highlight.
        if (_maturityHighlight != null)
            _maturityHighlight.SetActive(false);
    }

    private void ApplySunSprite()
    {
        if (sunIconObj == null) return;

        Sprite sunSprite = Resources.Load<Sprite>("Greenhouse/Texures/Sun_Coin");
        SpriteRenderer spriteRenderer = sunIconObj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.sprite = sunSprite;

        Image image = sunIconObj.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = sunSprite;
            image.preserveAspect = true;
        }
    }

    private void EnsureMaturityPresentation()
    {
        SpriteRenderer flowerRenderer = GetComponent<SpriteRenderer>();
        int sortingLayerId = flowerRenderer != null ? flowerRenderer.sortingLayerID : 0;
        int sortingOrder = flowerRenderer != null ? flowerRenderer.sortingOrder : 0;

        Transform existingHighlight = transform.Find("MaturityHighlight");
        if (existingHighlight == null)
        {
            _maturityHighlight = new GameObject("MaturityHighlight", typeof(SpriteRenderer), typeof(RotateObject));
            _maturityHighlight.transform.SetParent(transform, false);
        }
        else
        {
            _maturityHighlight = existingHighlight.gameObject;
            if (_maturityHighlight.GetComponent<RotateObject>() == null)
                _maturityHighlight.AddComponent<RotateObject>();
        }

        // The rose sprites use a soil-level pivot, so the bloom is near y=6.5.
        _maturityHighlight.transform.localPosition = new Vector3(0f, 6.4f, 0f);
        _maturityHighlight.transform.localScale = Vector3.one;
        SpriteRenderer highlightRenderer = _maturityHighlight.GetComponent<SpriteRenderer>();
        highlightRenderer.sprite = Resources.Load<Sprite>("Greenhouse/Texures/HighLight");
        highlightRenderer.sortingLayerID = sortingLayerId;
        // The greenhouse background covers negative sorting orders. The highlight
        // can share the flower's base order while the pot is rendered above both.
        highlightRenderer.sortingOrder = sortingOrder;

        Transform existingPlate = transform.Find("FlowerNamePlate");
        if (existingPlate == null)
        {
            _flowerNamePlate = new GameObject("FlowerNamePlate", typeof(SpriteRenderer));
            _flowerNamePlate.transform.SetParent(transform, false);

            GameObject textObject = new GameObject("Name", typeof(TextMeshPro));
            textObject.transform.SetParent(_flowerNamePlate.transform, false);
        }
        else
        {
            _flowerNamePlate = existingPlate.gameObject;
        }

        _flowerNamePlate.transform.localPosition = new Vector3(0f, -1.65f, 0f);
        _flowerNamePlate.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
        SpriteRenderer plateRenderer = _flowerNamePlate.GetComponent<SpriteRenderer>();
        plateRenderer.sprite = Resources.Load<Sprite>("Greenhouse/Texures/HeadLine_Green");
        plateRenderer.sortingLayerID = sortingLayerId;
        plateRenderer.sortingOrder = sortingOrder + 2;

        TextMeshPro nameText = _flowerNamePlate.GetComponentInChildren<TextMeshPro>(true);
        if (nameText != null)
        {
            nameText.text = "РОЗА";
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            if (nameText.font == null && TMP_Settings.defaultFontAsset != null)
                nameText.font = TMP_Settings.defaultFontAsset;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 2f;
            nameText.fontSizeMax = 6f;
            nameText.fontSize = 6f;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Overflow;
            nameText.color = Color.white;
            nameText.rectTransform.sizeDelta = new Vector2(3.3f, 0.8f);
            nameText.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            nameText.sortingLayerID = sortingLayerId;
            nameText.sortingOrder = sortingOrder + 3;
        }

        _maturityHighlight.SetActive(false);
        _flowerNamePlate.SetActive(false);
    }

    private void UpdateMaturityPresentation()
    {
        if (_maturityHighlight != null) _maturityHighlight.SetActive(_isFullyGrown);
        if (_flowerNamePlate != null) _flowerNamePlate.SetActive(_isFullyGrown);
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
            _hasGivenSun = false;
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
            SaveState();
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

    // ===== Сохранение / загрузка состояния цветка =====
    public string FlowerId
    {
        get
        {
            if (string.IsNullOrEmpty(flowerId))
            {
                flowerId = System.Guid.NewGuid().ToString();
            }
            return flowerId;
        }
    }

    /// <summary>Восстанавливает сохранённый идентификатор цветка (используется при загрузке).</summary>
    public void AssignFlowerId(string id)
    {
        if (!string.IsNullOrEmpty(id)) flowerId = id;
    }
    public string PrefabName { get => _prefabName; set => _prefabName = value; }
    public string SpriteName
    {
        get
        {
            var sr = GetComponent<SpriteRenderer>();
            return sr != null && sr.sprite != null ? sr.sprite.name : string.Empty;
        }
    }
    public int CurrentStageIndex => _currentStageIndex;
    public float TimeSinceLastWatering => _timeSinceLastWatering;
    public float TimeSinceLastSunGeneration => _timeSinceLastSunGeneration;
    public bool NeedWater => _needWater;
    public bool NeedFertilize => _needFertilize;
    public bool IsFullyGrown => _isFullyGrown;
    public int CareEventCount => _careEventCount;
    public string GrowthConditionsName => _conditions != null ? _conditions.name : string.Empty;
    public bool HasGivenSun => _hasGivenSun;

    /// <summary>
    /// Передаёт текущее состояние цветка в GameSaveManager для сохранения.
    /// Вызывается при любом изменении статуса (полив, рост, солнце, потребности).
    /// </summary>
    public void SaveState()
    {
        if (GameSaveManager.IsLoading) return;
        if (GameSaveManager.Instance != null)
        {
            GameSaveManager.Instance.SaveFlower(this);
        }
    }

    /// <summary>
    /// Применяет к цветку данные из сохранения.
    /// Вызывается GameSaveManager при загрузке игры.
    /// </summary>
    public void LoadFromData(FlowerSaveData data)
    {
        if (data == null) return;

        _prefabName = data.prefabName;
        _currentStageIndex = data.currentStageIndex;
        _timeSinceLastWatering = data.timeSinceLastWatering;
        _timeSinceLastSunGeneration = data.timeSinceLastSunGeneration;
        _needWater = data.needWater;
        _needFertilize = data.needFertilize;
        _isFullyGrown = data.isFullyGrown;
        _careEventCount = data.careEventCount;
        _hasGivenSun = data.hasGivenSun;

        if (_conditions != null && _conditions.StageSprites != null && _conditions.StageSprites.Length > 0 &&
            _currentStageIndex >= _conditions.StageSprites.Length - 1)
        {
            // Миграция старых сохранений, где взрослая стадия наступала на один полив позже.
            _isFullyGrown = true;
        }

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && _conditions != null && _conditions.StageSprites != null && _conditions.StageSprites.Length > 0)
        {
            sr.sprite = _conditions.StageSprites[Mathf.Clamp(_currentStageIndex, 0, _conditions.StageSprites.Length - 1)];
            UpdateColliderToCurrentSprite();
        }

        if (sunIconObj != null) sunIconObj.SetActive(_hasGivenSun);
        UpdateNeedIcons();
        UpdateMaturityPresentation();
    }

    /// <summary>
    /// Продвигает только временные процессы за период отсутствия игрока.
    /// Уход по-прежнему требует действия игрока, поэтому за офлайн-период может
    /// появиться максимум одна нерешённая потребность или одна иконка солнца.
    /// </summary>
    public void ApplyOfflineProgress(float elapsedSeconds)
    {
        if (elapsedSeconds <= 0f || _conditions == null) return;
        if (_needWater || _needFertilize || _hasGivenSun) return;

        float wateringInterval = Mathf.Max(0f, Conditions.TimeBetweenWatering);
        float timeToWater = Mathf.Max(0f, wateringInterval - _timeSinceLastWatering);

        float timeToSun = float.PositiveInfinity;
        if (_isFullyGrown)
        {
            float sunInterval = Mathf.Max(0f, Conditions.SunGenerationInterval);
            timeToSun = Mathf.Max(0f, sunInterval - _timeSinceLastSunGeneration);
        }

        float timeToFirstEvent = Mathf.Min(timeToWater, timeToSun);
        float activeTime = Mathf.Min(elapsedSeconds, timeToFirstEvent);
        _timeSinceLastWatering += activeTime;
        if (_isFullyGrown) _timeSinceLastSunGeneration += activeTime;

        // При одновременном срабатывании потребность в воде имеет тот же приоритет,
        // что и в Update().
        if (elapsedSeconds >= timeToWater)
        {
            _needWater = true;
        }
        else if (_isFullyGrown && elapsedSeconds >= timeToSun)
        {
            _timeSinceLastSunGeneration = 0f;
            SpawnSunIcon();
        }

        UpdateNeedIcons();
    }
}
