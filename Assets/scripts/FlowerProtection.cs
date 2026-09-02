using UnityEngine;

public class FlowerProtection : MonoBehaviour
{
    [SerializeField] private bool isProtected;
    [SerializeField] private GameObject attachedLeaves;
    [SerializeField] private GameObject butterflyPrefab;

    public bool IsProtected => isProtected;

    public void Grant(GameObject leavesVisual, Transform visualParent, GameObject butterfly)
    {
        if (attachedLeaves != null && attachedLeaves != leavesVisual)
            Destroy(attachedLeaves);

        isProtected = true;
        attachedLeaves = leavesVisual;
        butterflyPrefab = butterfly;
        if (attachedLeaves == null)
            return;

        attachedLeaves.transform.SetParent(visualParent != null ? visualParent : transform, true);
        attachedLeaves.transform.localPosition = new Vector3(0f, 0.08f, -0.15f);
        attachedLeaves.transform.localRotation = Quaternion.identity;

        foreach (Collider2D collider in attachedLeaves.GetComponentsInChildren<Collider2D>(true))
            collider.enabled = false;
        if (attachedLeaves.TryGetComponent(out Rigidbody2D body))
            body.simulated = false;
    }

    public bool TryConsumeAndTransform(enemy caterpillar)
    {
        if (!isProtected)
            return false;

        isProtected = false;
        if (attachedLeaves != null)
            Destroy(attachedLeaves);
        attachedLeaves = null;

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
}
