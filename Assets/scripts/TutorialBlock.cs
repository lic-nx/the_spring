using UnityEngine;
using System;

public class TutorialBlock : MonoBehaviour
{
    public static event Action OnAnyBlockChanged;

    private Vector3 startPosition;

    void Start()
    {

        Vector3 pos = transform.position;
        pos.z -= 1f;
        transform.position = pos;
        OnAnyBlockChanged += Return;
        startPosition = pos;
    }

    void Update()
    {
        if (transform.position != startPosition)
        {
            OnAnyBlockChanged?.Invoke();
            enabled = false;
        }
    }

    void OnDestroy()
    {
        if (this!=null)  
        OnAnyBlockChanged?.Invoke();
    }

    void Return()
    {
        if (this != null)
        {
        Vector3 pos = transform.position;
        pos.z += 1f;
        transform.position = pos;
        }

    }
}