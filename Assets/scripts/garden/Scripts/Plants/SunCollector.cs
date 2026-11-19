using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Скрипт для обработки клика по солнышку.
/// Работает через EventSystem (IPointerClickHandler), что гарантирует работу на мобилках.
/// 
/// ТРЕБОВАНИЯ:
/// 1. На объекте должен быть Collider2D (желательно isTrigger = true для Physics2DRaycaster)
/// 2. На камере должен быть Physics2DRaycaster
/// 3. На сцене должен быть EventSystem
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SunCollector : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler
{
    [Header("Область появления солнышек")]
    [Tooltip("Максимальное расстояние от центра растения в локальных единицах цветка. Нижний сектор 90° исключён.")]
    [SerializeField, Min(0f)] private float spawnRadius = 4f;

    [Tooltip("Минимальное расстояние от центра растения после вылета солнышка. Не превышает Spawn Radius.")]
    [SerializeField, Min(0f)] private float minimumSpawnRadius = 2f;

    private const float AppearanceDuration = 0.45f;

    private Flower _parentFlower;
    private Vector3 _spawnCenter;
    private Vector2 _spawnDirection;
    private float _spawnFraction;
    private float _spawnElapsed;
    private Vector3 _restingScale;
    private float _hoverPhase;
    private bool _collectRequested;

    private void Awake()
    {
        _restingScale = transform.localScale;
        // Ищем родительский Flower
        _parentFlower = GetComponentInParent<Flower>();
        if (_parentFlower == null)
        {
            _parentFlower = GetComponent<Flower>();
        }
        if (_parentFlower == null)
        {
            Debug.LogError($"[SunCollector] Не найден родительский Flower у объекта '{gameObject.name}'!");
        }

        // Для Physics2DRaycaster надёжнее, когда коллайдер настроен как триггер.
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        SpriteRenderer sunRenderer = GetComponent<SpriteRenderer>();
        if (collider is BoxCollider2D hitArea && sunRenderer != null && sunRenderer.sprite != null)
        {
            hitArea.size = (Vector2)sunRenderer.sprite.bounds.size * 1.15f;
            hitArea.offset = sunRenderer.sprite.bounds.center;
        }
    }

    private void OnEnable()
    {
        if (_parentFlower == null)
        {
            _parentFlower = GetComponentInParent<Flower>();
            if (_parentFlower == null)
            {
                _parentFlower = GetComponent<Flower>();
            }
        }
        _spawnCenter = transform.localPosition;
        if (_parentFlower != null)
        {
            SpriteRenderer flowerRenderer = _parentFlower.GetComponent<SpriteRenderer>();
            if (flowerRenderer != null && flowerRenderer.sprite != null)
                _spawnCenter = flowerRenderer.sprite.bounds.center;
        }

        // -45° to 225° leaves the 90° sector centred on downward (-Y) empty.
        float angle = Random.Range(-45f, 225f) * Mathf.Deg2Rad;
        _spawnDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        _spawnFraction = Random.value;
        _hoverPhase = Random.Range(0f, Mathf.PI * 2f);
        _spawnElapsed = 0f;
        _collectRequested = false;
        transform.localScale = Vector3.zero;
        transform.localPosition = new Vector3(_spawnCenter.x, _spawnCenter.y, -0.05f);
    }

    private void LateUpdate()
    {
        if (_collectRequested)
        {
            _collectRequested = false;
            TryCollect();
            return;
        }

        _spawnElapsed += Time.deltaTime;
        float appearanceProgress = Mathf.Clamp01(_spawnElapsed / AppearanceDuration);
        transform.localScale = _restingScale * Mathf.Sqrt(appearanceProgress);
        float flightProgress = Mathf.SmoothStep(0f, 1f, appearanceProgress);
        float radius = Mathf.Max(0f, spawnRadius);
        float innerRadius = Mathf.Clamp(minimumSpawnRadius, 0f, radius);
        float distance = Mathf.Sqrt(Mathf.Lerp(innerRadius * innerRadius, radius * radius, _spawnFraction));
        Vector2 relativePosition = _spawnDirection * distance;

        // Keep the idle hover inside the annular sector as well.
        float offset = Mathf.Sin(Time.time * 1.6f + _hoverPhase) * 0.18f;
        float circleHeight = Mathf.Sqrt(Mathf.Max(0f, radius * radius - relativePosition.x * relativePosition.x));
        float minimumY = Mathf.Max(-circleHeight, -Mathf.Abs(relativePosition.x));
        float maximumY = circleHeight;
        if (Mathf.Abs(relativePosition.x) < innerRadius)
        {
            float innerHeight = Mathf.Sqrt(innerRadius * innerRadius - relativePosition.x * relativePosition.x);
            if (_spawnDirection.y >= 0f)
                minimumY = Mathf.Max(minimumY, innerHeight);
            else
                maximumY = Mathf.Min(maximumY, -innerHeight);
        }
        float y = Mathf.Clamp(relativePosition.y + offset, minimumY, maximumY);
        transform.localPosition = new Vector3(
            _spawnCenter.x + relativePosition.x * flightProgress,
            _spawnCenter.y + y * flightProgress,
            -0.05f);

        // The next pointer raycast must use the same pose that was rendered.
        Physics2D.SyncTransforms();
    }

    private void OnMouseDown()
    {
        // Pointer raycasts respect the pot menu and other UI in front of the sun.
        if (EventSystem.current == null)
            TryCollect();
    }

    /// <summary>
    /// Вызывается при клике на солнышко (работает и на ПК, и на мобилках).
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            _collectRequested = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Keep this gesture on the sun instead of forwarding it to the pot.
    }

    private void TryCollect()
    {
        if (_parentFlower == null)
        {
            _parentFlower = GetComponentInParent<Flower>();
            if (_parentFlower == null)
            {
                _parentFlower = GetComponent<Flower>();
            }
        }

        if (_parentFlower != null)
        {
            _parentFlower.CollectSun();
        }
    }
}
