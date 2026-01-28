using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial : MonoBehaviour
{
    public GameObject activeBlock;
    public GameObject[] overlayObjects; // Массив объектов, которые нужно удалить
    private Vector3 activeObjPosition;

    void Start()
    {
        if (activeBlock != null)
        {
            Vector3 pos = activeBlock.transform.position;
            pos.z -= 1f; // В 2D: меньший Z = ближе к камере
            activeBlock.transform.position = pos;

            Debug.Log("Текущая Z активного блока: " + pos.z);
            activeObjPosition = pos;
        }
    }

    void Update()
    {
        if (activeBlock == null || activeObjPosition != activeBlock.transform.position)
        {
            // Удаляем все объекты из массива
            if (overlayObjects != null)
            {
                foreach (GameObject obj in overlayObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
            }

            // Опционально: отключаем скрипт, чтобы не проверять дальше
            enabled = false;
        }
    }
}