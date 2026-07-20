using UnityEngine;

public class Pot : DragDrop
{
    [SerializeField] private Transform flowerAttachment;
    [SerializeField] private Transform zoneAttachmentPoint;

    private Flower currentFlower;
    public Flower CurrentFlower => currentFlower;

    // Ссылка на зону, в которой сейчас находится горшок
    private iPotDropArea currentZone;

    // Метод для сохранения ссылки на зону (вызывается из LeftDropArea)
    public void SetCurrentZone(iPotDropArea zone)
    {
        currentZone = zone;
    }

    public void AlignToZone(Transform zoneRoot)
    {
        Transform zoneAttach = zoneRoot.childCount > 0 ? zoneRoot.GetChild(0) : zoneRoot;
        Transform potAttach = zoneAttachmentPoint != null ? zoneAttachmentPoint : (this.transform.childCount > 0 ? this.transform.GetChild(0) : this.transform);
        Vector3 originalOffset = potAttach.position - this.transform.position;
        this.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
    }

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
        
        flower.transform.SetParent(this.transform);
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

    // ПЕРЕОПРЕДЕЛЯЕМ OnMouseDown из базового класса, чтобы освободить зону при поднятии
    private void OnMouseDown()
    {
        base.OnMouseDown(); // Выполняем логику DragDrop (сохранение позиции и т.д.)

        // Если горшок стоял в зоне, освобождаем её
        if (currentZone != null)
        {
            currentZone.FreeZone();
            currentZone = null;
        }

        // Показываем все зоны, так как мы начали перетаскивание
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(true);
        }
    }

    private void OnMouseUp()
    {
        _collider.enabled = false;
        Collider2D dropArea = Physics2D.OverlapPoint(transform.position);
        _collider.enabled = true;

        bool isPlacedSuccessfully = false;

        if (dropArea != null && dropArea.GetComponent<iPotDropArea>() != null)
        {
            iPotDropArea area = dropArea.GetComponent<iPotDropArea>();
            isPlacedSuccessfully = area.OnPotDrop(this.gameObject);
        }

        if (!isPlacedSuccessfully)
        {
            Debug.Log("Не удалось поставить горшок, возвращаем на исходную позицию.");
            transform.position = _startDragPosition;
        }

        // Скрываем зоны после завершения перетаскивания (независимо от успеха)
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.SetZonesVisibility(false);
        }
    }
}