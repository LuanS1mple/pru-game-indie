using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    //Biến làm mịn
    public float smoothing;
    //Xác định khoảng cách từ cam đến player
    Vector3 offset;
    //Biến để nhân vật rơi khỏi khung hình thì cam k theO
    float lowY;
    void Start()
    {
        offset = transform.position - target.position;
        lowY = transform.position.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Vị trí của cam khi nv di chuyển
        Vector3 targetCamPos = target.position + offset;
        //thêm lục vật lí (lực đẩy) cho camera khi nhân vật di chuyển
        transform.position = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);
        if (transform.position.y < lowY)
        {
            transform.position = new Vector3(transform.position.x, lowY, transform.position.z);
        }
        if (transform.position.y > lowY)
        {
            transform.position = new Vector3(transform.position.x, lowY, transform.position.z);
        }
    }
}
