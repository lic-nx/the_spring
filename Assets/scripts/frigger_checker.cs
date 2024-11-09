using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frigger_checker : MonoBehaviour
{
    public bool OnTriggerEnter_ = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("An object entered.");
    }
    private void OnTriggerStay2D(Collider2D other)
    {

        OnTriggerEnter_ = true;
        Debug.Log("An object is still inside of the trigger");
    }
    private void OnTriggerExit2D(Collider2D other)
    {
   
        OnTriggerEnter_ = false;

        Debug.Log("An object left.");
    }


    // void OnTriggerStay(Collider other)
    // {
        
    //      if (other.attachedRigidbody)
    //     {
    //         OnTriggerEnter = true;
    //     }
    //     else{
    //         OnTriggerEnter = false;
    //     }
    //     Debug.Log(OnTriggerEnter);
    // }
}
