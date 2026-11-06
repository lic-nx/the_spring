using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Общий ввод мыши, пера и основного касания через Input System.</summary>
public static class InputHelper
{
    private static Pointer ActivePointer
    {
        get
        {
            var touch = Touchscreen.current;
            if (touch != null && (touch.primaryTouch.press.isPressed ||
                                  touch.primaryTouch.press.wasPressedThisFrame ||
                                  touch.primaryTouch.press.wasReleasedThisFrame))
                return touch;
            return Pointer.current;
        }
    }

    public static Vector3 GetScreenPosition()
    {
        var pointer = ActivePointer;
        return pointer != null ? (Vector3)pointer.position.ReadValue() : Vector3.zero;
    }

    public static Vector3 GetWorldPosition(Camera cam, float z = 0f)
    {
        Vector3 screenPos = GetScreenPosition();
        screenPos.z = -cam.transform.position.z + z;
        return cam.ScreenToWorldPoint(screenPos);
    }

    public static bool GetPointerDown() => ActivePointer?.press.wasPressedThisFrame ?? false;
    public static bool GetPointerUp() => ActivePointer?.press.wasReleasedThisFrame ?? false;
    public static bool GetPointerHeld() => ActivePointer?.press.isPressed ?? false;

    public static bool IsPointerOverObject(GameObject target, Camera camera = null)
    {
        if (ActivePointer == null || target == null) return false;
        if (camera == null) camera = Camera.main;
        if (camera == null) return false;
        Vector3 screenPosition = GetScreenPosition();
        if (!camera.pixelRect.Contains(screenPosition)) return false;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        int mask = camera.cullingMask & Physics.DefaultRaycastLayers;
        float distance = camera.farClipPlane - camera.nearClipPlane;
        RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, distance, mask);
        bool hit3D = Physics.Raycast(ray, out RaycastHit hit, distance, mask);
        if (hit2D.collider != null && (!hit3D || hit2D.distance <= hit.distance))
            return hit2D.collider.gameObject == target;
        return hit3D && hit.collider.gameObject == target;
    }
}
