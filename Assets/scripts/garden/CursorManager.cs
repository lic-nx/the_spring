using UnityEngine;

/// <summary>
/// Управляет изменением системного курсора для режима полива.
/// Курсор меняется на спрайт лейки и на вариант "нажата" при удержании кнопки мыши.
/// </summary>
public class CursorManager : MonoBehaviour
{
    private Texture2D _waterCan;
    private Texture2D _waterCanDown;
    // Точка привязки курсора (обычно кончик лейки). При необходимости скорректируйте.
    private readonly Vector2 _hotSpot = Vector2.zero;

    void Awake()
    {
        // Загрузка текстур из Resources/Cursors/WaterCan*.png
        _waterCan = Resources.Load<Texture2D>("Cursors/WaterCan");
        _waterCanDown = Resources.Load<Texture2D>("Cursors/WaterCanDown");
    }

    /// <summary>Установить обычный курсор лейки.</summary>
    public void SetWaterCanCursor()
    {
        if (_waterCan != null)
            Cursor.SetCursor(_waterCan, _hotSpot, CursorMode.Auto);
    }

    /// <summary>Установить курсор лейки в состоянии нажатия.</summary>
    public void SetWaterCanDownCursor()
    {
        if (_waterCanDown != null)
            Cursor.SetCursor(_waterCanDown, _hotSpot, CursorMode.Auto);
    }

    /// <summary>Сбросить курсор к системе.</summary>
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
