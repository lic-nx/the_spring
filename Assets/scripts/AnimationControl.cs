using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationControl : MonoBehaviour
{
    Animator leafAnimator;
    void Start()
    {
        leafAnimator = GetComponent<Animator>();
        float speed = Random.Range(0.1f, .5f);
        leafAnimator.speed = speed;
    }
}
