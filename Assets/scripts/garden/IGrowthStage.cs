using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public interface IGrowthStage
{
    /// <summary>
    /// Called each frame while the flower is in this growth stage.
    /// </summary>
    void Update();

    /// <summary>
    /// Returns the next growth stage instance. Returns the same instance if this is the final stage.
    /// </summary>
    IGrowthStage NextStage();
}

public class SeedStage : IGrowthStage
{
    private Flower _flower;
    private float _timeInStage;

    public SeedStage(Flower flower)
    {
        _flower = flower;
        _timeInStage = 0;
    }

    public void Update()
    {
        _timeInStage += Time.deltaTime;
        if (_timeInStage >= _flower.Conditions.TimeToSprout)
        {
            _flower.AdvanceToNextStage();
        }
    }

    public IGrowthStage NextStage() => new SproutStage(_flower);
}

// Placeholder for other stages (SproutStage, YoungShootStage, BloomedFlowerStage) – implement similarly when needed.