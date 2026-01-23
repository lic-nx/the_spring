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
        // Не нужно: audioSource.clip = soundClip; — PlayOneShot использует clip напрямую
    }
    public void PlayGame()
    {
        float delay = soundClip ? soundClip.length + 0.1f : 0.1f;
        PlayMusicWithDelay(soundClip, delay, () => SceneManager.LoadScene(YG2.saves.CompletedLevel + 2));
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