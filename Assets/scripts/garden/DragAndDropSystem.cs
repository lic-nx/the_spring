// ------------------------------------------------------------
// DragAndDropSystem.cs – система перетаскивания объектов в садовом режиме игры
// ------------------------------------------------------------
// Этот скрипт реализует систему перетаскивания объектов (семена и горшки) в садовом режиме.
// Он отвечает за начало, перемещение и завершение перетаскивания, а также предоставляет
// публичные методы для отмены и удаления текущего перетаскиваемого объекта через UI‑кнопки.
// Внутри кода присутствуют комментарии на русском языке, поясняющие каждый шаг.
// ------------------------------------------------------------
using System.Collections.Generic; // Подключаем пространство имён для возможных обобщённых коллекций (не используется сейчас, но оставлено для совместимости)
using UnityEngine; // Основные типы UnityEngine, необходимые для MonoBehaviour
// Русский комментарий: система перетаскивания для объектов сада

/// <summary>
/// Optimized drag‑and‑drop system for garden objects (seeds and pots).
/// Handles dragging via Update for simplicity but minimizes unnecessary checks.
/// Adds logic to return seeds to shop when no free pot exists and to revert objects
/// to their pre‑drag positions via a UI button.
/// </summary>
[RequireComponent(typeof(Collider2D))] // Обеспечиваем наличие Collider2D на объекте, иначе система не будет работать
/// <summary>
/// Система перетаскивания объектов в саду (семена и горшки).
/// Обрабатывает начало, перемещение и конец перетаскивания, а также возврат к исходному положению.
/// </summary>
public class DragAndDropSystem : MonoBehaviour
{
    private Camera _camera; // Ссылка на основную камеру сцены, используется для преобразования экранных координат в мировые
    private Transform _draggedTransform; // Трансформ объекта, который сейчас перетаскивается
    private IDraggable _currentDraggable; // Ссылка на объект, реализующий интерфейс IDraggable (обычно семя)

    // Сохраняем оригинальный трансформ и позицию, чтобы иметь возможность откатить действие
    private Transform _originalDragTransform; // Оригинальный трансформ перед началом перетаскивания
    private Vector3 _originalDragPosition; // Оригинальная позиция объекта перед началом перетаскивания

    private void Awake()
    {
        // Инициализируем ссылку на основную камеру
        _camera = Camera.main;
    }

    private void Update()
    {
        // При нажатии левой кнопки мыши пытаемся начать перетаскивание
        if (Input.GetMouseButtonDown(0))
            TryStartDrag();
        // Пока объект перетаскивается и кнопка удерживается – обновляем его позицию
        if (_draggedTransform != null && Input.GetMouseButton(0))
            DragToCursor();
        // При отпускании кнопки завершаем процесс перетаскивания
        if (_draggedTransform != null && Input.GetMouseButtonUp(0))
            EndDrag();
    }

    private void TryStartDrag()
    {
        // Предотвращаем начало нового перетаскивания, пока предыдущее действие не завершилось
        if (_draggedTransform != null) return;

        // Преобразуем позицию курсора в мировые координаты
        Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        // Выполняем 2D‑raycast, чтобы определить, над каким объектом находится курсор
        var hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        if (hit.collider == null) return; // Если луч не попал ни в один коллайдер – выходим

        // Проверяем, является ли объект горшком
        var pot = hit.collider.GetComponent<Pot>();
        if (pot != null)
        {
            // Сохраняем ссылки и позицию для возможного отката
            _draggedTransform = pot.transform;
            _originalDragTransform = _draggedTransform;
            _originalDragPosition = _draggedTransform.position;
            // Информируем горшок о начале перетаскивания
            pot.OnDragStart();
            return;
        }
        // Если это не горшок, проверяем, реализует ли объект интерфейс IDraggable (например, семя)
        var draggable = hit.collider.GetComponent<IDraggable>();
        if (draggable != null)
        {
            _draggedTransform = hit.collider.transform;
            _originalDragTransform = _draggedTransform;
            _originalDragPosition = _draggedTransform.position;
            _currentDraggable = draggable;
            // Информируем объект о начале перетаскивания
            draggable.OnDragStart();
        }
    }

    private void DragToCursor()
    {
        // Обновляем позицию перетаскиваемого объекта, следуя за курсором мыши
        Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
        _draggedTransform.position = worldPoint;
        // Если перетаскивается горшок, показываем визуальную подсказку зоны размещения цветов
        var pot = _draggedTransform.GetComponent<Pot>();
        if (pot != null)
            FlowerCareSystem.Instance.ShowDropZoneFeedback(pot, worldPoint);
    }

    private void EndDrag()
    {
        var pot = _draggedTransform.GetComponent<Pot>();
        if (pot != null)
        {
            // Если отпустили горшок, привязываем его к ближайшей разрешённой позиции
            Vector2 worldPoint = _camera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 closest = FindClosestAllowedPosition(pot, worldPoint);
            pot.transform.position = closest;
            pot.OnDragEnd(); // Сообщаем горшку, что перетаскивание завершилось
            FlowerCareSystem.Instance.HideDropZoneFeedback(); // Скрываем визуальную подсказку
        }
        else
        {
            // Обрабатываем окончание перетаскивания для объектов, не являющихся горшками (например, семян)
            if (_currentDraggable != null)
            {
                if (!HasFreePot())
                {
                    // Если свободных горшков нет, уничтожаем объект (семя исчезает)
                    Object.Destroy(_draggedTransform.gameObject);
                }
                else
                {
                    // Иначе вызываем завершение логики у самого draggable‑объекта
                    _currentDraggable.OnDragEnd();
                }
            }
        }
        // Сбрасываем ссылки, готовимся к следующему перетаскиванию
        _originalDragTransform = null;
        _draggedTransform = null;
        _currentDraggable = null;
    }

    // Публичный метод, вызываемый UI‑кнопкой, возвращает объект в его исходную позицию
    public void RevertCurrentDrag()
    {
        if (_originalDragTransform != null)
        {
            _originalDragTransform.position = _originalDragPosition;
        }
    }

    // Публичный метод, вызываемый UI‑кнопкой, удаляет текущий перетаскиваемый объект (например, семя)
    public void DeleteCurrentDrag()
    {
        // Удаляем только если объект не является горшком
        if (_originalDragTransform != null && _originalDragTransform.GetComponent<Pot>() == null)
        {
            Object.Destroy(_originalDragTransform.gameObject);
            // Очищаем все ссылки после удаления
            _originalDragTransform = null;
            _draggedTransform = null;
            _currentDraggable = null;
        }
    }

    private static bool HasFreePot()
    {
        // Проверяем, есть ли в сцене хотя бы один свободный горшок (без цветка)
        var pots = Object.FindObjectsOfType<Pot>();
        foreach (var p in pots)
        {
            if (p.CurrentFlower == null)
                return true; // Свободный горшок найден
        }
        return false; // Все горшки заняты
    }

    private static Vector2 FindClosestAllowedPosition(Pot pot, Vector2 target)
    {
        // Если у горшка не заданы допустимые позиции, возвращаем текущую позицию
        if (pot.AllowedPositions == null || pot.AllowedPositions.Length == 0)
            return pot.transform.position;
        // Ищем позицию из массива AllowedPositions, которая ближе всего к целевой точке
        Vector2 best = pot.AllowedPositions[0];
        float min = float.MaxValue;
        foreach (var pos in pot.AllowedPositions)
        {
            float d = Vector2.Distance(target, pos);
            if (d < min)
            {
                min = d;
                best = pos;
            }
        }
        return best; // Возвращаем найденную ближайшую позицию
    }
}

