using System.Collections;
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
    public void PlayGame()
    {
        string sceneName = "level_"+(YG2.saves.CompletedLevel+1);
        int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneName);
        if (buildIndex >= 0)
        {   
            string tutorName = "tutorial_" + YG2.saves.CompletedLevel+1;
            buildIndex = SceneUtility.GetBuildIndexByScenePath(tutorName);
            if (buildIndex >= 0) SceneManager.LoadScene(tutorName);
            else
                SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Сцены нет в билде — перекидываем на заглушку
            Debug.LogWarning($"⚠️ Сцена '{sceneName}' не найдена в Build Settings! Загружаем запасную.");
            
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