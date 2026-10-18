using System.Collections.Generic;
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
    private List<string> sceneList = new List<string>();

    private void LoadSceneList()
    {
        sceneList = LevelSceneCatalog.GetLevelScenes();
        Debug.Log($"Loaded {sceneList.Count} level scenes from Build Settings for Mainmenu.");
    }

    public void PlayGame()
    {
        // Ensure scene list is loaded
        if (sceneList == null || sceneList.Count == 0) LoadSceneList();

        int lineIndex = YG2.saves.CompletedLevel;
        if (lineIndex < 0 || lineIndex >= sceneList.Count)
        {
            Debug.LogWarning("Invalid level index from CompletedLevel; loading fallback.");
            return;
        }
        string targetScene = sceneList[lineIndex];
        string tutorialName = LevelSceneCatalog.GetTutorialScene(targetScene);
        if (string.IsNullOrEmpty(tutorialName))
        {
            if (Application.CanStreamedLevelBeLoaded(targetScene)) SceneManager.LoadScene(targetScene);
            else Debug.LogWarning($"Scene '{targetScene}' not found in Build Settings.");
        }
        else
        {
            SceneManager.LoadScene(tutorialName);
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
