using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class flower : MonoBehaviour
{
    public static flower _instance;

    // Объявляем событие как статическое UnityEvent
    public float maxRotationSpeed = 100f;
    public Animator animator;

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


    public void player_change_enabled()
    {
        Debug.Log("отключаем движение");
        player_move._instance.change_enabled();
    }

    public IEnumerator rotate_flower_to_sun(Vector3 sunPosition)
    {
        Vector3 direction = sunPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);
            yield return null; // Ждём один кадр
        }

        transform.rotation = targetRotation; // Точная установка
        Debug.Log("Цветок повернулся к солнцу.");
    }

public void RotateByDirection(Vector3 growDirection)
{
    if (growDirection == Vector3.zero)
        return;

    float angle = Mathf.Atan2(growDirection.y, growDirection.x) * Mathf.Rad2Deg - 90f;
    Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

    transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        targetRotation,
        maxRotationSpeed * Time.deltaTime
    );
}
    public void end_level()
    {
        Debug.Log("Вызываем сигнал");
        EventControllerScr.Instance.OnEndGame();
    }

    public void anim_flower(bool True)
    {
        animator = GetComponent<Animator>();
        animator.SetBool("End", True);
    }
}
