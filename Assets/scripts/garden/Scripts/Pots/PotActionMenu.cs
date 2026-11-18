using UnityEngine;
using UnityEngine.UI;

public class PotActionMenu : MonoBehaviour
{
    private const string ActionIconName = "ActionIcon";
    private static Sprite _solidSprite;

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
        ApplyActionIcon(deleteButton, "Greenhouse/Texures/Cross_T");
        ApplyActionIcon(replaceSpriteButton, "Greenhouse/Texures/Pencil_T");

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

        if (spriteReplacePanel == null)
        {
            Debug.LogError("PotActionMenu: Sprite replace panel is not assigned.");
            return;
        }

        spriteReplacePanel.SetActive(false);

        if (spriteReplacePanel.GetComponent<SpriteReplacePanel>() == null)
            Debug.LogError("PotActionMenu: SpriteReplacePanel component not found on spriteReplacePanel!");
    }

    private static void ApplyActionIcon(Button button, string resourcePath)
    {
        if (button == null) return;

        Sprite icon = Resources.Load<Sprite>(resourcePath);
        SpriteRenderer background = button.GetComponent<SpriteRenderer>();
        if (background != null)
        {
            // Keep the prefab's circular sprite as the button background.
            background.color = Color.white;

            Transform previousIcon = button.transform.Find(ActionIconName);
            if (previousIcon != null) Destroy(previousIcon.gameObject);

            GameObject iconRoot = new GameObject(ActionIconName);
            iconRoot.transform.SetParent(button.transform, false);
            iconRoot.transform.localPosition = new Vector3(0f, 0f, -0.01f);

            if (resourcePath.EndsWith("Cross_T"))
            {
                CreateCrossStroke(iconRoot.transform, 45f, background);
                CreateCrossStroke(iconRoot.transform, -45f, background);
            }
            else if (icon != null)
            {
                SpriteRenderer iconRenderer = iconRoot.AddComponent<SpriteRenderer>();
                iconRenderer.sprite = icon;
                iconRenderer.color = Color.black;
                iconRenderer.sortingLayerID = background.sortingLayerID;
                iconRenderer.sortingOrder = background.sortingOrder + 1;
                float sourceSize = Mathf.Max(icon.bounds.size.x, icon.bounds.size.y);
                float scale = sourceSize > 0f ? 0.78f / sourceSize : 1f;
                iconRoot.transform.localScale = new Vector3(scale, scale, 1f);
            }

            return;
        }

        Image backgroundImage = button.GetComponent<Image>();
        if (backgroundImage != null)
        {
            // UI fallback: preserve the circular background and place the icon above it.
            Transform previousIcon = button.transform.Find(ActionIconName);
            GameObject iconObject = previousIcon != null
                ? previousIcon.gameObject
                : new GameObject(ActionIconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(button.transform, false);
            RectTransform rect = iconObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.2f);
            rect.anchorMax = new Vector2(0.8f, 0.8f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = icon;
            iconImage.preserveAspect = true;
            iconImage.color = Color.black;
            iconImage.raycastTarget = false;
        }
    }

    private static void CreateCrossStroke(Transform parent, float angle, SpriteRenderer background)
    {
        if (_solidSprite == null)
            _solidSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        GameObject stroke = new GameObject("Stroke", typeof(SpriteRenderer));
        stroke.transform.SetParent(parent, false);
        stroke.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        stroke.transform.localScale = new Vector3(0.66f, 0.12f, 1f);
        SpriteRenderer renderer = stroke.GetComponent<SpriteRenderer>();
        renderer.sprite = _solidSprite;
        renderer.color = Color.black;
        renderer.sortingLayerID = background.sortingLayerID;
        renderer.sortingOrder = background.sortingOrder + 1;
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
