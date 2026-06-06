// // ------------------------------------------------------------
// // FlowerCareSystem.cs – система ухода за цветками в садовом режиме игры
// // ------------------------------------------------------------
// // Этот скрипт реализует систему ухода за цветками, отвечая за визуальную подсветку
// // допустимых зон размещения горшков, хранение списка всех цветов и предоставление
// // методов регистрации/удаления цветов. Внутри кода присутствуют подробные
// // комментарии на русском языке, объясняющие каждую часть реализации.
// // ------------------------------------------------------------
// using System.Collections; // Базовые коллекции C# (не используются напрямую, но включены для совместимости)
// using System.Collections.Generic; // Обобщённые коллекции, используемые для списка цветов
// using UnityEngine; // Основные типы UnityEngine, необходимые для MonoBehaviour
// using UnityEngine.SceneManagement; // Позволяет управлять сценами (в данном скрипте пока не используется)
// using UnityEngine.Rendering.PostProcessing; // Пакет пост‑обработки, может потребоваться для визуальных эффектов

// public class FlowerCareSystem : MonoBehaviour
// {
//     // Синглтон‑инстанс для удобного доступа из других систем (например, DragAndDropSystem)
//     public static FlowerCareSystem Instance { get; private set; }

//     // Список всех цветков в сцене, доступен только для чтения извне
//     public List<Flower> Flowers { get; private set; } = new List<Flower>();

//     // Компоненты визуальной подсветки зоны размещения горшка
//     private SpriteRenderer _zoneHighlightRenderer; // Рендерер, отображающий полупрозрачный квадрат
//     [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.3f);   // Полупрозрачный зелёный цвет – допустимая позиция
//     [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.3f); // Полупрозрачный красный цвет – недопустимая позиция

//     private void Awake()
//     {
//         // Гарантируем, что существует только один экземпляр этого класса (паттерн Singleton)
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//         DontDestroyOnLoad(this); // Сохраняем объект при смене сцен

//         // Находим или создаём объект визуального выделения зоны размещения (DropZoneHighlight)
//         Transform highlightTransform = transform.Find("DropZoneHighlight");
//         GameObject highlightObj;
//         if (highlightTransform != null)
//         {
//             highlightObj = highlightTransform.gameObject; // Используем уже существующий объект
//         }
//         else
//         {
//             highlightObj = new GameObject("DropZoneHighlight"); // Создаём новый объект
//             highlightObj.transform.SetParent(transform);
//         }
//         // Получаем или добавляем компонент SpriteRenderer для отображения квадрата
//         _zoneHighlightRenderer = highlightObj.GetComponent<SpriteRenderer>();
//         if (_zoneHighlightRenderer == null)
//         {
//             _zoneHighlightRenderer = highlightObj.AddComponent<SpriteRenderer>();
//         }
//         _zoneHighlightRenderer.enabled = false; // Отключаем рендерер по умолчанию
//         // Если спрайт не назначен, объект останется невидимым до назначения в инспекторе
//     }

//     // Регистрация нового цветка в системе ухода
//     public void RegisterFlower(Flower flower)
//     {
//         if (!Flowers.Contains(flower))
//             Flowers.Add(flower); // Добавляем в список, если ещё не присутствует
//     }

//     // Удаление цветка из системы ухода
//     public void UnregisterFlower(Flower flower)
//     {
//         Flowers.Remove(flower); // Удаляем из списка (если отсутствует, метод безопасно ничего не делает)
//     }

//     /// <summary>
//     /// Вызывается системой DragAndDropSystem во время перетаскивания горшка.
//     /// Показывает полупрозрачный квадрат в ближайшей допустимой позиции.
//     /// </summary>
//     /// <param name="pot">Горшок, который сейчас перетаскивается.</param>
//     /// <param name="mouseWorldPos">Текущие координаты курсора в мировом пространстве.</param>
//     public void ShowDropZoneFeedback(Pot pot, Vector2 mouseWorldPos)
//     {
//         // Если горшок null или у него нет допустимых позиций – скрываем подсказку
//         if (pot == null || pot.AllowedPositions == null || pot.AllowedPositions.Length == 0)
//         {
//             _zoneHighlightRenderer.enabled = false;
//             return;
//         }

//         // Находим ближайшую допустимую позицию к текущему курсору
//         Vector2 closest = pot.AllowedPositions[0];
//         float minDist = Vector2.Distance(mouseWorldPos, closest);
//         foreach (var pos in pot.AllowedPositions)
//         {
//             float d = Vector2.Distance(mouseWorldPos, pos);
//             if (d < minDist)
//             {
//                 minDist = d;
//                 closest = pos;
//             }
//         }

//         // Обновляем позицию и цвет рендерера: зелёный – валидно, красный – недопустимо
//         _zoneHighlightRenderer.transform.position = closest;
//         _zoneHighlightRenderer.color = minDist < 1.0f ? _validColor : _invalidColor; // Порог в 1 единицу мира
//         _zoneHighlightRenderer.enabled = true; // Включаем визуализацию
//     }

//     /// <summary>
//     /// Скрывает визуальную подсказку, когда перетаскивание заканчивается.
//     /// </summary>
//     public void HideDropZoneFeedback()
//     {
//         _zoneHighlightRenderer.enabled = false; // Отключаем рендерер
//     }
// }
