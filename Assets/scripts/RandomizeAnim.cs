using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RandomAnimationStart : MonoBehaviour
{
    void Start()
    {
        Animator anim = GetComponent<Animator>();

        float randomOffset = Random.value; // 0..1

        anim.Play(0, -1, randomOffset);
    }
}
