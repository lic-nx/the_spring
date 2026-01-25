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
        EventControllerScr.Instance.RegisterUI(this);
        HidePause();
        HideWin();
        ShowGameplayButtons();
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
        winPanel.SetActive(true);
        gameplayButtons.SetActive(false);
    }

    public void HideLoose()
    {
        winPanel.SetActive(false);
        gameplayButtons.SetActive(true);
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