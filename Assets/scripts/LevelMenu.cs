using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;
using System;
using System.IO; 
using UnityEngine.Rendering.PostProcessing;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;
using System.Collections.Generic;
using System.IO;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    public Image Complite;
    public Image Lock;         // Объект с компонентом Image
    public Image Unlock;
    public GameObject levelButtons;
    public GameObject openLevel;
    private const string FALLBACK_SCENE_NAME = "sorry_not_founded"; // имя сцены если нет уровня
    private static bool isFallbackLoading = false;
    private List<string> sceneList = new List<string>(); // список сцен из файла
    private const string SCENE_LIST_PATH = "Assets/scripts/SceneList.txt"; // relative to project root

    private void Awake()
    {
        LoadSceneList();
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

    // Load scene names from the external text file. Each line should contain the exact scene name.
    private void LoadSceneList()
    {
        try
        {
            string fullPath = Path.Combine(Application.dataPath, "..", SCENE_LIST_PATH);
            if (File.Exists(fullPath))
            {
                var lines = File.ReadAllLines(fullPath);
                sceneList.Clear();
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed)) sceneList.Add(trimmed);
                }
                Debug.Log($"Loaded {sceneList.Count} scene names from {SCENE_LIST_PATH}");
            }
            else
            {
                Debug.LogWarning($"Scene list file not found at {fullPath}. Falling back to empty list.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading scene list: {e.Message}");
        }
    }

    void ButtonsToArray()
    {
        int childCount = levelButtons.transform.childCount;
        buttons = new Button[childCount];
        for (int i = 0; i < childCount; i++)
        {
            buttons[i] = levelButtons.transform.GetChild(i).GetComponent<Button>();
            // Assign button label based on its index (1‑based)
            var txt = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = (i + 1).ToString();
        }
    }

    public void OpenLevel(int buttonIndex)
    {
        buttonIndex = buttonIndex - 1;
        // buttonIndex is expected to be 0‑based from the UI button click
        if (buttonIndex < 0 || buttonIndex >= sceneList.Count)
        {
            Debug.LogWarning($"Invalid level index {buttonIndex}. Loading fallback.");
            LoadFallbackScene();
            return;
        }
        string sceneName = sceneList[buttonIndex];
        Debug.LogWarning($" level index {buttonIndex}. scenename = {sceneName}");
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (buildIndex >= 0)
        {
            // Check for a tutorial with matching numeric suffix if it exists
            string tutorialName = $"tutorial_{sceneName}"; // assumes numeric suffix matches button order
            int tutorIndex = SceneUtility.GetBuildIndexByScenePath(tutorialName);
            if (tutorIndex >= 0) SceneManager.LoadScene(tutorialName);
            else SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning($"Scene '{sceneName}' not found in Build Settings. Loading fallback.");
            LoadFallbackScene();
        }
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
