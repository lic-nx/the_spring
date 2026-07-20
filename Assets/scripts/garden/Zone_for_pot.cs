using UnityEngine;

public class LeftDropArea : MonoBehaviour, iPotDropArea
{
    public bool isEmpty = true;

    private void Start()
    {
        // Сообщаем менеджеру о существовании этой зоны
        if (DropZoneManager.Instance != null)
        {
            DropZoneManager.Instance.RegisterZone(this.gameObject);
        }
        
        // Скрываем зону при старте игры (по желанию, можно оставить true, если изначально горшки уже стоят)
        this.gameObject.SetActive(false); 
    }

    public bool OnPotDrop(GameObject pot)
    {
        if (!isEmpty)
        {
            Debug.Log("Зона уже занята! Горшок не установлен.");
            return false;
        }

        var potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            isEmpty = false;
            potComponent.AlignToZone(this.transform);
            // Сообщаем горшку, в какой зоне он теперь находится
            potComponent.SetCurrentZone(this); 
            Debug.Log("Горшок успешно установлен в левую зону.");
            return true;
        }
        
        // Fallback логика
        Transform zoneAttach = transform.childCount > 0 ? transform.GetChild(0) : transform;
        Transform potAttach = pot.transform.childCount > 0 ? pot.transform.GetChild(0) : pot.transform;
        Vector3 originalOffset = potAttach.position - pot.transform.position;
        pot.transform.position = zoneAttach.position - originalOffset;
        potAttach.position = zoneAttach.position;
        
        isEmpty = false;
        potComponent.SetCurrentZone(this);
        return true;
    }

    public void FreeZone()
    {
        isEmpty = true;
        Debug.Log("Зона освобождена.");
    }
}