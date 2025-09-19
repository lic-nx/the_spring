using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class level_text : MonoBehaviour
{

    public TextMeshProUGUI levelText;

    void Start()
    {
        int CompletedLevel = PlayerPrefs.GetInt("CompletedLevel", 0) + 1;

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

