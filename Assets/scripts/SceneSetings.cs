using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class SceneSerings : MonoBehaviour
{
    public GameObject PausePanel;
    private PostProcessVolume ppVolume;
    public TextMeshProUGUI level_number;

    void Start() {
        ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        level_number.text = "Уровень " + (SceneManager.GetActiveScene().buildIndex - 2);
    }
    public void PauseButtonPressed()
    {

        // вызываем остановку
        PausePanel.SetActive(true);
        ppVolume.enabled = true;
        Time.timeScale = 0f;
    }

    public void ContinueButtonPressed()
    {
        // вызываем продолжаем игру
        PausePanel.SetActive(false);
        ppVolume.enabled = false;
        Time.timeScale = 1f;
    }

    public void ChangeScene(int scene)
    {
        SceneManager.LoadScene(scene);
        ppVolume.enabled = false;
        Time.timeScale = 1f; 
    }

    public void RestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ppVolume.enabled = false;
        Time.timeScale = 1f; 
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        ppVolume.enabled = false;

    }
}
