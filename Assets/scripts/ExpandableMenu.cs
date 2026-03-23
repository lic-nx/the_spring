using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using YG;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Mask))]
[RequireComponent(typeof(RectTransform))]
public class ExpandableMenu : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [Tooltip("Панель, которая будет скрываться/раскрываться")]
    public RectTransform menuPanel;

    [Tooltip("Кнопка открытия (стрелка вниз)")]
    public Button openButton;

    [Tooltip("Кнопка закрытия (стрелка вверх)")]
    public Button closeButton;

    [Header("Настройки анимации")]
    [Tooltip("Скорость открытия/закрытия")]
    public float animSpeed = 5f;

    [Tooltip("Высота меню в открытом состоянии")]
    public float openHeight = 300f;

    [Tooltip("Высота меню в закрытом состоянии (0 = полностью скрыть)")]
    public float closedHeight = 0f;

    [Header("Настройки сворачивания")]
    [Tooltip("Откуда сворачивать: 1 = сверху (вниз), 0 = снизу (вверх)")]
    [Range(0f, 1f)]
    public float collapseDirection = 1f;

    [Header("Звук (опционально)")]
    public AudioClip openCloseSound;
    private AudioSource audioSource;
    private CanvasGroup canvasGroup;

    private bool isOpen = false;
    private Coroutine animCoroutine;
    private Vector2 originalPanelSize;

    // 🔧 ОТЛАДКА: принудительное открытие через контекстное меню
    [ContextMenu("Force Open Menu")]
    public void ForceOpenMenu()
    {
        Debug.Log("[ForceOpenMenu] === ЗАПУСК ===");
        isOpen = true;
        SetMenuHeight(openHeight);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        openButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(true);
        Debug.Log($"[ForceOpenMenu] === ГОТОВО: height={menuPanel.rect.height}, alpha={canvasGroup.alpha} ===");
    }

    private void OnEnable()
    {
        // 🔧 ОТЛАДКА: проверяем родительские CanvasGroup
        var parentGroups = GetComponentsInParent<CanvasGroup>();
        foreach (var group in parentGroups)
        {
            if (group != canvasGroup)
                Debug.LogWarning($"[OnEnable] РОДИТЕЛЬская CanvasGroup '{group.gameObject.name}': alpha={group.alpha}, interactable={group.interactable}");
        }
        Debug.Log($"[OnEnable] Мой CanvasGroup: alpha={canvasGroup.alpha}, interactable={canvasGroup.interactable}");
    }

    private void Awake()
    {
        if (menuPanel == null) menuPanel = GetComponent<RectTransform>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Настройка пивота
        menuPanel.pivot = new Vector2(0.5f, collapseDirection);

        // Настройка Mask
        var mask = GetComponent<Mask>();
        if (mask != null)
        {
            mask.showMaskGraphic = false;

            // 🔧 ОТЛАДКА: проверяем наличие Image для Mask
            var image = GetComponent<Image>();
            if (image == null)
            {
                Debug.LogWarning("[Awake] Mask есть, но Image нет! Добавляем прозрачный Image.");
                image = gameObject.AddComponent<Image>();
                image.color = Color.clear;
            }
        }

        // Настройка CanvasGroup
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;

        openButton.onClick.AddListener(OpenMenu);
        closeButton.onClick.AddListener(CloseMenu);

        originalPanelSize = menuPanel.sizeDelta;
        SetMenuHeight(closedHeight);

        // 🔧 ОТЛАДКА: начальные параметры
        Debug.Log($"[Awake] init: height={menuPanel.rect.height}, sizeDelta.y={menuPanel.sizeDelta.y}, anchors=[{menuPanel.anchorMin},{menuPanel.anchorMax}]");
    }

    private void OpenMenu()
    {
        Debug.Log("[OpenMenu] >>> ВЫЗВАН | isOpen=" + isOpen);

        if (isOpen)
        {
            Debug.LogWarning("[OpenMenu] <<< Уже открыто, выход");
            return;
        }

        PlaySound();
        isOpen = true;
        Debug.Log("[OpenMenu] isOpen=true, targetHeight=" + openHeight);

        if (animCoroutine != null)
        {
            Debug.Log("[OpenMenu] Останавливаем старую корутину");
            StopCoroutine(animCoroutine);
        }

        animCoroutine = StartCoroutine(AnimateMenu(openHeight));
        Debug.Log("[OpenMenu] >>> Coroutine ЗАПУЩЕН");

        openButton.gameObject.SetActive(false);
    }

    private void CloseMenu()
    {
        Debug.Log("[CloseMenu] >>> ВЫЗВАН | isOpen=" + isOpen);

        if (!isOpen)
        {
            Debug.LogWarning("[CloseMenu] <<< Уже закрыто, выход");
            return;
        }

        PlaySound();
        isOpen = false;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateMenu(closedHeight));
        Debug.Log("[CloseMenu] >>> Coroutine ЗАПУЩЕН (закрытие)");
    }

    private IEnumerator AnimateMenu(float targetHeight)
    {
        float startHeight = menuPanel.rect.height;
        float timer = 0f;

        Debug.Log($"[AnimateMenu] >>> START: startHeight={startHeight}, target={targetHeight}, speed={animSpeed}");

        while (timer < 1f)
        {
            timer += Time.unscaledDeltaTime* animSpeed;
            float t = Mathf.Clamp01(timer);

            float newHeight = Mathf.Lerp(startHeight, targetHeight, t);
            SetMenuHeight(newHeight);

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // 🔧 ОТЛАДКА: прогресс анимации (только один раз в середине)
            if (t >= 0.5f && timer < 0.51f)
                Debug.Log($"[AnimateMenu] ~ PROGRESS: t={t:F2}, height={newHeight:F1}, alpha={canvasGroup.alpha:F2}");

            yield return null;
        }

        // Финализация
        SetMenuHeight(targetHeight);

        if (isOpen)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            closeButton.gameObject.SetActive(true);
            Debug.Log("[AnimateMenu] <<< END (OPEN): height=" + menuPanel.rect.height + ", alpha=1");
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            openButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(false);
            Debug.Log("[AnimateMenu] <<< END (CLOSE): height=" + menuPanel.rect.height + ", alpha=0");
        }

        animCoroutine = null;
    }

    private void SetMenuHeight(float height)
    {
        if (menuPanel == null) return;

        Vector2 size = menuPanel.sizeDelta;
        size.y = Mathf.Max(0, height);
        menuPanel.sizeDelta = size;

        // 🔧 ОТЛАДКА: только при изменении (раскомментируйте если нужно)
        // Debug.Log($"[SetMenuHeight] y={size.y}");
    }

    private void PlaySound()
    {
        if (openCloseSound != null && audioSource != null && YG2.saves.EffectMusicEnabled)
        {
            audioSource.PlayOneShot(openCloseSound);
        }
    }
}