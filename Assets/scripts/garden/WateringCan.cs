using UnityEngine;

// Скрипт для лейки. Прикрепите его к объекту лейки (у которого должен быть триггерный коллайдер).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class WateringCan : DragDrop
{
    private Vector3 _startPosition; // Начальная позиция лейки
    private Collider2D _collider;    // Коллайдер лейки
    private Flower _currentFlower; // Текущий цветок под лейкой
    private bool flower_is_enter = false;

    // Размер области проверки полива (половина размеров бокса)
    private readonly Vector2 _checkHalfSize = new Vector2(0.5f, 0.5f);

    private void Awake()
    {
        // Сохраняем начальную позицию для возможного сброса
        _startPosition = transform.position;
        _collider = GetComponent<Collider2D>();

        // Проверяем, что коллайдер есть и он триггер
        if (_collider == null)
        {
            Debug.LogError("Лейка должна иметь коллайдер!");
        }
        else if (!_collider.isTrigger)
        {
            Debug.LogWarning("Коллайдер лейки должен быть триггером!");
        }
    }

    // Отслеживаем любой объект, содержащий компонент Flower, попавший в триггер
    // Проверяем растения под лейкой при отпускании кнопки мыши
    private void OnMouseUp()
    {
        Debug.Log($"[WateringCan] OnMouseUp at {transform.position}");
        int plantLayerMask = LayerMask.GetMask("plant");
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(
            transform.position,
            _checkHalfSize,
            0f,
            plantLayerMask
        );
        Debug.Log($"[WateringCan] Found {hitColliders.Length} potential objects in overlap area.");
        bool anyWatered = false;
        foreach (Collider2D col in hitColliders)
        {
            if (col.TryGetComponent<Flower>(out Flower plant))
            {
                Debug.Log($"[WateringCan] Watering flower '{plant.name}' at {plant.transform.position}");
                plant.Water();
                anyWatered = true;
            }
        }
        if (!anyWatered)
        {
            Debug.Log("[WateringCan] No flowers found, resetting to start position.");
            transform.position = _startPosition;
        }
    }

    // Визуализация области полива в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, _checkHalfSize * 2);
    }
}
