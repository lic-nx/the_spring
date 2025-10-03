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

    // Update is called once per frame
    void FixedUpdate()
    { 
        if (position >= 60 && enabled == true){
            StartCoroutine(Reset());
        }      
        if (position < 60 && enabled == true ){
            if (Vector3.Distance(nextPosition, transform.position) > 0.2){
                flower._instance.rotate_flower(nextPosition); // Вызываем метод поворота
                transform.position +=  position/600 * (nextPosition - body_position);
                line_render.UpdateLastPoint(transform.position); // двигаем последнюю точку вместе с цветком
            } 
            position++;
        }
        
    }
}