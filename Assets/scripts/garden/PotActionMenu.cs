using UnityEngine;
using UnityEngine.UI;

public class PotActionMenu : MonoBehaviour
{
    [Header("Кнопки меню")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button replaceSpriteButton;
    
    [Header("Панель замены спрайтов")]
    [SerializeField] private GameObject spriteReplacePanel;
    [SerializeField] private Sprite[] availableSprites;
    [SerializeField] private float buttonSpacing = 1.2f;

    public Pot targetPot;
    private bool isSpritePanelVisible = false;

    private void Awake()
    {
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteClicked);
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Delete button is not assigned.");
        }

        if (replaceSpriteButton != null)
        {
            replaceSpriteButton.onClick.AddListener(OnReplaceSpriteClicked);
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Replace sprite button is not assigned.");
        }

        if (spriteReplacePanel != null)
        {
            spriteReplacePanel.SetActive(false);
        }
        


        SpriteReplacePanel panelComponent = spriteReplacePanel.GetComponent<SpriteReplacePanel>();
        if (panelComponent != null)
        {
            panelComponent.Initialize(targetPot, availableSprites, buttonSpacing);
        }
        else
        {
            Debug.LogError("PotActionMenu: SpriteReplacePanel component not found on spriteReplacePanel!");
            return;
        }
    }

    public void SetTargetPot(Pot pot)
    {
        targetPot = pot;
    }

    public void ResetTargetPot()
    {
        targetPot = null;
        HideSpriteReplacePanel();
    }

    public void ReturnToPool()
    {
        if (PotActionMenuPool.Instance != null)
        {
            PotActionMenuPool.Instance.ReturnMenu(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDeleteClicked()
    {
        if (targetPot != null)
        {
            targetPot.DeletePot();
            ReturnToPool();
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Target pot is null. Cannot delete.");
        }
    }

    private void OnReplaceSpriteClicked()
    {
        if (isSpritePanelVisible)
        {
            HideSpriteReplacePanel();
        }
        else
        {
            ShowSpriteReplacePanel();
        }
    }

    private void ShowSpriteReplacePanel()
    {
        if (spriteReplacePanel == null || availableSprites == null || targetPot == null)
        {
            Debug.LogWarning("PotActionMenu: Sprite replace panel, sprites, or target pot is null.");
            return;
        }

        SpriteReplacePanel panelComponent = spriteReplacePanel.GetComponent<SpriteReplacePanel>();
        if (panelComponent != null)
        {
            panelComponent.Initialize(targetPot, availableSprites, buttonSpacing);
        }
        else
        {
            Debug.LogError("PotActionMenu: SpriteReplacePanel component not found on spriteReplacePanel!");
            return;
        }

        spriteReplacePanel.SetActive(true);
        isSpritePanelVisible = true;
    }

    private void HideSpriteReplacePanel()
    {
        if (spriteReplacePanel != null)
        {
            spriteReplacePanel.SetActive(false);
        }
        isSpritePanelVisible = false;
    }
}
