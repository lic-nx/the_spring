using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LeafPowerup : MonoBehaviour
{
    [SerializeField] private GameObject butterflyPrefab;
    private bool consumed;

    private void Awake()
    {
        // When the leaves are attached to a movable block, their colliders must
        // belong to that block's Rigidbody2D so they extend its solid shape.
        // Standalone leaves still need a body for trigger callbacks.
        if (GetComponentInParent<Rigidbody2D>() == null)
        {
            Rigidbody2D body = gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
        }
    }

    public void Configure(GameObject butterfly)
    {
        butterflyPrefab = butterfly;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed)
            return;

        enemy caterpillar = other.GetComponentInParent<enemy>();
        if (caterpillar != null)
        {
            TransformCaterpillar(caterpillar);
            return;
        }

        player_move player = other.GetComponentInParent<player_move>();
        Collider2D bodyCollider = player != null ? player.GetComponent<Collider2D>() : null;
        if (other == bodyCollider)
            AttachProtection(player);
    }

    private void TransformCaterpillar(enemy caterpillar)
    {
        consumed = true;
        Transform caterpillarTransform = caterpillar.transform;
        if (butterflyPrefab != null)
        {
            GameObject butterfly = Instantiate(
                butterflyPrefab,
                caterpillarTransform.position,
                Quaternion.identity);
            butterfly.name = "Butterfly (from caterpillar)";
        }

        Destroy(caterpillar.gameObject);
        Destroy(gameObject);
        player_move._instance?.OnWorldChanged();
    }

    private void AttachProtection(player_move player)
    {
        consumed = true;
        FlowerProtection protection = player.GetComponent<FlowerProtection>();
        if (protection == null)
            protection = player.gameObject.AddComponent<FlowerProtection>();

        protection.Grant(gameObject, butterflyPrefab);
    }
}
