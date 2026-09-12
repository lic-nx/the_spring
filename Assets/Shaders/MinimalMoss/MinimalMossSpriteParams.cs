using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MinimalMossSpriteParams : MonoBehaviour
{
    private static readonly int MossLocalMinYId = Shader.PropertyToID("_MossLocalMinY");
    private static readonly int MossLocalMaxYId = Shader.PropertyToID("_MossLocalMaxY");
    private static readonly int SeedId = Shader.PropertyToID("_Seed");

    [SerializeField] private float seed;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void OnEnable()
    {
        CacheReferences();
        Apply();
    }

    private void OnValidate()
    {
        CacheReferences();
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    [ContextMenu("Apply Moss Sprite Parameters")]
    public void Apply()
    {
        CacheReferences();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        var bounds = spriteRenderer.sprite.bounds;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(MossLocalMinYId, bounds.min.y);
        propertyBlock.SetFloat(MossLocalMaxYId, bounds.max.y);
        propertyBlock.SetFloat(SeedId, seed);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void CacheReferences()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }
}
