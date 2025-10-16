using System.Collections;
using UnityEngine;
using Pathfinding;

public class GoblinBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Transform target;
    [SerializeField] private Animator animator;
    [Header("Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1.2f;

    private bool isAttacking = false;
    private bool isDead = false;
    private float lastAttackTime = -999f;

    void Start()
    {
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();
        if (animator == null)
            animator = GetComponent<Animator>();

        aiPath.canMove = true;
        animator.SetFloat("Hp", hp);
    }

    void Update()
    {
        if (isDead || target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // --- Flip hướng ---
        if (aiPath.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (aiPath.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // --- Tính tốc độ di chuyển ---
        float moveSpeed = Mathf.Abs(aiPath.desiredVelocity.x);
        if (aiPath.reachedDestination || moveSpeed < 0.2f)
            moveSpeed = 0;

        animator.SetFloat("speed", moveSpeed);

        // --- Logic phát hiện & hành vi ---
        if (distance <= detectRange && distance > attackRange)
        {
            aiPath.canMove = true;
            aiPath.destination = target.position;
        }
        else if (distance <= attackRange)
        {
            aiPath.canMove = false;
            animator.SetFloat("speed", 0);

            if (!isAttacking && timeSinceLastAttack >= attackCooldown)
                StartCoroutine(AttackRoutine());
        }
        else
        {
            aiPath.canMove = false;
            animator.SetFloat("speed", 0);
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        aiPath.canMove = false;
        animator.SetFloat("speed", 0);

        animator.SetTrigger("attack");
        Debug.Log("⚔️ Goblin Attack!");

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        aiPath.canMove = true;
        lastAttackTime = Time.time;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        hp -= damage;
        animator.SetFloat("Hp", hp);
        animator.SetTrigger("takehit");

        if (hp <= 0.01f)
        {
            isDead = true;
            aiPath.canMove = false;
            Debug.Log("☠️ Goblin died!");
            StartCoroutine(DisappearAfterDelay());
        }
    }

    IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
