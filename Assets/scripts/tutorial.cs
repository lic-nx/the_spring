using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial : MonoBehaviour
{
    public GameObject activeBlock;
    public GameObject[] overlayObjects; // ������ ��������, ������� ����� �������
    private Vector3 activeObjPosition;

    void Start()
    {
        if (activeBlock != null)
        {
            Vector3 pos = activeBlock.transform.position;
            pos.z -= 1f; // � 2D: ������� Z = ����� � ������
            activeBlock.transform.position = pos;

            Debug.Log("������� Z ��������� �����: " + pos.z);
            activeObjPosition = pos;
        }
    }

    void Update()
    {
        if (activeBlock == null || activeObjPosition != activeBlock.transform.position)
        {
            // ������� ��� ������� �� �������
            if (overlayObjects != null)
            {
                foreach (GameObject obj in overlayObjects)
                {
                    if (obj != null)
                    {
                        Destroy(obj);
                    }
                }
                Vector3 pos = activeBlock.transform.position;
                pos.z += 1f;
                activeBlock.transform.position = pos;
                activeObjPosition = pos;
            }

            // �����������: ��������� ������, ����� �� ��������� ������
            enabled = false;
        }
    }
}