using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeStat : MonoBehaviour
{
    public bool IsDeath;
    // Start is called before the first frame update
    //Animator  
    Animator animator;
    void Start()
    {
        IsDeath = false;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDeath)
        {
            animator.SetBool("IsDeath",true);
        }
    }
}
