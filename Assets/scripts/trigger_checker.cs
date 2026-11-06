using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_checker : MonoBehaviour
{
    // Оставлены для совместимости с уже сохранёнными префабами/сценами.
    // Источником истины теперь является набор occupants.
    public bool OnTriggerEnter_ = false;
    public int CounterTrigger = 0;
    public float TriggerValue = 0f; // 0, 0.5, or 1

    private readonly HashSet<Collider2D> occupants = new HashSet<Collider2D>();

    public bool IsOccupied => occupants.Count > 0;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool hasOriginalColor;
    private Color lastDebugColor;
    private bool hasDebugColor;

    private void Update()
    {
        if (occupants.Count == 0)
        {
            // Если объект уничтожили/отключили без OnTriggerExit2D — сбрасываем
            // устаревшее состояние, чтобы триггер не остался «занятым» навсегда.
            if (OnTriggerEnter_ || CounterTrigger != 0)
            {
                SyncLegacyState();
            }
            return;
        }

        occupants.RemoveWhere(IsInvalidOccupant);
        SyncLegacyState();
    }

    private static bool IsInvalidOccupant(Collider2D occupant)
    {
        return occupant == null || !occupant.enabled || !occupant.gameObject.activeInHierarchy;
    }

    private void SyncLegacyState()
    {
        CounterTrigger = occupants.Count;
        OnTriggerEnter_ = occupants.Count > 0;
    }

    public void SetDebugColor(Color color)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer == null) return;

        if (!hasOriginalColor)
        {
            originalColor = spriteRenderer.color;
            hasOriginalColor = true;
        }

        if (hasDebugColor && lastDebugColor == color) return;

        spriteRenderer.color = color;
        lastDebugColor = color;
        hasDebugColor = true;
    }

    public void ResetDebugColor()
    {
        if (spriteRenderer == null || !hasOriginalColor || !hasDebugColor) return;

        spriteRenderer.color = originalColor;
        hasDebugColor = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!ShouldIgnore(other))
        {
            occupants.Add(other);
            SyncLegacyState();
            Debug.Log("An object entered.");
        }
        // player_move._instance.stop();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("sun"))
        {
            Debug.Log("is sun");
            Debug.Log("Object name: " + other.name);
            Debug.Log("find sun = " + other.transform.position);
            StartCoroutine(HandleSunInteraction(other.transform));
        }
        else if (!ShouldIgnore(other))
        {
            occupants.Add(other);
            SyncLegacyState();
        }
    }

    private IEnumerator HandleSunInteraction(Transform sunTransform)
    {
        yield return flower._instance.rotate_flower_to_sun(sunTransform.position);

        player_move._instance.is_sun();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        // Удаляем всегда, а не только если объект проходит ShouldIgnore:
        // теги/компоненты могли измениться, пока объект был внутри.
        if (occupants.Remove(other))
        {
            SyncLegacyState();
        }
    }

    private void OnDisable()
    {
        occupants.Clear();
        SyncLegacyState();
    }

    private bool ShouldIgnore(Collider2D other)
    {
        if (other == null) return true;

        // Нельзя проверять transform.root: в уровнях игрок и блоки находятся
        // под общим родителем GameSpace. Из-за этого триггеры игнорировали
        // вообще все блоки и никогда не становились занятыми.
        if (other.GetComponentInParent<player_move>() != null) return true;
        if (other.GetComponentInParent<flower>() != null) return true;

        return other.CompareTag("Player") ||
               other.CompareTag("sun") ||
               other.CompareTag("stem") ||
               other.CompareTag("enemy") ||
               other.GetComponentInParent<LeafPowerup>() != null ||
               other.GetComponentInParent<enemy>() != null;
    }
}
