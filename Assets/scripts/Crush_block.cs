using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class Crush_block : MonoBehaviour, IPointerClickHandler
{
    public bool canCrush = true;
    public string obj_name;
    public int count = 0;
    public int max_count = 1;

    [Header("Optional: override particles")]
    public GameObject breakEffectOverride;

    private Material currentMaterial; // текущий материал объекта
    private string materialBasePath = ""; // путь без цифры в конце (например: "Materials/Pink_Crush_")

    private const string DefaultEffectName = "Parts_stones";

    [SerializeField] private AudioClip[] soundClips;

    void Start()
    {
        // Сохраняем базовый путь материала для будущих загрузок
        Material mat = GetComponent<Renderer>().sharedMaterial;
        if (mat != null)
        {
            // Извлекаем путь из имени материала (предполагаем формат "Имя_1")
            string matName = mat.name;
            int lastUnderscore = matName.LastIndexOf('_');
            if (lastUnderscore > 0 && char.IsDigit(matName[lastUnderscore + 1]))
            {
                materialBasePath = matName.Substring(0, lastUnderscore); // "Pink_Crush"
            }
            else
            {
                materialBasePath = matName; // fallback
            }
            currentMaterial = mat;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canCrush) return;

        // Загрузка префаба частиц
        GameObject effectPrefab = breakEffectOverride != null
            ? breakEffectOverride
            : Resources.Load<GameObject>(DefaultEffectName);

        if (effectPrefab == null)
        {
            Debug.LogError($"[Crush_block] Префаб частиц '{DefaultEffectName}' не найден в Resources!");
            return;
        }

        GameObject explosion = Instantiate(effectPrefab, transform.position, Quaternion.identity);

        // ✅ Загружаем материал для частиц по пути
        string particlesMatPath = materialBasePath; // + (count + 1);
        Material particlesMat = Resources.Load<Material>(particlesMatPath);

        if (particlesMat != null)
        {
            ApplyMaterialToParticles(explosion, particlesMat);
        }
        else
        {
            Debug.LogWarning($"Материал для частиц не найден: '{particlesMatPath}'. Используем материал по умолчанию.");
        }

        // Воспроизведение частиц
        var ps = explosion.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        count++;
        if (count >= max_count)
        {
            RandomSound();
            Destroy(gameObject);
            player_move._instance?.OnWorldChanged();
        }
        else
        {
            RandomSound();
            ChangeMaterial();
        }
    }

    void ApplyMaterialToParticles(GameObject particleObj, Material sourceMaterial)
    {
        var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
        if (renderer == null)
        {
            Debug.LogError("ParticleSystemRenderer не найден на префабе частиц!");
            return;
        }

        // Создаём инстанс материала, чтобы не модифицировать оригинал
        renderer.material = new Material(sourceMaterial);
        Debug.Log($"[Crush_block] Применён материал к частицам: {sourceMaterial.name}");
    }

    void ChangeMaterial()
    {
        string newMatPath = materialBasePath + '_' + (count + 1);
        Material newMat = Resources.Load<Material>(newMatPath);
        SpriteShapeRenderer spriteShape = GetComponent<SpriteShapeRenderer>();
        SpriteShapeController spriteController = GetComponent<SpriteShapeController>();

        if (spriteShape != null && newMat != null)
        {
            // Создаём уникальный инстанс материала для изоляции изменений
            spriteShape.material = newMat;
            spriteController.RefreshSpriteShape();
            // Сохраняем ссылку на КЛОН (важно для будущих изменений!)
            currentMaterial = spriteShape.material;

            Debug.Log($"[Crush_block] Материал SpriteShape изменён на: {newMat.name}");
        }

        if (newMat != null)
        {
            // Применяем к рендереру (создаём инстанс для изоляции)
            GetComponent<Renderer>().material = new Material(newMat);
            currentMaterial = newMat;
            Debug.Log($"[Crush_block] Материал объекта изменён на: {newMat.name}");
        }
        else
        {
            Debug.LogWarning($"Материал не найден: '{newMatPath}'. Материал объекта не изменён.");
        }
    }

    void RandomSound()
    {
        if (soundClips == null || soundClips.Length == 0) return;

        int index = Random.Range(0, soundClips.Length);
        AudioSource.PlayClipAtPoint(soundClips[index], transform.position, 1f);
    }
}