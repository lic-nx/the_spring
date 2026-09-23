using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using YG;

public class Mainmenu : Button_sound_controller
{
    private const string GardenSceneName = "Garden";
    public AudioClip soundClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        EnsureGardenButton();
    }

    private void EnsureGardenButton()
    {
        if (GameObject.Find("GardenButton") != null) return;

        var template = GameObject.Find("ChooseLevelButton");
        if (template == null)
        {
            Debug.LogWarning("[Mainmenu] ChooseLevelButton не найден — кнопка сада не создана.");
            return;
        }

        var gardenButtonObject = Instantiate(template, template.transform.parent);
        gardenButtonObject.name = "GardenButton";
        gardenButtonObject.transform.SetSiblingIndex(template.transform.GetSiblingIndex() + 1);

        var rect = gardenButtonObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(-300f, -304f);
            rect.localScale = new Vector3(1.1f, 1.1f, 1f);
        }

        var label = gardenButtonObject.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = "Сад";

        var button = gardenButtonObject.GetComponent<Button>();
        if (button == null)
        {
            Destroy(gardenButtonObject);
            Debug.LogError("[Mainmenu] На шаблоне ChooseLevelButton нет компонента Button.");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OpenGarden);
    }
    private const string SCENE_LIST_PATH = "Assets/scripts/SceneList.txt";
    private List<string> sceneList = new List<string>();

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
                Debug.Log($"Loaded {sceneList.Count} scenes for Mainmenu.");
            }
            else
            {
                Debug.LogWarning($"Scene list not found at {fullPath}");
            }
        }
        
        catch ( Exception e)
        {
            Debug.LogError($"Error loading scene list in Mainmenu: {e.Message}");
        }
    }

    public void PlayGame()
    {
        // Ensure scene list is loaded
        if (sceneList == null || sceneList.Count == 0) LoadSceneList();

        int lineIndex = YG2.saves.CompletedLevel + 1; // zero‑based index; next level is +1
        if (lineIndex < 0 || lineIndex >= sceneList.Count)
        {
            Debug.LogWarning("Invalid level index from CompletedLevel; loading fallback.");
            return;
        }
        string targetScene = sceneList[lineIndex];
        // Check for tutorial with same numeric suffix (assumes naming convention)
        string tutorialName = $"tutorial_{targetScene}";
        if (SceneUtility.GetBuildIndexByScenePath(tutorialName) < 0)
        {
            int sceneIdx = SceneUtility.GetBuildIndexByScenePath(targetScene);
            if (sceneIdx >= 0) SceneManager.LoadScene(targetScene);
            else Debug.LogWarning($"Scene '{targetScene}' not found in Build Settings.");
        }
        else
        {
            Debug.LogWarning($"Tutorial scene '{tutorialName}' exists; not loading '{targetScene}'.");
        }
    }

    public void ChooseLevel()
    {
        float delay = soundClip ? soundClip.length + 0.1f : 0.1f;
        PlayMusicWithDelay(soundClip, delay, () => SceneManager.LoadScene("levelSelection"));
    }

    public void OpenGarden()
    {
        float delay = soundClip ? soundClip.length + 0.1f : 0.1f;
        PlayMusicWithDelay(soundClip, delay, () => SceneManager.LoadScene(GardenSceneName));
    }

    public void ExitGame()
    {
        float delay = soundClip ? soundClip.length + 0.1f : 0.1f;
        PlayMusicWithDelay(soundClip, delay, () =>
        {
            Debug.Log("Игра закрыта");
            Application.Quit();
        });
    }
}
