using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using TMPro;

public class SceneSetings : MonoBehaviour
{
    public GameObject PausePanel;

    private Image imageComponent;
    public TextMeshProUGUI level_number;

    void Start() {
        Debug.Log("payse Camera.main: " + (Camera.main ? Camera.main.name : "null"));
        imageComponent = GetComponent<Image>();
        level_number.text = "Уровень " + (SceneManager.GetActiveScene().buildIndex - 1);
    }
    public void PauseButtonPressed()
    {

        // вызываем остановку
        PausePanel.SetActive(true);
        imageComponent.enabled = false;
        Time.timeScale = 0f;
    }

    public void ContinueButtonPressed()
    {
        // вызываем продолжаем игру
        imageComponent.enabled = true;
        PausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ChangeScene(int scene)
    {
        SceneManager.LoadScene(scene);
        Time.timeScale = 1f; 
    }

}
