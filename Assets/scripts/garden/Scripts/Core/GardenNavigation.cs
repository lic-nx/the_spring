using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Restyles the existing Garden controls and adds only the controls that do not exist yet.</summary>
public static class GardenNavigation
{
    private const string GardenSceneName = "Garden";
    private const string MenuSceneName = "menu";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureGardenHud()
    {
        if (SceneManager.GetActiveScene().name != GardenSceneName) return;

        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[GardenNavigation] Canvas not found; Garden HUD was not configured.");
            return;
        }

        PanelSkript panelController = Object.FindObjectOfType<PanelSkript>();
        Shop shop = Object.FindObjectOfType<Shop>();
        if (panelController != null && panelController.ShopPanel != null)
            GardenShopPresentation.Install(panelController.ShopPanel, shop);

        DisableLegacyShopButtons(canvas);
        Button shopButton = GetOrCreateButton(canvas.transform, "GardenShopButton");
        shopButton.onClick.RemoveAllListeners();
        if (panelController != null)
            shopButton.onClick.AddListener(panelController.ShowHidePanel);

        shopButton.gameObject.SetActive(true);
        DisableDuplicateGeneratedButtons(canvas, shopButton, "GardenShopButton");
        StyleIconButton(
            shopButton,
            new Vector2(34f, -34f),
            Resources.Load<Sprite>("UI/UI_Button_Green"),
            Resources.Load<Sprite>("UI/UI_ShoppingCart"),
            new Vector2(118f, 118f),
            new Vector2(92f, 92f),
            TextAnchor.UpperLeft);

        Button flowersButton = GetOrCreateButton(canvas.transform, "GardenFlowersButton");
        flowersButton.onClick.RemoveAllListeners();
        StyleIconButton(
            flowersButton,
            new Vector2(166f, -34f),
            Resources.Load<Sprite>("UI/UI_Button_Orange"),
            Resources.Load<Sprite>("UI/UI_Flower"),
            new Vector2(118f, 118f),
            new Vector2(88f, 88f),
            TextAnchor.UpperLeft);
        flowersButton.transition = Selectable.Transition.None;
        flowersButton.interactable = false;
        flowersButton.image.color = Color.white;

        Button homeButton = GetOrCreateButton(canvas.transform, "GardenHomeButton");
        homeButton.onClick.RemoveAllListeners();
        homeButton.onClick.AddListener(ReturnToMenu);
        StyleIconButton(
            homeButton,
            new Vector2(-34f, 34f),
            Resources.Load<Sprite>("UI/UI_Button_Orange"),
            Resources.Load<Sprite>("UI/UI_Home"),
            new Vector2(126f, 126f),
            new Vector2(92f, 92f),
            TextAnchor.LowerRight);

        HideLegacyCurrencyUi(canvas);
        ConfigureCurrencyPanel(canvas.transform);
    }

    private static void DisableLegacyShopButtons(Canvas canvas)
    {
        int index = 0;
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) != nameof(PanelSkript.ShowHidePanel)) continue;

                button.gameObject.name = $"LegacyGardenShopButton_{index++}";
                button.gameObject.SetActive(false);
                break;
            }
        }
    }

    private static void DisableDuplicateGeneratedButtons(Canvas canvas, Button activeButton, string objectName)
    {
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            if (button != activeButton && button.gameObject.name == objectName)
                button.gameObject.SetActive(false);
        }
    }

    private static void HideLegacyCurrencyUi(Canvas canvas)
    {
        foreach (Button button in canvas.GetComponentsInChildren<Button>(true))
        {
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                if (button.onClick.GetPersistentMethodName(i) == nameof(CurrencyManager.AddCurrency))
                {
                    button.gameObject.SetActive(false);
                    break;
                }
            }
        }

        foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.GetComponent<CurrencyUIText>() != null) continue;

            string content = text.text.ToLowerInvariant();
            if (content.Contains("плюс") || content.Contains("коинов"))
                text.gameObject.SetActive(false);
        }
    }

    private static Button GetOrCreateButton(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        Button button = existing != null ? existing.GetComponent<Button>() : null;
        return button != null ? button : CreateButton(parent, name);
    }

    private static Button CreateButton(Transform parent, string name)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.layer = parent.gameObject.layer;
        buttonObject.transform.SetParent(parent, false);
        return buttonObject.GetComponent<Button>();
    }

    private static void StyleIconButton(
        Button button,
        Vector2 offset,
        Sprite backgroundSprite,
        Sprite iconSprite,
        Vector2 buttonSize,
        Vector2 iconSize,
        TextAnchor corner)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        SetCorner(rect, corner, offset);
        rect.sizeDelta = buttonSize;
        rect.localScale = Vector3.one;

        Image background = button.GetComponent<Image>();
        background.sprite = backgroundSprite;
        background.color = Color.white;
        background.preserveAspect = true;
        background.type = Image.Type.Simple;
        button.targetGraphic = background;

        Image icon = null;
        foreach (Image childImage in button.GetComponentsInChildren<Image>(true))
        {
            if (childImage.gameObject == button.gameObject) continue;
            icon = childImage;
            break;
        }

        if (icon == null)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = button.gameObject.layer;
            iconObject.transform.SetParent(button.transform, false);
            icon = iconObject.GetComponent<Image>();
        }

        icon.gameObject.name = "Icon";
        icon.gameObject.SetActive(true);
        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = iconSize;
        iconRect.localScale = Vector3.one;
        icon.sprite = iconSprite;
        icon.color = Color.white;
        icon.preserveAspect = true;
        icon.raycastTarget = false;
    }

    private static void ConfigureCurrencyPanel(Transform canvasTransform)
    {
        Transform oldGeneratedPanel = canvasTransform.Find("GardenCurrencyPanel");
        TMP_Text existingAmount = null;

        foreach (CurrencyUIText currencyText in Object.FindObjectsOfType<CurrencyUIText>(true))
        {
            if (oldGeneratedPanel != null && currencyText.transform.IsChildOf(oldGeneratedPanel))
                continue;

            existingAmount = currencyText.GetComponent<TMP_Text>();
            if (existingAmount != null) break;
        }

        if (oldGeneratedPanel != null)
            Object.Destroy(oldGeneratedPanel.gameObject);

        GameObject panelObject = new GameObject("GardenCurrencyPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.layer = canvasTransform.gameObject.layer;
        panelObject.transform.SetParent(canvasTransform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        SetCorner(panelRect, TextAnchor.UpperRight, new Vector2(-34f, -34f));
        panelRect.sizeDelta = new Vector2(380f, 126f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.sprite = Resources.Load<Sprite>("UI/UI_Rectangle");
        panelImage.color = Color.white;
        panelImage.preserveAspect = true;
        panelImage.raycastTarget = false;

        CreatePanelIcon(panelObject.transform, "Plus", Resources.Load<Sprite>("UI/UI_Plus_Colored"), new Vector2(-157f, 0f), new Vector2(54f, 54f));
        CreatePanelIcon(panelObject.transform, "Sun", Resources.Load<Sprite>("Greenhouse/Texures/Sun_Coin"), new Vector2(150f, 0f), new Vector2(92f, 92f));

        if (existingAmount == null)
        {
            GameObject textObject = new GameObject("Amount", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.layer = panelObject.layer;
            textObject.SetActive(false);
            textObject.transform.SetParent(panelObject.transform, false);
            existingAmount = textObject.GetComponent<TextMeshProUGUI>();
            textObject.AddComponent<CurrencyUIText>();
            textObject.SetActive(true);
        }
        else
        {
            existingAmount.gameObject.SetActive(true);
            existingAmount.gameObject.name = "Amount";
            existingAmount.transform.SetParent(panelObject.transform, false);
        }

        RectTransform textRect = existingAmount.rectTransform;
        textRect.anchorMin = textRect.anchorMax = textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(4f, 0f);
        textRect.sizeDelta = new Vector2(210f, 88f);
        textRect.localScale = Vector3.one;
        existingAmount.alignment = TextAlignmentOptions.Center;
        existingAmount.fontSize = 54f;
        existingAmount.fontStyle = FontStyles.Bold;
        existingAmount.color = Color.white;
        existingAmount.raycastTarget = false;

        panelObject.transform.SetAsLastSibling();
    }

    private static void CreatePanelIcon(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject iconObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObject.layer = parent.gameObject.layer;
        iconObject.transform.SetParent(parent, false);
        RectTransform rect = iconObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = iconObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private static void SetCorner(RectTransform rect, TextAnchor corner, Vector2 offset)
    {
        Vector2 anchor;
        switch (corner)
        {
            case TextAnchor.UpperLeft:
                anchor = new Vector2(0f, 1f);
                break;
            case TextAnchor.UpperRight:
                anchor = new Vector2(1f, 1f);
                break;
            default:
                anchor = new Vector2(1f, 0f);
                break;
        }

        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = offset;
    }

    private static void ReturnToMenu()
    {
        if (GameSaveManager.Instance != null)
            GameSaveManager.Instance.SaveAllGardenState();

        SceneManager.LoadScene(MenuSceneName);
    }
}
