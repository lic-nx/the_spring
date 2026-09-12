using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpriteButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SpriteRenderer buttonRenderer;
    [SerializeField] private Collider2D buttonCollider;
    [SerializeField] private Image buttonImage;

    private SpriteReplacePanel parentPanel;
    private Sprite sprite;
    private int spriteIndex;

    public void Initialize(SpriteReplacePanel panel, Sprite spriteToUse, int index)
    {
        parentPanel = panel;
        sprite = spriteToUse;
        spriteIndex = index;

        if (buttonRenderer != null)
        {
            buttonRenderer.sprite = sprite;
        }
        else if (buttonImage != null)
        {
            buttonImage.sprite = sprite;
        }
        else
        {
            Debug.LogError("SpriteButton: Neither SpriteRenderer nor Image component found!");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (parentPanel != null)
        {
            parentPanel.OnSpriteSelected(sprite);
        }
    }
}
