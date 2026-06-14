using System; // Подключаем базовые типы C#
using System.Collections; // Для работы с коллекциями IEnumerable
using System.Collections.Generic; // Для обобщённых коллекций List<T>
using UnityEngine; // Основное пространство имён Unity
using UnityEngine.SceneManagement; // Управление сценами (может не использоваться в этом файле)
using UnityEngine.Rendering.PostProcessing; // Пост‑процессинг (не используется в этом файле)

public class Pot : DragDrop // Класс Pot, реализует интерфейс IDraggable для поддержки перетаскивания
{
        private void OnMouseUp()
    {
        Debug.Log("Отпускаем мышку");
        // Проверяем, куда упала карта
        _collider.enabled = false;
        Collider2D dropArea = Physics2D.OverlapPoint(transform.position);
        _collider.enabled = true;
        Debug.Log("Seed needs watering");
        if (dropArea != null && dropArea.GetComponent<iPotDropArea>() != null)
        {
            Debug.Log("Мы нашли куда поставить");
            dropArea.GetComponent<iPotDropArea>().OnPotDrop(this.gameObject);
        }
        else
        {
            Debug.Log("мы не нашли куда поместить ");
            // Возвращаем в исходное положение
            transform.position = _startDragPosition;
        }
    }
    
}
