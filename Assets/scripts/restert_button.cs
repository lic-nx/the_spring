using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restert_button : MonoBehaviour
{
    public AudioClip restartSound; // Назначьте звук в инспекторе

    private AudioSource audioSource;

    void Start()
    {
        // Добавляем AudioSource, если его нет
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D-звук
        }
    }

    public void RestartButtonPressed()
    {
        StartCoroutine(RestartWithSound());
    }

    IEnumerator RestartWithSound()
    {
        // Проигрываем звук
        if (restartSound != null)
        {
            audioSource.PlayOneShot(restartSound);
            // Ждём окончания звука + небольшой запас
            yield return new WaitForSecondsRealtime(0.01f);
        }

        // Восстанавливаем время (на случай паузы)
        Time.timeScale = 1f;

        // Только теперь перезагружаем сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}