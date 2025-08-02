using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;

public class WinPanel : MonoBehaviour
{
    public GameObject winPanel;
    private PostProcessVolume ppVolume;

    void Start()
    {
        Debug.Log("Camera.main: " + (Camera.main ? Camera.main.name : "null"));
        ppVolume = Camera.main.GetComponent<PostProcessVolume>();
    }

    public void OnEventTriggered()
    {
        Debug.Log("Игра завершена! идет расчет");
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.SetInt("CompletedLevel", PlayerPrefs.GetInt("CompletedLevel", 0) + 1);
            Debug.Log($"new CompletedLevel {PlayerPrefs.GetInt("CompletedLevel", 0)}");
            PlayerPrefs.Save();
        }
        ppVolume = Camera.main.GetComponent<PostProcessVolume>();
        ppVolume.enabled = true;
        Debug.LogWarning("PostProcessVolume не найден. Эффект блюра не применён.");
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("winPanel не назначен!");
        }

        Debug.Log("Игра завершена! Панель победы показана.");
    }


}
