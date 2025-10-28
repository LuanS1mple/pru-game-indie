//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(Rigidbody2D))]
//public class EnemyStats_IDamageable : MonoBehaviour
//{
//    [Header("HP")]
//    public int maxHealth = 60;
//    public int currentHealth = 60;

//    [Header("Hit Reaction")]
//    public float invulnTime = 0.15f;
//    public float knockbackForce = 4.5f;
//    public float stunTime = 0.2f;

//    [Header("Refs")]
//    public Animator animator;                         // kéo Animator nếu muốn, hoặc để tự tìm
//    public EnemyAI2D_Animator enemyAI;                // <<< kiểu cụ thể, kéo thả dễ

//    Rigidbody2D rb;
//    float lastHitTime = -999f;
//    bool dead = false;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        if (!animator) animator = GetComponentInChildren<Animator>();

//        // Tự tìm nếu bạn quên kéo:
//        if (!enemyAI) enemyAI = GetComponent<EnemyAI2D_Animator>()
//                         ?? GetComponentInChildren<EnemyAI2D_Animator>();
//        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
//    }

//    public void TakeDamage(int damage, Vector2 hitFromWorldPos)
//    {
//        Debug.Log($"[EnemyHP] {name} TAKE {damage} from {hitFromWorldPos} (hp {currentHealth}->{Mathf.Max(0, currentHealth - damage)})");

//        if (dead) { Debug.Log("[EnemyHP] already dead"); return; }
//        if (Time.time - lastHitTime < invulnTime) { Debug.Log("[EnemyHP] invuln"); return; }

//        lastHitTime = Time.time;
//        currentHealth = Mathf.Max(0, currentHealth - damage);

//        Vector2 dir = ((Vector2)transform.position - hitFromWorldPos).normalized;
//        rb.velocity = new Vector2(dir.x * knockbackForce, rb.velocity.y);

//        if (animator) { Debug.Log("[EnemyHP] Trigger HIT"); animator.SetTrigger("Hit"); }

//        if (currentHealth <= 0)
//        {
//            dead = true;
//            if (enemyAI) enemyAI.enabled = false;
//            if (animator) { Debug.Log("[EnemyHP] Trigger DEATH"); animator.SetTrigger("Death"); }
//            StartCoroutine(CoDeathCleanup());
//        }
//        else
//        {
//            if (enemyAI) { Debug.Log($"[EnemyHP] STUN {stunTime}s (AI off)"); StartCoroutine(CoStun(stunTime)); }
//        }
//    }

//    IEnumerator CoStun(float t)
//    {
//        if (enemyAI) enemyAI.enabled = false;
//        yield return new WaitForSeconds(t);
//        if (!dead && enemyAI) enemyAI.enabled = true;
//    }

//    IEnumerator CoDeathCleanup()
//    {
//        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
//        yield return new WaitForSeconds(2f);
//        Destroy(gameObject);
//    }

//    public void OnDeathCleanupEvent()
//    {
//        foreach (var c in GetComponentsInChildren<Collider2D>()) c.enabled = false;
//        Destroy(gameObject, 2f);
//    }
//}
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyStats_IDamageable : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnDeath;

    [Header("HP")]
    public int maxHealth = 60;
    public int currentHealth = 60;

    [Header("Hit Reaction")]
    public float invulnTime = 0.15f;
    public float knockbackForce = 4.5f;
    public float stunTime = 0.2f;

    [Header("Refs")]
    public Animator animator;
    public EnemyAI2D_Animator enemyAI;

    [Header("Death Timing")]
    public float deathDelay = 1.0f; // phát Death rồi chờ 1s -> Destroy

    Rigidbody2D rb;
    float lastHitTime = -999f;
    bool dead = false;

    // lưu trạng thái rigidbody để (nếu cần) restore
    RigidbodyType2D _origBodyType;
    float _origGravity;
    RigidbodyConstraints2D _origConstraints;

    public bool IsDead => dead;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!enemyAI) enemyAI = GetComponent<EnemyAI2D_Animator>() ?? GetComponentInChildren<EnemyAI2D_Animator>();

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        _origBodyType = rb.bodyType;
        _origGravity = rb.gravityScale;
        _origConstraints = rb.constraints;
    }

    public void TakeDamage(int damage, Vector2 hitFromWorldPos)
    {
        if (dead) return;
        if (Time.time - lastHitTime < invulnTime) return;

        lastHitTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - damage);

        // knockback ngang
        Vector2 dir = ((Vector2)transform.position - hitFromWorldPos).normalized;
        rb.velocity = new Vector2(dir.x * knockbackForce, rb.velocity.y);

        if (currentHealth > 0)
        {
            animator?.SetTrigger("Hit");
            if (enemyAI) StartCoroutine(CoStun(stunTime));
        }
        else
        {
            StartCoroutine(CoDieSequence());
        }
    }

    IEnumerator CoStun(float t)
    {
        if (enemyAI) enemyAI.enabled = false;
        yield return new WaitForSeconds(t);
        if (!dead && enemyAI) enemyAI.enabled = true;
    }

    IEnumerator CoDieSequence()
    {
        if (dead) yield break;
        dead = true;

        OnDeath?.Invoke();

        // Tắt AI & dừng mọi chuyển động
        if (enemyAI) enemyAI.enabled = false;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // ❗ NGỪNG PHYSICS để không rơi xuyên đất
        // Cách 1 (an toàn, vẫn giữ collider để render/trigger event khác nếu cần):
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // (Tuỳ chọn) đảm bảo animator không dịch chuyển root
        if (animator) animator.applyRootMotion = false;

        // Phát animation Death
        animator?.SetTrigger("Death");

        // Chờ đúng 1 giây (hoặc theo deathDelay)
        yield return new WaitForSeconds(Mathf.Max(0f, deathDelay));

        // Giờ mới huỷ — nếu có LootDropper2D(trigger OnDestroy) nó sẽ drop tại đây
        Destroy(gameObject);
    }

    // Nếu dùng Animation Event cuối clip Death -> gọi hàm này thay vì đợi deathDelay
    public void OnDeathCleanupEvent()
    {
        if (!dead) return;
        Destroy(gameObject);
    }
}
