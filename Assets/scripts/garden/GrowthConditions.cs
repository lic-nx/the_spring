using UnityEngine;
/// <summary>
/// Holds configuration values that define how a flower grows and what care it needs.
/// These values are typically set per flower species in the Unity inspector.
/// </summary>
[CreateAssetMenu(fileName = "GrowthConditions", menuName = "Garden/Growth Conditions")]
public class GrowthConditions : ScriptableObject
{
    [Header("Care timing (seconds)")]
    [Min(0f)]
    public float TimeBetweenWatering = 30f;
    [Min(0f)]
    public float TimeBetweenFertilizing = 60f;
    [Min(0f)]
    public float TimeBetweenSunlight = 45f;

    [Header("Growth timing (seconds)")]
    [Min(0f)]
    public float TimeToSprout = 20f;
    [Min(0f)]
    public float TimeToYoungShoot = 40f;
    [Min(0f)]
    public float TimeToBloom = 80f;

    private void OnValidate()
    {
        // Ensure no negative timings.
        TimeBetweenWatering = Mathf.Max(0f, TimeBetweenWatering);
        TimeBetweenFertilizing = Mathf.Max(0f, TimeBetweenFertilizing);
        TimeBetweenSunlight = Mathf.Max(0f, TimeBetweenSunlight);
        TimeToSprout = Mathf.Max(0f, TimeToSprout);
        TimeToYoungShoot = Mathf.Max(0f, TimeToYoungShoot);
        TimeToBloom = Mathf.Max(0f, TimeToBloom);
    }
}
