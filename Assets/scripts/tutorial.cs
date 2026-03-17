using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class tutorial : MonoBehaviour
{
    public GameObject[] overlayObjects;

    void OnEnable()
    {
        TutorialBlock.OnAnyBlockChanged += DisableTutorial;
    }

    void OnDisable()
    {
        TutorialBlock.OnAnyBlockChanged -= DisableTutorial;
    }

    void DisableTutorial()
    {
        foreach (var obj in overlayObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        enabled = false;
    }
}