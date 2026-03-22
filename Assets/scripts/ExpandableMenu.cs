using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))] // ← Гарантирует наличие CanvasGroup
[RequireComponent(typeof(Mask))]         // ← Гарантирует обрезку контента
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
    public float collapseDirection = 1f; // 1 = сворачивать ВВЕРХ (от верхнего края)

    [Header("Звук (опционально)")]
    public AudioClip openCloseSound;
    private AudioSource audioSource;
    private CanvasGroup canvasGroup; // ← Для управления прозрачностью и кликами

    private bool isOpen = false;
    private Coroutine animCoroutine;
    private Vector2 originalPanelSize; // Запоминаем исходный размер

    private void Awake()
    {
        if (menuPanel == null) menuPanel = GetComponent<RectTransform>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        canvasGroup = GetComponent<CanvasGroup>(); // ← Получаем CanvasGroup

        // ← НАСТРОЙКА ПИВОТА для сворачивания вверх
        // Если сворачиваем вверх (collapseDirection = 1), ставим пивот наверх (y=1)
        menuPanel.pivot = new Vector2(0.5f, collapseDirection);

        // ← НАСТРОЙКА MASK для обрезки детей
        var mask = GetComponent<Mask>();
        mask.showMaskGraphic = false; // Скрываем саму подложку маски, оставляем только функцию обрезки

        // ← НАСТРОЙКА CANVAS GROUP
        canvasGroup.interactable = false; // В закрытом состоянии нельзя кликать по детям
        canvasGroup.blocksRaycasts = false; // В закрытом состоянии дети не блокируют клики
        canvasGroup.alpha = 0f; // Прозрачность

        openButton.onClick.AddListener(OpenMenu);
        closeButton.onClick.AddListener(CloseMenu);

        originalPanelSize = menuPanel.sizeDelta;
        SetMenuHeight(closedHeight);
    }

    private void OpenMenu()
    {
        if (isOpen) return;

        PlaySound();
        isOpen = true;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateMenu(openHeight));

        openButton.gameObject.SetActive(false); // Скрываем кнопку открытия
    }

    private void CloseMenu()
    {
        if (!isOpen) return;

        PlaySound();
        isOpen = false;

        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(AnimateMenu(closedHeight));
    }

    private IEnumerator AnimateMenu(float targetHeight)
    {
        float startHeight = menuPanel.rect.height;
        float timer = 0f;

        while (timer < 1f)
        {
            timer += Time.deltaTime * animSpeed;
            float newHeight = Mathf.Lerp(startHeight, targetHeight, timer);
            SetMenuHeight(newHeight);

            // Плавно меняем прозрачность и доступность для кликов
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer);
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            yield return null;
        }

        SetMenuHeight(targetHeight);

        // Финализируем состояние
        if (isOpen)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            closeButton.gameObject.SetActive(true);
        }
        else
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            openButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(false);
        }
    }

    private void SetMenuHeight(float height)
    {
        Vector2 size = menuPanel.sizeDelta;
        size.y = height;
        menuPanel.sizeDelta = size;
    }

    private void PlaySound()
    {
        if (openCloseSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(openCloseSound);
        }
    }
}