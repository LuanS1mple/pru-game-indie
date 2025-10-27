using System.Collections;
using UnityEngine;
using Pathfinding;

public class FlyingEyeBehaviors : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [Tooltip("Nếu để trống sẽ tự Find bằng tag 'Player'")]
    [SerializeField] private GameObject Target;
    private Animator animator;
    private BaseStats playerStats;

    [Header("Stats")]
    [SerializeField] public int maxHealth = 40;
    private int currentHealth;
    private bool isDead = false;

    [Header("Ranges")]
    [SerializeField] private float DetectRange = 6f;
    [SerializeField] private float AttackRange = 2.5f;

    [Header("Attack Settings")]
    [SerializeField] private float AttackCooldown = 2.5f;
    [SerializeField] private int damage = 3;
    private float lastAttackTime;
    private bool isAttackingNow = false;

    private bool facingRight = true;

    // implement interface
    public int GetCurrentHealth() => currentHealth;

    void Start()
    {
        animator = GetComponent<Animator>();
        aiPath = aiPath ?? GetComponent<AIPath>();

        if (aiPath != null)
            aiPath.canMove = true;

        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player");

        if (Target == null)
        {
            Debug.LogError("[FlyingEye] ❌ Không tìm thấy GameObject có tag 'Player'.");
        }
        else
        {
            playerStats = Target.GetComponent<BaseStats>() ??
                          Target.GetComponentInChildren<BaseStats>() ??
                          Target.GetComponentInParent<BaseStats>();

            if (playerStats == null)
                Debug.LogWarning($"[FlyingEye] ⚠️ Không tìm thấy BaseStats trên '{Target.name}'!");
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

        // --- Flip hướng dựa trên vị trí Player ---
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
        if (isDead) yield break; // ✅ Enemy chết thì không tấn công
        isAttackingNow = true;

        if (animator != null)
            animator.SetTrigger("attack");

        yield return new WaitForSeconds(0.4f);

        // ✅ Kiểm tra lại trước khi gây damage
        if (!isDead && Target != null && playerStats != null)
        {
            float distance = Vector2.Distance(transform.position, Target.transform.position);
            if (distance <= AttackRange)
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"🩸 Player bị FlyingEye gây {damage} damage!");
            }
        }

        // ✅ Chờ cooldown nhưng vẫn kiểm tra sống/chết
        float timer = 0f;
        while (timer < AttackCooldown)
        {
            if (isDead) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        if (!isDead)
            isAttackingNow = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("TestAttack"))
        {
            BaseStats attacker = collision.GetComponentInParent<BaseStats>();
            int damageTaken = attacker != null ? Mathf.RoundToInt(attacker.attack) : 5;
            TakeDamage(damageTaken);
            Debug.Log($"FlyingEye nhận {damageTaken} damage từ {attacker?.name ?? "Unknown"}");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        StartCoroutine(CoTakeHit());
        Debug.Log($"FlyingEye bị đánh! Mất {damage} HP. Còn {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator CoTakeHit()
    {
        if (animator != null) animator.SetTrigger("takehit");
        aiPath.canMove = false;
        yield return new WaitForSeconds(0.4f);
        aiPath.canMove = true;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // ✅ Dừng toàn bộ coroutine và logic tấn công
        StopAllCoroutines();
        aiPath.canMove = false;

        // ✅ Tắt collider của toàn bộ object và con
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // ✅ Tắt rigidbody vật lý nếu có
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // ✅ Ẩn thanh máu nếu có (EnemyHealthBarUI nằm con)
        var healthBar = GetComponentInChildren<EnemyHealthBarUI>();
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        if (animator != null)
            animator.SetTrigger("die");

        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        float deathAnimLength = 1f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains("death"))
                {
                    deathAnimLength = clip.length;
                    break;
                }
            }
        }

        yield return new WaitForSeconds(deathAnimLength);

        // 🔥 Thay dòng này:
        // Destroy(gameObject);

        // ✅ Thành một trong các dòng sau, tùy cấu trúc prefab:

        // 👉 Nếu FlyingEyeBehaviors nằm trong child của một enemy root object:
        Destroy(transform.root.gameObject);

        // Hoặc nếu chỉ muốn hủy cha trực tiếp:
        // Destroy(transform.parent.gameObject);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
