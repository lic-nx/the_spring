using UnityEngine;

/// <summary>
/// Универсальный менеджер курсора.
/// Позволяет установить любой спрайт/текстуру курсора и, при необходимости,
/// отдельный спрайт для состояния "нажата кнопка".
/// </summary>
public class GenericCursorManager : MonoBehaviour
{
    // Текущие текстуры курсора
    private Texture2D _defaultCursor;
    private Texture2D _pressedCursor;

    // Точка привязки – можно задать из инспектора, если нужен смещения
    [SerializeField] private Vector2 hotSpot = Vector2.zero;

    /// <summary>
    /// Устанавливает курсор.
    /// <param name="defaultCursor">Текстура обычного курсора.</param>
    /// <param name="pressedCursor">Текстура курсора в состоянии нажатия (может быть null).</param>
    /// </summary>
    public void SetCursor(Texture2D defaultCursor, Texture2D pressedCursor = null)
    {
        _defaultCursor = defaultCursor;
        _pressedCursor = pressedCursor;
        ApplyCursor(_defaultCursor);
    }

    /// <summary>
    /// Переключить курсор на вариант, соответствующий текущему состоянию мыши.
    /// Вызывается каждый кадр из Update, но можно вызывать вручную, если известно
    /// когда меняется состояние.
    /// </summary>
    public void UpdateCursorState()
    {
        if (Input.GetMouseButton(0) && _pressedCursor != null)
        {
            ApplyCursor(_pressedCursor);
        }
        else
        {
            ApplyCursor(_defaultCursor);
        }
    }

    private void ApplyCursor(Texture2D tex)
    {
        Cursor.SetCursor(tex, hotSpot, CursorMode.Auto);
    }

    /// <summary>
    /// Сбросить к системному курсору.
    /// </summary>
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        _defaultCursor = null;
        _pressedCursor = null;
    }

    private void Update()
    {
        // Обновляем состояние только если курсор уже установлен
        if (_defaultCursor != null)
            UpdateCursorState();
    }
}
