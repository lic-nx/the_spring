using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject loosePanel;

    [Header("Gameplay Buttons")]
    [SerializeField] private GameObject gameplayButtons;
    [SerializeField] private TMP_FontAsset growthRestartFont;
    private Button growthRestartButton;
    private TextMeshProUGUI growthRestartLabel;

    private void Awake()
    {

    }
    
    private void Start()
    {
        EventControllerScr.Instance?.RegisterUI(this);
    }

    private void LateUpdate()
    {
        player_move player = player_move._instance;
        bool show = player != null && player.RestartSuggested &&
                    player.State != player_move.GrowthState.ReachedSun && Time.timeScale > 0f &&
                    gameplayButtons != null && gameplayButtons.activeInHierarchy;
        if (show && growthRestartButton == null)
            CreateGrowthRestartButton();
        if (growthRestartButton == null)
            return;
        growthRestartButton.gameObject.SetActive(show);
        if (show)
        {
            LocalizationManager localization = LocalizationManager.Instance;
            growthRestartLabel.text = localization != null
                ? localization.GetText("restart_growth") + "\n<size=70%>" +
                  localization.GetText("restart_growth_hint") + "</size>"
                : "Restart growth\n<size=70%>A new path opened behind the flower</size>";
        }
    }

    private void CreateGrowthRestartButton()
    {
        if (gameplayButtons == null)
            return;
        TextMeshProUGUI existingText = GetComponentInChildren<TextMeshProUGUI>(true);
        TMP_FontAsset font = growthRestartFont != null ? growthRestartFont :
            (existingText != null ? existingText.font : TMP_Settings.defaultFontAsset);
        GameObject buttonObject = new GameObject("Restart growth", typeof(RectTransform),
            typeof(CanvasRenderer), typeof(Image), typeof(Button));
        // Authored GameplayButtons is only a 100x100 group at the screen center.
        // Anchor the new control to the canvas, while honoring the group's visibility above.
        Canvas canvas = gameplayButtons.GetComponentInParent<Canvas>();
        buttonObject.transform.SetParent(canvas != null ? canvas.transform : gameplayButtons.transform, false);
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 36f);
        rect.sizeDelta = new Vector2(460f, 96f);
        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.16f, 0.31f, 0.23f, 0.96f);
        growthRestartButton = buttonObject.GetComponent<Button>();
        growthRestartButton.targetGraphic = background;
        growthRestartButton.onClick.AddListener(RestartFlowerGrowth);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 8f);
        labelRect.offsetMax = new Vector2(-12f, -8f);
        growthRestartLabel = labelObject.GetComponent<TextMeshProUGUI>();
        if (font != null)
            growthRestartLabel.font = font;
        growthRestartLabel.alignment = TextAlignmentOptions.Center;
        growthRestartLabel.fontSize = 26f;
        growthRestartLabel.color = Color.white;
        growthRestartLabel.raycastTarget = false;
    }

    public void RestartFlowerGrowth()
    {
        player_move._instance?.RestartGrowth();
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
