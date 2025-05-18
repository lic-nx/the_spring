using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class WinPanel : MonoBehaviour
{
    public GameObject winPanel;

    void OnEnable()
    {
        // Подписываемся на событие окончания игры
        flower.onGameEnd.AddListener(OnEventTriggered);
    }

    void OnDisable()
    {
        // Отписываемся от события при отключении объекта
        flower.onGameEnd.RemoveListener(OnEventTriggered);
    }

    void OnEventTriggered()
    {
        winPanel.SetActive(true);
        // Time.timeScale = 0f;
        Debug.Log("Игра завершена! Панель победы показана.");
    }

    // public void WinEndPanel()  // Открываем панель победы
    // {
        
    // }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void Mainmenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }
}
