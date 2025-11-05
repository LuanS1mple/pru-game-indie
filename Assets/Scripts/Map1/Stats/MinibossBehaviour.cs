using System.Collections;
using UnityEngine;
using Pathfinding;
using UnityEngine.Events;

public class MinibossBehaviour : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnBossDied;

    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [Tooltip("Nếu để trống sẽ tự Find bằng tag 'Player'")]
    [SerializeField] private GameObject Target;
    private Animator animator;
    private BaseStats playerStats;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 150;
    private int currentHealth;
    private bool isDead = false;

    [Header("Ranges")]
    [SerializeField] private float DetectRange = 7f;
    [SerializeField] private float AttackRange = 2.8f;

    [Header("Attack Settings")]
    [SerializeField] private float AttackCooldown = 2.5f;
    [SerializeField] private int normalDamage = 15;
    [SerializeField] private int specialDamage = 35;
    private float lastAttackTime;
    private bool isAttackingNow = false;
    private int normalAttackCount = 0;

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        aiPath = aiPath ?? GetComponent<AIPath>();

        if (aiPath != null)
        {
            aiPath.canMove = true;
            aiPath.updateRotation = false;
        }

        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player");

        if (Target != null)
        {
            playerStats = Target.GetComponent<BaseStats>() ??
                          Target.GetComponentInChildren<BaseStats>() ??
                          Target.GetComponentInParent<BaseStats>();
        }
        else
        {
            Debug.LogError("[Miniboss] ❌ Không tìm thấy Player có tag 'Player'!");
        }

        currentHealth = maxHealth;
        lastAttackTime = -AttackCooldown;
    }

    void Update()
    {
        if (isDead || Target == null) return;

        float distance = Vector2.Distance(transform.position, Target.transform.position);
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // --- Di chuyển & tấn công ---
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
    }

    IEnumerator AttackRoutine()
    {
        isAttackingNow = true;

        // 🔁 3 lần tấn công thường, 1 lần đặc biệt
        normalAttackCount++;
        bool isSpecial = normalAttackCount >= 4;
        if (isSpecial) normalAttackCount = 0;

        if (animator != null)
        {
            if (isSpecial)
                animator.SetTrigger("trig_special");
            else
                animator.SetTrigger("trig_attack");
        }

        yield return new WaitForSeconds(0.5f);

        if (!isDead && Target != null && playerStats != null)
        {
            float distance = Vector2.Distance(transform.position, Target.transform.position);
            if (distance <= AttackRange)
            {
                int dmg = isSpecial ? specialDamage : normalDamage;
                playerStats.TakeDamage(dmg);
            }
        }

        yield return new WaitForSeconds(AttackCooldown);
        isAttackingNow = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("TestAttack"))
        {
            BaseStats attacker = collision.GetComponentInParent<BaseStats>();
            int damageTaken = attacker != null ? Mathf.RoundToInt(attacker.attack) : 10;
            TakeDamage(damageTaken);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log($"Miniboss bị đánh! Mất {damage} HP. Còn {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnBossDied?.Invoke();
        aiPath.canMove = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("💀 Miniboss đã chết!");

        GameObject toDestroy = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(toDestroy, 2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
