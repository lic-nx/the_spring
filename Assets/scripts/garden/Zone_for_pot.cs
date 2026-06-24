using UnityEngine;
using UnityEngine.EventSystems; 

public class LeftDropArea : MonoBehaviour, iPotDropArea
{
    public void OnPotDrop(GameObject pot)
    {
        // Use Pot's AlignToZone method to align attachment points
        var potComponent = pot.GetComponent<Pot>();
        if (potComponent != null)
        {
            potComponent.AlignToZone(this.transform);
            Debug.Log("Карта упала в левую зону и выровнена через Pot.AlignToZone");
        }
        else
        {
            // Fallback: align using original logic if Pot component missing
            Transform zoneAttach = transform.childCount > 0 ? transform.GetChild(0) : transform;
            Transform potAttach = pot.transform.childCount > 0 ? pot.transform.GetChild(0) : pot.transform;
            Vector3 originalOffset = potAttach.position - pot.transform.position;
            pot.transform.position = zoneAttach.position - originalOffset;
            potAttach.position = zoneAttach.position;
            Debug.Log("Карта упала в левую зону (fallback alignment)");
        }
    }
}
