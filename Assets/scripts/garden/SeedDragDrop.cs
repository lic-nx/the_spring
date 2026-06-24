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
        if (seedItem.flowerPrefab == null)
        {
            Debug.LogWarning("No flower prefab assigned to seed item.");
            return;
        }
        // Get Pot component
        Pot pot = potTransform.GetComponent<Pot>();
        if (pot == null)
        {
            Debug.LogWarning("Dropped object is not a Pot.");
            return;
        }
        // Prevent planting if pot already occupied
        if (pot.CurrentFlower != null)
        {
            Debug.LogWarning("Pot already has a flower attached. Abort planting.");
            return;
        }
        // Instantiate the flower prefab
        GameObject flowerObj = Instantiate(seedItem.flowerPrefab, Vector3.zero, Quaternion.identity);
        Flower flowerComp = flowerObj.GetComponent<Flower>();
        if (flowerComp == null)
        {
            Debug.LogWarning("Instantiated object does not contain a Flower component.");
            Destroy(flowerObj);
            return;
        }
        // Initialize growth conditions if provided
        if (seedItem.growthConditions != null)
        {
            flowerComp.Initialize(seedItem.growthConditions);
        }
        // Use Pot's PlantFlower method to attach and position
        bool placed = pot.PlantFlower(flowerComp);
        if (!placed)
        {
            // If planting failed, destroy the instantiated flower
            Destroy(flowerObj);
        }
    }
}