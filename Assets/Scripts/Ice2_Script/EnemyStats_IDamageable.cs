using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStats_IDamageable : MonoBehaviour
{
    [Header("HP")]
    public int maxHealth = 60;
    public int currentHealth = 60;

    [Header("Hit Reaction")]
    public float invulnTime = 0.15f;
    public float knockbackForce = 4.5f;
    public float stunTime = 0.2f;

    [Header("Refs")]
    public Animator animator;                         // kéo Animator nếu muốn, hoặc để tự tìm
    public EnemyAI2D_Animator enemyAI;                // <<< kiểu cụ thể, kéo thả dễ

    Rigidbody2D rb;
    float lastHitTime = -999f;
    bool dead = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();

        // Tự tìm nếu bạn quên kéo:
        if (!enemyAI) enemyAI = GetComponent<EnemyAI2D_Animator>()
                         ?? GetComponentInChildren<EnemyAI2D_Animator>();
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(int damage, Vector2 hitFromWorldPos)
    {
        Debug.Log($"[EnemyHP] {name} TAKE {damage} from {hitFromWorldPos} (hp {currentHealth}->{Mathf.Max(0, currentHealth - damage)})");

        if (dead) { Debug.Log("[EnemyHP] already dead"); return; }
        if (Time.time - lastHitTime < invulnTime) { Debug.Log("[EnemyHP] invuln"); return; }

        lastHitTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        Vector2 dir = ((Vector2)transform.position - hitFromWorldPos).normalized;
        rb.velocity = new Vector2(dir.x * knockbackForce, rb.velocity.y);

        if (animator) { Debug.Log("[EnemyHP] Trigger HIT"); animator.SetTrigger("Hit"); }

        if (currentHealth <= 0)
        {
            dead = true;
            if (enemyAI) enemyAI.enabled = false;
            if (animator) { Debug.Log("[EnemyHP] Trigger DEATH"); animator.SetTrigger("Death"); }
            StartCoroutine(CoDeathCleanup());
        }
        else
        {
            if (enemyAI) { Debug.Log($"[EnemyHP] STUN {stunTime}s (AI off)"); StartCoroutine(CoStun(stunTime)); }
        }
    }

    IEnumerator CoStun(float t)
    {
        if (enemyAI) enemyAI.enabled = false;
        yield return new WaitForSeconds(t);
        if (!dead && enemyAI) enemyAI.enabled = true;
    }

    IEnumerator CoDeathCleanup()
    {
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void OnDeathCleanupEvent()
    {
        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
        Destroy(gameObject, 2f);
    }
}
