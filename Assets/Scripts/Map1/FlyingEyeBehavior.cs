using System.Collections;
using UnityEngine;
using Pathfinding;

public class FlyingEyeBehavior : BaseStats
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject target;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private float hp = 100f;
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private float attackDamage = 5f;

    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isTakingHit = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();

        aiPath.canMove = true;
        animator.SetFloat("Hp", hp);
    }

    void Update()
    {
        if (hp <= 0.01f)
        {
            Die(); 
            return;
        }

        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // --- Flip hướng theo vận tốc ---
        if (aiPath.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (aiPath.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);

        // --- Logic hành vi ---
        if (distance <= detectRange)
        {
            aiPath.destination = target.transform.position;

            if (distance <= attackRange)
            {
                if (canAttack && !isAttacking && !isTakingHit)
                    StartCoroutine(AttackRoutine());
            }
            else
            {
                aiPath.canMove = true;
            }
        }
        else
        {
            aiPath.canMove = false;
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;
        aiPath.canMove = false;

        animator.SetTrigger("attack");
        Debug.Log("⚔️ FlyingEye Attack!");

        // 🕐 Gây sát thương ở giữa thời gian tấn công
        yield return new WaitForSeconds(attackDuration * 0.5f);
        DealDamageToPlayer();

        yield return new WaitForSeconds(attackDuration * 0.5f);

        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);

        aiPath.canMove = true;
        canAttack = true;
        Debug.Log("FlyingEye sẵn sàng tấn công lại!");
    }

    private void DealDamageToPlayer()
    {
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange + 0.5f) return;

        BaseStats playerStats = target.GetComponentInParent<BaseStats>();
        if (playerStats != null && !playerStats.isDead)
        {
            playerStats.TakeDamage(attackDamage);
            Debug.Log($"FlyingEye gây {attackDamage} sát thương cho {playerStats.entityName}");
        }
        else
        {
            Debug.Log("Player không có BaseStats hoặc đã chết!");
        }
    }

    // 🔥 Nhận sát thương từ Player
    public override void TakeDamage(float damage)
    {
        if (isTakingHit || hp <= 0.01f) return;

        hp -= damage;
        animator.SetFloat("Hp", hp);
        Debug.Log($"FlyingEye nhận {damage} sát thương! (Còn {hp})");

        StartCoroutine(TakeHitRoutine());
    }

    IEnumerator TakeHitRoutine()
    {
        isTakingHit = true;
        animator.SetTrigger("takehit");

        yield return new WaitForSeconds(0.5f);

        isTakingHit = false;
    }

    protected override void Die()
    {
        Debug.Log("💀 FlyingEye đã chết!");
        animator.SetTrigger("die");
        aiPath.canMove = false;
        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(1.2f); 
        Destroy(gameObject);
    }
}
