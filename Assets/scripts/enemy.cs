using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    private bool encounterResolved;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���������, ��� �������� ������ � ��� �����
        Debug.Log("who touch me " + other.tag);
        if (other.CompareTag("Player"))
        {
            if (encounterResolved)
                return;

            encounterResolved = true;

            FlowerProtection protection = other.GetComponentInParent<FlowerProtection>();
            if (protection != null && protection.TryConsumeAndTransform(this))
            {
                Debug.Log("Caterpillar hit was absorbed and transformed into a butterfly.");
                return;
            }

            Debug.Log("change enabled");
            EventControllerScr.Instance.PlayerLose();
        }
    }

}
