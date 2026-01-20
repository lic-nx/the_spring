using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial : MonoBehaviour
{
    public GameObject activeBlock;
    public GameObject[] overlayObjects; // Массив объектов, которые нужно удалить

    void Start()
    {
        if (activeBlock != null)
        {
            Vector3 pos = activeBlock.transform.position;
            pos.z -= 1f; // В 2D: меньший Z = ближе к камере
            activeBlock.transform.position = pos;

            Debug.Log("Текущая Z активного блока: " + pos.z);
        }
    }

    void Update()
    {
        if (activeBlock == null)
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