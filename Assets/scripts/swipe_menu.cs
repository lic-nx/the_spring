using UnityEngine;
using UnityEngine.UI;

public class swipe_menu : MonoBehaviour
{
    [Header("Ссылки")]
    public ScrollRect scrollRect;        // Ссылка на ScrollRect

    [Header("Настройки")]
    public bool isHorizontal = true;     // Горизонтальная прокрутка?
    public float stepSize = 200f;        // На сколько пикселей сдвигать контент
    public float scrollSpeed = 10f;      // Скорость прокрутки (в пикселях/сек)
    public bool smoothScroll = true;     // Плавная прокрутка или мгновенная?

    private RectTransform content;
    private Vector2 targetAnchoredPosition;

    void Awake()
    {
        if (scrollRect == null)
        {
            Debug.LogError("ScrollByFixedStep: ScrollRect не назначен!");
            return;
        }
        content = scrollRect.content;
        if (content == null)
        {
            Debug.LogError("ScrollByFixedStep: Content не найден в ScrollRect!");
            return;
        }
        targetAnchoredPosition = content.anchoredPosition;
    }

    void Update()
    {
        if (smoothScroll && content != null)
        {
            Vector2 current = content.anchoredPosition;
            content.anchoredPosition = Vector2.Lerp(current, targetAnchoredPosition,
                Mathf.Clamp01(scrollSpeed * Time.unscaledDeltaTime));
        }
    }

    public void ScrollPrev()
    {
        if (content == null) return;

        float delta = isHorizontal ? +stepSize : -stepSize;
        MoveContent(delta);
    }

    public void ScrollNext()
    {
        if (content == null) return;

        float delta = isHorizontal ? -stepSize : +stepSize;
        MoveContent(delta);
    }

    private void MoveContent(float delta)
    {
        Vector2 currentPos = smoothScroll ? targetAnchoredPosition : content.anchoredPosition;
        Vector2 newPos = currentPos + (isHorizontal ? Vector2.right : Vector2.up) * delta;

        // Ограничим прокрутку, чтобы не уйти "за край"
        // Unity автоматически ограничивает прокрутку через ScrollRect,
        // но мы можем дополнительно ограничить вручную, если нужно.
        // Для упрощения здесь просто обновляем позицию.

        targetAnchoredPosition = newPos;

        if (!smoothScroll)
        {
            content.anchoredPosition = targetAnchoredPosition;
        }
    }

    // Опционально: сброс в начало
    public void ResetToStart()
    {
        targetAnchoredPosition = Vector2.zero;
        if (!smoothScroll && content != null)
            content.anchoredPosition = Vector2.zero;
    }
}