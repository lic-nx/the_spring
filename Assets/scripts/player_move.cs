using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;


public class player_move : MonoBehaviour
{
    public static player_move _instance;
    [SerializeField] private Transform[] points;
    [SerializeField] private Line_rendered line;
    public GameObject body;
    // public GameObject body_trace;
    public frigger_checker[] Triggers;
    public Vector3 nextPosition;
    float position;
    Vector3 body_position;
<<<<<<< HEAD

=======
    
    
>>>>>>> 993d3230a67285e85586b5842c41bd4c87b9e262
    // Start is called before the first frame update
    void Start()
    { Debug.Log("Start game");
        body_position = body.transform.position;
        nextPosition =   body.transform.position;
<<<<<<< HEAD
        line.SetUpLine(body.transform);
=======
        
        Debug.Log(body_position+" "+nextPosition);
>>>>>>> 993d3230a67285e85586b5842c41bd4c87b9e262
        position = 1;
        if (_instance == null){
        _instance = this;
        }
    }

    public void stop(){
        nextPosition = body.transform.position;
        position = 60;
    }

    IEnumerator Reset() {
        
        yield return new WaitForSeconds(0);
            Debug.Log("check whats is free");
            foreach (frigger_checker tag in Triggers)
            {
                if (tag.OnTriggerEnter_ == false){
                    position = 1;
                    Debug.Log("move to new pos");

                    body_position = body.transform.position;
                    Debug.Log(tag.OnTriggerEnter_);

                    nextPosition =  tag.transform.position;
                    
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
        
        if (position >= 60){
            StartCoroutine(Reset());         
        }
        
        if (position < 60){
            for (int i = 0; i < pathCreator.bezierPath.NumPoints; i++)
            {
                Vector3 pointPosition = pathCreator.bezierPath.GetPoint(i);
                Debug.Log("Point " + i + ": " + pointPosition);
            }
            // x1 y1, x2 y2; (y2 - y1) / (x2 - x1) = K (x2 - x1 / 60) = x y = k * x + y1
            body_position = body.transform.position;
            body.transform.position +=  position/60 * (nextPosition - body_position);
<<<<<<< HEAD
=======
            Debug.Log("position " + position + "body  " +( body_position + position/60 * (nextPosition - body_position)));
            Debug.Log("next position "+nextPosition +" "+ body_position +" "+ (nextPosition - body_position));
            pathCreator.bezierPath.MovePoint(pathCreator.bezierPath.NumPoints - 1, body_position +  position/60 * (nextPosition - body_position) );
            Debug.Log(" pathCreator.TriggerPathUpdate();");
            pathCreator.TriggerPathUpdate();
>>>>>>> 993d3230a67285e85586b5842c41bd4c87b9e262
            position++;
        }
        
    }
}
