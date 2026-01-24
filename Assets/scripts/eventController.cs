using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using YG;

public class eventController : Button_sound_controller
{
    public static eventController Instance { get; private set; }
    public GameObject winPanel;
    public ParticleSystem particles;
    public GameObject gameOverPanel;
    public GameObject someObject;
    public bool isTutorial;

    public AudioClip gameWinSound;
    public AudioClip gameErrorSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Не нужно: audioSource.clip = gameErrorSound; — PlayOneShot использует clip напрямую
    }

    //void Start()
    //{
    //    Debug.Log("Camera.main: " + (Camera.main ? Camera.main.name : "null"));
        
    //}

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void OnEndGame() {
        if (someObject != null)
        {
            someObject.SetActive(false);
            Debug.Log("Кнопка паузы выключена");
        }

        //Time.timeScale = 0f;
        //Debug.Log("Игра на паузе");
        audioSource.PlayOneShot(gameWinSound);
        if (SceneManager.GetActiveScene().buildIndex >= YG2.saves.ReachedIndex && !isTutorial)
        {
            YG2.saves.UnlockedLevel += 1;
            YG2.saves.CompletedLevel += 1;
            YG2.saves.ReachedIndex += 1;
            YG2.SaveProgress();
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            particles.Play();
            Debug.Log("winPanel активен");
        }
        else
        {
            Debug.LogError("winPanel не указан!");
        }

        Debug.Log("Вызов победного экрана окончен");

    }
    public void PlayerLose()
    {
        if (someObject != null)
        {
            someObject.SetActive(false);
        }

        //Time.timeScale = 0f;
        //Debug.Log("Игра на паузе");

        if (gameOverPanel != null)
        {
            audioSource.PlayOneShot(gameErrorSound);
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("gameOverPanel не установлен!");
        }

    }
}
