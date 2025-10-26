using System.Collections;
using UnityEngine;
using Pathfinding;

public class MinibossBehaviour : MonoBehaviour
{
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

    private bool facingRight = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        aiPath = aiPath ?? GetComponent<AIPath>();

        if (aiPath != null)
        {
            aiPath.canMove = true;
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

        // --- Flip hướng dựa vào player ---
        if (Target != null)
        {
            float dirToTarget = Target.transform.position.x - transform.position.x;
            if (dirToTarget > 0 && !facingRight)
                Flip();
            else if (dirToTarget < 0 && facingRight)
                Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator AttackRoutine()
    {
        isAttackingNow = true;

        // 🔁 3 lần tấn công thường, 1 lần đặc biệt
        normalAttackCount++;
        bool isSpecial = false;

        if (normalAttackCount >= 4)
        {
            isSpecial = true;
            normalAttackCount = 0;
        }

        if (animator != null)
        {
            if (isSpecial)
                animator.SetTrigger("trig_special");
            else
                animator.SetTrigger("trig_attack");
        }

        Debug.Log(isSpecial ? "💥 Miniboss dùng SPECIAL Attack!" : "⚔️ Miniboss dùng Attack thường!");

        yield return new WaitForSeconds(0.5f); // delay gây damage

        if (!isDead && Target != null && playerStats != null)
        {
            float distance = Vector2.Distance(transform.position, Target.transform.position);
            if (distance <= AttackRange)
            {
                int dmg = isSpecial ? specialDamage : normalDamage;
                playerStats.TakeDamage(dmg);
                Debug.Log($"🩸 Player bị Miniboss gây {dmg} damage ({(isSpecial ? "SPECIAL" : "NORMAL")})!");
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
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Miniboss bị đánh! Mất {damage} HP. Còn {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        aiPath.canMove = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log("💀 Miniboss đã chết!");
        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
