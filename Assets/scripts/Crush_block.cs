using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Crush_block : MonoBehaviour, IPointerClickHandler
{
    public bool canCrush = true;
    public string obj_name; // имя объекта (можно использовать для сохранения)
    public int count = 0;
    public int max_count = 1;

    [Header("Optional: override particles")]
    public GameObject breakEffectOverride; // можно указать вручную

    private SpriteRenderer spriteRenderer;
    private Material matDefault;
    private Material matCrash;

    private const string DefaultEffectName = "Parts_stones";

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        matDefault = spriteRenderer.material;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canCrush) return;

        GameObject effectPrefab = breakEffectOverride != null
            ? breakEffectOverride
            : Resources.Load<GameObject>(DefaultEffectName);

        if (effectPrefab == null)
        {
            Debug.LogError($"[Crush_block] Префаб частиц '{DefaultEffectName}' не найден в папке Resources!");
            return;
        }

        GameObject explosion = Instantiate(effectPrefab, transform.position, Quaternion.identity);

        ApplyMaterialToParticles(explosion, spriteRenderer.material);

        var ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(explosion, ps.main.startLifetime.constantMax);
        }

        count++;
        if (count >= max_count)
        {
            Destroy(gameObject);
            player_move._instance?.OnWorldChanged();
        }

        ChangeMaterial();
    }

    void ApplyMaterialToParticles(GameObject particleObj, Material sourceMaterial)
    {
        var ps = particleObj.GetComponent<ParticleSystem>();
        if (ps == null) return;

        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        if (renderer == null)
        {
            Debug.LogError("ParticleSystemRenderer не найден на префабе частиц!");
            return;
        }

        Material instanceMat = new Material(sourceMaterial);
        renderer.material = instanceMat;

        Debug.Log($"[Crush_block] Материал применён к частицам: {sourceMaterial.name}");
    }

    void ChangeMaterial()
    {
        string currentMaterialName = spriteRenderer.material.name;
        string[] nameParts = currentMaterialName.Split('_', ' ');

        if (nameParts.Length >= 2)
        {
            if (int.TryParse(nameParts[nameParts.Length - 2], out int currentNumber))
            {
                currentNumber += 1;
                nameParts[nameParts.Length - 2] = currentNumber.ToString();
            }
        }

        string newMaterialName = string.Join("_", nameParts, 0, nameParts.Length - 1);
        matCrash = Resources.Load<Material>(newMaterialName);

        if (matCrash != null)
        {
            spriteRenderer.material = matCrash;
        }
        else
        {
            Debug.LogError("Материал не найден: " + newMaterialName);
        }
    }
}