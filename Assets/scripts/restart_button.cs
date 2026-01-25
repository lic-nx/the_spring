using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class restart_button : MonoBehaviour
{
    public AudioClip restartSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D-����
        }
    }

    public void RestartButtonPressed()
    {
        StartCoroutine(RestartWithSound());
    }

    IEnumerator RestartWithSound()
    {
        if (restartSound != null)
        {
            audioSource.PlayOneShot(restartSound);
            yield return new WaitForSecondsRealtime(0.01f);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}