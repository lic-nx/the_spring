using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LeafPowerup : MonoBehaviour
{
    [SerializeField] private GameObject butterflyPrefab;
    private bool consumed;

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
        if (player != null)
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
