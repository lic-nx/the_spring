using UnityEngine;

public class SeedDragDrop : DragDrop
{
    public SeedItem seedItem; // Ссылка на ScriptableObject с данными семени
    private bool isFollowingMouse = true;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSeedItem(SeedItem item)
    {
        seedItem = item;
        if (seedItem != null && seedItem.seedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = seedItem.seedSprite;
        }
    }

     public void on_mouse_follow()
    {
        isFollowingMouse = true;
    }
private void Update()
    {
        if (isFollowingMouse)
        {
            Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            transform.position = mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

                if (hit.collider != null && hit.collider.CompareTag("Pot"))
                {
                    OnDropInPot(hit.collider.transform);
                    Destroy(gameObject);
                }
            }
        }
    }

    // Метод для обработки падения в горшок (переопределяется в наследнике)
    protected void OnDropInPot(Transform potTransform)
    {
        if (seedItem.flowerPrefab != null)
        {
            // Instantiate the flower prefab
            GameObject flowerObj = Instantiate(seedItem.flowerPrefab, transform.position, Quaternion.identity);
            // Parent it to the pot
            flowerObj.transform.SetParent(potTransform);
            // Position it correctly
            flowerObj.transform.localPosition = new Vector3(0f, 0.5f, 0f);

            // Initialise the Flower component with its GrowthConditions if available
            var flowerComp = flowerObj.GetComponent<Flower>();
            if (flowerComp != null && seedItem.growthConditions != null)
            {
                flowerComp.Initialize(seedItem.growthConditions);
            }
            else
            {
                Debug.LogWarning("Flower component or GrowthConditions missing on seed drop.");
            }
            Debug.Log("Flower planted at the top of the pot!");
        }
    }
}