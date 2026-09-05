using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class Level_display : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public bool Tutorial=false;
    public bool Win;
    public bool Pause;

    private LocalizationManager localizationManager;

    private void Start()
    {
        if (levelText == null)
        {
            Debug.LogError($"{nameof(Level_display)} on '{name}' has no levelText assigned.", this);
            enabled = false;
            return;
        }

        localizationManager = LocalizationManager.Instance;
        if (localizationManager == null)
        {
            Debug.LogError($"{nameof(LocalizationManager)} is not available.", this);
            enabled = false;
            return;
        }

        var match = Regex.Match(SceneManager.GetActiveScene().name, @"\d+$");
        levelText.text = match.Success
            ? $"{match.Value}"
            : $"?";

        UpdateText();
        localizationManager.OnLanguageChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (localizationManager != null)
            localizationManager.OnLanguageChanged -= UpdateText;
    }

    void UpdateText()
    {
        if (Pause)
        {
            levelText.text = localizationManager.GetText("level", levelText.text);
            return;
        }
        if (Win)
        {
            levelText.text = localizationManager.GetText(Tutorial ? "tutorial_win" : "win", levelText.text);
            return;
        }
        else
        {
            levelText.text = localizationManager.GetText(Tutorial ? "tutorial_loose" :"loose", levelText.text);
            return;
        }

    }
}
