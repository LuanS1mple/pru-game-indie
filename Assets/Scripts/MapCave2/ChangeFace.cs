using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class ChangeFace : MonoBehaviour
{
    public AIPath aiPath;
    [Tooltip("Nếu sprite mặc định quay sang trái, bật tùy chọn này.")]
    public bool flip = false;

    void Update()
    {
        if (aiPath == null) return;

        float direction = aiPath.desiredVelocity.x;

        if (direction >= 0.01f)
        {
            transform.localScale = flip
                ? new Vector3(-1f, 1f, 1f)  // nếu quái mặc định quay trái
                : new Vector3(1f, 1f, 1f);  // nếu quái mặc định quay phải
        }
        else if (direction <= -0.01f)
        {
            transform.localScale = flip
                ? new Vector3(1f, 1f, 1f)
                : new Vector3(-1f, 1f, 1f);
        }
    }
}
