using System; // Подключаем базовые типы C#
using System.Collections; // Для работы с коллекциями IEnumerable
using System.Collections.Generic; // Для обобщённых коллекций List<T>
using UnityEngine; // Основное пространство имён Unity
using UnityEngine.SceneManagement; // Управление сценами (может не использоваться в этом файле)
using UnityEngine.Rendering.PostProcessing; // Пост‑процессинг (не используется в этом файле)

public class Pot : MonoBehaviour, IDraggable // Класс Pot, реализует интерфейс IDraggable для поддержки перетаскивания
{
    // Внутренний метод для установки допустимых позиций горшка из внешних скриптов (например, ZoneSelector).
    internal void SetAllowedPositions(Vector2[] positions) => _allowedPositions = positions; // Сеттер массива позиций
    // Фиксированные позиции, где может находиться горшок. Задаются в инспекторе Unity.
    [SerializeField] private Vector2[] _allowedPositions; // Массив позиций, задаётся в редакторе
    public Vector2[] AllowedPositions => _allowedPositions; // Публичный геттер для доступа к массиву позиций

    // Текущий цветок, посаженный в горшке (null, если пустой).
    [SerializeField] private Flower _currentFlower; // Ссылка на объект Flower
    public Flower CurrentFlower => _currentFlower; // Публичный геттер для получения текущего цветка

    // Префаб цветка, который будет создан при покупке семени.
    [SerializeField] private GameObject _flowerPrefab; // Ссылка на префаб цветка
    public GameObject FlowerPrefab => _flowerPrefab; // Публичный геттер для доступа к префабу

    /// <summary>
    /// Посадить цветок в этот горшок. Выбрасывает исключение, если горшок уже занят.
    /// </summary>
    public void PlantFlower(Flower flower)
    {
        if (CurrentFlower != null)
            throw new InvalidOperationException("Pot already occupied."); // Ошибка, если в горшке уже есть цветок
        _currentFlower = flower; // Сохраняем ссылку на новый цветок
        flower.transform.SetParent(transform); // Делает горшок родителем цветка в иерархии сцены
    }

    /// <summary>
    /// Удалить текущий цветок из горшка и уничтожить его объект.
    /// </summary>
    public void RemoveFlower()
    {
        if (_currentFlower != null)
        {
            Destroy(_currentFlower.gameObject); // Удаляем объект цветка из сцены
            _currentFlower = null; // Очищаем ссылку на цветок
        }
    }

    // Реализация IDraggable – делегирует визуальную обратную связь системе FlowerCareSystem.
    public void OnDragStart() { /* Нет действий при начале перетаскивания */ }

    public void OnDrag()
    {
        var cam = Camera.main; // Получаем главную камеру
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition); // Преобразуем позицию мыши в мировые координаты
        FlowerCareSystem.Instance.ShowDropZoneFeedback(this, mouseWorld); // Показать визуальную подсказку зоны посадки
    }

    public void OnDragEnd()
    {
        var cam = Camera.main; // Главная камера
        Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition); // Текущая позиция мыши в мировых координатах
        Vector2 closest = FindClosestAllowedPosition(mouseWorld); // Находим ближайшую допустимую позицию
        transform.position = closest; // Перемещаем горшок в найденную позицию
        FlowerCareSystem.Instance.HideDropZoneFeedback(); // Скрываем визуальную подсказку
    }

    private Vector2 FindClosestAllowedPosition(Vector2 target)
    {
        if (AllowedPositions == null || AllowedPositions.Length == 0) return transform.position; // Если нет позиций, остаёмся на месте
        Vector2 best = AllowedPositions[0]; // Начинаем с первой позиции в массиве
        float min = Vector2.Distance(target, best); // Вычисляем начальное расстояние до цели
        foreach (var pos in AllowedPositions)
        {
            float d = Vector2.Distance(target, pos); // Вычисляем расстояние до текущей позиции
            if (d < min) { min = d; best = pos; } // Обновляем лучшую позицию, если найдено ближе
        }
        return best; // Возвращаем ближайшую допустимую позицию
    }
}