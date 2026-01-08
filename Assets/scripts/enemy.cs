using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем, что вошедший объект — это игрок
        Debug.Log("Тег вошедшего объекта: " + other.tag);
        if (other.CompareTag("Player"))
        {
            Debug.Log("change enabled");
            eventController.Instance.PlayerLose();
        }
    }

}
