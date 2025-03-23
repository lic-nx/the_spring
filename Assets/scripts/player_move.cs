using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class player_move : MonoBehaviour
{
    // public Vector3 first_dot;
    public float maxRotationSpeed = 100f;
    public static player_move _instance;
    public GameObject body;
    private Animator animator;
    // ответсвенное за рисование стебля цветка 
    public Line_rendered line_render;  
    public frigger_checker[] Triggers;
    private Vector3 nextPosition;
    float position;
    Vector3 body_position;
    public bool enabled = false;
    public GameObject first_dot;
    public void change_enabled(){
        if (enabled == true){
            enabled = false;
        }
        else{
            enabled = true;
        }
    }
    void Start()
    { 
        animator = GetComponent<Animator>();
        Debug.Log("Start game");
        Debug.Log(first_dot.transform.position);
        line_render.SetUpLine(first_dot.transform.position);
        // line_render.SetLastPoint(first_dot.transform.position);
        body_position = transform.position;
        line_render.SetUpLine(body_position);
        body_position = transform.position;
        nextPosition = transform.position;
        position = 1;
        if (_instance == null){
        _instance = this;
        }
    }
    public void stop(){
        nextPosition = body.transform.position;
        position = 60;
    }

    public void is_sun(){
        animator.SetBool("End", true);
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
                    line_render.SetUpLine(body_position); // добавляем последнее место нахождения цветка
                    Debug.Log(tag.OnTriggerEnter_);
                    nextPosition =  tag.transform.position; // То куда тело должно прийти 
                    break;
                }
                else{
                    Debug.Log("can't move");
                }
            }

  // continue process
} 

    // Update is called once per frame
    void FixedUpdate()
    { 
        if (position >= 60 && enabled == true){
            StartCoroutine(Reset());
        }      
        if (position < 60 && enabled == true){
                Vector3 direction = nextPosition - body.transform.position;
                float rotateZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.Euler(0f, 0f, rotateZ - 90);

                // Rotate towards the target rotation at a constant speed
                body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);
            transform.position +=  position/600 * (nextPosition - body_position);
            line_render.SetLastPoint(transform.position); // двигаем последнюю точку вместе с цветком 
            position++;
        }
        
    }
}
