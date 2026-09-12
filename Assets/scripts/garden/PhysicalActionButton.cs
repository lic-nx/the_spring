using UnityEngine;
using UnityEngine.EventSystems;

public enum ActionButtonType
{
    Delete,
    ReplaceSprite,
    Move
}

public class PhysicalActionButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Collider2D buttonCollider;
    [SerializeField] private Sprite deleteSprite;
    [SerializeField] private Sprite replaceSpriteSprite;
    [SerializeField] private Sprite moveSprite;

    private Pot targetPot;
    private ActionButtonType buttonType;

    public void Initialize(Pot pot, ActionButtonType type, Vector3 position)
    {
        targetPot = pot;
        buttonType = type;
        transform.position = position;

        UpdateButtonAppearance();
    }

    private void UpdateButtonAppearance()
    {
        if (buttonRenderer == null) return;

        switch (buttonType)
        {
            case ActionButtonType.Delete:
                buttonRenderer.sprite = deleteSprite;
                break;
            case ActionButtonType.ReplaceSprite:
                buttonRenderer.sprite = replaceSpriteSprite;
                break;
            case ActionButtonType.Move:
                buttonRenderer.sprite = moveSprite;
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (targetPot == null) return;

        switch (buttonType)
        {
            case ActionButtonType.Delete:
                targetPot.DeletePot();
                break;
            case ActionButtonType.ReplaceSprite:
                if (SpriteReplacePanelManager.Instance != null)
                {
                    SpriteReplacePanelManager.Instance.ShowPanel(targetPot);
                }
                return;
            case ActionButtonType.Move:
                targetPot.StartMoving();
                break;
        }

        if (PotActionMenuPool.Instance != null)
        {
            PotActionMenuPool.Instance.ReturnAllMenus();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
