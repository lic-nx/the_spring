using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_move : MonoBehaviour
{
    public GameObject Sun;
    public static player_move _instance;

    // Ответственное за рисование стебля цветка
    public Line_rendered line_render;
    public Trigger_checker[] Triggers;
    private Vector3 body_position;
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
    private Vector3 flowerFacingDirection = Vector3.up;
    private bool isMoving = false;
    [SerializeField] private LayerMask obstacleMask;
    [Header("Сегмент-пробник")]
    public float probeLength = 1.0f;
    public float probeWidth = 0.2f;
    public LayerMask probeObstacleMask;

    // Ссылка на текущий целевой триггер для проверки во время движения
    private Trigger_checker currentTargetTrigger = null;

    [Header("Отладка: подсветка триггеров")]
    [Tooltip("Показывать состояние триггеров цветом (зелёный — цель, красный — занят, жёлтый — запрещённый угол, чёрный — перекрыт препятствием, белый — свободен)")]
    public bool showTriggerDebugColors = true;
    public Color targetTriggerColor = new Color(0.1f, 1f, 0.1f, 0.85f);
    public Color occupiedTriggerColor = new Color(1f, 0.15f, 0.15f, 0.85f);
    public Color angleBlockedTriggerColor = new Color(1f, 0.9f, 0.1f, 0.85f);
    public Color obstacleBlockedTriggerColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);
    public Color freeTriggerColor = new Color(1f, 1f, 1f, 0.85f);

    [Header("Собственный триггер и отступление")]
    [Tooltip("Насколько близким к запрещенному направлению должен быть триггер, чтобы тоже считаться заблокированным")]
    public float blockedDirectionAngle = 35f;

    private readonly HashSet<Collider2D> flowerTriggerBlockers = new HashSet<Collider2D>();
    private readonly HashSet<Collider2D> failureBlockers = new HashSet<Collider2D>();
    private readonly HashSet<Trigger_checker> blockedTriggers = new HashSet<Trigger_checker>();
    private readonly HashSet<Trigger_checker> occupiedAtFailedPosition = new HashSet<Trigger_checker>();
    private readonly List<Vector3> pathHistory = new List<Vector3>();
    private Vector3 blockedRetreatDirection = Vector3.zero;
    private bool hasBlockedRetreatDirection = false;
    private bool retreatRequested = false;
    private bool isRetreating = false;
    private int retreatPathIndex = -1;

    // Cached components
    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;

    public void change_enabled()
    {
        Debug.Log("change enabled");
        enabled = !enabled;
        player_move._instance?.OnWorldChanged();
    }

    private void Awake()
    {
        // Cache frequently used components
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
        if (probeObstacleMask == 0)
        {
            probeObstacleMask = obstacleMask;
        }
    }

    void Start()
    {
        Debug.Log("Start game");
        Debug.Log(first_dot.transform.position);

        line_render.AddPoint(first_dot.transform.position);
        line_render.AddPoint(this.transform.position);

        body_position = transform.position;
        pathHistory.Add(transform.position);

        if (_instance == null)
        {
            _instance = this;
        }
    }

    private void Update()
    {
        if (isRetreating)
        {
            line_render.RetractHead(this.transform.position);
        }
        else
        {
            line_render.UpdateHead(this.transform.position);
        }

        if (failureBlockers.Count > 0)
        {
            int removedFailures = failureBlockers.RemoveWhere(IsInvalidBlocker);
            if (removedFailures > 0 && failureBlockers.Count == 0)
            {
                ClearMovementBlocks();
                if (!isMoving && !isRetreating && enabled)
                {
                    OnWorldChanged();
                }
            }
        }

        if (flowerTriggerBlockers.Count > 0)
        {
            int removed = flowerTriggerBlockers.RemoveWhere(IsInvalidBlocker);
            if (removed > 0 && flowerTriggerBlockers.Count == 0 && !isMoving && enabled)
            {
                OnWorldChanged();
            }
        }

        UpdateTriggerDebugColors();
    }

    private void UpdateTriggerDebugColors()
    {
        if (Triggers == null) return;

        foreach (Trigger_checker trigger in Triggers)
        {
            if (trigger == null) continue;

            if (!showTriggerDebugColors)
            {
                trigger.ResetDebugColor();
                continue;
            }

            Color color;
            if (trigger == currentTargetTrigger)
            {
                color = targetTriggerColor;
            }
            else if (trigger.OnTriggerEnter_)
            {
                color = occupiedTriggerColor;
            }
            else if (blockedTriggers.Contains(trigger) || !CanFitAt(trigger.transform.position))
            {
                color = obstacleBlockedTriggerColor;
            }
            else if (IsTriggerAngleBlocked(trigger))
            {
                color = angleBlockedTriggerColor;
            }
            else
            {
                color = freeTriggerColor;
            }

            trigger.SetDebugColor(color);
        }
    }

    private bool IsTriggerAngleBlocked(Trigger_checker trigger)
    {
        Vector3 newDirection = (trigger.transform.position - transform.position).normalized;
        if (newDirection == Vector3.zero) return false;

        if (hasBlockedRetreatDirection &&
            Vector3.Angle(blockedRetreatDirection, newDirection) <= blockedDirectionAngle)
        {
            return true;
        }

        Vector3 segmentDir = flowerFacingDirection;
        if (segmentDir == Vector3.zero && lastGrowthDirection != Vector3.zero)
        {
            segmentDir = lastGrowthDirection;
        }

        if (segmentDir != Vector3.zero)
        {
            return Vector3.Angle(segmentDir, newDirection) > max_angle;
        }

        if (lastGrowthDirection != Vector3.zero)
        {
            return Vector3.Angle(lastGrowthDirection, newDirection) > max_angle;
        }

        return false;
    }

    private static bool IsInvalidBlocker(Collider2D blocker)
    {
        return blocker == null || !blocker.enabled || !blocker.gameObject.activeInHierarchy;
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
        // Use cached capsule collider when available
        CapsuleCollider2D col = capsule ?? GetComponent<CapsuleCollider2D>();
        if (col == null) return true;

        Vector2 size = col.size * 0.9f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, size, 0f, obstacleMask);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject && !hit.isTrigger)
                return false;
        }

        // Also check if too close to obstacles in the direction we're moving
        Vector3 dir = (position - transform.position).normalized;
        if (dir != Vector3.zero)
        {
            RaycastHit2D hit2 = Physics2D.CircleCast(transform.position, size.magnitude * 0.3f, dir, Vector3.Distance(transform.position, position) * 0.8f, obstacleMask);
            if (hit2.collider != null && hit2.collider.gameObject != gameObject && !hit2.collider.isTrigger)
            {
                return false;
            }
        }
        return true;
    }

    public void OnWorldChanged()
    {
        if (isMoving || isRetreating || !enabled) return;

        if (HasFlowerTriggerBlocker())
        {
            if (!BeginRetreat())
            {
                Debug.Log("Цветок ждет, пока объект покинет его триггер.");
            }
            return;
        }

        if (TryFindNextPosition(out Vector3 target, out Trigger_checker trigger))
        {
            currentTargetTrigger = trigger;
            StartCoroutine(MoveTo(target, trigger));
        }
        else
        {
            Debug.Log("Цветок не может начать движение: нет доступных путей.");
        }
    }

    public void OnFlowerTriggerEnter(Collider2D other)
    {
        if (!enabled || other == null || ShouldIgnoreFlowerTrigger(other)) return;

        bool wasEmpty = flowerTriggerBlockers.Count == 0;
        flowerTriggerBlockers.Add(other);
        failureBlockers.Add(other);
        if (!wasEmpty) return;

        Debug.Log("Объект попал в собственный триггер цветка — движение назад");

        if (isMoving)
        {
            retreatRequested = true;
        }
        else if (!isRetreating)
        {
            BeginRetreat();
        }
    }

    public void OnFlowerTriggerExit(Collider2D other)
    {
        if (other == null) return;

        bool wasBlocking = flowerTriggerBlockers.Remove(other);
        if (wasBlocking && flowerTriggerBlockers.Count == 0 && !isMoving && !isRetreating && enabled)
        {
            OnWorldChanged();
        }
    }

    public void OnObstacleRemoved(GameObject obstacle)
    {
        if (obstacle == null) return;

        Collider2D[] obstacleColliders = obstacle.GetComponentsInChildren<Collider2D>();
        foreach (Collider2D obstacleCollider in obstacleColliders)
        {
            if (obstacleCollider == null) continue;
            failureBlockers.Remove(obstacleCollider);
            flowerTriggerBlockers.Remove(obstacleCollider);
        }

        // Мир изменился: блок разрушен или перемещён — снимаем блокировки с триггеров.
        ClearMovementBlocks();

        if (!isMoving && !isRetreating && enabled)
        {
            OnWorldChanged();
        }
    }

    private bool ShouldIgnoreFlowerTrigger(Collider2D other)
    {
        if (other.transform.IsChildOf(transform)) return true;
        if (other.CompareTag("Player") || other.CompareTag("sun") || other.CompareTag("stem")) return true;
        if (other.GetComponentInParent<LeafPowerup>() != null) return true;
        if (other.GetComponentInParent<enemy>() != null) return true;
        return false;
    }

    private bool HasFlowerTriggerBlocker()
    {
        if (flowerTriggerBlockers.Count == 0) return false;

        flowerTriggerBlockers.RemoveWhere(IsInvalidBlocker);
        return flowerTriggerBlockers.Count > 0;
    }

    private bool BeginRetreat()
    {
        if (!enabled || isRetreating) return false;

        // Отступаем назад по уже пройденному пути, а не просто вдоль стебля.
        PrepareRetreatPath();

        Vector3 backwardDir = GetRetreatDirection();
        Vector3 retreatTarget = retreatPathIndex >= 0
            ? pathHistory[retreatPathIndex]
            : transform.position + backwardDir * 0.03f;

        CaptureFailureState(retreatTarget);

        isRetreating = true;
        StartCoroutine(RetreatUntilTriggerClear());
        return true;
    }

    private void PrepareRetreatPath()
    {
        retreatPathIndex = pathHistory.Count - 1;

        // Если цветок уже стоит в последней сохранённой точке, идём к предыдущей.
        while (retreatPathIndex >= 0 &&
               Vector3.Distance(transform.position, pathHistory[retreatPathIndex]) < 0.05f)
        {
            retreatPathIndex--;
        }
    }

    private Vector3 GetRetreatDirection()
    {
        Vector3 forward = currentGrowDirection != Vector3.zero
            ? currentGrowDirection
            : (lastGrowthDirection != Vector3.zero
                ? lastGrowthDirection
                : (flowerFacingDirection != Vector3.zero ? flowerFacingDirection : transform.up));

        return -forward.normalized;
    }

    private void CaptureFailureState(Vector3 retreatTarget)
    {
        occupiedAtFailedPosition.Clear();
        flowerTriggerBlockers.RemoveWhere(IsInvalidBlocker);
        failureBlockers.UnionWith(flowerTriggerBlockers);

        if (Triggers != null)
        {
            foreach (Trigger_checker trigger in Triggers)
            {
                if (trigger != null && trigger.OnTriggerEnter_)
                {
                    occupiedAtFailedPosition.Add(trigger);
                }
            }
        }

        blockedTriggers.UnionWith(occupiedAtFailedPosition);

        if (currentTargetTrigger != null)
        {
            blockedTriggers.Add(currentTargetTrigger);
        }

        Vector3 direction = currentGrowDirection != Vector3.zero
            ? currentGrowDirection
            : (lastGrowthDirection != Vector3.zero ? lastGrowthDirection : flowerFacingDirection);

        if (direction != Vector3.zero)
        {
            blockedRetreatDirection = direction.normalized;
            hasBlockedRetreatDirection = true;
        }
        else if (Vector3.Distance(transform.position, retreatTarget) > 0.01f)
        {
            blockedRetreatDirection = (transform.position - retreatTarget).normalized;
            hasBlockedRetreatDirection = true;
        }
    }

    private IEnumerator RetreatUntilTriggerClear()
    {
        float actualSpeed = Mathf.Max(moveSpeed * speedMultiplier, 0.01f);

        Debug.Log("Цветок отступает от объекта в собственном триггере.");

        // Двигаемся назад по сохранённому пути, пока все объекты не покинут триггер.
        while (enabled && HasFlowerTriggerBlocker())
        {
            if (retreatPathIndex < 0)
            {
                // Путь закончился — ждём, пока объект сам освободит триггер.
                yield return null;
                continue;
            }

            Vector3 target = pathHistory[retreatPathIndex];
            Vector3 toTarget = target - transform.position;
            float step = actualSpeed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                transform.position = target;
                retreatPathIndex--;
            }
            else
            {
                transform.position += toTarget.normalized * step;
            }

            yield return null;
        }

        isRetreating = false;
        TrimPathHistoryAfterRetreat();

        if (HasFlowerTriggerBlocker())
        {
            if (!BeginRetreat())
            {
                Debug.Log("Цветок ждет, пока объект покинет его триггер.");
            }
        }
        else if (enabled)
        {
            OnWorldChanged();
        }
    }

    private void TrimPathHistoryAfterRetreat()
    {
        int keepCount = retreatPathIndex + 1;
        if (keepCount < 0)
            keepCount = 0;

        if (keepCount < pathHistory.Count)
        {
            pathHistory.RemoveRange(keepCount, pathHistory.Count - keepCount);
        }

        if (pathHistory.Count > 0 &&
            Vector3.Distance(pathHistory[pathHistory.Count - 1], transform.position) < 0.01f)
        {
            pathHistory[pathHistory.Count - 1] = transform.position;
        }
        else
        {
            pathHistory.Add(transform.position);
        }

        retreatPathIndex = -1;
    }

    private void RecordReachedPoint(Vector3 point)
    {
        if (pathHistory.Count == 0 ||
            Vector3.Distance(pathHistory[pathHistory.Count - 1], point) > 0.01f)
        {
            pathHistory.Add(point);
        }
    }

    private void ClearMovementBlocks()
    {
        failureBlockers.Clear();
        blockedTriggers.Clear();
        occupiedAtFailedPosition.Clear();
        hasBlockedRetreatDirection = false;
        blockedRetreatDirection = Vector3.zero;
        if (!isRetreating)
            retreatPathIndex = -1;
    }

    private void ComputeTriggerValues()
    {
        if (Triggers == null) return;
        int count = Triggers.Length;
        if (count == 0) return;

        // Mark occupied or temporarily blocked triggers as value 1
        for (int i = 0; i < count; i++)
        {
            if (Triggers[i] == null) continue;
            if (Triggers[i].OnTriggerEnter_ || blockedTriggers.Contains(Triggers[i]))
            {
                Triggers[i].TriggerValue = 1f;
            }
            else
            {
                Triggers[i].TriggerValue = 0f;
            }
        }

        // For free triggers, if any neighbor is occupied (value 1) -> 0.5
        for (int i = 0; i < count; i++)
        {
            if (Triggers[i] == null) continue;
            if (Triggers[i].OnTriggerEnter_ || blockedTriggers.Contains(Triggers[i])) continue;

            // check neighbors (spatial neighbors by angle)
            int left = (i - 1 + count) % count;
            int right = (i + 1) % count;
            if ((Triggers[left] != null && Triggers[left].TriggerValue >= 0.999f) ||
                (Triggers[right] != null && Triggers[right].TriggerValue >= 0.999f))
            {
                Triggers[i].TriggerValue = 0.5f;
            }
            else
            {
                // neighbors have value 0 or 0.5 (not occupied) -> trigger gets value 0
                Triggers[i].TriggerValue = 0f;
            }
        }
    }

    // Обновлённый метод: возвращает и позицию, и триггер
    public bool TryFindNextPosition(out Vector3 targetPosition, out Trigger_checker targetTrigger)
    {
        targetPosition = transform.position;
        targetTrigger = null;

        if (Triggers == null || Triggers.Length == 0)
        {
            return false;
        }

        // Убираем только уничтоженные триггеры. Освободившиеся триггеры
        // остаются заблокированными до тех пор, пока цветок не найдёт новый
        // свободный триггер и не переместится на него (ClearMovementBlocks).
        blockedTriggers.RemoveWhere(blockedTrigger => blockedTrigger == null);
        ComputeTriggerValues();

        // Priority: 0.5 takes priority for movement; prefer 0.5 over 0; never pick 1
        // If all other triggers are =1 or 0.5, move in that direction (so can move to 0.5)
        // First pass: look for triggers with value 0.5
        Trigger_checker bestTrigger = null;
        float bestScore = float.PositiveInfinity;
        int bestIndex = -1;

        // Try priority order: first check 0.5, then 0
        float[] priorities = new float[] { 0f, 0.5f };
        foreach (float desiredValue in priorities)
        {
            bestTrigger = null;
            bestScore = float.PositiveInfinity;
            bestIndex = -1;
            for (int i = 0; i < Triggers.Length; i++)
            {
                Trigger_checker trigger = Triggers[i];
                if (trigger == null) continue;
                if (trigger.OnTriggerEnter_) continue;
                if (blockedTriggers.Contains(trigger)) continue;
                if (Mathf.Abs(trigger.TriggerValue - desiredValue) > 0.001f) continue;

                Vector3 candidate = trigger.transform.position;
                if (!CanFitAt(candidate)) continue;

                Vector3 newDirection = (candidate - transform.position).normalized;

                if (hasBlockedRetreatDirection &&
                    Vector3.Angle(blockedRetreatDirection, newDirection) <= blockedDirectionAngle)
                {
                    continue;
                }

                Vector3 segmentDir = flowerFacingDirection;
                if (segmentDir == Vector3.zero && lastGrowthDirection != Vector3.zero)
                {
                    segmentDir = lastGrowthDirection;
                }
                if (segmentDir != Vector3.zero)
                {
                    float angle = Vector3.Angle(segmentDir, newDirection);
                    Debug.Log("Угол между сегментом и " + trigger.name + "=" + angle);
                    if (angle > max_angle)
                    {
                        continue;
                    }
                }
                else if (lastGrowthDirection != Vector3.zero)
                {
                    float angle = Vector3.Angle(lastGrowthDirection, newDirection);
                    Debug.Log("Угол между старым направлением и " + trigger.name + "=" + angle);
                    if (angle > max_angle)
                    {
                        continue;
                    }
                }

                // Prefer earlier in array if all else equal? Keep original behavior as fallback
                float score = i; // prefer first in array for same priority
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTrigger = trigger;
                    bestIndex = i;
                    currentGrowDirection = newDirection;
                }
            }
            if (bestTrigger != null)
            {
                Debug.Log($"сейчас движемся к триггеру: {bestTrigger.gameObject.name} (позиция: {bestTrigger.transform.position:F2}) со значением {desiredValue}");
                targetPosition = bestTrigger.transform.position;
                targetTrigger = bestTrigger;
                lastGrowthDirection = currentGrowDirection;
                return true;
            }
        }

        return false;
    }

    // Обновлённая корутина с проверкой занятости триггера во время движения
    IEnumerator MoveTo(Vector3 target, Trigger_checker targetTrigger)
    {
        isMoving = true;

        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, target);
        float actualSpeed = moveSpeed * speedMultiplier;
        float duration = distance / actualSpeed;
        duration = Mathf.Max(duration, 0.05f);

        if (targetTrigger != null)
            Debug.Log($"Движение к {targetTrigger.gameObject.name}: расстояние={distance:F2}, скорость={actualSpeed:F2}, время={duration:F2}с");

        float elapsed = 0f;

        while (elapsed < duration && enabled)
        {
            if (retreatRequested)
            {
                break;
            }

            if (targetTrigger != null && targetTrigger.OnTriggerEnter_)
            {
                Debug.LogWarning($"Триггер {targetTrigger.gameObject.name} занят другим объектом! Движение прервано на позиции {transform.position:F2}");
                break;
            }

            // Check segment/probe in flower's facing direction - if hits stone/obstacle, stop in that direction
            if (flowerFacingDirection == Vector3.zero)
            {
                flowerFacingDirection = currentGrowDirection;
            }
            if (flowerFacingDirection != Vector3.zero)
            {
                Vector2 probeOrigin = transform.position;
                // Cast in a small cone/box to better detect obstacles
                RaycastHit2D hit = Physics2D.Raycast(probeOrigin, flowerFacingDirection, probeLength, probeObstacleMask);
                if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                {
                    // Check if it's too close - stop to avoid going into stones
                    Debug.LogWarning("Сегмент столкнулся с препятствием, движение приостановлено");
                    break;
                }
                // Also sphere cast as backup
                RaycastHit2D hit2 = Physics2D.CircleCast(probeOrigin, 0.15f, flowerFacingDirection, probeLength * 0.5f, probeObstacleMask);
                if (hit2.collider != null && hit2.collider.gameObject != gameObject && !hit2.collider.isTrigger)
                {
                    Debug.LogWarning("Сегмент столкнулся с препятствием (круг), движение приостановлено");
                    break;
                }
            }

            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, target, t);
            if (currentGrowDirection != Vector3.zero)
            {
                flowerFacingDirection = currentGrowDirection.normalized;
            }

            Vector2 headPos = transform.position;
            line_render.UpdateHead(headPos);
            line_render.TryCommitSegment(headPos);
            flower._instance?.RotateByDirection(currentGrowDirection);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Фиксируем текущую позицию (даже если движение прервано)
        line_render.UpdateHead(transform.position);
        line_render.TryCommitSegment(transform.position);
        if (currentGrowDirection != Vector3.zero)
        {
            flowerFacingDirection = currentGrowDirection.normalized;
        }
        flower._instance?.RotateByDirection(currentGrowDirection);

        isMoving = false;

        if (retreatRequested)
        {
            retreatRequested = false;
            BeginRetreat();
            currentTargetTrigger = null;
            yield break;
        }

        if (HasFlowerTriggerBlocker())
        {
            BeginRetreat();
            currentTargetTrigger = null;
            yield break;
        }

        currentTargetTrigger = null;

        // Продолжаем движение только если:
        // 1. Достигли цели (погрешность 0.1f)
        // 2. Триггер всё ещё свободен
        // 3. Объект включён
        bool reachedTarget = targetTrigger != null && Vector3.Distance(transform.position, target) < 0.1f;
        bool triggerStillFree = targetTrigger == null || !targetTrigger.OnTriggerEnter_;

        if (enabled && reachedTarget && triggerStillFree && targetTrigger != null)
        {
            RecordReachedPoint(target);
            ClearMovementBlocks();
            Debug.Log($"Цель {targetTrigger.gameObject.name} достигнута успешно");
            OnWorldChanged();
        }
        else if (enabled)
        {
            if (targetTrigger != null && !triggerStillFree)
                Debug.Log("Триггер занят — ищем альтернативный путь...");
            else if (!reachedTarget)
                Debug.Log("Движение прервано — ищем новый путь...");

            // Пытаемся найти другой свободный триггер
            OnWorldChanged();
        }
    }
}