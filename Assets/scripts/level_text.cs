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
        UpdateText();
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }

    void UpdateText()
    {
        int completedLevel = YG2.saves.CompletedLevel + 1;
        levelText.text = LocalizationManager.Instance.GetText("level", completedLevel);
    }
}

