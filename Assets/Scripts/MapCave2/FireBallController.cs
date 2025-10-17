using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBallController : MonoBehaviour
{
    //Khai bao bien toc do cho vien dan 
    public float bulletSpeed;

    //Khai bao component RigiBody de lam viec
    Rigidbody2D myBody;

    internal void RemoveForce()
    {
        //Triet tieu van toc cua vien dan 
        myBody.velocity = new Vector2(0, 0);
    }

    private void Awake()
    {
        myBody = GetComponent<Rigidbody2D>();
        if (transform.localRotation.z > 0) //VIên đạn đang quay theo hướng mặc  định của thiết kế
        {
            //Tác động vào 1 lực vật lý lên đối tượng (Có RigiBody) -> giúp cho đối tượng có thể di chuyển, tăng tốc hoặc bị đẩy theo 1 hướng
            myBody.AddForce(new Vector2(-1, 0) * bulletSpeed, ForceMode2D.Impulse);
        }
        else
        {
            myBody.AddForce(new Vector2(1, 0) * bulletSpeed, ForceMode2D.Impulse);
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
