using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using YG;
using System.Diagnostics;

public class EventControllerScr : Button_sound_controller
{
    public static EventControllerScr Instance { get; private set; }
    [SerializeField] private UIController UI;
    public bool isTutorial;

    public AudioClip gameWinSound;
    public AudioClip gameErrorSound;
    public AudioClip buttonPressed;   
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void RegisterUI(UIController ui)
    {
        UI = ui;
    }

    public void OnEndGame() {

        if (SceneManager.GetActiveScene().buildIndex >= YG2.saves.ReachedIndex && !isTutorial)
        {
            YG2.saves.UnlockedLevel += 1;
            YG2.saves.CompletedLevel += 1;
            YG2.saves.ReachedIndex += 1;
            YG2.SaveProgress();
        }

        audioSource.PlayOneShot(gameWinSound);

        UI.ShowWin();

        }
    public void PlayerLose()
    {

        audioSource.PlayOneShot(gameErrorSound);
        UI.ShowLoose();

    }

    public void PlayerPause()
    {
        audioSource.PlayOneShot(buttonPressed);
        UI.ShowPause();
        Time.timeScale = 0f;
    }

    public void PlayerUnPause()
    {
        audioSource.PlayOneShot(buttonPressed);
        UI.HidePause();
        Time.timeScale = 1f;
    }

    public void ButtonPressed()
    {
        audioSource.PlayOneShot(buttonPressed);        

    }
}
