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

    [Header("Сегмент-пробник")]
    [Tooltip("Длина проверки препятствий перед цветком")]
    public float probeLength = 1.0f;
    public float probeWidth = 0.2f;

    // Ссылка на текущий целевой триггер для проверки во время движения
    private Trigger_checker currentTargetTrigger = null;

    [Header("Отладка: подсветка триггеров")]
    [Tooltip("Показывать состояние триггеров цветом (зелёный — цель, красный — занят, жёлтый — запрещённый угол, серый — временный кулдаун, чёрный — перекрыт препятствием, белый — свободен)")]
    public bool showTriggerDebugColors;
    public Color targetTriggerColor = new Color(0.1f, 1f, 0.1f, 0.85f);
    public Color occupiedTriggerColor = new Color(1f, 0.15f, 0.15f, 0.85f);
    public Color angleBlockedTriggerColor = new Color(1f, 0.9f, 0.1f, 0.85f);
    public Color obstacleBlockedTriggerColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);
    public Color cooldownTriggerColor = new Color(0.55f, 0.55f, 0.55f, 0.85f);
    public Color freeTriggerColor = new Color(1f, 1f, 1f, 0.85f);

    [Header("Ограничения триггеров")]
    [Tooltip("Кулдаун на конкретный триггер после неудачной попытки дойти до него")]
    public float triggerBlockCooldown = 0.35f;

    [Tooltip("Кулдаун на занятые триггеры после столкновения/отступления")]
    public float failureBlockCooldown = 1.5f;

    [Tooltip("Сколько секунд не реагировать на объект, из-за которого пришлось отступить")]
    public float blockerIgnoreCooldown = 2f;

    [Tooltip("Максимальное время отступления, после которого цветок перестаёт ждать объект")]
    public float retreatTimeout = 2f;

    [Tooltip("Как часто пересчитывать путь, если цветок стоит без движения")]
    public float retryInterval = 0.25f;

    private readonly HashSet<Collider2D> flowerTriggerBlockers = new HashSet<Collider2D>();
    private readonly HashSet<Collider2D> failureBlockers = new HashSet<Collider2D>();
    private readonly Dictionary<Trigger_checker, float> triggerBlockedUntil = new Dictionary<Trigger_checker, float>();
    private readonly Dictionary<Collider2D, float> ignoredFlowerBlockerUntil = new Dictionary<Collider2D, float>();
    private readonly List<Vector3> pathHistory = new List<Vector3>();
    private bool retreatRequested = false;
    private bool isRetreating = false;
    private int retreatPathIndex = -1;
    private float nextRetryTime;
    private float nextNoPathLogTime;

    // Cached components
    private Rigidbody2D rb;
    private CapsuleCollider2D capsule;

    public Collider2D BodyCollider => capsule;

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

        // Сам player_move висит на родительском объекте, а капсула цветка —
        // на дочернем Flower. Без этого CanFitAt всегда возвращал true,
        // и триггеры/движение не видели препятствий.
        if (capsule == null)
        {
            foreach (CapsuleCollider2D candidate in GetComponentsInChildren<CapsuleCollider2D>(true))
            {
                if (candidate != null && candidate.CompareTag("Player"))
                {
                    capsule = candidate;
                    break;
                }
            }
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
                ClearMovementBlocks(true);
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

        PruneTriggerBlocks();

        // Страховка от залипшего состояния: если цветок стоит и не движется,
        // периодически заново проверяем мир. Иначе одна неудачная попытка
        // могла оставить его неподвижным до конца уровня.
        if (enabled && !isMoving && !isRetreating)
        {
            nextRetryTime -= Time.deltaTime;
            if (nextRetryTime <= 0f)
            {
                nextRetryTime = Mathf.Max(0.05f, retryInterval);
                OnWorldChanged();
            }
        }
        else
        {
            nextRetryTime = Mathf.Max(0.05f, retryInterval);
        }

        UpdateTriggerDebugColors();
    }

    private void PruneTriggerBlocks()
    {
        float now = Time.time;

        if (triggerBlockedUntil.Count > 0)
        {
            List<Trigger_checker> expiredTriggers = null;

            foreach (KeyValuePair<Trigger_checker, float> pair in triggerBlockedUntil)
            {
                if (pair.Key == null || now >= pair.Value)
                {
                    if (expiredTriggers == null)
                    {
                        expiredTriggers = new List<Trigger_checker>();
                    }
                    expiredTriggers.Add(pair.Key);
                }
            }

            if (expiredTriggers != null)
            {
                foreach (Trigger_checker trigger in expiredTriggers)
                {
                    triggerBlockedUntil.Remove(trigger);
                }
            }
        }

        if (ignoredFlowerBlockerUntil.Count > 0)
        {
            List<Collider2D> expiredBlockers = null;

            foreach (KeyValuePair<Collider2D, float> pair in ignoredFlowerBlockerUntil)
            {
                if (pair.Key == null || now >= pair.Value)
                {
                    if (expiredBlockers == null)
                    {
                        expiredBlockers = new List<Collider2D>();
                    }
                    expiredBlockers.Add(pair.Key);
                }
            }

            if (expiredBlockers != null)
            {
                foreach (Collider2D blocker in expiredBlockers)
                {
                    ignoredFlowerBlockerUntil.Remove(blocker);
                }
            }
        }
    }

    private bool IsFlowerBlockerIgnored(Collider2D blocker)
    {
        if (blocker == null) return true;
        if (!ignoredFlowerBlockerUntil.TryGetValue(blocker, out float until)) return false;
        if (Time.time < until) return true;

        ignoredFlowerBlockerUntil.Remove(blocker);
        return false;
    }

    private void IgnoreFlowerBlocker(Collider2D blocker, float duration)
    {
        if (blocker == null) return;

        float until = Time.time + Mathf.Max(0.05f, duration);
        if (ignoredFlowerBlockerUntil.TryGetValue(blocker, out float existing) && existing > until)
        {
            until = existing;
        }

        ignoredFlowerBlockerUntil[blocker] = until;
    }

    private bool IsTriggerTemporarilyBlocked(Trigger_checker trigger)
    {
        if (trigger == null) return true;
        if (!triggerBlockedUntil.TryGetValue(trigger, out float until)) return false;
        if (Time.time < until) return true;

        triggerBlockedUntil.Remove(trigger);
        return false;
    }

    private void BlockTriggerTemporarily(Trigger_checker trigger, float duration = -1f)
    {
        if (trigger == null) return;

        if (duration < 0f)
        {
            duration = triggerBlockCooldown;
        }

        float until = Time.time + Mathf.Max(0.05f, duration);

        // Не сокращаем уже установленный более длинный кулдаун.
        if (triggerBlockedUntil.TryGetValue(trigger, out float existing) && existing > until)
        {
            until = existing;
        }

        triggerBlockedUntil[trigger] = until;
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
            else if (trigger.IsOccupied || trigger.OnTriggerEnter_)
            {
                color = occupiedTriggerColor;
            }
            else if (!CanFitAt(trigger.transform.position))
            {
                color = obstacleBlockedTriggerColor;
            }
            else if (IsTriggerTemporarilyBlocked(trigger))
            {
                color = cooldownTriggerColor;
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

        Vector3 segmentDir = GetReferenceGrowthDirection();
        if (segmentDir == Vector3.zero) return false;

        return Vector3.Angle(segmentDir, newDirection) > max_angle;
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
        CapsuleCollider2D col = capsule;
        if (col == null) return true;

        Vector3 delta = position - transform.position;
        float distance = delta.magnitude;

        Vector3 direction = distance > 0.001f
            ? delta / distance
            : GetReferenceGrowthDirection();

        // Проверяем именно тело цветка (капсулу с учётом её offset), а не
        // абстрактный прямоугольник в точке триггера. Иначе голова цветка
        // могла оказаться внутри блока, а проверка этого не видела.
        if (HasObstacleAtFlowerBody(position, direction))
        {
            return false;
        }

        // Проверяем весь путь от текущей позиции до триггера, чтобы цветок
        // не проходил сквозь блок, который находится между точками.
        if (distance > 0.001f)
        {
            float probeRadius = Mathf.Max(0.05f, col.size.x * 0.45f);
            if (HasObstacleAlong(transform.position, direction, distance, probeRadius))
            {
                return false;
            }
        }
        return true;
    }

    private Vector3 GetReferenceGrowthDirection()
    {
        if (flowerFacingDirection.sqrMagnitude > 0.0001f)
        {
            return flowerFacingDirection.normalized;
        }

        if (lastGrowthDirection.sqrMagnitude > 0.0001f)
        {
            return lastGrowthDirection.normalized;
        }

        return Vector3.zero;
    }

    private bool HasObstacleAtFlowerBody(Vector3 basePosition, Vector3 direction)
    {
        if (capsule == null) return false;

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = GetReferenceGrowthDirection();
        }
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.up;
        }
        direction.Normalize();

        Vector2 size = capsule.size * 0.9f;
        float angle = capsule.direction == CapsuleDirection2D.Vertical
            ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f
            : Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Смещаем капсулу так же, как это делает сам CapsuleCollider2D,
        // повёрнутый по направлению роста.
        Vector3 worldOffset = Quaternion.Euler(0f, 0f, angle) * capsule.offset;
        Vector2 center = (Vector2)basePosition + new Vector2(worldOffset.x, worldOffset.y);

        Collider2D[] hits = Physics2D.OverlapCapsuleAll(center, size, capsule.direction, angle, Physics2D.AllLayers);
        return HasObstacle(hits);
    }

    private bool HasObstacle(Collider2D[] hits)
    {
        if (hits == null) return false;

        foreach (Collider2D hit in hits)
        {
            if (IsObstacleCollider(hit))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasObstacleAlong(Vector2 origin, Vector2 direction, float distance, float radius)
    {
        if (distance <= 0.001f || direction.sqrMagnitude < 0.0001f) return false;

        RaycastHit2D[] hits = radius > 0.001f
            ? Physics2D.CircleCastAll(origin, radius, direction, distance, Physics2D.AllLayers)
            : Physics2D.RaycastAll(origin, direction, distance, Physics2D.AllLayers);

        if (hits == null) return false;

        foreach (RaycastHit2D hit in hits)
        {
            if (IsObstacleCollider(hit.collider))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsObstacleCollider(Collider2D hit)
    {
        if (hit == null || hit.isTrigger) return false;
        if (hit.gameObject == gameObject) return false;

        // Важно: НЕ использовать transform.root. В уровнях игрок и блоки лежат
        // под общим родителем GameSpace, поэтому IsChildOf(transform.root)
        // отфильтровывал вообще все препятствия.
        if (hit.transform.IsChildOf(transform)) return false;

        if (hit.CompareTag("Player") ||
            hit.CompareTag("stem") ||
            hit.CompareTag("sun") ||
            hit.CompareTag("enemy") ||
            hit.CompareTag("trigger") ||
            hit.CompareTag("flower"))
        {
            return false;
        }

        if (hit.GetComponentInParent<Trigger_checker>() != null) return false;
        if (hit.GetComponentInParent<flower>() != null) return false;
        if (hit.GetComponentInParent<player_move>() != null) return false;
        if (hit.GetComponentInParent<LeafPowerup>() != null) return false;
        if (hit.GetComponentInParent<enemy>() != null) return false;

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
        else if (Time.time >= nextNoPathLogTime)
        {
            Debug.Log("Цветок не может начать движение: нет доступных путей.");
            nextNoPathLogTime = Time.time + 1f;
        }
    }

    public void OnFlowerTriggerEnter(Collider2D other)
    {
        if (!enabled || other == null || ShouldIgnoreFlowerTrigger(other)) return;

        // Не реагируем повторно на объект, от которого только что отступили.
        // Иначе цветок дёргается на границе триггера: шаг вперёд — вход,
        // шаг назад — выход, и так бесконечно.
        if (IsFlowerBlockerIgnored(other)) return;

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
            ignoredFlowerBlockerUntil.Remove(obstacleCollider);
        }

        // Мир изменился: блок разрушен или перемещён — снимаем блокировки с триггеров.
        ClearMovementBlocks(true);

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

        CaptureFailureState();

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

    private void CaptureFailureState()
    {
        flowerTriggerBlockers.RemoveWhere(IsInvalidBlocker);
        failureBlockers.UnionWith(flowerTriggerBlockers);

        // Запоминаем, от кого отступаем: пока кулдаун не истёк, повторный
        // вход этого объекта в триггер цветка не запускает новое отступление.
        foreach (Collider2D blocker in flowerTriggerBlockers)
        {
            IgnoreFlowerBlocker(blocker, blockerIgnoreCooldown);
        }

        // После столкновения временно исключаем все занятые триггеры, а не
        // только цель: иначе цветок сразу же снова лезет в тот же камень.
        if (Triggers != null)
        {
            foreach (Trigger_checker trigger in Triggers)
            {
                if (trigger != null && (trigger.IsOccupied || trigger.OnTriggerEnter_))
                {
                    BlockTriggerTemporarily(trigger, failureBlockCooldown);
                }
            }
        }

        BlockTriggerTemporarily(currentTargetTrigger, failureBlockCooldown);
    }

    private IEnumerator RetreatUntilTriggerClear()
    {
        float actualSpeed = Mathf.Max(moveSpeed * speedMultiplier, 0.01f);

        float maxRetreatTime = Mathf.Max(0.1f, retreatTimeout);
        float retreatTime = 0f;

        Debug.Log("Цветок отступает от объекта в собственном триггере.");

        // Двигаемся назад по сохранённому пути, пока все объекты не покинут триггер.
        // Ограничены по времени, чтобы не ждать статичный камень бесконечно.
        while (enabled && HasFlowerTriggerBlocker() && retreatTime < maxRetreatTime)
        {
            retreatTime += Time.deltaTime;

            if (retreatPathIndex < 0)
            {
                // Путь закончился — не ждём, пока объект сам освободит триггер.
                Debug.LogWarning("Путь отступления закончился, а триггер всё ещё занят.");
                break;
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
            // Не блокируем цветок навсегда: забываем оставшееся перекрытие,
            // пока объект не выйдет из триггера и не войдёт снова.
            Debug.LogWarning("Цветок не смог отступить за отведённое время — перекрытие временно игнорируется.");
            flowerTriggerBlockers.Clear();
        }

        if (enabled)
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

    private void ClearMovementBlocks(bool clearTriggerCooldowns = false)
    {
        failureBlockers.Clear();

        if (clearTriggerCooldowns)
        {
            triggerBlockedUntil.Clear();
            ignoredFlowerBlockerUntil.Clear();
        }

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
            if (Triggers[i].IsOccupied || Triggers[i].OnTriggerEnter_ || IsTriggerTemporarilyBlocked(Triggers[i]))
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
            if (Triggers[i].IsOccupied || Triggers[i].OnTriggerEnter_ || IsTriggerTemporarilyBlocked(Triggers[i])) continue;

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

        // Блокировки триггеров теперь временные: просроченные записи удаляются,
        // чтобы освободившийся триггер снова стал доступным.
        PruneTriggerBlocks();
        ComputeTriggerValues();

        // Порядок приоритета: сначала полностью свободные (0), затем точки,
        // соседние с занятыми (0.5). Занятые (1) не выбираются никогда.
        Trigger_checker bestTrigger = null;
        float bestScore = float.PositiveInfinity;
        int bestIndex = -1;

        Vector3 referenceDirection = GetReferenceGrowthDirection();

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
                if (trigger.IsOccupied || trigger.OnTriggerEnter_) continue;
                if (IsTriggerTemporarilyBlocked(trigger)) continue;
                if (Mathf.Abs(trigger.TriggerValue - desiredValue) > 0.001f) continue;

                Vector3 candidate = trigger.transform.position;

                // Триггер, в котором цветок уже стоит, не должен выбираться
                // снова: иначе получаем движение «вперёд-назад» на месте.
                if (Vector3.Distance(candidate, transform.position) < 0.1f) continue;

                if (!CanFitAt(candidate)) continue;

                Vector3 newDirection = (candidate - transform.position).normalized;
                if (newDirection == Vector3.zero) continue;

                float angle = referenceDirection != Vector3.zero
                    ? Vector3.Angle(referenceDirection, newDirection)
                    : 0f;

                if (angle > max_angle)
                {
                    continue;
                }

                // Выбираем триггер, который продолжает текущее направление
                // роста. Раньше побеждал первый по порядку в массиве, из-за
                // чего цветок уходил в случайную сторону, а «передние»
                // триггеры потом становились жёлтыми (мнимый угол).
                float score = angle + i * 0.0001f;
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

            if (targetTrigger != null && (targetTrigger.IsOccupied || targetTrigger.OnTriggerEnter_))
            {
                Debug.LogWarning($"Триггер {targetTrigger.gameObject.name} занят другим объектом! Движение прервано на позиции {transform.position:F2}");
                break;
            }

            // Проверяем реальное направление движения, а не только поворот цветка:
            // при повороте facing может ещё смотреть в старую сторону.
            Vector3 probeDirection = target - transform.position;
            if (probeDirection.sqrMagnitude > 0.000001f)
            {
                probeDirection.Normalize();
            }
            else
            {
                probeDirection = flowerFacingDirection != Vector3.zero
                    ? flowerFacingDirection
                    : currentGrowDirection;
            }

            if (probeDirection != Vector3.zero)
            {
                Vector2 probeOrigin = transform.position;
                float probeRadius = capsule != null
                    ? Mathf.Max(0.05f, capsule.size.x * 0.45f)
                    : 0.15f;

                if (HasObstacleAlong(probeOrigin, probeDirection, probeLength, probeRadius))
                {
                    // Check if it's too close - stop to avoid going into stones
                    Debug.LogWarning("Сегмент столкнулся с препятствием, движение приостановлено");
                    break;
                }
            }

            float t = elapsed / duration;
            Vector3 nextPosition = Vector3.Lerp(start, target, t);

            // Финальная проверка шага: даже если тонкий луч не задел блок,
            // само тело цветка не должно в него войти.
            if (probeDirection != Vector3.zero &&
                HasObstacleAtFlowerBody(nextPosition, probeDirection))
            {
                Debug.LogWarning("Тело цветка упёрлось в препятствие, движение приостановлено");
                break;
            }

            transform.position = nextPosition;
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
        bool triggerStillFree = targetTrigger == null ||
                                (!targetTrigger.IsOccupied && !targetTrigger.OnTriggerEnter_);

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

            // Не возвращаемся сразу к той же цели: именно это вызывало
            // бесконечное «вперёд-назад» на одном месте.
            BlockTriggerTemporarily(targetTrigger, failureBlockCooldown);

            // Пытаемся найти другой свободный триггер
            OnWorldChanged();
        }
    }
}