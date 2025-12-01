using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.1f; // высота покачивания
    [SerializeField] private float frequency = 1;   // скорость покачивания
    private float direction = 1;

    private Vector3 startPos;

    void Start()
    {
        frequency *= Random.Range(0.5f, 1f); //обновляем ее

        direction = Random.Range(0, 2) == 0 ? 1 : -1; //если 0 - то 1, если 1 - то -1

        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude * direction;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}