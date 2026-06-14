// ------------------------------------------------------------
// GrowthConditions.cs – конфигурация условий роста цветка (ScriptableObject)
// ------------------------------------------------------------
// Этот скрипт описывает набор параметров, определяющих требования к уходу и
// временные интервалы роста для каждого вида цветка. Параметры задаются в
// инспекторе Unity и сохраняются в виде отдельного ассета ScriptableObject.
// ------------------------------------------------------------
using UnityEngine; // Базовые типы Unity, включая ScriptableObject и Mathf

/// <summary>
/// ScriptableObject, содержащий конфигурационные параметры, определяющие
/// как цветок растёт и какой уход требуется. Значения обычно задаются в
/// инспекторе для каждого вида цветка.
/// </summary>
[CreateAssetMenu(fileName = "GrowthConditions", menuName = "Garden/Growth Conditions")]
public class GrowthConditions : ScriptableObject // Наследуем от ScriptableObject, а не MonoBehaviour
{
    [SerializeField] private Sprite[] stageSprites; // Массив спрайтов для разных стадий роста
    // Public read‑only accessor for stage sprites
    public Sprite[] StageSprites => stageSprites;
    // ------------------- Параметры ухода (в секундах) -------------------
    [Header("Care timing (seconds)")]
    [Min(0f)]
    public float TimeBetweenWatering = 30f;
    [Min(0f)]
    public float TimeBetweenFertilizing = 60f;
    [Min(0f)]
    public float TimeBetweenSunlight = 45f;

    // ----------------- Growth event logic -------------------
    [Header("Growth event logic")]
    [Tooltip("Base number of care events required for first stage transition.")]
    [Min(1)]
    public int BaseEventCount = 4;
    [Tooltip("Multiplier applied to required events for each subsequent stage.")]
    [Min(1f)]
    public float StageMultiplier = 2f;

    // Helper to compute required events for a given stage index (0‑based, first sprite is stage 0)
    public int GetRequiredEventsForStage(int stageIndex)
    {
        // Stage 0 (seed) requires 0 events
        if (stageIndex <= 0) return 0;
        // Required = BaseEventCount * StageMultiplier^(stageIndex-1)
        return Mathf.RoundToInt(BaseEventCount * Mathf.Pow(StageMultiplier, stageIndex - 1));
    }

    // ----------------- Параметры роста (в секундах) -------------------
    [Header("Growth timing (seconds)")]
    [Min(0f)]
    public float TimeToSprout = 20f;
    [Min(0f)]
    public float TimeToYoungShoot = 40f;
    [Min(0f)]
    public float TimeToBloom = 80f;

    // Unity‑callback, вызываемый при изменении значения в инспекторе
    private void OnValidate()
    {
        TimeBetweenWatering = Mathf.Max(0f, TimeBetweenWatering);
        TimeBetweenFertilizing = Mathf.Max(0f, TimeBetweenFertilizing);
        TimeBetweenSunlight = Mathf.Max(0f, TimeBetweenSunlight);
        BaseEventCount = Mathf.Max(1, BaseEventCount);
        StageMultiplier = Mathf.Max(1f, StageMultiplier);
        TimeToSprout = Mathf.Max(0f, TimeToSprout);
        TimeToYoungShoot = Mathf.Max(0f, TimeToYoungShoot);
        TimeToBloom = Mathf.Max(0f, TimeToBloom);
    }
}
