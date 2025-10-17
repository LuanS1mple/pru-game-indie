using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float chaseSpeed = 5f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float jumpForce = 7f;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public Rigidbody2D rb;

    private bool isAttacking = false;
    private bool facingRight = true;
    private float nextAttackTime = 0f;

    void Start()
    {
        // Tự động tìm player nếu chưa gán
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            TryAttack();
        }

        FlipTowardsPlayer();
    }

    // 🏃 Boss đuổi theo người chơi
    private void ChasePlayer()
    {
        if (isAttacking) return;

        animator.SetBool("isRunning", true);

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);
    }

    // ⚔️ Boss tấn công khi đủ gần
    private void TryAttack()
    {
        if (Time.time < nextAttackTime || isAttacking) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        animator.SetBool("isRunning", false);
        animator.SetTrigger("attack");
        rb.velocity = Vector2.zero;

        // Delay 0.5s để trùng với animation đánh
        yield return new WaitForSeconds(0.5f);

        // (Tại đây có thể thêm logic gây sát thương nếu muốn)

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    // 🪶 Hướng về phía người chơi
    private void FlipTowardsPlayer()
    {
        bool playerIsRight = player.position.x > transform.position.x;
        if (playerIsRight && !facingRight)
        {
            Flip();
        }
        else if (!playerIsRight && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // 🦘 Boss nhảy
    public void Jump()
    {
        animator.SetTrigger("jump");
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
}
