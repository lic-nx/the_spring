using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))] // ← 1. Гарантируем наличие AudioSource
public class DragBlock2D : MonoBehaviour
{
    public float followSpeed = 15f;
    private Rigidbody2D rb;
    private Camera cam;
    private bool isDragging;
    private Vector2 offset;

    private AudioSource audioSource; // ← 2. Ссылка на компонент

    [SerializeField] private AudioClip soundTake;
    [SerializeField] private AudioClip soundRelease;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        audioSource = GetComponent<AudioSource>(); // ← 3. Получаем компонент

        // Настройки для 2D звука (чтобы громкость не зависела от расстояния до камеры)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void OnMouseDown()
    {
        isDragging = true;

        // ✅ 4. Вызываем метод у экземпляра (audioSource), а не у класса
        if (soundTake != null)
            audioSource.PlayOneShot(soundTake, 0.1f);

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = rb.position - mouseWorldPos;

        rb.velocity = Vector2.zero;
    }

    void OnMouseUp()
    {
        isDragging = false;

        // ✅ 5. Используем тот же метод PlayOneShot (без координат)
        if (soundRelease != null)
            audioSource.PlayOneShot(soundRelease, 1f);

        rb.velocity = Vector2.zero;
        player_move._instance?.OnWorldChanged();
    }

    void FixedUpdate()
    {
        if (!isDragging) return;

        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetPos = mouseWorldPos + offset;

        Vector2 newPos = Vector2.MoveTowards(
            rb.position,
            targetPos,
            followSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPos);
    }
}