using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    ~player_move(){
       player_.Clear();
    }

    Queue<Player> player_ = new Queue<Player>();
    public GameObject body;
    public GameObject body_trace;
    public frigger_checker[] Triggers;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Reset());
    }

    IEnumerator Reset() {
       // your process
       while (true){
        
        yield return new WaitForSeconds(3);
            Debug.Log("check whats is free");
            foreach (frigger_checker tag in Triggers)
            {
                if (tag.OnTriggerEnter_ == false){
                    Debug.Log("move to new pos");
                    Player player = new Player(body.transform.position, body_trace);
                    player.create_on_map();
                    player_.Enqueue(player);
                    Debug.Log(tag.OnTriggerEnter_);
                    // while(body.transform.position != tag.transform.position)
                    // body.transform.position = Vector3.MoveTowards(body.transform.position,tag.transform.position, 1);
                    body.transform.position =  tag.transform.position;
                    break;
                }
                else{
                    Debug.Log("can't move");
                }
            }
    }
  // continue process
} 

    // Update is called once per frame
    void Update()
    {
        //  StartCoroutine("Reset");
        

    }
}
