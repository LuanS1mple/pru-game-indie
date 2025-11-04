using UnityEngine;
using Pathfinding;

public class DetectionZone : MonoBehaviour
{
    public AIDestinationSetter aiDestinationSetter;  // tham chiếu tới AIDestinationSetter của miniboss
    public AIPath aiPath;                            // để bật/tắt di chuyển
    public Transform player;                         // để chỉ đích đến là player

    private void Start()
    {
        // Ban đầu miniboss không đuổi
        if (aiDestinationSetter != null)
            aiDestinationSetter.target = null;

        if (aiPath != null)
            aiPath.canMove = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã vào vùng phát hiện!");
            aiDestinationSetter.target = player;  // chỉ định player làm đích đến
            aiPath.canMove = true;                // cho phép di chuyển
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã rời khỏi vùng phát hiện!");
            aiDestinationSetter.target = null;    // ngừng theo dõi
            aiPath.canMove = false;               // dừng di chuyển
        }
    }
}
