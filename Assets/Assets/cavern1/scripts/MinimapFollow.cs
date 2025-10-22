using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    public float height = -10f;

    void LateUpdate()
    {
        if (player == null) return;
        transform.position = new Vector3(player.position.x, player.position.y, height);
    }
}
