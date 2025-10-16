using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster2Controller : MonoBehaviour
{
    public Transform gunTip;
    public GameObject bullet;
    public float fireRate = 0.5f;   
    private float nextFire = 0f;

    void Update()
    {
        AutoFire();
    }

    private void AutoFire()
    {
        if (Time.time > nextFire)
        {

            nextFire = Time.time + fireRate;
            Instantiate(bullet, gunTip.position, Quaternion.Euler(new Vector3(0, 0, 180)));
        }
    }
}
