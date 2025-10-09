using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEyeTest : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            anim.SetTrigger("Attack");
        if (Input.GetKeyDown(KeyCode.H))
            anim.SetTrigger("TakeHit");
        if (Input.GetKeyDown(KeyCode.D))
            anim.SetTrigger("Die");
    }
}
