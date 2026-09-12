using UnityEngine;
using UnityEngine.UI;

public class swipe_menu : MonoBehaviour
{
    [Header("?????????")]
    public ScrollRect scrollRect;        // ScrollRect component
    public int MaxCountOfLists = 3;      // total pages
    public int NowList = 1;              // current page (1?based)
    [Header("????????? ?????????")]
    public bool isHorizontal = true;    // horizontal or vertical scroll
    public float stepSize = 200f;        // distance per page
    public float scrollSpeed = 10f;      // lerp speed when smoothScroll
    public bool smoothScroll = true;    // smooth (lerp) or instant

    // Cached reference and state
    private RectTransform content;
    private Vector2 targetAnchoredPosition;
    private bool isScrolling = false;   // update only during animation

    void Awake()
    {
        if (scrollRect == null)
        {
            Debug.LogError("SwipeMenu: ScrollRect not assigned!");
            return;
        }
        content = scrollRect.content;
        if (content == null)
        {
            Debug.LogError("SwipeMenu: ScrollRect has no content!");
            return;
        }
        targetAnchoredPosition = content.anchoredPosition;
    }

    void Update()
    {
        if (!isScrolling) return;
        // Smoothly lerp to target
        Vector2 current = content.anchoredPosition;
        content.anchoredPosition = Vector2.Lerp(current, targetAnchoredPosition,
            Mathf.Clamp01(scrollSpeed * Time.unscaledDeltaTime));
        // Stop when close enough
        if (Vector2.Distance(content.anchoredPosition, targetAnchoredPosition) < 0.1f)
        {
            content.anchoredPosition = targetAnchoredPosition;
            isScrolling = false;
        }
    }

    public void ScrollPrev()
    {
        if (content == null) return;
        if (NowList <= 1) return;
        NowList--;
        float delta = isHorizontal ? +stepSize : -stepSize;
        MoveContent(delta);
    }

    public void ScrollNext()
    {
        if (content == null) return;
        if (NowList >= MaxCountOfLists) return;
        NowList++;
        float delta = isHorizontal ? -stepSize : +stepSize;
        MoveContent(delta);
    }

    private void MoveContent(float delta)
    {
        Vector2 currentPos = smoothScroll ? targetAnchoredPosition : content.anchoredPosition;
        Vector2 newPos = currentPos + (isHorizontal ? Vector2.right : Vector2.up) * delta;
        targetAnchoredPosition = newPos;
        if (smoothScroll)
            isScrolling = true; // trigger Update
        else
        {
            content.anchoredPosition = targetAnchoredPosition;
            isScrolling = false;
        }
    }

    // Reset to first page
    public void ResetToStart()
    {
        targetAnchoredPosition = Vector2.zero;
        if (smoothScroll)
            isScrolling = true;
        else if (content != null)
            content.anchoredPosition = Vector2.zero;
    }
}
