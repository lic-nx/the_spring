using UnityEngine;

public class Flower : MonoBehaviour
{
    [SerializeField] private GrowthConditions _conditions;

    [Header("UI Icons")]
    [SerializeField] private Sprite wateringIcon; // Иконка полива
    [SerializeField] private Sprite fertilizingIcon; // Иконка удобрения
    [SerializeField] private Sprite sunlightIcon; // Иконка света

    [Header("Icon Positioning")]
    [SerializeField] private Vector3 wateringIconOffset = new Vector3(0.5f, 0.5f, 0); // Смещение для иконки полива
    [SerializeField] private Vector3 fertilizingIconOffset = new Vector3(-0.5f, 0.5f, 0); // Смещение для иконки удобрения
    [SerializeField] private Vector3 sunlightIconOffset = new Vector3(0f, 0.8f, 0); // Смещение для иконки света

    // Ссылки на GameObject иконок
    private GameObject wateringIconObj;
    private GameObject fertilizingIconObj;
    private GameObject sunlightIconObj;

    public IGrowthStage CurrentStage { get; private set; }
    public GrowthConditions Conditions => _conditions;

    private float _timeSinceLastWatering;
    private float _timeSinceLastFertilizing;
    private float _timeSinceLastSunlight;

    private bool _needWater;
    private bool _needFertilize;
    private bool _needSunlight;
    private bool _timerPaused;
    private int _careEventCount;
    private int _currentStageIndex;

    public void Initialize(GrowthConditions conditions)
    {
        _conditions = conditions;
        CurrentStage = new SeedStage(this);
        _timeSinceLastWatering = 0f;
        _timeSinceLastFertilizing = 0f;
        _timeSinceLastSunlight = 0f;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && _conditions.StageSprites != null && _conditions.StageSprites.Length > 0)
        {
            sr.sprite = _conditions.StageSprites[0];
        }
        else
        {
            Debug.LogWarning("GrowthConditions missing stage sprites; flower will have no visual.");
        }

        CreateIconObjects(); // Создаем объекты иконок
    }

    private void Awake()
    {
        if (_conditions == null)
        {
            _conditions = Resources.Load<GrowthConditions>("DefaultGrowthConditions");
            if (_conditions == null)
            {
                _conditions = ScriptableObject.CreateInstance<GrowthConditions>();
            }
        }

        if (CurrentStage == null)
        {
            CurrentStage = new SeedStage(this);
        }

        CreateIconObjects(); // Убедимся, что иконки созданы
    }

    // Создание объектов для иконок
    private void CreateIconObjects()
    {
        // Иконка полива
        if (wateringIconObj == null && wateringIcon != null)
        {
            wateringIconObj = new GameObject("WateringIcon");
            wateringIconObj.transform.SetParent(transform);
            wateringIconObj.transform.localPosition = wateringIconOffset;
            var renderer = wateringIconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = wateringIcon;
            renderer.sortingOrder = 10; // Отображаем поверх цветка
            wateringIconObj.SetActive(false); // Изначально скрыта
        }

        // Иконка удобрения
        if (fertilizingIconObj == null && fertilizingIcon != null)
        {
            fertilizingIconObj = new GameObject("FertilizingIcon");
            fertilizingIconObj.transform.SetParent(transform);
            fertilizingIconObj.transform.localPosition = fertilizingIconOffset;
            var renderer = fertilizingIconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = fertilizingIcon;
            renderer.sortingOrder = 10;
            fertilizingIconObj.SetActive(false);
        }

        // Иконка света
        if (sunlightIconObj == null && sunlightIcon != null)
        {
            sunlightIconObj = new GameObject("SunlightIcon");
            sunlightIconObj.transform.SetParent(transform);
            sunlightIconObj.transform.localPosition = sunlightIconOffset;
            var renderer = sunlightIconObj.AddComponent<SpriteRenderer>();
            renderer.sprite = sunlightIcon;
            renderer.sortingOrder = 10;
            sunlightIconObj.SetActive(false);
        }
    }

    private void Update()
    {
        _timeSinceLastWatering += Time.deltaTime;
        _timeSinceLastFertilizing += Time.deltaTime;
        _timeSinceLastSunlight += Time.deltaTime;

        CheckGrowthConditions();
        CurrentStage.Update();
    }

    public void Water()
    {
        _timeSinceLastWatering = 0f;
        _needWater = false;
        RegisterCareEvent();
        TryResumeTimer();
        UpdateNeedIcons();
    }

    public void Fertilize()
    {
        _timeSinceLastFertilizing = 0f;
        _needFertilize = false;
        RegisterCareEvent();
        TryResumeTimer();
        UpdateNeedIcons();
    }

    public void ProvideSunlight()
    {
        _timeSinceLastSunlight = 0f;
        _needSunlight = false;
        RegisterCareEvent();
        TryResumeTimer();
        UpdateNeedIcons();
    }

    private void RegisterCareEvent()
    {
        _careEventCount++;
        TryAdvanceStage();
    }

    private void TryAdvanceStage()
    {
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
                }
            }
            _careEventCount = 0;
            _needWater = _needFertilize = _needSunlight = false;
            _timerPaused = false;
            UpdateNeedIcons();
        }
    }

    private void TryResumeTimer()
    {
        if (!_needWater && !_needFertilize && !_needSunlight)
        {
            _timerPaused = false;
        }
    }

    private void CheckGrowthConditions()
    {
        if (_timerPaused) return;

        _needWater = _timeSinceLastWatering >= Conditions.TimeBetweenWatering;
        _needFertilize = _timeSinceLastFertilizing >= Conditions.TimeBetweenFertilizing;
        _needSunlight = _timeSinceLastSunlight >= Conditions.TimeBetweenSunlight;

        if (_needWater || _needFertilize || _needSunlight)
        {
            float minInterval = float.MaxValue;
            if (_needWater) minInterval = Mathf.Min(minInterval, Conditions.TimeBetweenWatering);
            if (_needFertilize) minInterval = Mathf.Min(minInterval, Conditions.TimeBetweenFertilizing);
            if (_needSunlight) minInterval = Mathf.Min(minInterval, Conditions.TimeBetweenSunlight);

            _timerPaused = true;
            UpdateNeedIcons();
        }
    }

    // Обновление отображения иконок
    private void UpdateNeedIcons()
    {
        if (wateringIconObj != null)
            wateringIconObj.SetActive(_needWater);

        if (fertilizingIconObj != null)
            fertilizingIconObj.SetActive(_needFertilize);

        if (sunlightIconObj != null)
            sunlightIconObj.SetActive(_needSunlight);
    }

    public void AdvanceToNextStage() => CurrentStage = CurrentStage.NextStage();
}