using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class CirclePatrol2D : MonoBehaviour
{
    [Header("Circle")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float angularSpeed = 90f;

    [Header("Collision")]
    [SerializeField] private LayerMask blockLayers;

    private float angle;
    private int direction = -1;

    private SpriteRenderer sprite;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();

        UpdateFlip();
    }

    void Update()
    {
        MoveAlongCircle();
        RotateTowardCenter();
    }

    void MoveAlongCircle()
    {
        angle += angularSpeed * direction * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 localPos = new Vector3(
            Mathf.Cos(rad) * radius,
            Mathf.Sin(rad) * radius,
            0f
        );

        transform.localPosition = localPos;
    }

    void RotateTowardCenter()
    {
        Vector2 toCenter = -transform.localPosition;

        float angleDeg = Mathf.Atan2(toCenter.y, toCenter.x) * Mathf.Rad2Deg;

        transform.localRotation = Quaternion.Euler(0, 0, angleDeg + 90f);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if ((blockLayers.value & (1 << col.gameObject.layer)) == 0)
            return;

        Reverse();
    }

    void Reverse()
    {
        direction *= -1;
        UpdateFlip();
    }

    void UpdateFlip()
    {
        sprite.flipX = direction > 0;
    }
}