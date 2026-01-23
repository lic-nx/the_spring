using UnityEngine;
using System.Collections;

public class Button_sound_controller : MonoBehaviour
{
    public void PlayMusicWithDelay(AudioClip musicClip, float delay, System.Action onComplete = null)
    {
        if (musicClip == null)
        {
            Debug.LogWarning("Attempted to play null audio clip.");
            onComplete?.Invoke();
            return;
        }

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Воспроизводим музыку
        audioSource.PlayOneShot(musicClip);

        // Запускаем корутину с задержкой
        StartCoroutine(DelayedCallback(delay, onComplete));
    }

    private IEnumerator DelayedCallback(float delay, System.Action callback)
    {
        Debug.LogWarning("начал остановку");
        yield return new WaitForSecondsRealtime(delay);
        Debug.LogWarning("закончил остановку");
        callback?.Invoke();
    }
}