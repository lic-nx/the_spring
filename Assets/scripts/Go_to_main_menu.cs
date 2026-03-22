using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using YG;


//public class Button_sound_controller : MonoBehaviour
//{
//    public void PlayMusicWithDelay(AudioClip musicClip, float delay, System.Action onComplete = null)
//    {
//        if (musicClip == null)
//        {
//            Debug.LogWarning("Attempted to play null audio clip.");
//            onComplete?.Invoke();
//            return;
//        }

//        AudioSource audioSource = GetComponent<AudioSource>();
//        if (audioSource == null)
//        {
//            audioSource = gameObject.AddComponent<AudioSource>();
//        }

//        // Воспроизводим музыку
//        audioSource.PlayOneShot(musicClip);
//        Debug.LogWarning("это я до вызова кроутины");
//        // Запускаем корутину с задержкой
//        StartCoroutine(DelayedCallback(delay, onComplete));
//    }

//    private IEnumerator DelayedCallback(float delay, System.Action callback)
//    {
//        Debug.LogWarning("начал остановку");
//        yield return new WaitForSecondsRealtime(delay);
//        Debug.LogWarning("закончил остановку");
//        callback?.Invoke();
//    }
//}

public class Go_to_main_menu : Button_sound_controller
{
    public AudioClip restartSound; // Назначьте звук в инспекторе

    public void Mainmenu()
    {
        if (YG2.saves.EffectMusicEnabled)
            PlayMusicWithDelay(
                restartSound,
                restartSound.length + 0.1f,
                () => SceneManager.LoadScene(0),
                volume: 0.5f 
            );
        else SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}