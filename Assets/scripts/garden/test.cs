using UnityEngine;
using UnityEngine.EventSystems;

public class Test : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("✅ 测试方块：开始拖拽！(说明 Physics2DRaycaster 正常工作)");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 让方块跟着鼠标走
        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        transform.position = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("✅ 测试方块：结束拖拽！");
    }
}