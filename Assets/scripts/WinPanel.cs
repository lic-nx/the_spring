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

    public void Restart()
    {
        //ppVolume.enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void Mainmenu()
    {
        ppVolume.enabled = false;
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void NextLevel()
    {
        ppVolume.enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }
}
