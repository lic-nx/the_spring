using UnityEngine;

public class SeedDragManager : MonoBehaviour
{
    public static SeedDragManager Instance { get; private set; }

    [Header("Настройки")]
    // Префаб, который будет следовать за мышкой (должен иметь SpriteRenderer и SeedDragDrop)
    public GameObject seedDragPrefab; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Этот метод мы будем вызывать из UI-слота инвентаря
    public void StartDraggingSeed(SeedItem seed, InventorySlotUI sourceSlot)
    {
        // Проверяем, есть ли уже активный "призрак"
        if (FindObjectOfType<SeedDragDrop>() != null)
        {
            Debug.Log("Уже что-то перетаскивается!");
            return;
        }

        // Создаем "призрак" в позиции мыши
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        
        GameObject ghost = Instantiate(seedDragPrefab, mousePos, Quaternion.identity);
        
        // Настраиваем его
        SeedDragDrop dragScript = ghost.GetComponent<SeedDragDrop>();
        if (dragScript != null)
        {
            dragScript.Setup(seed, sourceSlot);
        }
    }
}