using System.Collections;
using UnityEngine;
using Pathfinding;

public class GoblinBehavior : BaseStats
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private Transform target; // Player (hoặc child của player)
    [SerializeField] private Animator animator;

    [Header("Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1.2f;
    [SerializeField] private float attackDamage = 10f; // 💥 damage gây cho player

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

        // --- Tốc độ di chuyển ---
        float moveSpeed = Mathf.Abs(aiPath.desiredVelocity.x);
        animator.SetFloat("speed", moveSpeed);

        // --- Logic hành vi ---
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

        // ⏳ Gây damage ở giữa thời gian đánh
        yield return new WaitForSeconds(attackDuration * 0.5f);
        DealDamageToPlayer();

        // ⏳ chờ hết animation
        yield return new WaitForSeconds(attackDuration * 0.5f);

        isAttacking = false;
        aiPath.canMove = true;
        lastAttackTime = Time.time;
    }

    // 💥 Gây sát thương cho Player
    private void DealDamageToPlayer()
    {
        if (target == null) return;

        // Kiểm tra khoảng cách
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance > attackRange + 0.3f) return;

        // Lấy BaseStats của Player (dù Player hay con của Player)
        BaseStats playerStats = target.GetComponentInParent<BaseStats>();
        if (playerStats != null && !playerStats.isDead)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log($"Goblin gây {attackDamage} sát thương cho {playerStats.entityName}");
        }
        else
        {
            Debug.Log("Player không có BaseStats hoặc đã chết!");
        }
    }

    // 💀 Nhận sát thương từ Player
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
