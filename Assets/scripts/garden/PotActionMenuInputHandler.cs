using UnityEngine;
using UnityEngine.EventSystems;

public class PotActionMenuInputHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (PotActionMenuPool.Instance != null)
            {
                PotActionMenuPool.Instance.ReturnAllMenus();
            }
        }
    }
}
