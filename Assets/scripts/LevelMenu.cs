using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;
using System;
using UnityEngine.Rendering.PostProcessing;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;

public class LevelMenu : MonoBehaviour
{
    private Canvas canvas;
    public Button[] buttons;
    public Image Complite;
    public Image Lock;         // Объект с компонентом Image
    public Image Unlock;
    public GameObject levelButtons;
    public GameObject openLevel;
    private const string FALLBACK_SCENE_NAME = "sorry_not_founded"; // имя сцены если нет уровня
    private static bool isFallbackLoading = false;

    private void Awake()
    {
        ButtonsToArray();
        int unlockedLevel = YG2.saves.UnlockedLevel;
        int CompletedLevel = YG2.saves.CompletedLevel;
        Debug.Log($"unlockedLevel {unlockedLevel}");
        Debug.Log($"CompletedLevel {CompletedLevel}");
        Debug.Log($"buttons.Length {buttons.Length}");
        for (int i = unlockedLevel; i < buttons.Length ; i++)
        {
            buttons[i].enabled = false;
            buttons[i].GetComponent<Image>().sprite = Lock.sprite;
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
        }

        for (int i = CompletedLevel; i < unlockedLevel && i < buttons.Length; i++)
        {
            buttons[i].enabled = true;
            buttons[i].GetComponent<Image>().sprite = Unlock.sprite;
            // Use index from scene list (1‑based)
            int displayNumber = i + 1;
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = displayNumber.ToString();
            if (i == CompletedLevel && openLevel != null)
            {
                Transform buttonParent = buttons[i].transform.parent;
                GameObject instance = Instantiate(openLevel, buttonParent);
                instance.transform.position = buttons[i].transform.position;
                instance.transform.SetSiblingIndex(Mathf.Max(0, buttons[i].transform.GetSiblingIndex() - 1));
                Image bg = instance.GetComponent<Image>();
                if (bg != null) bg.raycastTarget = false;
            }
        }
        for (int i = 0; i < CompletedLevel; i++)
        {
            buttons[i].enabled = true;
            buttons[i].GetComponent<Image>().sprite = Complite.sprite;
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = (i + 1).ToString();
        }
    }

    void ButtonsToArray()
    {
        int childCount = levelButtons.transform.childCount;
        buttons = new Button[childCount];
        for (int i = 0; i < childCount; i++)
        {
            var btn = levelButtons.transform.GetChild(i).GetComponent<Button>();
            buttons[i] = btn;
            // Assign button label based on its index (1‑based)
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = (i + 1).ToString();
        }
        // Cache for visibility checks
        canvas = GetComponentInParent<Canvas>();
    }

    public void OpenLevel(int buttonIndex)
    {
        if (!TryLoadLevel(buttonIndex))
        {
            Debug.LogWarning($"Level {buttonIndex} is not included in the build.");
            LoadFallbackScene();
        }
    }

    internal static bool TryLoadLevel(int levelNumber)
    {
        if (levelNumber < 1)
            return false;

        int level = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/level_{levelNumber}.unity");
        if (level < 0)
            return false;

        int tutorial = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/tutorial_level_{levelNumber}.unity");
        if (tutorial < 0)
            tutorial = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/tutorial_{levelNumber}.unity");

        SceneManager.LoadScene(tutorial >= 0 ? tutorial : level);
        return true;
    }

    private void LoadFallbackScene()
    {
        isFallbackLoading = true;
        Scene scene = SceneManager.GetSceneByName(FALLBACK_SCENE_NAME);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
        }
        else
        {
            try
            {
                SceneManager.LoadScene(FALLBACK_SCENE_NAME);
            }
            catch (Exception e)
            {
                Debug.LogError($"Critical error: Could not load fallback scene '{FALLBACK_SCENE_NAME}'. {e.Message}");
            }
        }
    }

    public void Restart_button()
    {
        YG2.saves.UnlockedLevel = 1;
        YG2.saves.CompletedLevel = 0;
        YG2.saves.ReachedIndex = 0;
        YG2.SaveProgress();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
