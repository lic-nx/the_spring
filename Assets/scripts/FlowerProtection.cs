using UnityEngine;

public class FlowerProtection : MonoBehaviour
{
    [SerializeField] private bool isProtected;
    [SerializeField] private GameObject leavesVisual;
    [SerializeField] private GameObject butterflyPrefab;

    public bool IsProtected => isProtected;

    private void Awake()
    {
        FindLeavesVisual();
        if (leavesVisual != null && !isProtected)
            leavesVisual.SetActive(false);
    }

    public void Grant(GameObject collectedLeaves, GameObject butterfly)
    {
        FindLeavesVisual();

        isProtected = true;
        butterflyPrefab = butterfly;
        if (leavesVisual != null)
            leavesVisual.SetActive(true);
        else
            Debug.LogWarning("Leaves visual was not found inside the player prefab.", this);

        if (collectedLeaves != null)
        {
            collectedLeaves.SetActive(false);
            Destroy(collectedLeaves);
        }
    }

    public bool TryConsumeAndTransform(enemy caterpillar)
    {
        if (!isProtected)
            return false;

        isProtected = false;
        if (leavesVisual != null)
            leavesVisual.SetActive(false);

        if (caterpillar != null)
        {
            if (butterflyPrefab != null)
            {
                GameObject butterfly = Instantiate(
                    butterflyPrefab,
                    caterpillar.transform.position,
                    Quaternion.identity);
                butterfly.name = "Butterfly (from protected flower)";
            }

            Destroy(caterpillar.gameObject);
            player_move._instance?.OnWorldChanged();
        }

        butterflyPrefab = null;
        return true;
    }

    private void FindLeavesVisual()
    {
        if (leavesVisual != null)
            return;

        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Leaves")
            {
                leavesVisual = child.gameObject;
                return;
            }
        }
    }
}
