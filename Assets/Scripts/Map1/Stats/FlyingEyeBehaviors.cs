using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class FlyingEyeBehaviors : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject Target;
    private Animator animator;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 40;
    private int currentHealth;
    private bool isDead = false;

    [Header("Ranges")]
    [SerializeField] private float DetectRange = 6f;
    [SerializeField] private float AttackRange = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float AttackCooldown = 2.5f;
    private float lastAttackTime;
    private bool isAttackingNow = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();
        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player");

        currentHealth = maxHealth;
        aiPath.canMove = true;
        lastAttackTime = -AttackCooldown;
    }

    void Update()
    {
        if (isDead || Target == null) return;

        float distance = Vector2.Distance(transform.position, Target.transform.position);
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // --- Hành vi di chuyển và tấn công ---
        if (distance <= DetectRange && distance > AttackRange)
        {
            aiPath.canMove = true;
            aiPath.destination = Target.transform.position;
        }
        else if (distance <= AttackRange)
        {
            aiPath.canMove = false;
            if (!isAttackingNow && timeSinceLastAttack >= AttackCooldown)
            {
                StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
        else
        {
            aiPath.canMove = false;
        }

        // --- Lật hướng theo vận tốc ---
        if (aiPath.desiredVelocity.x > 0.01f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (aiPath.desiredVelocity.x < -0.01f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    IEnumerator AttackRoutine()
    {
        isAttackingNow = true;
        animator.SetTrigger("attack");
        Debug.Log("🦅 FlyingEye tấn công!");

        // FlyingEye lao nhanh trong 1s
        float originalSpeed = aiPath.maxSpeed;
        aiPath.maxSpeed = originalSpeed * 2f;
        aiPath.canMove = true;

        float attackDuration = 1.0f;
        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            if (Target != null)
                aiPath.destination = Target.transform.position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        aiPath.maxSpeed = originalSpeed;
        aiPath.canMove = false;
        isAttackingNow = false;
    }

    // 🩸 Bị đánh từ PlayerAttack hoặc EnemyHitbox gọi hàm này
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        animator.SetTrigger("takehit");
        Debug.Log($"FlyingEye bị đánh! Mất {damage} HP. Còn {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        aiPath.canMove = false;
        animator.SetTrigger("die");
        Debug.Log("💀 FlyingEye chết!");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        float deathAnimLength = 0f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains("die") || clip.name.ToLower().Contains("death"))
                {
                    deathAnimLength = clip.length;
                    break;
                }
            }
        }

        yield return new WaitForSeconds(deathAnimLength > 0 ? deathAnimLength : 1.0f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
