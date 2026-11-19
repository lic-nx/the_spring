using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GlobalClickHandler : MonoBehaviour
{
    private readonly List<RaycastResult> _hits = new List<RaycastResult>();

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            HandlePointerDown(Touchscreen.current.primaryTouch.position.ReadValue());
            return;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            HandlePointerDown(Mouse.current.position.ReadValue());
#else
        if (InputHelper.GetPointerDown())
            HandlePointerDown(InputHelper.GetScreenPosition());
#endif
    }

    public void HandlePointerDown(Vector2 screenPosition)
    {
        PotActionMenuPool pool = PotActionMenuPool.Instance;
        if (pool == null) return;

        _hits.Clear();
        if (EventSystem.current != null)
        {
            var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
            EventSystem.current.RaycastAll(pointer, _hits);
        }

        if (_hits.Count > 0)
        {
            GameObject hit = _hits[0].gameObject;
            if (hit.GetComponentInParent<PotActionMenu>() != null) return;
            // A pot handles its own click: toggle this menu or open another one.
            if (hit.GetComponentInParent<Flower>() == null && hit.GetComponentInParent<Pot>() != null)
                return;
        }

        pool.ReturnAllMenus();
    }
}
