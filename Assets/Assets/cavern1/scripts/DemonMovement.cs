using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class DemonMovement : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public LayerMask playerLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private bool movingRight = true;
    private bool isChasing = false;
    private bool canAttack = true;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
            player = foundPlayer.transform;

        rb.freezeRotation = true;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Player nằm trong vùng phát hiện
        if (distance <= detectionRange && distance > attackRange)
        {
            isChasing = true;
            if (!isAttacking) MoveTowardsPlayer();
        }
        // Player nằm trong vùng tấn công
        else if (distance <= attackRange)
        {
            if (canAttack && !isAttacking)
                StartCoroutine(AttackPlayer());
        }
        // Player ra khỏi vùng phát hiện
        else
        {
            isChasing = false;
            if (!isAttacking) Idle();
        }
    }

    void MoveTowardsPlayer()
    {
        animator.Play("DemonWalk");

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Quay mặt đúng hướng
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight))
            Flip();

        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    IEnumerator AttackPlayer()
    {
        isAttacking = true;
        canAttack = false;
        rb.velocity = Vector2.zero;

        animator.Play("DemonAttack");

        yield return new WaitForSeconds(0.5f); // thời gian đánh
        animator.Play("DemonWalk"); // sau khi đánh xong quay lại đi

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        isAttacking = false;
    }

    void Idle()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.Play("DemonIdle");
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // vùng phát hiện
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow; // vùng tấn công
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
