using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class flower : MonoBehaviour
{

    public static flower _instance;
    public static UnityEvent onGameEnd = new UnityEvent();
    void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
    public float maxRotationSpeed = 100f; // Максимальная скорость вращения

    // Метод для изменения состояния объекта
    public void player_change_enabled()
    {
        player_move._instance.change_enabled();
    }

    // Метод для поворота цветка к заданной точке
    public void rotate_flower(Vector3 nextPosition)
    {
        // Вычисляем направление и угол поворота
        Vector3 direction = nextPosition - transform.position;
        float rotateZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotateZ - 90);

        // Поворачиваем объект к целевой точке
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);

        // Обновляем позицию и линию

    }

    public void end_level()
    {
        onGameEnd.Invoke();
    }
}
