using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frigger_checker : MonoBehaviour
{
    public bool OnTriggerEnter_ = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        player_move._instance.stop();
        // Debug.Log("An object entered.");
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("sun"))
        {
            OnTriggerEnter_ = true;
        }
        else{
            player_move._instance.stop();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerEnter_ = false;
    }

}
