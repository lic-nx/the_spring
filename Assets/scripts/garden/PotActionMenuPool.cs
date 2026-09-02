using UnityEngine;
using System.Collections.Generic;

public class PotActionMenuPool : MonoBehaviour
{
    public static PotActionMenuPool Instance { get; private set; }

    [SerializeField] private GameObject menuPrefab;
    [SerializeField] private int initialPoolSize = 3;

    private Queue<PotActionMenu> availableMenus = new Queue<PotActionMenu>();
    private List<PotActionMenu> allMenus = new List<PotActionMenu>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewMenu();
        }
    }

    private PotActionMenu CreateNewMenu()
    {
        GameObject menuObj = Instantiate(menuPrefab, transform);
        PotActionMenu menu = menuObj.GetComponent<PotActionMenu>();
        if (menu == null)
        {
            Debug.LogError("[PotActionMenuPool] Menu prefab does not have PotActionMenu component!");
            Destroy(menuObj);
            return null;
        }
        menuObj.SetActive(false);
        allMenus.Add(menu);
        availableMenus.Enqueue(menu);
        return menu;
    }

    public PotActionMenu GetMenu()
    {
        if (availableMenus.Count == 0)
        {
            Debug.LogWarning("[PotActionMenuPool] No available menus in pool!");
            return null;
        }

        PotActionMenu menu = availableMenus.Dequeue();
        menu.gameObject.SetActive(true);
        return menu;
    }

    public void ReturnMenu(PotActionMenu menu)
    {
        if (menu == null || !allMenus.Contains(menu)) return;

        menu.gameObject.SetActive(false);
        menu.ResetTargetPot();
        availableMenus.Enqueue(menu);
    }

    public void ReturnAllMenus()
    {
        foreach (var menu in allMenus)
        {
            if (menu.gameObject.activeSelf)
            {
                menu.gameObject.SetActive(false);
                menu.ResetTargetPot();
                availableMenus.Enqueue(menu);
            }
        }
    }

    public bool IsPointerOverAnyMenu(Vector2 screenPosition)
    {
        foreach (var menu in allMenus)
        {
            if (menu.gameObject.activeSelf)
            {
                RectTransform rectTransform = menu.GetComponent<RectTransform>();
                if (rectTransform != null &&
                    RectTransformUtility.RectangleContainsScreenPoint(
                        rectTransform,
                        screenPosition,
                        null))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
