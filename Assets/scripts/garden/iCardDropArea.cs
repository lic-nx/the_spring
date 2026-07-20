using UnityEngine;
using UnityEngine.EventSystems; 

public interface iPotDropArea
{
    bool OnPotDrop(GameObject pot);
    void FreeZone(); // Новый метод для освобождения зоны
}

