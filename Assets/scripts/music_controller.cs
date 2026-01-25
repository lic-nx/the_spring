using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using YG;

public class MusicController : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle; // UI Toggle вместо кнопки

    private bool isEnabled = true;
    private AudioSource[] musicSources;

    //private static readonly string MUSIC_ENABLED_KEY = "MusicEnabled"; // Ключ в PlayerPrefs

    void Start()
    {
        // Загружаем сохранённое состояние (по умолчанию — включено)
        isEnabled = YG2.saves.MusicEnabled == 1;

        // Находим все источники музыки на сцене
        FindMusicSources();

        // Применяем текущее состояние (воспроизведение или пауза)
        ApplyMusicState();

        // Настраиваем UI Toggle
        if (musicToggle != null)
        {
            musicToggle.isOn = isEnabled; // Устанавливаем положение тумблера
            musicToggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    // Поиск всех объектов с тегом "Music" и получение их AudioSource
    private void FindMusicSources()
    {
        GameObject[] musicObjects = GameObject.FindGameObjectsWithTag("Music");
        musicSources = new AudioSource[musicObjects.Length];

        for (int i = 0; i < musicObjects.Length; i++)
        {
            musicSources[i] = musicObjects[i].GetComponent<AudioSource>();
            if (musicSources[i] == null)
            {
                Debug.LogWarning($"Объект {musicObjects[i].name} имеет тег 'Music', но не имеет компонента AudioSource.");
            }
        }
    }

    // Применить состояние музыки: включить или поставить на паузу
    private void ApplyMusicState()
    {
        if (!isEnabled)
        {
            ResumeAll();
        }
        else
        {
            PauseAll();
        }
    }

    // Обработчик изменения состояния Toggle
    private void OnToggleValueChanged(bool isOn)
    {
        isEnabled = isOn;

        if (!isEnabled)
        {
            ResumeAll();
        }
        else
        {
            PauseAll();
        }

        // Сохраняем новое состояние
        YG2.saves.MusicEnabled = YG2.saves.MusicEnabled == 0 ? 1 : 0;
        YG2.SaveProgress();
        //PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, isEnabled ? 1 : 0);
        //PlayerPrefs.Save(); // Надёжное сохранение
    }

    // Продолжить воспроизведение всех источников
    private void ResumeAll()
    {
        foreach (AudioSource source in musicSources)
        {
            if (source != null && !source.isPlaying)
            {
                source.UnPause();
            }
        }
    }

    // Поставить на паузу все источники
    private void PauseAll()
    {
        foreach (AudioSource source in musicSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Pause();
            }
        }
    }
}