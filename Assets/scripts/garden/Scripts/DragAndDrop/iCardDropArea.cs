using UnityEngine;
using UnityEngine.EventSystems; 

public interface iPotDropArea
{
    bool OnPotDrop(GameObject pot);
    void FreeZone(); // Новый метод для освобождения зоны
    void AlignPotToZone(Pot pot); // Align an existing pot to this zone
}

