using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class button_pushsound : MonoBehaviour
{
        [SerializeField] private AudioClip soundClip;
    private AudioSource audioSource;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); 
        }
    }

    void Update()
    {

    }
    
    public void ButtonPush()
    {
        audioSource.PlayOneShot(soundClip);
    }
}
