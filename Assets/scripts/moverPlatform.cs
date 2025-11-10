using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moverPlatform : MonoBehaviour
{
    public Transform pointA; // Точка A для задания вектора
    public Transform pointB; // Точка B для задания вектора
    public float speed = 20f; // Скорость перемещения
    public bool mode = false; // если true — движение ограничено отрезком A–B

    private Vector3 movementVector;
    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool isEnter = false;

    void Start()
    {
        // Вычисляем вектор перемещения
        movementVector = pointB.position - pointA.position;
        movementVector.Normalize(); // Нормализуем вектор для получения направления
        initialPosition = pointA.position;
    }

    void OnMouseEnter()
    {
        isEnter = true;
        Debug.Log("mouse is enter");
    }

    void OnMouseExit()
    {
        isEnter = false;
        Debug.Log("mouse is out");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isEnter)
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
            mousePosition.z = 0;

            Vector3 targetPosition;

            if (mode)
            {
                // Проекция мыши на отрезок между pointA и pointB
                Vector3 a = pointA.position;
                Vector3 b = pointB.position;
                Vector3 ab = b - a;
                float t = Vector3.Dot(mousePosition - a, ab) / Vector3.Dot(ab, ab);
                t = Mathf.Clamp01(t); // Ограничиваем отрезком
                targetPosition = a + t * ab;
            }
            else
            {
                // Свободное следование за мышью
                targetPosition = mousePosition;
            }

            Vector3 objNextPosition = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            if (!WillCollide(objNextPosition))
            {
                if (mode)
                {
                    // Ограничиваем позицию отрезком A–B
                    objNextPosition = ClampToSegment(objNextPosition, pointA.position, pointB.position);
                }
                transform.position = objNextPosition;
            }
            else
            {
                Debug.Log("Пересечение обнаружено!");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            player_move._instance?.OnWorldChanged();
        }
    }

    bool WillCollide(Vector3 position)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(position, transform.lossyScale, 0);
        foreach (var collider in colliders)
        {
            if (collider != GetComponent<Collider2D>() && !collider.isTrigger)
            {
                return true;
            }
        }
        return false;
    }

    // Ограничивает точку отрезком между min и max (в 2D, но работает и в 3D)
    Vector3 ClampToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}