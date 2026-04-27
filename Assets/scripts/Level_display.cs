using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class Level_display : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public bool Win;
    public bool Pause;

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
        if (Pause)
        {
            levelText.text = LocalizationManager.Instance.GetText("level", levelText.text);
            return;
        }
        if (Win)
        {
            levelText.text = LocalizationManager.Instance.GetText("win", levelText.text);
            return;
        }
        else
        {
            levelText.text = LocalizationManager.Instance.GetText("loose", levelText.text);
            return;
        }

    }
}