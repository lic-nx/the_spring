using UnityEngine;
using UnityEngine.EventSystems;

public class GlobalClickHandler : MonoBehaviour
{
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            bool isOverPotMenu = IsPointerOverPotActionMenu();
            bool isOverPot = IsPointerOverPot();

            // Если клик НЕ по меню горшка и НЕ по горшку — скрываем меню
            if (!isOverPotMenu && !isOverPot)
            {
                PotActionMenuPool.Instance?.ReturnAllMenus();
            }
        }
    }

    private bool IsPointerOverPotActionMenu()
    {
        if (PotActionMenuPool.Instance == null)
            return false;

        return PotActionMenuPool.Instance.IsPointerOverAnyMenu(Input.mousePosition);
    }

    private bool IsPointerOverPot()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        Vector2 mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Pot pot = hit.collider.GetComponent<Pot>();
            if (pot != null)
            {
                return true;
            }
        }
        return false;
    }
}