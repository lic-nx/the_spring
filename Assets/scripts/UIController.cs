using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject loosePanel;

    [Header("Gameplay Buttons")]
    [SerializeField] private GameObject gameplayButtons;

    private void Awake()
    {

    }
    
    private void Start()
    {
        EventControllerScr.Instance.RegisterUI(this);
    }

    // ===== Pause =====
    public void ShowPause()
    {
        pausePanel.SetActive(true);
        gameplayButtons.SetActive(false);
    }

    public void HidePause()
    {
        pausePanel.SetActive(false);
        gameplayButtons.SetActive(true);
    }

    // ===== Win =====
    public void ShowWin()
    {
        winPanel.SetActive(true);
        gameplayButtons.SetActive(false);
    }

    public void HideWin()
    {
        winPanel.SetActive(false);
        gameplayButtons.SetActive(true);
    }

    // ===== Loose =====
        public void ShowLoose()
    {
        loosePanel.SetActive(true);
        gameplayButtons.SetActive(false);
        Time.timeScale = 0;
    }

    public void HideLoose()
    {
        loosePanel.SetActive(false);
        gameplayButtons.SetActive(true);
        Time.timeScale = 1;
    }

    // ===== Gameplay Buttons =====
    public void ShowGameplayButtons()
    {
        gameplayButtons.SetActive(true);
    }

    public void HideGameplayButtons()
    {
        gameplayButtons.SetActive(false);
    }
}