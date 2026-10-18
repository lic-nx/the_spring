using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Replaces the legacy shop widgets with the greenhouse shelf presentation.</summary>
public sealed class GardenShopPresentation : MonoBehaviour
{
    private const string DecorName = "GardenShopDecor";

    private Shop _shop;
    private PanelSkript _panelController;
    private GameObject _seedContent;
    private GameObject _potContent;
    private Image _seedTabBackground;
    private Image _potTabBackground;
    private Sprite _activeTabSprite;
    private Sprite _inactiveTabSprite;
    private SeedItem _roseSeed;

    public static void Install(GameObject shopPanel, Shop shop)
    {
        if (shopPanel == null) return;

        GardenShopPresentation presentation = shopPanel.GetComponent<GardenShopPresentation>();
        if (presentation == null)
            presentation = shopPanel.AddComponent<GardenShopPresentation>();

        presentation._shop = shop;
        presentation._panelController = Object.FindObjectOfType<PanelSkript>();
        presentation.Build();
    }

    private void OnEnable()
    {
        if (_seedContent != null)
            ShowSeeds();
    }

    private void Build()
    {
        RectTransform panelRect = GetComponent<RectTransform>();
        Stretch(panelRect);
        panelRect.localScale = Vector3.one;

        Image backdrop = GetComponent<Image>();
        if (backdrop != null)
        {
            backdrop.color = new Color(1f, 1f, 1f, 0.392f);
            backdrop.raycastTarget = true;
        }

        Transform previousDecor = transform.Find(DecorName);
        if (previousDecor != null)
            Destroy(previousDecor.gameObject);

        foreach (Transform child in transform)
        {
            if (child.name != DecorName)
                child.gameObject.SetActive(false);
        }

        GameObject decor = CreateRectObject(DecorName, transform);
        Stretch(decor.GetComponent<RectTransform>());

        _activeTabSprite = Resources.Load<Sprite>("UI/UI_Button_Green");
        _inactiveTabSprite = Resources.Load<Sprite>("UI/UI_Button_Orange");

        Button seedTab = CreateTabButton(decor.transform, "SeedsTab", new Vector2(245f, -300f), Resources.Load<Sprite>("UI/UI_Flower"));
        _seedTabBackground = seedTab.GetComponent<Image>();
        seedTab.onClick.AddListener(ShowSeeds);

        Button potTab = CreateTabButton(decor.transform, "PotsTab", new Vector2(405f, -300f), Resources.Load<Sprite>("Greenhouse/Texures/Pot_Base_Green_T"));
        _potTabBackground = potTab.GetComponent<Image>();
        potTab.onClick.AddListener(ShowPots);

        _seedContent = CreateRectObject("SeedsContent", decor.transform);
        Stretch(_seedContent.GetComponent<RectTransform>());
        _potContent = CreateRectObject("PotsContent", decor.transform);
        Stretch(_potContent.GetComponent<RectTransform>());

        BuildSeedShop(_seedContent.transform);
        BuildPotShop(_potContent.transform);
        ShowSeeds();
    }

    private void BuildSeedShop(Transform parent)
    {
        Sprite shelfSprite = Resources.Load<Sprite>("Greenhouse/Texures/Shelf_T");
        Sprite bagSprite = Resources.Load<Sprite>("Greenhouse/Texures/SeedsBag_T");
        Sprite roseSticker = Resources.Load<Sprite>("Greenhouse/Texures/FlowerSeed_Rose");

        _roseSeed = _shop != null && _shop.availableSeedsForSale != null && _shop.availableSeedsForSale.Count > 0
            ? _shop.availableSeedsForSale[0]
            : null;

        CreateImage(parent, "ShelfRack", shelfSprite, new Vector2(0f, -940f), new Vector2(1000f, 394f));

        const float firstShelfY = -770f;
        const float secondShelfY = -1045f;
        float[] columns = { -280f, 0f, 280f };
        for (int row = 0; row < 2; row++)
        {
            float shelfY = row == 0 ? firstShelfY : secondShelfY;
            for (int column = 0; column < columns.Length; column++)
            {
                int slotIndex = row * columns.Length + column;
                bool isRoseSlot = slotIndex == 0;
                CreateSeedSlot(
                    parent,
                    $"SeedSlot_{slotIndex}",
                    new Vector2(columns[column], shelfY + 140f),
                    new Vector2(columns[column], shelfY - 40f),
                    bagSprite,
                    isRoseSlot ? roseSticker : null,
                    isRoseSlot);
            }
        }
    }

    private void CreateSeedSlot(Transform parent, string name, Vector2 bagPosition, Vector2 pricePosition, Sprite bagSprite, Sprite stickerSprite, bool isRoseSlot)
    {
        Image bag = CreateImage(parent, name, bagSprite, bagPosition, new Vector2(230f, 280f));

        if (stickerSprite != null)
        {
            Image sticker = CreateImage(bag.transform, "FlowerSticker", stickerSprite, Vector2.zero, new Vector2(90f, 90f));
            RectTransform stickerRect = sticker.rectTransform;
            stickerRect.anchorMin = stickerRect.anchorMax = stickerRect.pivot = new Vector2(0.5f, 0.5f);
            stickerRect.anchoredPosition = new Vector2(-5f, -5f);
            sticker.raycastTarget = false;
        }

        if (!isRoseSlot || _roseSeed == null) return;

        bag.raycastTarget = true;
        Button buyButton = bag.gameObject.AddComponent<Button>();
        buyButton.targetGraphic = bag;
        buyButton.onClick.AddListener(BuyRoseSeed);
        CreatePrice(parent, name + "_Price", pricePosition, _roseSeed.price.ToString());
    }

    private void BuildPotShop(Transform parent)
    {
        Sprite shelfSprite = Resources.Load<Sprite>("Greenhouse/Texures/Shelf_T");
        CreateImage(parent, "ShelfRack", shelfSprite, new Vector2(0f, -940f), new Vector2(1000f, 394f));

        if (_shop == null || _shop.PotItems == null) return;

        const float firstShelfY = -770f;
        const float secondShelfY = -1045f;
        float[] columns = { -280f, 0f, 280f };
        int visibleCount = Mathf.Min(_shop.PotItems.Count, 6);
        for (int index = 0; index < visibleCount; index++)
        {
            Sprite potSprite = _shop.PotItems[index];
            if (potSprite == null) continue;

            int row = index / columns.Length;
            int column = index % columns.Length;
            float shelfY = row == 0 ? firstShelfY : secondShelfY;
            Image potImage = CreateImage(parent, $"PotSlot_{index}", potSprite, new Vector2(columns[column], shelfY + 105f), new Vector2(230f, 230f));
            potImage.raycastTarget = true;
            Button button = potImage.gameObject.AddComponent<Button>();
            button.targetGraphic = potImage;
            int capturedIndex = index;
            button.onClick.AddListener(() => BuyPot(capturedIndex));
        }
    }

    private void CreatePrice(Transform parent, string name, Vector2 position, string price)
    {
        GameObject priceObject = CreateRectObject(name, parent);
        RectTransform priceRect = priceObject.GetComponent<RectTransform>();
        SetTopCenter(priceRect, position, new Vector2(150f, 70f));

        TextMeshProUGUI priceText = priceObject.AddComponent<TextMeshProUGUI>();
        priceText.text = price;
        priceText.alignment = TextAlignmentOptions.Center;
        priceText.fontStyle = FontStyles.Bold;
        priceText.fontSize = 44f;
        priceText.color = new Color(0.30f, 0.16f, 0.16f, 1f);
        priceText.margin = new Vector4(0f, 0f, 48f, 0f);
        priceText.raycastTarget = false;

        Image sun = CreateImage(priceObject.transform, "Sun", Resources.Load<Sprite>("Greenhouse/Texures/Sun_Coin"), Vector2.zero, new Vector2(46f, 46f));
        RectTransform sunRect = sun.rectTransform;
        sunRect.anchorMin = sunRect.anchorMax = sunRect.pivot = new Vector2(0.5f, 0.5f);
        sunRect.anchoredPosition = new Vector2(48f, 0f);
    }

    private void BuyRoseSeed()
    {
        if (_shop != null && _roseSeed != null)
            _shop.TryPurchaseSeed(_roseSeed);
    }

    private void BuyPot(int index)
    {
        if (_shop == null) return;
        _shop.PurchasePot(index);
        gameObject.SetActive(false);
        if (_panelController != null) _panelController.show = true;
    }

    private void ShowSeeds()
    {
        if (_seedContent == null || _potContent == null) return;
        _seedContent.SetActive(true);
        _potContent.SetActive(false);
        if (_seedTabBackground != null) _seedTabBackground.sprite = _activeTabSprite;
        if (_potTabBackground != null) _potTabBackground.sprite = _inactiveTabSprite;
    }

    private void ShowPots()
    {
        if (_seedContent == null || _potContent == null) return;
        _seedContent.SetActive(false);
        _potContent.SetActive(true);
        if (_seedTabBackground != null) _seedTabBackground.sprite = _inactiveTabSprite;
        if (_potTabBackground != null) _potTabBackground.sprite = _activeTabSprite;
    }

    private Button CreateTabButton(Transform parent, string name, Vector2 position, Sprite iconSprite)
    {
        Image background = CreateImage(parent, name, _inactiveTabSprite, position, new Vector2(140f, 100f));
        background.raycastTarget = true;
        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        Image icon = CreateImage(background.transform, "Icon", iconSprite, Vector2.zero, new Vector2(72f, 72f));
        RectTransform iconRect = icon.rectTransform;
        iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        return button;
    }

    private static GameObject CreateRectObject(string name, Transform parent)
    {
        GameObject result = new GameObject(name, typeof(RectTransform));
        result.layer = parent.gameObject.layer;
        result.transform.SetParent(parent, false);
        return result;
    }

    private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        SetTopCenter(rect, position, size);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        return image;
    }

    private static void SetTopCenter(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }
}
