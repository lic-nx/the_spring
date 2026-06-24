using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PatrolBetweenPoints2D : MonoBehaviour
{
    private bool CanRotate = true;
    [Header("Patrol Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;

    [Header("Collision")]
    [SerializeField] private LayerMask blockLayers;

    private Transform target;
    private SpriteRenderer sprite;

    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        target = pointB;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // разворот спрайта
        Vector3 dir = target.position - transform.position;
        sprite.flipX = dir.x < 0;

        // достигли точки
        if (Vector3.Distance(transform.position, target.position) < 0.05f)
        {
            SwitchTarget();
        }
    }

    void SwitchTarget()
    {
        target = (target == pointA) ? pointB : pointA;
        StartCoroutine(Rotate());
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if ((blockLayers.value & (1 << col.gameObject.layer)) == 0)
            return;
        if (CanRotate)
        SwitchTarget();
    }
    IEnumerator Rotate()
    {
        CanRotate = false;
        yield return new WaitForSeconds(1f);
        CanRotate = true;
    }
}