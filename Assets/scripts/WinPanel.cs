using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using YG;

public class WinPanel : MonoBehaviour
{
    public GameObject winPanel;
    private PostProcessVolume ppVolume;
    public GameObject someObject;

    void Start()
    {
        Debug.Log("Camera.main: " + (Camera.main ? Camera.main.name : "null"));
        ppVolume = Camera.main.GetComponent<PostProcessVolume>();
        if (someObject != null)
        {
            someObject.SetActive(false); // объект становится неактивным
        }
    }

    public void OnEventTriggered()
    {
        Debug.Log("Игра завершена! идет расчет");
        if (SceneManager.GetActiveScene().buildIndex >= YG2.saves.ReachedIndex)
        {
            YG2.saves.UnlockedLevel += 1;
            YG2.saves.CompletedLevel += 1;
            YG2.saves.ReachedIndex += 1;
            YG2.SaveProgress();
            //PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            //PlayerPrefs.Save();
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
