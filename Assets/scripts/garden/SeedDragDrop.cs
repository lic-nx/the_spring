using UnityEngine;

public class SeedDragDrop : MonoBehaviour
{
    public SeedItem seedItem; // Данные семени
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    
    // Ссылка на слот в UI, откуда мы взяли семя
    private InventorySlotUI sourceSlotUI; 

    private void Awake()
    {
        Debug.Log("🟢 [SeedDragDrop] Awake: Инициализация 'призрака' семени...");
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("🔴 [SeedDragDrop] КРИТИЧЕСКАЯ ОШИБКА: Camera.main не найдена! Убедитесь, что у вашей камеры стоит тег 'MainCamera'.");
        }
        else
        {
            Debug.Log("✅ [SeedDragDrop] Камера найдена успешно.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("🔴 [SeedDragDrop] КРИТИЧЕСКАЯ ОШИБКА: На этом объекте отсутствует компонент SpriteRenderer!");
        }
    }

    // Инициализация при создании "призрака"
    public void Setup(SeedItem item, InventorySlotUI sourceSlot)
    {
        Debug.Log($"🌱 [SeedDragDrop] Setup: Настройка призрака для семени '{item.name}'");
        seedItem = item;
        sourceSlotUI = sourceSlot;

        if (seedItem != null && seedItem.seedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = seedItem.seedSprite;
            Debug.Log($"✅ [SeedDragDrop] Спрайт '{seedItem.seedSprite.name}' успешно назначен на SpriteRenderer.");
        }
        else
        {
            Debug.LogWarning("⚠️ [SeedDragDrop] Не удалось назначить спрайт. Проверьте seedItem, seedSprite или SpriteRenderer.");
        }
    }

    private void Update()
    {
        if (mainCamera == null) return; // Защита от ошибок, если камера не найдена

        // 1. Семя всегда следует за курсором мыши в мировых координатах
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f; // Для 2D игры
        transform.position = mousePosition;

        // 2. Левый клик: попытка посадки
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("🖱️ [SeedDragDrop] Обнаружен Левый клик! Пускаем Raycast...");
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider == null)
            {
                Debug.Log("⚪ [SeedDragDrop] Raycast ни во что не попал (пустое пространство).");
            }
            else if (!hit.collider.CompareTag("Pot"))
            {
                Debug.Log($"⚪ [SeedDragDrop] Попали в объект '{hit.collider.name}', но его тег НЕ равен 'Pot' (Текущий тег: {hit.collider.tag}).");
            }
            else
            {
                Debug.Log($"🎯 [SeedDragDrop] Успешное попадание в горшок '{hit.collider.name}' с тегом 'Pot'! Пытаемся посадить...");
                if (TryPlantInPot(hit.collider.transform))
                {
                    Debug.Log("✅ [SeedDragDrop] Посадка успешна! Уничтожаем 'призрак'.");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("❌ [SeedDragDrop] Посадка не удалась (см. ошибки выше). 'Призрак' остается активным.");
                }
            }
        }

        // 3. Правый клик: отмена перетаскивания
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("❌ [SeedDragDrop] Обнаружен Правый клик! Отмена перетаскивания. Уничтожение 'призрака' без траты семени.");
            Destroy(gameObject);
        }
    }

    private bool TryPlantInPot(Transform potTransform)
    {
        Debug.Log($"🪴 [SeedDragDrop] TryPlantInPot: Начинаем процесс посадки в '{potTransform.name}'...");

        if (seedItem.flowerPrefab == null)
        {
            Debug.LogError("🔴 [SeedDragDrop] ОШИБКА: В ScriptableObject семени не назначен Flower Prefab!");
            return false;
        }

        Pot pot = potTransform.GetComponent<Pot>();
        if (pot == null) 
        {
            Debug.LogError($"🔴 [SeedDragDrop] ОШИБКА: На объекте '{potTransform.name}' с тегом 'Pot' отсутствует компонент скрипта Pot!");
            return false;
        }

        if (pot.CurrentFlower != null)
        {
            Debug.Log("⚠️ [SeedDragDrop] Отмена: Этот горшок уже занят другим цветком.");
            return false;
        }

        Debug.Log($"🌸 [SeedDragDrop] Создаем экземпляр префаба цветка: '{seedItem.flowerPrefab.name}'...");
        GameObject flowerObj = Instantiate(seedItem.flowerPrefab, Vector3.zero, Quaternion.identity);
        Flower flowerComp = flowerObj.GetComponent<Flower>();
        
        if (flowerComp == null)
        {
            Debug.LogError($"🔴 [SeedDragDrop] ОШИБКА: На префабе '{seedItem.flowerPrefab.name}' отсутствует компонент скрипта Flower! Уничтожаем объект.");
            Destroy(flowerObj);
            return false;
        }

        if (seedItem.growthConditions != null)
        {
            Debug.Log("⚙️ [SeedDragDrop] Инициализируем условия роста (GrowthConditions)...");
            flowerComp.Initialize(seedItem.growthConditions);
        }
        else
        {
            Debug.Log("⚪ [SeedDragDrop] GrowthConditions не указаны, пропускаем инициализацию.");
        }

        Debug.Log("📞 [SeedDragDrop] Вызываем метод pot.PlantFlower(flowerComp)...");
        bool placed = pot.PlantFlower(flowerComp);
        
        if (placed)
        {
            Debug.Log($"✅ [SeedDragDrop] pot.PlantFlower вернул TRUE! Цветок успешно размещен.");
            Debug.Log($"➖ [SeedDragDrop] Запрашиваем у InventoryManager удаление 1 шт. '{seedItem.name}'...");
            
            // !!! КЛЮЧЕВОЙ МОМЕНТ: Уменьшаем количество в инвентаре !!!
            InventoryManager.Instance.RemoveItem(seedItem, 1);
            
            return true;
        }
        else
        {
            Debug.LogWarning("⚠️ [SeedDragDrop] pot.PlantFlower вернул FALSE! Размещение не удалось. Уничтожаем созданный цветок.");
            Destroy(flowerObj);
            return false;
        }
    }
}