using UnityEngine;

public class MinimapFollowMapCave2 : MonoBehaviour
{
    public Transform target; // player
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 newPos = target.position + offset;
        transform.position = newPos;
    }
}
