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
    private Image imageComponent;
    public TextMeshProUGUI level_number;

    void Start() {
        imageComponent = GetComponent<Image>();
        ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        level_number.text = "Уровень " + (SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void PauseButtonPressed()
    {

        // вызываем остановку
        PausePanel.SetActive(true);
        ppVolume.enabled = true;
        imageComponent.enabled = false;
        Time.timeScale = 0f;
    }

    public void ContinueButtonPressed()
    {
        // вызываем продолжаем игру
        imageComponent.enabled = true;
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

}
