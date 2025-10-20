using UnityEngine;

public class DemonMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public Transform player;

    [Header("Stats")]
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private bool isAttacking = false;
    private bool facingRight = true;
    private float nextAttackTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        animator.Play("DemonIdle");
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lật hướng về phía player
        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();

        // Khi đang tấn công thì không xử lý logic khác
        if (isAttacking) return;

        // Nếu player trong vùng tấn công
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                nextAttackTime = Time.time + attackCooldown;
            }
            else
            {
                // Nếu vẫn trong cooldown, Demon vẫn walk xung quanh player
                MoveTowardsPlayer();
            }
        }
        // Nếu player trong vùng phát hiện
        else if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.velocity = Vector2.zero;
            animator.Play("DemonIdle");
        }
    }

    void MoveTowardsPlayer()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("DemonAttack")) return;

        animator.Play("DemonWalk");
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        animator.Play("DemonAttack");

        // Giả lập thời gian tấn công
        yield return new WaitForSeconds(0.8f);

        // Sau khi tấn công xong → trở lại Walk
        isAttacking = false;
        animator.Play("DemonWalk");
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
