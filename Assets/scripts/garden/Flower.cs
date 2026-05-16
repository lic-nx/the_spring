using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class Flower : MonoBehaviour
{
    // Serialized backing fields – visible in the Inspector
    [SerializeField] private GrowthConditions _conditions; // assign via inspector
    // Sprites for each growth stage (0 – seed, 1 – sprout, 2 – young shoot, 3 – bloom)
    [SerializeField] private Sprite[] stageSprites;
    // Optional: you can expose the current stage for debugging
    // [SerializeField] private SeedStage _seedStage;

    public IGrowthStage CurrentStage { get; private set; }
    public GrowthConditions Conditions => _conditions;

    // Time counters for care actions
    private float _timeSinceLastWatering;
    private float _timeSinceLastFertilizing;
    private float _timeSinceLastSunlight;

    /// <summary>
    /// Initializes the flower with its growth conditions and starts at the seed stage.
    /// </summary>
    public void Initialize(GrowthConditions conditions)
    {
        _conditions = conditions;
        CurrentStage = new SeedStage(this);
        // Reset care timers
        _timeSinceLastWatering = 0f;
        _timeSinceLastFertilizing = 0f;
        _timeSinceLastSunlight = 0f;
    }

    private void Awake()
    {
        // Ensure we always have a GrowthConditions reference.
        if (_conditions == null)
        {
            _conditions = Resources.Load<GrowthConditions>("DefaultGrowthConditions");
            if (_conditions == null)
            {
                // Fallback to a default instance with validated values.
                _conditions = ScriptableObject.CreateInstance<GrowthConditions>();
                // Values are already validated by GrowthConditions.OnValidate().
            }
        }
        // Initialise the growth stage if not set.
        if (CurrentStage == null)
        {
            CurrentStage = new SeedStage(this);
        }
    }

    private void Update()
    {
        // Update timers
        _timeSinceLastWatering += Time.deltaTime;
        _timeSinceLastFertilizing += Time.deltaTime;
        _timeSinceLastSunlight += Time.deltaTime;

        CheckGrowthConditions();
        CurrentStage.Update();
    }

    public void Water() => _timeSinceLastWatering = 0f;
    public void Fertilize() => _timeSinceLastFertilizing = 0f;
    public void ProvideSunlight() => _timeSinceLastSunlight = 0f;

    private void CheckGrowthConditions()
    {
        // Notify player if care is overdue – actual UI handling is done elsewhere.
        if (CurrentStage is SeedStage && _timeSinceLastWatering >= Conditions.TimeBetweenWatering)
        {
            Debug.Log("Seed needs watering");
        }
        if (CurrentStage is SeedStage && _timeSinceLastFertilizing >= Conditions.TimeBetweenFertilizing)
        {
            Debug.Log("Seed needs fertilizing");
        }
        if (CurrentStage is SeedStage && _timeSinceLastSunlight >= Conditions.TimeBetweenSunlight)
        {
            Debug.Log("Seed needs sunlight");
        }
    }

    public void AdvanceToNextStage() => CurrentStage = CurrentStage.NextStage();
}