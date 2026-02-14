using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    public GameObject Sun;
    public static player_move _instance;

    // Ответственное за рисование стебля цветка 
    public Line_rendered line_render;
    public frigger_checker[] Triggers;
    Vector3 body_position;
    private Vector3 currentGrowDirection;
    public bool enabled = false;
    public GameObject first_dot;

    // Настройка скорости
    [Header("Настройка скорости")]
    [Tooltip("Скорость движения в юнитах в секунду")]
    public float moveSpeed = 2.5f;

    [Tooltip("Множитель скорости (1 = нормальная, 2 = в 2 раза быстрее)")]
    [Range(0.1f, 5f)]
    public float speedMultiplier = 1f;

    // Для обратной совместимости
    [HideInInspector]
    public float duration_of_move = 0.4f;

    public float max_angle = 120f;
    private Vector3 lastGrowthDirection = Vector3.zero;
    private bool isMoving = false;
    [SerializeField] private LayerMask obstacleMask;

    // Ссылка на текущий целевой триггер для проверки во время движения
    private frigger_checker currentTargetTrigger = null;

    public void change_enabled()
    {
        Debug.Log("change enabled");
        enabled = !enabled;
    }

    void Start()
    {
        Debug.Log("Start game");
        Debug.Log(first_dot.transform.position);

        line_render.AddPoint(first_dot.transform.position);
        line_render.AddPoint(this.transform.position);

        body_position = transform.position;

        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Update()
    {
        line_render.UpdateHead(this.transform.position);
    }

    public void is_sun()
    {
        Debug.Log("take sun");
        isMoving = false;
        enabled = false;
        flower._instance?.anim_flower(true);
        Destroy(Sun);
    }

    bool CanFitAt(Vector3 position)
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col == null) return true;

        Vector2 size = col.size * 0.9f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, size, 0f, obstacleMask);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject && !hit.isTrigger)
                return false;
        }
        return true;
    }

    public void OnWorldChanged()
    {
        if (isMoving || !enabled) return;

        if (TryFindNextPosition(out Vector3 target, out frigger_checker trigger))
        {
            currentTargetTrigger = trigger;
            StartCoroutine(MoveTo(target, trigger));
        }
        else
        {
            Debug.Log("Цветок не может начать движение: нет доступных путей.");
        }
    }

    // Обновлённый метод: возвращает и позицию, и триггер
    public bool TryFindNextPosition(out Vector3 targetPosition, out frigger_checker targetTrigger)
    {
        targetPosition = transform.position;
        targetTrigger = null;

        foreach (frigger_checker trigger in Triggers)
        {
            if (!trigger.OnTriggerEnter_) // триггер не занят
            {
                Vector3 candidate = trigger.transform.position;
                if (CanFitAt(candidate))
                {
                    Vector3 newDirection = (candidate - transform.position).normalized;

                    if (lastGrowthDirection != Vector3.zero)
                    {
                        float angle = Vector3.Angle(lastGrowthDirection, newDirection);
                        if (angle > max_angle)
                        {
                            continue;
                        }
                    }

                    Debug.Log($"сейчас движемся к триггеру: {trigger.gameObject.name} (позиция: {candidate:F2})");

                    currentGrowDirection = newDirection;
                    targetPosition = candidate;
                    targetTrigger = trigger; // сохраняем ссылку на триггер
                    lastGrowthDirection = newDirection;
                    return true;
                }
            }
        }

        return false;
    }

    // Обновлённая корутина с проверкой занятости триггера во время движения
    IEnumerator MoveTo(Vector3 target, frigger_checker targetTrigger)
    {
        isMoving = true;

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, target);
        float actualSpeed = moveSpeed * speedMultiplier;
        float duration = distance / actualSpeed;
        duration = Mathf.Max(duration, 0.05f);

        Debug.Log($"Движение к {targetTrigger.gameObject.name}: расстояние={distance:F2}, скорость={actualSpeed:F2}, время={duration:F2}с");

        float elapsed = 0f;

        while (elapsed < duration && enabled)
        {
            // ⭐ ПРОВЕРКА: триггер не стал занят другим объектом во время движения
            if (targetTrigger.OnTriggerEnter_)
            {
                Debug.LogWarning($"Триггер {targetTrigger.gameObject.name} занят другим объектом! Движение прервано на позиции {transform.position:F2}");
                break;
            }

            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, target, t);

            Vector2 headPos = transform.position;
            line_render.UpdateHead(headPos);
            line_render.TryCommitSegment(headPos);
            flower._instance?.RotateByDirection(currentGrowDirection);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Фиксируем текущую позицию (даже если движение прервано)
        transform.position = transform.position; // актуальная позиция после Lerp
        line_render.UpdateHead(transform.position);
        line_render.TryCommitSegment(transform.position);
        flower._instance?.RotateByDirection(currentGrowDirection);

        isMoving = false;
        currentTargetTrigger = null;

        // Продолжаем движение только если:
        // 1. Достигли цели (погрешность 0.1f)
        // 2. Триггер всё ещё свободен
        // 3. Объект включён
        bool reachedTarget = Vector3.Distance(transform.position, target) < 0.1f;
        bool triggerStillFree = !targetTrigger.OnTriggerEnter_;

        if (enabled && reachedTarget && triggerStillFree)
        {
            Debug.Log($"Цель {targetTrigger.gameObject.name} достигнута успешно");
            OnWorldChanged();
        }
        else if (enabled)
        {
            if (!triggerStillFree)
                Debug.Log("Триггер занят — ищем альтернативный путь...");
            else if (!reachedTarget)
                Debug.Log("Движение прервано — ищем новый путь...");

            // Пытаемся найти другой свободный триггер
            OnWorldChanged();
        }
    }
}