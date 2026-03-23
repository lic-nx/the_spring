using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using YG;

public class EventControllerScr : Button_sound_controller
{
    public static EventControllerScr Instance { get; private set; }
    
    [SerializeField] private UIController UI;
    public bool isTutorial = false;

    public AudioClip gameWinSound;
    public AudioClip gameErrorSound;
    public AudioClip buttonPressed;
    
    private AudioSource audioSource;

    private void Awake()
    {
        // ✅ 1. Сначала инициализируем AudioSource (ДО любого использования!)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // ✅ 2. Настраиваем AudioSource
        audioSource.ignoreListenerPause = true;  // звук играет даже на паузе
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;           // 2D-звук (без затухания)

        // ✅ 3. Логика синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        // DontDestroyOnLoad(gameObject); // раскомментируйте, если нужно сохранение между сценами
    }

    public void RegisterUI(UIController ui)
    {
        UI = ui;
    }

    // ✅ Хелпер-метод для безопасного воспроизведения звуков
    private void PlaySFX(AudioClip clip)
    {
        if (!YG2.saves.EffectMusicEnabled) return;
        if (audioSource == null || clip == null) return;
        
        audioSource.PlayOneShot(clip, 0.5f);
        
    }

    public void OnEndGame() 
    {
        string sceneName = SceneManager.GetActiveScene().name;
        string[] nameParts = sceneName.Split('_');
        int levelType = 0;
        
        if (nameParts.Length > 0)
        {
            int.TryParse(nameParts[nameParts.Length - 1], out levelType);
        }
        
        UnityEngine.Debug.Log($"Уровень обучающий: {isTutorial} | Текущий индекс сцены: {levelType} | ReachedIndex: {YG2.saves.ReachedIndex}");

        if (levelType >= YG2.saves.ReachedIndex && !isTutorial)
        {
            YG2.saves.UnlockedLevel += 1;
            YG2.saves.CompletedLevel += 1;
            YG2.saves.ReachedIndex += 1;
            YG2.SaveProgress();
        }
        
        PlaySFX(gameWinSound);  // ✅ Безопасный вызов
        UI?.ShowWin();          // ✅ Null-conditional оператор
    }

    public void PlayerLose()
    {
        PlaySFX(gameErrorSound);
        UI?.ShowLoose();
    }

    public void PlayerPause()
    {
        PlaySFX(buttonPressed);
        UI?.ShowPause();
        Time.timeScale = 0f;
    }

    public void PlayerUnPause()
    {
        PlaySFX(buttonPressed);
        UI?.HidePause();
        Time.timeScale = 1f;
    }
}