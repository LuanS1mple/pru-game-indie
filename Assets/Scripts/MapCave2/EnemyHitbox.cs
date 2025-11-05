using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float knockbackToPlayer = 6f;
    public float stunToPlayer = 0.15f;

    [Header("Cooldown Settings")]
    public float hitCooldown = 0.5f; // thời gian cho phép gây damage lại

    private float lastHitTime;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning($"⚠️ [Hitbox] Không tìm thấy Rigidbody2D trong cha của {name}. Va chạm có thể KHÔNG hoạt động!");
        else
            Debug.Log($"✅ [Hitbox] Rigidbody2D cha: {rb.gameObject.name}, type = {rb.bodyType}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        BaseStats bs = other.GetComponentInParent<BaseStats>();
        PlayerStats ps = other.GetComponentInParent<PlayerStats>();

        if (bs == null)
        {
            Debug.LogWarning($"⚠️ [EnemyHitbox] Không tìm thấy BaseStats trên {other.name} hoặc cha của nó!");
            return;
        }

        float oldHP = bs.currentHP;
        bool wasDeadBefore = bs.isDead;

        // --- Gây damage (BaseStats tự tính armor) ---
        bs.TakeDamage(damage);

        float newHP = bs.currentHP;
        float actualDamage = Mathf.Clamp(oldHP - newHP, 0, damage);
        bool isDeadNow = bs.isDead;

        Debug.Log($"[EnemyHitbox] 💢 Gây {actualDamage} damage thực tế cho {bs.entityName} (sau giáp), còn {newHP}/{bs.maxHP}");

        // --- Cập nhật HUD đúng ---
        if (ps != null)
        {
            ps.TakeDamage((int)actualDamage);
            Debug.Log($"❤️ [EnemyHitbox] HUD cập nhật: Player mất {actualDamage}, còn {ps.CurrentHealth}/{ps.MaxHealth}");
        }

        // --- Knockback (chỉ nếu có Rigidbody) ---
        Rigidbody2D playerRb = other.attachedRigidbody;
        if (playerRb != null)
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            playerRb.AddForce(dir * knockbackToPlayer, ForceMode2D.Impulse);
        }

        // --- Log trạng thái ---
        if (isDeadNow && !wasDeadBefore)
        {
            Debug.Log($"☠️ Player {bs.entityName} CHẾT tại thời điểm {Time.time:F2}");
        }
        else if (!isDeadNow)
        {
            Debug.Log($"🩸 Player {bs.entityName} sống sót sau đòn đánh này ({newHP}/{bs.maxHP})");
        }
    }
}
