using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class Level_display : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public bool Win;

    private void Start()
    {
        var match = Regex.Match(SceneManager.GetActiveScene().name, @"\d+$");
        levelText.text = match.Success
            ? $"{match.Value}"
            : $"?";

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
        if (Win)
        {
            levelText.text = LocalizationManager.Instance.GetText("win", levelText.text);
        } else
        {
            levelText.text = LocalizationManager.Instance.GetText("win", levelText.text);
        }
    }
}