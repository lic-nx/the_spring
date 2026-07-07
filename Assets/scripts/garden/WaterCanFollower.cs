using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Показывает небольшую иконку лейки, следующую за системным курсором,
/// когда включён режим полива.
/// </summary>
public class WaterCanFollower : MonoBehaviour
{
    [SerializeField] private Image followerImage; // UI Image placed under Canvas

    // Флаг, включён ли режим полива
    public bool IsWateringMode { get; private set; } = false;

    private void Awake()
    {
        if (followerImage == null)
            followerImage = GetComponent<Image>();
        // По умолчанию скрываем
        followerImage.enabled = false;
    }

    private void Update()
    {
        if (!IsWateringMode)
        {
            if (followerImage.enabled) followerImage.enabled = false;
            return;
        }

        // Показать, если скрыт
        if (!followerImage.enabled) followerImage.enabled = true;

        // Позиция мыши в экранных координатах
        Vector2 mousePos = Input.mousePosition;
        // При необходимости можно задать небольшое смещение
        Vector2 offset = Vector2.zero; // e.g. new Vector2(8, -8);
        followerImage.rectTransform.position = mousePos + offset;

        // При удержании кнопки можно скрыть, если курсор уже меняется
        if (Input.GetMouseButton(0))
            followerImage.enabled = false;
    }

    // Вызывается из кода выбора инструмента
    public void EnableFollower()
    {
        IsWateringMode = true;
    }

    public void DisableFollower()
    {
        IsWateringMode = false;
        followerImage.enabled = false;
    }
}
