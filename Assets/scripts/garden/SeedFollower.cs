using UnityEngine; // Подключаем пространство имён UnityEngine

/// <summary>
/// Makes the attached seed prefab follow the mouse cursor in world space.
/// On left‑click it attempts to place the seed into a Pot that is under the cursor.
/// After successful placement the prefab becomes a child of the Pot and this
/// component destroys itself.
/// </summary>
public class SeedFollower : MonoBehaviour // Класс, управляющий поведением семени в сцене
{
    // Prefab of the flower to instantiate when placed in a pot.
    [SerializeField] private GameObject flowerPrefab; // Префаб цветка, создаваемый при посадке
    // Public setter for assigning the flower prefab from Shop.
    public GameObject FlowerPrefab { set => flowerPrefab = value; } // Свойство для установки префаба из магазина
    // Cached reference to the SpriteRenderer for the seed visual.
    private SpriteRenderer _spriteRenderer; // Кешированная ссылка на SpriteRenderer семени

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); // Получаем SpriteRenderer компонента
        if (_spriteRenderer == null)
        {
            Debug.LogError($"SeedFollower requires a SpriteRenderer on {name}"); // Ошибка, если отсутствует SpriteRenderer
        }
    }

    private void Update()
    {
        // Follow mouse cursor.
        var cam = Camera.main; // Главная камера сцены
        if (cam == null) return; // Выходим, если камера не найдена
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition); // Переводим позицию мыши в мир
        mouseWorld.z = 0f; // Assuming 2D plane. // Устанавливаем Z=0 для 2D
        transform.position = mouseWorld; // Перемещаем объект к позиции курсора

        // On left click try to place into a Pot.
        if (Input.GetMouseButtonDown(0)) // Проверка клика левой кнопкой мыши
        {
            TryPlaceInPot(); // Пытаемся разместить семя в горшке
        }
    }

    private void TryPlaceInPot()
    {
        // Raycast against colliders in 2D. Pots are expected to have a Collider2D.
        var cam = Camera.main; // Получаем камеру для преобразования координат
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition); // Позиция мыши в 2D
        var hit = Physics2D.Raycast(mousePos, Vector2.zero); // Выполняем лучевой каст в точке мыши
        if (hit.collider != null)
        {
            var pot = hit.collider.GetComponent<Pot>(); // Пытаемся получить компонент Pot из коллайдера
            if (pot != null)
            {
                // Plant the flower into the pot.
                try
                {
                    // Instantiate the flower prefab and parent it to the pot.
                    var flowerObj = Instantiate(flowerPrefab); // Создаём объект цветка из префаба
                    var flowerComp = flowerObj.GetComponent<Flower>(); // Получаем компонент Flower
                    if (flowerComp == null)
                    {
                        Debug.LogError("SeedFollower: instantiated prefab lacks Flower component."); // Ошибка, если префаб не содержит Flower
                    }
                    else
                    {
                        pot.PlantFlower(flowerComp); // Сажаем цветок в горшок
                        // Position at pot center.
                        flowerObj.transform.position = pot.transform.position; // Размещаем цветок в центре горшка
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to plant flower: {ex.Message}"); // Логируем исключение при посадке
                    return;
                }
                // Remove this component and destroy the seed sprite object.
                Destroy(this); // Удаляем компонент SeedFollower
                Destroy(gameObject); // Удаляем объект семени
                return;
            }
        }
        // If we reach here, no pot was clicked – keep following.
    }
}
