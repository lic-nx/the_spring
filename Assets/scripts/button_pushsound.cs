using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_pushsound : MonoBehaviour
{
    [SerializeField] private AudioClip soundClip;
    
    public void ButtonPush()
    {
        AudioSource.PlayClipAtPoint(
                soundClip,
                transform.position,
                1f
            ); 
    }
}
