using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Growth stage representing a sprouted flower.
public class SproutStage : IGrowthStage
{
    private Flower _flower;
    private float _timeInStage;

    public SproutStage(Flower flower)
    {
        _flower = flower;
        _timeInStage = 0f;
    }

    // Update called each frame; transition to next stage when time elapsed.
    public void Update()
    {
        _timeInStage += Time.deltaTime;
        if (_timeInStage >= _flower.Conditions.TimeToYoungShoot)
        {
            _flower.AdvanceToNextStage();
        }
    }

    // Return next stage (placeholder – stays in same stage for now).
    public IGrowthStage NextStage()
    {
        // TODO: implement further stages like YoungShootStage.
        return this;
    }
}
