using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class player_move : MonoBehaviour
{
    // public Vector3 first_dot;
    public float maxRotationSpeed = 100f;
    public GameObject Sun;
    public static player_move _instance;
    public Animator animator;
    // ответсвенное за рисование стебля цветка 
    public Line_rendered line_render;  
    public frigger_checker[] Triggers;
    private Vector3 nextPosition;
    float position;
    Vector3 body_position;
    public bool enabled = false;
    float duration = 0.8f;  
    public GameObject first_dot;

    public void change_enabled(){
        Debug.Log("change enabled");

        if (enabled == true){
            enabled = false;
        }
        else{
            enabled = true;
        }
    }
    void Start()
    {  
        // animator = GetComponent<Animator>();
        Debug.Log("Start game");
        Debug.Log(first_dot.transform.position);
        line_render.AddPoint(first_dot.transform.position);
        body_position = transform.position;
        line_render.AddPoint(body_position);
        body_position = transform.position;
        nextPosition = transform.position;
        position = 1;
        if (_instance == null){
        _instance = this;
        }
    }
    public void stop(){
        nextPosition = transform.position;
        position = 60;
    }

    public void is_sun(){
        Debug.Log("take sun");
        animator.SetBool("End", true);
        Destroy(Sun);
        enabled = false;
    }

    IEnumerator Reset() {
        
        yield return new WaitForSeconds(0);
            Debug.Log("check whats is free");
            foreach (frigger_checker tag in Triggers)
            {
                if (tag.OnTriggerEnter_ == false){
                    position = 0;
                    Debug.Log("move to new pos");
                    body_position = transform.position; // то где сейчас тело
                    if (Vector3.Distance(line_render.GetLastPoint(), body_position) > 1.5){
                        line_render.AddPoint(body_position); // добавляем последнее место нахождения цветка
                    }
                    Debug.Log(tag.OnTriggerEnter_);
                    nextPosition =  tag.transform.position; // То куда тело должно прийти 
                    break;
                }
                else{
                    Debug.Log("can't move");
                }
            }
        position = 0;

  // continue process
}

    [SerializeField] private LayerMask obstacleMask; // В инспекторе укажи слой блоков

    private bool isMoving = false;

    // Проверяет, поместится ли цветок в позицию
    bool CanFitAt(Vector3 position)
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>();
        if (col == null) return true;

        Vector2 size = col.size * 0.9f; // немного меньше, чтобы не клипать
        Collider2D[] hits = Physics2D.OverlapBoxAll(position, size, 0f, obstacleMask);
        foreach (var hit in hits)
        {
            if (hit != null && hit.gameObject != gameObject && !hit.isTrigger)
                return false;
        }
        return true;
    }

    // Ищет первый свободный и подходящий триггер
    public bool TryFindNextPosition(out Vector3 targetPosition)
    {
        targetPosition = transform.position;
        foreach (frigger_checker trigger in Triggers)
        {
            if (!trigger.OnTriggerEnter_) // триггер не занят
            {
                Vector3 candidate = trigger.transform.position;
                if (CanFitAt(candidate))
                {
                    targetPosition = candidate;
                    return true;
                }
            }
        }
        return false;
    }

    // Вызывается извне (после разрушения/перемещения)
    public void OnWorldChanged()
    {
        if (isMoving || !enabled) return;

        if (TryFindNextPosition(out Vector3 target))
        {
            StartCoroutine(MoveTo(target));
        }
        else
        {
            Debug.Log("Цветок не может начать движение: нет доступных путей.");
        }
    }

    // Плавное движение к цели
    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        // Обновляем стебель
        if (Vector3.Distance(line_render.GetLastPoint(), transform.position) > 0.1f)
            line_render.AddPoint(transform.position);

        Vector3 start = transform.position;
        float duration = 0.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration);
            flower._instance?.rotate_flower(target);
            line_render.UpdateLastPoint(transform.position);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        flower._instance?.rotate_flower(target);
        line_render.UpdateLastPoint(target);

        isMoving = false;

        // 🔁 Сразу проверяем: можно ли идти дальше?
        // (только если мы всё ещё включены и не достигли солнца)
        if (enabled)
        {
            if (TryFindNextPosition(out Vector3 nextTarget))
            {
                yield return new WaitForSeconds(0.1f); // небольшая пауза для плавности
                StartCoroutine(MoveTo(nextTarget));
            }
            else
            {
                Debug.Log("Цветок остановлен: все триггеры заблокированы или недоступны.");
            }
        }
    }

}