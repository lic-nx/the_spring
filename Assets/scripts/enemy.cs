using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���������, ��� �������� ������ � ��� �����
        Debug.Log("who touch me " + other.tag);
        if (other.CompareTag("Player"))
        {
            Debug.Log("change enabled");
            EventControllerScr.Instance.PlayerLose();
        }
    }

}
