using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using YG;

public class eventController : MonoBehaviour
{
    public static eventController Instance { get; private set; }
    public GameObject winPanel;
    public GameObject gameOverPanel;
    private PostProcessVolume ppVolume;
    public GameObject someObject;



    void Start()
    {
        Debug.Log("Camera.main: " + (Camera.main ? Camera.main.name : "null"));
        ppVolume = Camera.main.GetComponent<PostProcessVolume>();
        if (someObject != null)
        {
            someObject.SetActive(false); // объект становитс€ неактивным
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // опционально
        }
    }

    public void OnEndGame() {

        Debug.Log("»гра завершена! идет расчет");
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
        Debug.LogWarning("PostProcessVolume не найден. Ёффект блюра не применЄн.");
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("winPanel не назначен!");
        }

        Debug.Log("»гра завершена! ѕанель победы показана.");

    }
    public void PlayerLose()
    {
        Debug.Log("»грок проиграл!");
        ppVolume = Camera.main.GetComponent<PostProcessVolume>();
        ppVolume.enabled = true;
        Debug.LogWarning("PostProcessVolume не найден. Ёффект блюра не применЄн.");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("gameOverPanel не назначен!");
        }

    }
}
