mkkusing UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Универсальный UI‑компонент, показывающий любую иконку, которая следует за системным курсором,
/// когда включён соответствующий режим (например, полив, удобрение и т.д.).
/// Иконка задаётся через инспектор (Sprite), а компонент автоматически обновляет
/// позицию в координатах Canvas.
/// </summary>
[RequireComponent(typeof(Image))]
public class FollowCursor : MonoBehaviour
{
    // Ссылка на UI‑Image, который будет отображать иконку
    private Image _image;

    // Флаг активности режима – включается извне (инструментом)
    public bool IsActive { get; private set; } = false;

    // Смещение курсора, если нужно, чтобы «кончик» иконки совпадал с точкой клика
    [SerializeField] private Vector2 offset = Vector2.zero;

    private void Awake()
    {
        _image = GetComponent<Image>();
        // По умолчанию скрываем иконку
        _image.enabled = false;
    }

    private void Update()
    {
        if (!IsActive)
        {
            if (_image.enabled) _image.enabled = false;
            return;
        }

        // Показать, если был скрыт
        if (!_image.enabled) _image.enabled = true;

        // Позиция мыши в экранных координатах; для Canvas в режиме Overlay это работает напрямую
        Vector2 mousePos = Input.mousePosition;
        _image.rectTransform.position = mousePos + offset;
    }

    /// <summary>
    /// Включить отображение иконки.
    /// </summary>
    public void Enable()
    {
        IsActive = true;
    }

    /// <summary>
    /// Выключить отображение иконки.
    /// </summary>
    public void Disable()
    {
        IsActive = false;
        _image.enabled = false;
    }
}
