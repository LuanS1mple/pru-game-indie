using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlockMoveInAir : MonoBehaviour
{
    public AIPath aiPath;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.1f;
    public float moveSpeed = 3f;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (aiPath != null)
        {
            aiPath.canMove = false;  // Không cho AIPath tự điều khiển
            aiPath.canSearch = true; // vẫn cho nó tìm đường để có hướng đi
        }
    }

    void FixedUpdate()
    {
        // Kiểm tra có đang đứng trên ground không
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // Nếu không ở trên mặt đất => không cho di chuyển
        if (!isGrounded)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // Nếu AIPath có mục tiêu thì lấy hướng từ nó
        if (aiPath != null && aiPath.hasPath)
        {
            Vector2 direction = ((Vector2)aiPath.steeringTarget - rb.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}
