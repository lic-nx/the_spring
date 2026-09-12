using UnityEngine;

/// <summary>
/// Скрипт лейки. При отпускании поливает ВСЕ цветы, коллайдеры которых 
/// попали в прямоугольную область под лейкой.
/// 
/// ВАЖНО: Лейка ВСЕГДА возвращается на исходную позицию, 
/// так как это инструмент, а не размещаемый объект.
/// 
/// ТРЕБОВАНИЯ К ОБЪЕКТУ:
/// 1. Collider2D (НЕ триггер!) — для работы EventSystem
/// 2. Слой "plant" должен быть назначен коллайдерам цветов
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class WateringCan : WorldDraggable
{
    [Header("Настройки полива")]
    [Tooltip("Половина размера прямоугольной области полива (ширина и высота)")]
    [SerializeField] private Vector2 checkHalfSize = new Vector2(0.5f, 0.5f);

    [Tooltip("Слой, на котором находятся цветы (для фильтрации при поливе)")]
    [SerializeField] private LayerMask plantLayerMask;

    [Header("Визуальный индикатор (опционально)")]
    [Tooltip("Спрайт-круг/прямоугольник, показывающий зону полива во время перетаскивания. Можно оставить пустым.")]
    [SerializeField] private GameObject wateringAreaIndicator;

    private Vector3 _startPosition; // Исходная позиция лейки на сцене

    private void Awake()
    {
        // ⚠️ ВАЖНО: вызываем Awake базового класса
        base.Awake();

        // Если слой не задан в инспекторе — берём стандартный "plant"
        if (plantLayerMask.value == 0)
        {
            plantLayerMask = LayerMask.GetMask("plant");
        }
    }

    private void Start()
    {
        // Сохраняем исходную позицию лейки (её "дом")
        _startPosition = transform.position;
    }

    // ===== Переопределение методов перетаскивания =====

    /// <summary>
    /// Начало перетаскивания — показываем индикатор зоны полива.
    /// </summary>
    protected override void OnDragStarted()
    {
        if (wateringAreaIndicator != null)
        {
            wateringAreaIndicator.SetActive(true);
        }
    }

    /// <summary>
    /// Каждый кадр во время перетаскивания.
    /// </summary>
    protected override void OnDragging()
    {
        // Индикатор — дочерний объект, он и так следует за лейкой.
    }

    /// <summary>
    /// КЛЮЧЕВОЕ ПЕРЕОПРЕДЕЛЕНИЕ:
    /// Лейка ВСЕГДА возвращается на исходную позицию, 
    /// независимо от того, удалось ли полить цветы.
    /// </summary>
    protected override void OnDragEnded(bool success)
    {
        // 1. ВСЕГДА возвращаем лейку на её "домашнюю" позицию
        transform.position = _startPosition;

        // 2. Скрываем индикатор зоны полива
        if (wateringAreaIndicator != null)
        {
            wateringAreaIndicator.SetActive(false);
        }

        // 3. Логируем результат полива (для отладки)
        if (success)
        {
            Debug.Log("🚿 Лейка успешно полила цветы. Возвращаемся на место.");
        }
        else
        {
            Debug.Log("🚿 Под лейкой нет цветов. Возвращаемся на место.");
        }
    }

    // ===== КЛЮЧЕВОЙ МЕТОД: переопределяем проверку дропа =====

    /// <summary>
    /// Переопределяем базовую логику TryDrop.
    /// Вместо проверки iPotDropArea ищем цветы под лейкой и поливаем их.
    /// Возвращает true, если удалось полить хотя бы один цветок.
    /// 
    /// ВАЖНО: Возвращаемое значение влияет только на логирование, 
    /// так как OnDragEnded ВСЕГДА возвращает лейку на место.
    /// </summary>
    protected override bool TryDrop(Vector2 screenPosition)
    {
        Vector2 worldPoint = ScreenToWorld(screenPosition);

        // Временно отключаем свой коллайдер, чтобы не попасть в себя
        _collider.enabled = false;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            worldPoint,
            checkHalfSize * 2f, // OverlapBoxAll принимает ПОЛНЫЙ размер, а не половину
            0f,
            plantLayerMask
        );

        _collider.enabled = true;

        int wateredCount = 0;
        foreach (Collider2D col in hits)
        {
            if (col.TryGetComponent<Flower>(out Flower plant))
            {
                plant.Water();
                wateredCount++;
            }
        }

        Debug.Log($"🚿 [WateringCan] Найдено объектов в зоне: {hits.Length}, полито цветов: {wateredCount}");

        return wateredCount > 0;
    }

    // ===== Визуализация в редакторе =====

    /// <summary>
    /// Рисует прямоугольник зоны полива в редакторе (при выделении объекта).
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f); // Полупрозрачный cyan
        Gizmos.DrawWireCube(transform.position, checkHalfSize * 2f);
    }
}