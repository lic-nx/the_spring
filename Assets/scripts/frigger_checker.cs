using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frigger_checker : MonoBehaviour
{
    public bool OnTriggerEnter_ = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        player_move._instance.stop();
        Debug.Log("An object entered.");
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("sun"))
        {

            Debug.Log("is sun");
            Debug.Log("Object name: " + other.name);
            Debug.Log("find sun = " + other.transform.position );
            flower._instance.rotate_flower(other.transform.position);
            player_move._instance.stop();
            player_move._instance.is_sun();
        }
        else{    
            OnTriggerEnter_ = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerEnter_ = false;
    }

}
