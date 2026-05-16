using UnityEngine;

/// <summary>
/// Makes the attached seed prefab follow the mouse cursor in world space.
/// On left‑click it attempts to place the seed into a Pot that is under the cursor.
/// After successful placement the prefab becomes a child of the Pot and this
/// component destroys itself.
/// </summary>
public class SeedFollower : MonoBehaviour
{
    // Cached reference to the Flower component on the instantiated prefab.
    private Flower _flower;

    private void Awake()
    {
        _flower = GetComponent<Flower>();
        if (_flower == null)
        {
            Debug.LogError($"SeedFollower requires a Flower component on {name}");
        }
    }

    private void Update()
    {
        // Follow mouse cursor.
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f; // Assuming 2D plane.
        transform.position = mouseWorld;

        // On left click try to place into a Pot.
        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceInPot();
        }
    }

    private void TryPlaceInPot()
    {
        // Raycast against colliders in 2D. Pots are expected to have a Collider2D.
        var cam = Camera.main;
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(mousePos, Vector2.zero);
        if (hit.collider != null)
        {
            var pot = hit.collider.GetComponent<Pot>();
            if (pot != null)
            {
                // Plant the flower into the pot.
                try
                {
                    pot.PlantFlower(_flower);
                // Ensure the planted flower appears at the center of the pot.
                _flower.transform.position = pot.transform.position;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to plant flower: {ex.Message}");
                    return;
                }
                // Remove this component to stop following.
                Destroy(this);
                // Optionally, destroy this GameObject if the flower should become child of pot (already re‑parented).
                // The Flower component's PlantFlower method re‑parents the whole GameObject.
                return;
            }
        }
        // If we reach here, no pot was clicked – keep following.
    }
}
