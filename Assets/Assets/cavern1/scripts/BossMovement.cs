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
    private bool isJumping = false;
    private float nextAttackTime = 0f;

    [Header("Jump Attack")]
    public GameObject jumpAttack;   // object con có script BossGroundAOE
    public Transform jumpPoint;     // vị trí spawn AOE (tùy chọn)

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
                // Chờ Boss chạm đất (OnCollisionEnter2D sẽ kích hoạt AOE)
                yield return new WaitUntil(() => isGrounded && !isJumping);
                animator.Play("BossAttack");
                yield return new WaitForSeconds(1f);
                break;
        }

        isAttacking = false;
        animator.Play("BossWalk");
    }

    int GetRandomAttackType()
    {
        float rand = Random.value; // 0.0 - 1.0
        if (rand < 0.2f)
            return 1; // 60%
        else if (rand < 0.4f)
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
        if (isGrounded && player != null)
        {
            Vector2 targetPos = new Vector2(player.position.x, player.position.y + 2f);
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
            Vector2 jumpDirection = new Vector2(direction.x, Mathf.Abs(direction.y) + 1.5f).normalized;
            rb.velocity = jumpDirection * jumpForce;

            isGrounded = false;
            isJumping = true;

            Debug.Log("🦘 Boss nhảy lên phía trên Player!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!isGrounded)
            {
                isGrounded = true;

                // 💥 Gây vùng sát thương khi chạm đất
                if (jumpAttack != null)
                {
                    var aoe = jumpAttack.GetComponent<BossGroundAOE>();
                    if (aoe != null)
                        aoe.ActivateAOE();
                }

                Debug.Log("💥 Boss chạm đất → kích hoạt JumpAttack!");

                StartCoroutine(ResetJumpFlag());
            }
        }
    }

    IEnumerator ResetJumpFlag()
    {
        yield return new WaitForSeconds(0.1f);
        isJumping = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
