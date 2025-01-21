using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moverPlatform : MonoBehaviour
{
    public Transform pointA; // Точка A для задания вектора
    public Transform pointB; // Точка B для задания вектора
    public float speed = 20f; // Скорость перемещения

    private Vector3 movementVector;
    private Vector3 initialPosition;
    private bool isDragging = false;

    void Start()
    {
        // Вычисляем вектор перемещения
        movementVector = pointB.position - pointA.position;
        movementVector.Normalize(); // Нормализуем вектор для получения направления

        initialPosition = pointA.transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Устанавливаем z-координату для 2D пространства

            // Вычисляем направление перемещения относительно вектора
            Vector3 direction = mousePosition - initialPosition;
            direction = Vector3.Project(direction, movementVector);
            Vector3 objNextPosition = Vector3.MoveTowards(transform.position, initialPosition + direction, speed * Time.deltaTime);
           
           
            objNextPosition = ClampPosition(objNextPosition, pointA.position, pointB.position);

            transform.position = objNextPosition;
                
        }
    }
    Vector3 ClampPosition(Vector3 position, Vector3 min, Vector3 max)
    {
        float x = Mathf.Clamp(position.x, Mathf.Min(min.x, max.x), Mathf.Max(min.x, max.x));
        float y = Mathf.Clamp(position.y, Mathf.Min(min.y, max.y), Mathf.Max(min.y, max.y));
        float z = Mathf.Clamp(position.z, Mathf.Min(min.z, max.z), Mathf.Max(min.z, max.z));
        return new Vector3(x, y, z);
    }
}
