using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class image_controler : MonoBehaviour
{
    [SerializeField] private Image spriteRenderer;

    private bool isEnadle = true;

   public void Update()
    {
        if (isEnadle)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }

    public void EnableImage()
    {
        if (isEnadle)
        {
            isEnadle = false;
        }
        else
        {
            isEnadle = true;
        }
    }
    
}

