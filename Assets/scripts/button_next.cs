using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
public class button_next : Button_sound_controller
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
    public void NextLevel()
    {
        int nextIndex = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings;
        PlayMusicWithDelay(soundClip, soundClip.length, () => SceneManager.LoadScene(nextIndex));

    }
}
