using UnityEngine;

public class MoverPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 20f;
    public bool mode = false; // false = свободное перемещение, true = только по отрезку

    private bool isDragging = false;
    private bool isEnter = false;

    [SerializeField] private AudioClip soundTake;
    [SerializeField] private AudioClip soundRelease;

    private AudioSource audioSource; // ← Ссылка на компонент

    private void Awake()
    {
        // Получаем AudioSource с этого объекта
        audioSource = GetComponent<AudioSource>();

        // Если компонента нет — добавляем автоматически
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Настройки для 2D звука (громкость не зависит от расстояния до камеры)
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void OnMouseEnter() => isEnter = true;
    void OnMouseExit() => isEnter = false;

    void Update()
    {
        // Начало перетаскивания
        if (Input.GetMouseButtonDown(0) && isEnter)
        {
            isDragging = true;

            // ✅ PlayOneShot принимает только (клип, громкость)
            if (soundTake != null)
                audioSource.PlayOneShot(soundTake, 1f);
        }
        // Завершение перетаскивания
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                // ✅ То же самое для звука отпускания
                if (soundRelease != null)
                    audioSource.PlayOneShot(soundRelease, 1f);
            }
            isDragging = false;
            player_move._instance?.OnWorldChanged();
        }

        // Перемещение при удержании
        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // Определяем целевую позицию
            Vector3 target = mode
                ? ProjectToSegment(mousePos, pointA.position, pointB.position)
                : mousePos;

            // Плавное движение к цели
            transform.position = Vector3.MoveTowards(
                transform.position,
                target,
                speed * Time.deltaTime
            );
        }
    }

    // Проекция точки на отрезок AB
    Vector3 ProjectToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float sqrLen = ab.sqrMagnitude;
        if (sqrLen < 0.001f) return a;
        float t = Vector3.Dot(point - a, ab) / sqrLen;
        return a + Mathf.Clamp01(t) * ab;
    }
}