using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class Mainmenu : Button_sound_controller
{
    public AudioClip soundClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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