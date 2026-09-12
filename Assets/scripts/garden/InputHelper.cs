using UnityEngine;

/// <summary>
/// Универсальный помощник ввода. Работает и с мышью (ПК/WebGL), и с тачем (мобилки).
/// Все скрипты должны использовать ЭТОТ класс вместо Input.mousePosition / Input.GetMouseButtonDown.
/// </summary>
public static class InputHelper
{
    /// <summary>
    /// Возвращает позицию первого пальца/мыши в экранных координатах (Screen Space).
    /// </summary>
    public static Vector3 GetScreenPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.mousePosition;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;
        return Input.mousePosition;
#endif
    }

    /// <summary>
    /// Возвращает позицию в мировых координатах (для 2D игр).
    /// </summary>
    public static Vector3 GetWorldPosition(Camera cam, float z = 0f)
    {
        Vector3 screenPos = GetScreenPosition();
        screenPos.z = -cam.transform.position.z + z; // Для 2D камеры
        return cam.ScreenToWorldPoint(screenPos);
    }

    /// <summary>
    /// Было ли нажатие в этом кадре? (ЛКМ или первый тач)
    /// </summary>
    public static bool GetPointerDown()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButtonDown(0);
#else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
#endif
    }

    /// <summary>
    /// Было ли отпускание в этом кадре?
    /// </summary>
    public static bool GetPointerUp()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButtonUp(0);
#else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended;
#endif
    }

    /// <summary>
    /// Удерживается ли нажатие?
    /// </summary>
    public static bool GetPointerHeld()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return Input.GetMouseButton(0);
#else
        return Input.touchCount > 0;
#endif
    }
} 