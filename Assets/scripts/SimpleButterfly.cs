using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleButterfly : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 1f;

    [Header("Noise")]
    [SerializeField] private float noiseAmplitude = 0.5f;
    [SerializeField] private float noiseFrequency = 1f;

    private Vector3 center;
    private Vector3 velocity;
    private float noiseOffset;

    void Start()
    {
        center = transform.position;

        // случайное начальное направление
        velocity = Random.insideUnitCircle.normalized;

        noiseOffset = Random.value * 100f;
    }

    void Update()
    {
        float t = Time.time;

        // плавный шум
        float nx = Mathf.PerlinNoise(t * noiseFrequency, noiseOffset) - 0.5f;
        float ny = Mathf.PerlinNoise(noiseOffset, t * noiseFrequency) - 0.5f;

        Vector3 noise = new Vector3(nx, ny, 0f) * noiseAmplitude;

        velocity += noise * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, 1f);

        transform.position += velocity * speed * Time.deltaTime;

        // ограничение радиуса (мягкий возврат к центру)
        Vector3 toCenter = center - transform.position;
        transform.position += toCenter * 0.5f * Time.deltaTime;

    }
}