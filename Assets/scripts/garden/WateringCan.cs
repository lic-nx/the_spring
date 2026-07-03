using UnityEngine;
using System.Collections;

// Скрипт для лейки. Прикрепите его к объекту лейки (у которого должен быть триггерный коллайдер).
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class WateringCan : MonoBehaviour
{
    // Позиция лейки (не перемещается)
    private Vector3 _startPosition;
    private Collider2D _collider;
    private Animator _animator;

    // Активна ли лейка (переключается по клику на ней)
    private bool _isActive = false;

    // Настраиваемый слой растений (по умолчанию "Plant")
    [SerializeField] private string plantLayerName = "Plant";
    // Размер проверочного бокса берётся из коллайдера
    private Vector2 _checkHalfSize => _collider != null ? _collider.bounds.extents : new Vector2(0.5f, 0.5f);

    private void Awake()
    {
        _startPosition = transform.position;
        _collider = GetComponent<Collider2D>();
        _animator = GetComponent<Animator>();

        if (_collider == null)
        {
            Debug.LogError("Лейка должна иметь коллайдер!");
        }
        else if (!_collider.isTrigger)
        {
            Debug.LogWarning("Коллайдер лейки должен быть триггером!");
        }
    }

    // Клик по лейке переключает её активность
    private void OnMouseDown()
    {
        _isActive = !_isActive;
        Debug.Log($"[WateringCan] Активна: {_isActive}");
    }

    // Пока лейка активна, проверяем каждый клик в мире
    private void Update()
    {
        if (!_isActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = Physics2D.GetRayIntersection(ray);
        // Log which object was clicked (if any)
        if (hit.collider != null)
        {
            Debug.Log($"[WateringCan] Клик по объекту: {hit.collider.gameObject.name}");
        }
        else
        {
            Debug.Log("[WateringCan] Клик по пустому пространству.");
        }
        if (hit.collider != null && hit.collider.TryGetComponent<Flower>(out var flower))
        {
            // Запускаем анимацию полива, лейка не перемещается
            _animator?.SetTrigger("Water");
            flower.Water();
            Debug.Log($"[WateringCan] Полив цветка '{flower.name}'.");
        }
        else if (hit.collider != null)
        {
            // При клике не по цветку ничего не делаем, но логируем
            Debug.Log($"[WateringCan] Клик по объекту '{hit.collider.gameObject.name}' – не цветок.");
        }
        else
        {
            // При клике в пустом месте
            Debug.Log("[WateringCan] Клик по пустому пространству – ничего не происходит.");
        }
    }

    // Гизмо для отладки (не используется в текущей логике, но оставлен)
    private void OnDrawGizmosSelected()
    {
        if (_collider != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, _checkHalfSize * 2);
        }
    }
}
