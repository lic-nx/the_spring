using UnityEngine;
using UnityEngine.UI;

public class SpriteReplacePanelManager : MonoBehaviour
{
    public static SpriteReplacePanelManager Instance { get; private set; }

    [Header("Настройки панели")]
    [SerializeField] private GameObject panel;
    [SerializeField] private ScrollRect scrollView;
    [SerializeField] private GameObject spriteButtonPrefab;
    [SerializeField] private Sprite[] availablePotSprites;

    private Pot currentTargetPot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowPanel(Pot targetPot)
    {
        if (panel == null)
        {
            Debug.LogError("[SpriteReplacePanelManager] Panel is not assigned!");
            return;
        }

        currentTargetPot = targetPot;
        panel.SetActive(true);
        GenerateSpriteButtons();
    }

    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        ClearSpriteButtons();
        currentTargetPot = null;
    }

    [SerializeField] private SpriteReplacePanel spriteReplacePanel;

    private void GenerateSpriteButtons()
    {
        ClearSpriteButtons();

        if (spriteButtonPrefab == null || availablePotSprites == null || scrollView == null || scrollView.content == null)
        {
            Debug.LogError("[SpriteReplacePanelManager] Sprite button prefab, available sprites, or scroll view content is not assigned!");
            return;
        }

        for (int i = 0; i < availablePotSprites.Length; i++)
        {
            GameObject buttonObj = Instantiate(spriteButtonPrefab, scrollView.content);
            SpriteButton spriteButton = buttonObj.GetComponent<SpriteButton>();
            if (spriteButton != null)
            {
                spriteButton.Initialize(spriteReplacePanel, availablePotSprites[i], i);
            }
            else
            {
                Debug.LogError($"[SpriteReplacePanelManager] SpriteButton component not found on prefab at index {i}!");
            }
        }
    }

    private void ClearSpriteButtons()
    {
        if (scrollView != null && scrollView.content != null)
        {
            foreach (Transform child in scrollView.content)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void OnSpriteSelected(Sprite sprite)
    {
        if (spriteReplacePanel != null)
        {
            spriteReplacePanel.OnSpriteSelected(sprite);
            HidePanel();
        }
        else
        {
            Debug.LogWarning("[SpriteReplacePanelManager] SpriteReplacePanel is null!");
        }
    }
}
