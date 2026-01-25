using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class WorldSpaceCanvasAspectScaler : MonoBehaviour
{
    [Header("Design reference")]
    [Tooltip("Aspect ratio, под который верстался UI (например 9:16 = 0.5625)")]
    public float referenceAspect = 9f / 16f;

    [Header("Scaling")]
    [Tooltip("Минимальный допустимый масштаб")]
    public float minScale = 0.7f;

    [Tooltip("Максимальный допустимый масштаб")]
    public float maxScale = 1f;

    [Tooltip("Скейлить только по ширине (рекомендовано)")]
    public bool scaleWidthOnly = true;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        ApplyScale();
    }

    void Update()
    {
#if UNITY_EDITOR
        ApplyScale();
#endif
    }

    void ApplyScale()
    {
        if (Screen.height == 0 || Screen.width == 0)
            return;

        if (referenceAspect <= 0f)
            return;
        
        float screenAspect = (float)Screen.width / Screen.height;

        float scaleFactor = screenAspect / referenceAspect;

        if (float.IsNaN(scaleFactor) || float.IsInfinity(scaleFactor))
            return;


        if (scaleWidthOnly)
        {
            scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale) / 100;
            rect.localScale = new Vector3(scaleFactor, 0.01f, 0.01f);
        }
        else
        {
            scaleFactor = Mathf.Clamp(scaleFactor, minScale, maxScale) / 100;
            rect.localScale = Vector3.one * scaleFactor;
        }
    }
}