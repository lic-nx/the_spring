using UnityEngine;

// Drag‑and‑drop visualisation for purchasing pots.
// Instantiated from Shop.PurchasePot and follows the mouse cursor
// until the player clicks to place it.
public class PotDragDrop : DragDrop
{
    private Pot pot; // reference to the pot data being purchased
    private bool isFollowingMouse = true;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // Assign the pot that this visualisation represents
    public void SetPot(Pot potItem)
    {
        pot = potItem;
        // Optionally set a sprite or visual representation here if needed
    }

    // Start following the mouse cursor (called by Shop after instantiation)
    public void OnMouseFollow()
    {
        isFollowingMouse = true;
    }

    private void Update()
    {
        if (!isFollowingMouse) return;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;

        // Place the pot on left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Here we simply instantiate the actual pot prefab at the current position.
            if (pot != null && pot.gameObject != null)
            {
                Instantiate(pot.gameObject, transform.position, Quaternion.identity);
            }
            // Destroy the drag visualisation after placing
            Destroy(gameObject);
        }
    }
}
