using System; // Подключаем базовые типы C#
using System.Collections; // Для работы с коллекциями IEnumerable
using System.Collections.Generic; // Для обобщённых коллекций List<T>
using UnityEngine; // Основное пространство имён Unity
using UnityEngine.SceneManagement; // Управление сценами (может не использоваться в этом файле)
using UnityEngine.Rendering.PostProcessing; // Пост‑процессинг (не используется в этом файле)

public class Pot : DragDrop // Класс Pot, реализует интерфейс IDraggable для поддержки перетаскивания
{
    // Child element for attaching to a flower
    [SerializeField] private Transform flowerAttachment;
    // Child element for attaching to a pot zone
    [SerializeField] private Transform zoneAttachmentPoint;

    // Reference to the currently planted flower (if any)
    private Flower currentFlower;
    public Flower CurrentFlower => currentFlower;

    // Align pot's attachment child with the provided zone's attachment child while preserving original offset
    public void AlignToZone(Transform zoneRoot)
    {
        // Zone's attachment point (first child or the zone itself)
        Transform zoneAttach = zoneRoot.childCount > 0 ? zoneRoot.GetChild(0) : zoneRoot;
        // Pot's attachment point (specified field or first child/fallback)
        Transform potAttach = zoneAttachmentPoint != null ? zoneAttachmentPoint : (this.transform.childCount > 0 ? this.transform.GetChild(0) : this.transform);
        Vector3 originalOffset = potAttach.position - this.transform.position;
        this.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
        Debug.Log("Горшок выровнен по точкам привязки к зоне");
    }

    // Plant a flower in this pot. Returns true if successful.
    public bool PlantFlower(Flower flower)
    {
        if (currentFlower != null)
        {
            Debug.LogWarning("Attempted to plant a flower in an already occupied pot.");
            return false;
        }
        if (flower == null)
        {
            Debug.LogWarning("Cannot plant a null flower.");
            return false;
        }
        // Parent the flower to the pot
        flower.transform.SetParent(this.transform);
        // Position the flower at the attachment point if defined, otherwise at the pot's origin
        if (flowerAttachment != null)
        {
            flower.transform.position = flowerAttachment.position;
        }
        else
        {
            flower.transform.localPosition = Vector3.zero;
        }
        currentFlower = flower;
        return true;
    }

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
