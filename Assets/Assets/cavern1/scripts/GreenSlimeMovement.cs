using UnityEngine;

public class GreenSlimeMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public Transform player;

    [Header("Stats")]
    public float moveForce = 6f;
    public float jumpAngle = 60f;
    public float jumpCooldown = 2f;
    public float detectionRange = 6f;
    public int maxHP = 3;
    private int currentHP;

    private bool isJumping = false;
    private bool isHurt = false;
    private bool isDead = false;
    private bool facingRight = true;
    private float nextJumpTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        currentHP = maxHP;
        animator.Play("GreenSlimeIdle");
    }

    void Update()
    {
        if (isDead || isHurt) return;

        if (player == null)
        {
            // Tự động tìm player nếu chưa gán
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Nếu trong phạm vi phát hiện → nhảy
        if (distanceToPlayer <= detectionRange && Time.time >= nextJumpTime)
        {
            JumpTowardsPlayer();
            nextJumpTime = Time.time + jumpCooldown;
        }
        else if (!isJumping)
        {
            animator.Play("GreenSlimeIdle");
        }

        // Lật hướng theo vị trí player
        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
    }

    void JumpTowardsPlayer()
    {
        if (isJumping) return;

        isJumping = true;
        animator.Play("GreenSlimeJump");

        // Tính hướng nhảy (góc 60 độ)
        Vector2 dir = (player.position - transform.position).normalized;
        float rad = jumpAngle * Mathf.Deg2Rad;
        Vector2 jumpDir = new Vector2(dir.x, Mathf.Tan(rad)).normalized;

        rb.velocity = Vector2.zero;
        rb.AddForce(jumpDir * moveForce, ForceMode2D.Impulse);

        // Sau khi nhảy, trở lại trạng thái Idle
        Invoke(nameof(EndJump), 1.2f);
    }

    void EndJump()
    {
        isJumping = false;
        animator.Play("GreenSlimeIdle");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Khi va chạm với Player
        if (collision.gameObject.CompareTag("Player"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(HurtRoutine());
        }
    }

    private System.Collections.IEnumerator HurtRoutine()
    {
        isHurt = true;
        animator.Play("GreenSlimeHurt");
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.6f);

        isHurt = false;
        animator.Play("GreenSlimeIdle");
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        animator.Play("GreenSlimeDie");
        Destroy(gameObject, 1.2f);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
