using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_pause : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayerPause()
    {
        EventControllerScr.Instance.PlayerPause();
    }
    
    public void PlayerUnPause()
    {
        EventControllerScr.Instance.PlayerUnPause();
    }
}
