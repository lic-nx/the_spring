using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flover : MonoBehaviour
{
    // Start is called before the first frame update
    public void player_change_enabled(){
        player_move._instance.change_enabled();
    }
}
