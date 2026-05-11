using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text.RegularExpressions;

public class Level_display : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public bool Win;
    public bool Pause;

    private static readonly System.Collections.Generic.Dictionary<string, int> sceneIndices;
    static Level_display()
    {
        var path = System.IO.Path.Combine(Application.dataPath, "scripts/SceneList.txt");
        var dict = new System.Collections.Generic.Dictionary<string, int>();
        if (System.IO.File.Exists(path))
        {
            var lines = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                var name = lines[i].Trim();
                if (!string.IsNullOrEmpty(name) && !dict.ContainsKey(name))
                {
                    dict[name] = i + 1; // 1‑based index
                }
            }
        }
        sceneIndices = dict;
    }

    private void Start()
    {
        var sceneName = SceneManager.GetActiveScene().name;
        int index = 0;
        if (sceneIndices != null && sceneIndices.TryGetValue(sceneName, out var val))
        {
            index = val;
        }
        levelText.text = index > 0 ? index.ToString() : "?";

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