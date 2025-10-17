using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AIPath))]
public class BossMove : MonoBehaviour
{
    private AIPath aiPath;
    private float groundY;

    void Start()
    {
        aiPath = GetComponent<AIPath>();
        // Lưu vị trí Y ban đầu làm mặt đất
        groundY = transform.position.y;
    }

    void Update()
    {
        // Khóa Y lại để không bay lên/xuống
        transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
    }
}