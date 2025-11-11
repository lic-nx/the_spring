using UnityEngine;
using System.Collections;

public class moverPlatform : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 20f;
    public bool mode = false;
    public float safeOffsetDistance = 0.1f; // Насколько отодвигаться от препятствий при откате
    public float returnSpeed = 10f;         // Скорость плавного возврата

    private bool isDragging = false;
    private bool isEnter = false;
    private Vector3 lastSafePosition;
    private Coroutine returnCoroutine;

    void Start()
    {
        lastSafePosition = transform.position;
    }

    void OnMouseEnter() => isEnter = true;
    void OnMouseExit() => isEnter = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isEnter)
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (IsCollidingNow())
            {
                Vector3 escapePosition = FindEscapePosition(safeOffsetDistance);
                returnCoroutine = StartCoroutine(SmoothReturnTo(escapePosition));
            }
            player_move._instance?.OnWorldChanged();
        }

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector3 target = mode
                ? ProjectToSegment(mousePos, pointA.position, pointB.position)
                : mousePos;

            // Вычисляем направление движения мыши
            Vector3 moveDirection = (target - transform.position).normalized;

            // Увеличиваем шаг для более плавного движения
            float step = 0.2f;

            // Проверяем, можно ли двигаться в этом направлении
            RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDirection, step);
            if (hit.collider != null && hit.collider.gameObject != gameObject)
            {
                // Если препятствие прямо на пути, останавливаемся
                return;
            }

            // Двигаемся только если нет коллизии
            Vector3 desiredNext = transform.position + moveDirection * step;
            if (!IsCollidingAt(desiredNext))
            {
                if (mode)
                    desiredNext = ClampToSegment(desiredNext, pointA.position, pointB.position);
                transform.position = desiredNext;
                lastSafePosition = desiredNext;
            }
            else
            {
                // Если коллизия, пытаемся двигаться вдоль препятствия
                Vector3 escapeDirection = FindEscapeDirection(desiredNext);
                if (escapeDirection != Vector3.zero)
                {
                    desiredNext = transform.position + escapeDirection * step;
                    if (!IsCollidingAt(desiredNext))
                    {
                        transform.position = desiredNext;
                        lastSafePosition = desiredNext;
                    }
                }
            }
        }
    }





    Vector3 ProjectToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float sqrLen = ab.sqrMagnitude;
        if (sqrLen < 0.001f) return a;
        float t = Vector3.Dot(point - a, ab) / sqrLen;
        return a + Mathf.Clamp01(t) * ab;
    }

    Vector3 ClampToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        return ProjectToSegment(point, a, b);
    }

    bool IsCollidingAt(Vector3 position)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return false;

        Vector3 original = transform.position;
        transform.position = position;

        Collider2D[] others = Physics2D.OverlapCircleAll(position, 1f);
        bool colliding = false;

        foreach (Collider2D other in others)
        {
            if (other == myCollider || other.isTrigger) continue;
            if ((Physics2D.GetLayerCollisionMask(gameObject.layer) & (1 << other.gameObject.layer)) == 0) continue;

            ColliderDistance2D dist = Physics2D.Distance(myCollider, other);
            if (dist.isValid && dist.distance < 0.01f)
            {
                colliding = true;
                break;
            }
        }

        transform.position = original;
        return colliding;
    }

    bool IsCollidingNow()
    {
        return IsCollidingAt(transform.position);
    }

    // Находит позицию, отодвинутую от ближайших препятствий
    Vector3 FindEscapePosition(float offset)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return lastSafePosition;

        Vector3 currentPosition = transform.position;
        Vector3 escape = lastSafePosition;
        Vector2 totalEscapeDirection = FindEscapeDirection(currentPosition);

        if (totalEscapeDirection != Vector2.zero)
        {
            escape = currentPosition + (Vector3)totalEscapeDirection * offset;
            // Проверяем, что escape не в коллизии
            if (!IsCollidingAt(escape))
                return escape;
            else
            {
                // Увеличиваем offset, если escape в коллизии
                float maxOffset = 5f;
                while (offset < maxOffset)
                {
                    offset += 0.1f;
                    escape = currentPosition + (Vector3)totalEscapeDirection * offset;
                    if (!IsCollidingAt(escape))
                        return escape;
                }
            }
        }

        // Если не нашли безопасную позицию, используем lastSafePosition
        if (!IsCollidingAt(lastSafePosition))
            return lastSafePosition;
        else
            return currentPosition;
    }

    Vector3 FindEscapeDirection(Vector3 desiredNext)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider == null) return Vector3.zero;

        Vector3 currentPosition = transform.position;
        Collider2D[] others = Physics2D.OverlapCircleAll(currentPosition, 1f);
        Vector2 totalEscapeDirection = Vector2.zero;
        int count = 0;

        foreach (Collider2D other in others)
        {
            if (other == myCollider || other.isTrigger) continue;
            if ((Physics2D.GetLayerCollisionMask(gameObject.layer) & (1 << other.gameObject.layer)) == 0) continue;

            ColliderDistance2D dist = Physics2D.Distance(myCollider, other);
            if (dist.isValid && dist.distance < 0.2f)
            {
                totalEscapeDirection -= dist.normal;
                count++;
            }
        }

        if (count > 0)
            totalEscapeDirection.Normalize();

        return totalEscapeDirection;
    }

    IEnumerator SmoothReturnTo(Vector3 targetPosition)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < 3f)
        {
            elapsed += Time.deltaTime * returnSpeed;
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed);
            yield return null;
        }

        transform.position = targetPosition;
        returnCoroutine = null;
    }
}