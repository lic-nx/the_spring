using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    public string key;
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateText();
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
    }

    void UpdateText()
    {
        textComponent.text = LocalizationManager.Instance.GetText(key);
    }
}
