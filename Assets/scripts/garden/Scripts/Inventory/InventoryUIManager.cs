using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public const int SeedsPerShelf = 3;

    [Header("Пакетики семян")]
    public GameObject inventorySlotPrefab;

    [Header("Полки (по три вида семян на каждой)")]
    [Tooltip("Контейнеры SeedPackets в порядке заполнения. Чтобы добавить полку, продублируйте SeedShelf и добавьте её SeedPackets в список.")]
    public List<Transform> shelfContainers = new List<Transform>();

    private readonly List<InventorySlotUI> _activeSlots = new List<InventorySlotUI>();
    private readonly List<Transform> _availableShelves = new List<Transform>();

    private void OnEnable()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[InventoryUIManager] InventoryManager не найден.", this);
            return;
        }

        InventoryManager.Instance.OnInventoryRefreshed += RenderInventory;
        RenderInventory();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryRefreshed -= RenderInventory;
    }

    private void RenderInventory()
    {
        _availableShelves.Clear();
        foreach (Transform shelf in shelfContainers)
        {
            if (shelf != null && !_availableShelves.Contains(shelf))
                _availableShelves.Add(shelf);
        }

        if (inventorySlotPrefab == null || _availableShelves.Count == 0)
        {
            Debug.LogError("[InventoryUIManager] Назначьте префаб пакетика и контейнер первой полки.", this);
            return;
        }

        int capacity = _availableShelves.Count * SeedsPerShelf;
        int index = 0;
        foreach (var item in InventoryManager.Instance.GetAllItems())
        {
            if (item.Key == null || item.Value <= 0) continue;
            // Additional shelves expose more stored seed types without changing inventory data.
            if (index >= capacity) break;

            Transform shelf = _availableShelves[index / SeedsPerShelf];
            if (index >= _activeSlots.Count)
            {
                GameObject slotObject = Instantiate(inventorySlotPrefab, shelf);
                InventorySlotUI slot = slotObject.GetComponent<InventorySlotUI>();
                if (slot == null)
                {
                    Debug.LogError("[InventoryUIManager] На префабе нет InventorySlotUI.", slotObject);
                    Destroy(slotObject);
                    break;
                }
                _activeSlots.Add(slot);
            }

            InventorySlotUI activeSlot = _activeSlots[index];
            activeSlot.transform.SetParent(shelf, false);
            activeSlot.transform.SetAsLastSibling();
            activeSlot.Setup(item.Key, item.Value);
            activeSlot.gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < _activeSlots.Count; i++)
            _activeSlots[i].gameObject.SetActive(false);
    }
}
