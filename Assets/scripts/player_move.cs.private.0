using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Player: MonoBehaviour{
    // public GameObject body;
    public GameObject playerPrefab;
    public Vector3 objPos;
    
    public Player(Vector3 new_objPos, GameObject new_playerPrefab){
        objPos = new_objPos;
        playerPrefab = new_playerPrefab;
    }

    public void create_on_map(){
        Instantiate(playerPrefab, objPos, Quaternion.identity);// Quaternion.identity, transform); это назначить поворот и родителя
    }
}

public class player_move : MonoBehaviour
{
    public static player_move _instance;
    ~player_move(){
       player_.Clear();
    }
    public PathCreator pathCreator;
    Queue<Player> player_ = new Queue<Player>();
    public GameObject body;
    public GameObject body_trace;
    public frigger_checker[] Triggers;
    public Vector3 nextPosition;
    float position;
    Vector3 body_position;
    
    
    // Start is called before the first frame update
    void Start()
    { Debug.Log("Start game");
        body_position = body.transform.position;
        nextPosition =   body.transform.position;
        
        Debug.Log(body_position+" "+nextPosition);
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
                    // Player player = new Player(body.transform.position, body_trace);
                    // player.create_on_map();
                    // player_.Enqueue(player);
                    body_position = body.transform.position;
                    pathCreator.bezierPath.AddSegmentToEnd(body.transform.position);
                    Debug.Log(tag.OnTriggerEnter_);
                    // while(body.transform.position != tag.transform.position)
                    // body.transform.position = Vector3.MoveTowards(body.transform.position,tag.transform.position, 1);
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
        Debug.Log(position +" "+pathCreator.bezierPath.NumPoints +" "+ body_position + " " + nextPosition);
        // Debug.Log(nextPosition);
        if (position >= 60){
            StartCoroutine(Reset());
            //  StartCoroutine("Reset");
            
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
            Debug.Log("position " + position + "body  " +( body_position + position/60 * (nextPosition - body_position)));
            Debug.Log("next position "+nextPosition +" "+ body_position +" "+ (nextPosition - body_position));
            pathCreator.bezierPath.MovePoint(pathCreator.bezierPath.NumPoints - 1, body_position +  position/60 * (nextPosition - body_position) );
            Debug.Log(" pathCreator.TriggerPathUpdate();");
            pathCreator.TriggerPathUpdate();
            position++;
        }
        
    }
}
