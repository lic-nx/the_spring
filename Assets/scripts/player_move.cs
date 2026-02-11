using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class player_move : MonoBehaviour
{
    public GameObject Sun;
    public static player_move _instance;

    // ответсвенное за рисование стебля цветка 
    public Line_rendered line_render;  
    public frigger_checker[] Triggers;
    Vector3 body_position;
    private Vector3 currentGrowDirection;
    public bool enabled = false;
    public GameObject first_dot;
    public float duration_of_move = 0.4f;
    public float max_angle = 120f;
    private Vector3 lastGrowthDirection = Vector3.zero; // Инициализируем как нулевой вектор


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
        Debug.Log("Start game");
        Debug.Log(first_dot.transform.position);

        line_render.AddPoint(first_dot.transform.position);
        line_render.AddPoint(this.transform.position);


        body_position = transform.position; ;

        if (_instance == null)
        {
            _instance = this;
        }

    }
    private void Update() {
                    line_render.UpdateHead(this.transform.position);
    }


    public void is_sun(){
        Debug.Log("take sun");

        isMoving = false;
        enabled = false;
        flower._instance?.anim_flower(true);

        Destroy(Sun);
    }

//     IEnumerator Reset() {
        
//         yield return new WaitForSeconds(0);
//             Debug.Log("check whats is free");
//             foreach (frigger_checker tag in Triggers)
//             {
//                 if (tag.OnTriggerEnter_ == false){
//                     Debug.Log("move to new pos");
//                     body_position = transform.position; // то где сейчас тело
//                     if (Vector3.Distance(line_render.GetLastPoint(), body_position) > 1.5){
//                         line_render.AddPoint(body_position); // добавляем последнее место нахождения цветка
//                     }
//                     Debug.Log(tag.OnTriggerEnter_);
//                     break;
//                 }
//                 else{
//                     Debug.Log("can't move");
//                 }
//             }
// }

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
                    Vector3 newDirection = (candidate - transform.position).normalized;

                    // Проверяем, есть ли предыдущее направление
                    if (lastGrowthDirection != Vector3.zero)
                    {
                        float angle = Vector3.Angle(lastGrowthDirection, newDirection);
                        if (angle > max_angle)
                        {
                            continue; // Отклоняем этот кандидат
                        }
                    }

                    // Принимаем кандидата
                    currentGrowDirection = newDirection;
                    targetPosition = candidate;
                    lastGrowthDirection = newDirection; // Сохраняем новое направление
                    return true;
                }
            }
        }

        return false;
    }

    // Плавное движение к цели
    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;

        //if (Vector3.Distance(line_render.GetLastPoint(), transform.position) > 0.1f)
        //line_render.AddPoint(transform.position);

        Vector3 start = transform.position;
        float elapsed = 0f;

        while (elapsed < duration_of_move && enabled)
        {
            transform.position = Vector3.Lerp(start, target, elapsed / duration_of_move);

            Vector2 headPos = transform.position;
            line_render.UpdateHead(headPos);        // ← всегда
            line_render.TryCommitSegment(headPos);
    

            flower._instance?.RotateByDirection(currentGrowDirection);

            //line_render.UpdateLastPoint(transform.position);
            elapsed += Time.deltaTime;
            yield return null;
        }


        // ⭐ фикс финальной позиции
        transform.position = target;

        flower._instance?.RotateByDirection(currentGrowDirection);

        isMoving = false;

        if (enabled)
        {
            OnWorldChanged();
        }

    }

}