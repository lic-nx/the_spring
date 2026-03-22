using UnityEngine;
using System.Collections;

public class Button_sound_controller : MonoBehaviour
{
   public void PlayMusicWithDelay(AudioClip musicClip, float delay, System.Action onComplete = null, float? volume = null)
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

    // ✅ Если громкость не передана — берём из сохранений, иначе используем переданное значение
    float finalVolume = volume ?? 0.5f;
    
    // Ограничиваем диапазон 0..1 на всякий случай
    finalVolume = Mathf.Clamp01(finalVolume);

    // ✅ Передаём громкость вторым параметром в PlayOneShot
    audioSource.PlayOneShot(musicClip, finalVolume);

    // Запускаем корутину с задержкой
    StartCoroutine(DelayedCallback(delay, onComplete));
}

    private IEnumerator DelayedCallback(float delay, System.Action callback)
    {
        Debug.LogWarning("����� ���������");
        yield return new WaitForSecondsRealtime(delay);
        Debug.LogWarning("�������� ���������");
        callback?.Invoke();
    }
}