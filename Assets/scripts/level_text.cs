using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using YG;

public class level_text : MonoBehaviour
{

    public TextMeshProUGUI levelText;

    void Start()
    {
        int CompletedLevel = YG2.saves.CompletedLevel + 1;
        YG2.SaveProgress();
        if (levelText != null)
        {
            levelText.text =  CompletedLevel + " уровень";
        }
        else
        {
            Debug.LogError("TextMeshPro component is not assigned!");
        }
    }
}

