using UnityEngine;

public class ButterflyFlyAway : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameRate = 9f;
    [SerializeField] private float upwardSpeed = 2.4f;
    [SerializeField] private float sidewaysAmplitude = 0.55f;
    [SerializeField] private float sidewaysFrequency = 2.2f;
    [SerializeField] private float lifetime = 5f;

    private SpriteRenderer spriteRenderer;
    private float age;
    private float phase;
    private float sideSign;

    public void Configure(Sprite[] animationFrames, SpriteRenderer targetRenderer)
    {
        frames = animationFrames;
        spriteRenderer = targetRenderer;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        phase = Random.Range(0f, Mathf.PI * 2f);
        sideSign = Random.value < 0.5f ? -1f : 1f;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float sideways = Mathf.Sin(age * sidewaysFrequency + phase) * sidewaysAmplitude * sideSign;
        transform.position += new Vector3(sideways, upwardSpeed, 0f) * Time.deltaTime;

        if (spriteRenderer != null && frames != null && frames.Length > 0)
        {
            int frame = Mathf.FloorToInt(age * frameRate) % frames.Length;
            spriteRenderer.sprite = frames[frame];
        }

        if (age >= lifetime)
            Destroy(gameObject);
    }
}
