using System.Collections;
using UnityEngine;

public class SlimeMovement : MonoBehaviour
{
    [Header("Movement")]
    public float jumpForce = 5f;
    public float moveSpeed = 2f;
    public float jumpDelay = 2f;

    [Header("Attack")]
    public float attackRange = 4f;
    public float attackJumpForce = 8f;
    public float attackMoveSpeed = 3f;
    public float attackCooldown = 2f;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool movingRight = true;
    private bool canAttack = true;
    private float nextJumpTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Kiểm tra chạm đất
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.6f, LayerMask.GetMask("Ground"));

        // Kiểm tra có player trong vùng tấn công
        Collider2D player = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);

        // Nếu phát hiện player → tấn công
        if (player && canAttack && isGrounded)
        {
            StartCoroutine(AttackPlayer(player.transform));
        }
        // Ngược lại → nhảy di chuyển bình thường
        else if (isGrounded && Time.time >= nextJumpTime)
        {
            NormalJump();
            nextJumpTime = Time.time + jumpDelay;
        }

        UpdateAnimation();
    }

    IEnumerator AttackPlayer(Transform target)
    {
        canAttack = false;
        animator.Play("BlueSlimeJump");

        yield return new WaitForSeconds(0.1f);

        float direction = Mathf.Sign(target.position.x - transform.position.x);
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight))
            Flip();

        rb.velocity = new Vector2(direction * attackMoveSpeed, attackJumpForce);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void NormalJump()
    {
        animator.Play("BlueSlimeJump");
        float direction = movingRight ? 1 : -1;
        rb.velocity = new Vector2(direction * moveSpeed, jumpForce);
    }

    void UpdateAnimation()
    {
        if (!isGrounded && rb.velocity.y < 0)
            animator.Play("BlueSlimeFall");
        else if (isGrounded && Mathf.Abs(rb.velocity.x) < 0.1f)
            animator.Play("BlueSlimeIdle");
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    Flip();
                    break;
                }
            }

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
