using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class PopUpUI : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private float delay = 0f;
    [SerializeField] private AnimationCurve scaleCurve;
    public bool autoActive = false;

    private RectTransform rectTransform;
    private Vector3 startScale;
    private Vector3 targetScale;
    private Coroutine animRoutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startScale = rectTransform.localScale * scaleCurve.Evaluate(0.0f);
        targetScale = rectTransform.localScale;

        
    }

    private void Start()
    {
        if (autoActive)
        {
            StartAnim();
        }
    }

    private void OnEnable()
    {
        rectTransform.localScale = startScale;
        StartAnim();
    }

    private void StartAnim()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

    animRoutine = StartCoroutine(PlayPopUp());
    }
    
    private IEnumerator PlayPopUp()
    {
        // ⏳ Задержка перед началом
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float normalized = t / duration;

            float curveValue = scaleCurve.Evaluate(normalized);
            rectTransform.localScale = targetScale * curveValue;

            yield return null;
        }

        rectTransform.localScale = targetScale;
    }
}