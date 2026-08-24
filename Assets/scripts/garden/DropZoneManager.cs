using UnityEngine;
using System.Collections.Generic;

public class DropZoneManager : MonoBehaviour
{
    public static DropZoneManager Instance { get; private set; }
    
    // Список всех зон на сцене
    private List<GameObject> allZones = new List<GameObject>();

    private void Awake()
    {
        // Паттерн Singleton для удобного доступа из любого места
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Зона регистрирует себя при старте
    public void RegisterZone(GameObject zone)
    {
        if (!allZones.Contains(zone))
        {
            allZones.Add(zone);
        }
    }

    // Включает или выключает все зоны
    public void SetZonesVisibility(bool isVisible)
    {
        foreach (var zone in allZones)
        {
            if (zone != null)
            {
                zone.SetActive(isVisible);
            }
        }
    }

    // Возвращает список всех зон
    public List<GameObject> GetAllZones()
    {
        return new List<GameObject>(allZones);
    }
}