using UnityEngine;
using UnityEngine.EventSystems; 

public class LeftDropArea : MonoBehaviour, iPotDropArea
{
    public void OnPotDrop(GameObject pot)
    {
        // Размещаем карту в центре зоны
        pot.transform.position = transform.position;
        Debug.Log("Карта упала в левую зону");
    }
}
