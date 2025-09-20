using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing_Luan : MonoBehaviour
{
    [SerializeField]
    float tocdo = 0.01f;
    [SerializeField]
    float xoayvong = 0.1f;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Welcome");
        Debug.LogWarning("ohnno");
    }

    // Update is called once per frame
    void Update()
    {
        float dichuyen = Input.GetAxis("Vertical") * tocdo;
        float doihuong = Input.GetAxis("Horizontal") * xoayvong;
        transform.Translate(0, dichuyen, 0);
        transform.Rotate(0, 0, -doihuong);
    }
}