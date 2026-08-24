using UnityEngine;
using UnityEngine.UI;

public class PotActionMenu : MonoBehaviour
{
    [Header("Кнопки меню")]
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button replaceSpriteButton;
    [SerializeField] private Button moveButton;

    [Header("Меню замены спрайта")]
    [SerializeField] private GameObject spriteReplacePanel;
    [SerializeField] private Button[] spriteOptionButtons;
    [SerializeField] private Sprite[] availablePotSprites;

    public Pot targetPot;

    private void Awake()
    {
        Debug.Log("PotActionMenu: Awake called.");

        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(OnDeleteClicked);
            Debug.Log("PotActionMenu: Delete button listener added.");
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Delete button is not assigned.");
        }

        if (replaceSpriteButton != null)
        {
            replaceSpriteButton.onClick.AddListener(OnReplaceSpriteClicked);
            Debug.Log("PotActionMenu: Replace sprite button listener added.");
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Replace sprite button is not assigned.");
        }

        if (moveButton != null)
        {
            moveButton.onClick.AddListener(OnMoveClicked);
            Debug.Log("PotActionMenu: Move button listener added.");
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Move button is not assigned.");
        }

        for (int i = 0; i < spriteOptionButtons.Length && i < availablePotSprites.Length; i++)
        {
            int index = i;
            spriteOptionButtons[i].onClick.AddListener(() => OnSpriteSelected(index));
            Debug.Log($"PotActionMenu: Sprite option button {i} listener added.");
        }

        if (spriteReplacePanel != null)
        {
            spriteReplacePanel.SetActive(false);
            Debug.Log("PotActionMenu: Sprite replace panel initialized and hidden.");
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Sprite replace panel is not assigned.");
        }
    }

    public void Initialize()
    {
        // targetPot = this;
        Debug.Log($"PotActionMenu: Initialized with target pot: {(targetPot != null ? targetPot.name : "null")}");
    }

    private void OnDeleteClicked()
    {
        Debug.Log("PotActionMenu: Delete button clicked.");
        if (targetPot != null)
        {
            Debug.Log($"PotActionMenu: Deleting pot: {targetPot.name}");
            targetPot.DeletePot();
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Target pot is null. Cannot delete.");
        }
    }

    private void OnReplaceSpriteClicked()
    {
        Debug.Log("PotActionMenu: Replace sprite button clicked.");
        if (spriteReplacePanel != null)
        {
            spriteReplacePanel.SetActive(true);
            Debug.Log("PotActionMenu: Sprite replace panel activated.");
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Sprite replace panel is null. Cannot activate.");
        }
    }

    private void OnSpriteSelected(int spriteIndex)
    {
        Debug.Log($"PotActionMenu: Sprite selected with index: {spriteIndex}");
        if (targetPot != null && spriteIndex >= 0 && spriteIndex < availablePotSprites.Length)
        {
            Debug.Log($"PotActionMenu: Replacing sprite for pot: {targetPot.name} with sprite index: {spriteIndex}");
            targetPot.ReplaceSprite(availablePotSprites[spriteIndex]);
        }
        else
        {
            Debug.LogWarning($"PotActionMenu: Invalid sprite index or target pot is null. Sprite index: {spriteIndex}, Available sprites: {availablePotSprites.Length}");
        }
    }

    private void OnMoveClicked()
    {
        Debug.Log("PotActionMenu: Move button clicked.");
        if (targetPot != null)
        {
            Debug.Log($"PotActionMenu: Starting to move pot: {targetPot.name}");
            targetPot.StartMoving();
        }
        else
        {
            Debug.LogWarning("PotActionMenu: Target pot is null. Cannot move.");
        }
    }
}