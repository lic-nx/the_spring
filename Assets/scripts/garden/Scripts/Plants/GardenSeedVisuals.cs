using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Applies the new seed bag, flower sticker, and circular quantity badge.</summary>
public static class GardenSeedVisuals
{
    private const string StickerObjectName = "FlowerSticker";
    private const string CounterObjectName = "SeedCountBadge";

    public static void Apply(Image iconImage, TMP_Text quantityText, Sprite flowerSticker)
    {
        if (iconImage != null)
        {
            iconImage.sprite = Resources.Load<Sprite>("Greenhouse/Texures/SeedsBag_T");
            iconImage.preserveAspect = true;
            iconImage.enabled = iconImage.sprite != null;
            CreateOrUpdateSticker(iconImage, flowerSticker);
        }

        if (quantityText != null)
            CreateOrUpdateCounter(quantityText);
    }

    public static void Clear(Image iconImage, TMP_Text quantityText)
    {
        if (iconImage != null)
        {
            Transform sticker = iconImage.transform.Find(StickerObjectName);
            if (sticker != null) sticker.gameObject.SetActive(false);
        }

        if (quantityText != null && quantityText.transform.parent != null)
        {
            Transform counter = quantityText.transform.parent.Find(CounterObjectName);
            if (counter != null) counter.gameObject.SetActive(false);
        }
    }

    private static void CreateOrUpdateSticker(Image bagImage, Sprite stickerSprite)
    {
        Transform existing = bagImage.transform.Find(StickerObjectName);
        GameObject stickerObject;
        if (existing == null)
        {
            stickerObject = new GameObject(StickerObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            stickerObject.layer = bagImage.gameObject.layer;
            stickerObject.transform.SetParent(bagImage.transform, false);
        }
        else
        {
            stickerObject = existing.gameObject;
        }

        stickerObject.SetActive(true);
        RectTransform rect = stickerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.27f, 0.34f);
        rect.anchorMax = new Vector2(0.73f, 0.70f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        Image sticker = stickerObject.GetComponent<Image>();
        sticker.sprite = stickerSprite;
        sticker.preserveAspect = true;
        sticker.raycastTarget = false;
    }

    private static void CreateOrUpdateCounter(TMP_Text quantityText)
    {
        Transform parent = quantityText.transform.parent;
        if (parent == null) return;

        Transform existing = parent.Find(CounterObjectName);
        GameObject counterObject;
        if (existing == null)
        {
            counterObject = new GameObject(CounterObjectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            counterObject.layer = quantityText.gameObject.layer;
            counterObject.transform.SetParent(parent, false);
        }
        else
        {
            counterObject = existing.gameObject;
        }

        // Apply can run just before the caller enables the quantity text.
        // The badge belongs to seed slots only and Clear hides it for other items.
        counterObject.SetActive(true);
        RectTransform textRect = quantityText.rectTransform;
        RectTransform counterRect = counterObject.GetComponent<RectTransform>();
        counterRect.anchorMin = textRect.anchorMin;
        counterRect.anchorMax = textRect.anchorMax;
        counterRect.pivot = textRect.pivot;
        counterRect.anchoredPosition = textRect.anchoredPosition;
        counterRect.sizeDelta = new Vector2(74f, 74f);
        counterRect.localRotation = Quaternion.identity;
        counterRect.localScale = Vector3.one;

        Image counter = counterObject.GetComponent<Image>();
        counter.sprite = Resources.Load<Sprite>("Greenhouse/Texures/SeedCounter");
        counter.preserveAspect = true;
        counter.raycastTarget = false;

        counterObject.transform.SetSiblingIndex(quantityText.transform.GetSiblingIndex());
        quantityText.transform.SetAsLastSibling();
        quantityText.alignment = TextAlignmentOptions.Center;
        quantityText.fontStyle = FontStyles.Bold;
        quantityText.color = new Color(0.30f, 0.16f, 0.16f, 1f);
    }
}
