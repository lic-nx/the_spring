using UnityEngine;
using UnityEngine.Events;

public class flower : MonoBehaviour
{
    public static flower _instance;

    // Объявляем событие как статическое UnityEvent
    [SerializeField]
    private UnityEvent onGameEnd;

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

    public float maxRotationSpeed = 100f;

    public void player_change_enabled()
    {
        player_move._instance.change_enabled();
    }

    public void rotate_flower(Vector3 nextPosition)
    {
        Vector3 direction = nextPosition - transform.position;
        float rotateZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotateZ - 90);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);
    }

    public void end_level()
    {
        Debug.Log("Вызываем сигнал");
        onGameEnd.Invoke();
    }
}
