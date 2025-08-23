using UnityEngine;
using System.Collections;

public class ShakeOnClick : MonoBehaviour
{
    public float duration = 0.2f;   // сколько длится тряска
    public float magnitude = 0.1f;  // сила смещения

    private Vector3 originalPos;

    void OnMouseDown()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}