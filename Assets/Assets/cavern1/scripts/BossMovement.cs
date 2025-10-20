using System.Collections;
using UnityEngine;

public class BossMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public Transform player;

    [Header("Stats")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpForce = 8f;
    public float detectionRange = 12f;
    public float attackRange = 2.5f;
    public float attackCooldown = 2f;

    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isGrounded = true;
    private float nextAttackTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator.Play("BossIdle");
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lật hướng về phía player
        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();

        // Player trong tầm tấn công
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                int attackType = GetRandomAttackType();
                StartCoroutine(AttackRoutine(attackType));
                nextAttackTime = Time.time + attackCooldown;
            }
            else
            {
                animator.Play("BossIdle");
                rb.velocity = Vector2.zero;
            }
        }
        // Player trong tầm phát hiện
        else if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        // Player ngoài vùng phát hiện
        else
        {
            animator.Play("BossIdle");
            rb.velocity = Vector2.zero;
        }
    }

    void MoveTowardsPlayer()
    {
        animator.Play("BossWalk");
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * walkSpeed, rb.velocity.y);
    }

    IEnumerator AttackRoutine(int attackType)
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;

        switch (attackType)
        {
            case 1: // Attack thường (60%)
                animator.Play("BossAttack");
                yield return new WaitForSeconds(1f);
                break;

            case 2: // Run -> Attack (20%)
                animator.Play("BossRun");
                yield return new WaitForSeconds(0.5f);
                RunTowardsPlayer();
                yield return new WaitForSeconds(0.7f);
                rb.velocity = Vector2.zero;
                animator.Play("BossAttack");
                yield return new WaitForSeconds(1f);
                break;

            case 3: // Jump -> Attack (20%)
                animator.Play("BossJump");
                yield return new WaitForSeconds(0.3f);
                Jump();
                yield return new WaitUntil(() => isGrounded);
                animator.Play("BossAttack");
                yield return new WaitForSeconds(1f);
                break;
        }

        // Quay lại trạng thái Walk
        isAttacking = false;
        animator.Play("BossWalk");
    }

    int GetRandomAttackType()
    {
        float rand = Random.value; // 0.0 - 1.0
        if (rand < 0.6f)
            return 1; // 60%
        else if (rand < 0.8f)
            return 2; // 20%
        else
            return 3; // 20%
    }

    void RunTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * runSpeed, rb.velocity.y);
    }

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }
}
