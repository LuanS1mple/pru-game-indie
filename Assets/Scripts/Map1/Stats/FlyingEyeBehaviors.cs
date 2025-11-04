using System.Collections;
using UnityEngine;
using Pathfinding;

public class FlyingEyeBehaviors : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject Target;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // ✅ only flip sprite, not object
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

    // ✅ Pixel enemies dạng này mặc định nhìn TRÁI
    private bool facingRight = false;

    public int GetCurrentHealth() => currentHealth;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>(); // ✅ get sprite

        aiPath = aiPath ?? GetComponent<AIPath>();

        if (aiPath != null)
            aiPath.canMove = true;

        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player");

        if (Target != null)
        {
            playerStats = Target.GetComponent<BaseStats>() ??
                          Target.GetComponentInChildren<BaseStats>() ??
                          Target.GetComponentInParent<BaseStats>();
        }

        currentHealth = maxHealth;
        lastAttackTime = -AttackCooldown;
    }

    void Update()
    {
        if (isDead || Target == null) return;

        float distance = Vector2.Distance(transform.position, Target.transform.position);
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // Movement logic
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

        HandleFlip();
    }

    // ✅ Flip đúng hướng dựa theo SpriteRenderer
    private void HandleFlip()
    {
        float dirToTarget = Target.transform.position.x - transform.position.x;

        if (dirToTarget > 0 && !facingRight)
            Flip();
        else if (dirToTarget < 0 && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;

        // ✅ Chỉ lật sprite, không lật object (AIPath sẽ bị lỗi)
        if (spriteRenderer != null)
            spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    IEnumerator AttackRoutine()
    {
        if (isDead) yield break;
        isAttackingNow = true;

        animator?.SetTrigger("attack");

        yield return new WaitForSeconds(0.4f);

        if (!isDead && Target != null && playerStats != null)
        {
            float distance = Vector2.Distance(transform.position, Target.transform.position);
            if (distance <= AttackRange)
                playerStats.TakeDamage(damage);
        }

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
            int dmg = attacker != null ? Mathf.RoundToInt(attacker.attack) : 5;
            TakeDamage(dmg);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHealth -= dmg;
        currentHealth = Mathf.Max(currentHealth, 0);

        StartCoroutine(CoTakeHit());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator CoTakeHit()
    {
        animator?.SetTrigger("takehit");
        aiPath.canMove = false;
        yield return new WaitForSeconds(0.4f);
        aiPath.canMove = true;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        aiPath.canMove = false;

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        var hb = GetComponentInChildren<EnemyHealthBarUI>();
        if (hb != null) hb.gameObject.SetActive(false);

        animator?.SetTrigger("die");
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        float delay = 1f;
        if (animator != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
                if (clip.name.ToLower().Contains("death")) delay = clip.length;
        }

        yield return new WaitForSeconds(delay);

        Destroy(transform.root.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, DetectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, AttackRange);
    }
}
