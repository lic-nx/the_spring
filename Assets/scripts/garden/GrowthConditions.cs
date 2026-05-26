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
    // ------------------- Параметры ухода (в секундах) -------------------
    [Header("Care timing (seconds)")] // Заголовок группы в инспекторе
    [Min(0f)] // Минимальное значение – не допускаем отрицательных чисел
    public float TimeBetweenWatering = 30f; // Интервал между поливами
    [Min(0f)]
    public float TimeBetweenFertilizing = 60f; // Интервал между удобрениями
    [Min(0f)]
    public float TimeBetweenSunlight = 45f; // Интервал между воздействием света

    // ----------------- Параметры роста (в секундах) -------------------
    [Header("Growth timing (seconds)")]
    [Min(0f)]
    public float TimeToSprout = 20f; // Время от семени до всхода
    [Min(0f)]
    public float TimeToYoungShoot = 40f; // Время от всхода до молодого побега
    [Min(0f)]
    public float TimeToBloom = 80f; // Время от молодого побега до цветения

    // Unity‑callback, вызываемый при изменении значения в инспекторе
    private void OnValidate()
    {
        // Защищаем от отрицательных значений, принудительно задавая минимум 0
        TimeBetweenWatering = Mathf.Max(0f, TimeBetweenWatering);
        TimeBetweenFertilizing = Mathf.Max(0f, TimeBetweenFertilizing);
        TimeBetweenSunlight = Mathf.Max(0f, TimeBetweenSunlight);
        TimeToSprout = Mathf.Max(0f, TimeToSprout);
        TimeToYoungShoot = Mathf.Max(0f, TimeToYoungShoot);
        TimeToBloom = Mathf.Max(0f, TimeToBloom);
    }
}
