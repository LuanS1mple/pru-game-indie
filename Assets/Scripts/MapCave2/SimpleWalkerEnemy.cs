using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleWalkerEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float wallCheckDistance = 0.3f;
    public LayerMask wallLayer;
    public Transform wallCheck; // Điểm check tường (đặt phía trước enemy)

    [Header("Animation")]
    public Animator animator;

    private Rigidbody2D rb;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        Move();
        CheckWall();
    }

    void Move()
    {
        // Đi liên tục
        rb.velocity = new Vector2((facingRight ? 1 : -1) * moveSpeed, rb.velocity.y);

        // Gửi tốc độ cho animation nếu có
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        }
    }

    void CheckWall()
    {
        if (wallCheck == null) return;

        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hitWall = Physics2D.Raycast(wallCheck.position, dir, wallCheckDistance, wallLayer);

        if (hitWall.collider != null)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        // Đảo hướng sprite
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck == null) return;

        Gizmos.color = Color.red;
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)dir * wallCheckDistance);
    }
}
