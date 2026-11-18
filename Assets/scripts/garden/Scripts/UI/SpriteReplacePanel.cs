using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteReplacePanel : MonoBehaviour
{
    [SerializeField] private GameObject spriteButtonPrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private float buttonSize = 0.8f;

    private Pot targetPot;
    private Sprite[] availableSprites;

    public void Initialize(Pot pot, Sprite[] sprites, float spacing)
    {
        targetPot = pot;
        availableSprites = sprites;

        CreateSpriteButtons(spacing);
    }

    private void CreateSpriteButtons(float spacing)
    {
        if (spriteButtonPrefab == null || availableSprites == null || contentTransform == null) return;

        ClearExistingButtons();

        float startX = -(availableSprites.Length - 1) * spacing / 2f;
        
        for (int i = 0; i < availableSprites.Length; i++)
        {
            GameObject buttonObj = Instantiate(spriteButtonPrefab, contentTransform);
            buttonObj.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            buttonObj.transform.localScale = Vector3.one * buttonSize;

            SpriteButton spriteButton = buttonObj.GetComponent<SpriteButton>();
            if (spriteButton != null)
            {
                spriteButton.Initialize(this, availableSprites[i], i);
            }
            else
            {
                Debug.LogError("SpriteButton component not found on prefab!");
            }
        }
    }

    private void ClearExistingButtons()
    {
        if (contentTransform == null) return;

        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnSpriteSelected(Sprite sprite)
    {
        if (targetPot != null)
        {
            targetPot.ReplaceSprite(sprite);
        }
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}
