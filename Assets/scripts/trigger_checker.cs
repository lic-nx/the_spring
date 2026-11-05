using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_checker : MonoBehaviour
{
    public bool OnTriggerEnter_ = false;
    public int CounterTrigger = 0;
    public float TriggerValue = 0f; // 0, 0.5, or 1

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool hasOriginalColor;
    private Color lastDebugColor;
    private bool hasDebugColor;

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
            CounterTrigger++;
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
            OnTriggerEnter_ = true;
        }
    }

    private IEnumerator HandleSunInteraction(Transform sunTransform)
    {
        yield return flower._instance.rotate_flower_to_sun(sunTransform.position);

        player_move._instance.is_sun();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!ShouldIgnore(other))
        {
            CounterTrigger--;
            if (CounterTrigger <= 0)
            {
                OnTriggerEnter_ = false;
            }
        }
    }

    private static bool ShouldIgnore(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("enemy") ||
               other.GetComponentInParent<LeafPowerup>() != null;
    }

}
