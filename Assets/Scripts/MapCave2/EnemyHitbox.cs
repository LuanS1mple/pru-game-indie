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

        Debug.Log($"🧩 [EnemyHitbox] {name} va chạm với {other.name} lúc {Time.time:F2}");

        // ✅ Lấy BaseStats và PlayerStats từ Player
        BaseStats bs = other.GetComponentInParent<BaseStats>();
        PlayerStats ps = other.GetComponentInParent<PlayerStats>();

        if (bs == null)
        {
            Debug.LogWarning($"⚠️ [EnemyHitbox] Không tìm thấy BaseStats trên {other.name} hoặc cha của nó!");
            return;
        }

        float beforeHP = bs.currentHP;
        bool wasDeadBefore = bs.isDead;

        Debug.Log($"[EnemyHitbox] 💢 Gây damage {damage} cho {bs.entityName} (trước: {beforeHP}/{bs.maxHP}, dead={wasDeadBefore})");

        // --- Gây damage ---
        bs.TakeDamage(damage);

        float afterHP = bs.currentHP;
        bool isDeadNow = bs.isDead;

        Debug.Log($"[EnemyHitbox] 📊 Sau khi trừ damage: currentHP={afterHP}/{bs.maxHP}, isDead={isDeadNow}");

        // --- HUD cập nhật ---
        if (ps != null)
        {
            if (!isDeadNow)
            {
                ps.TakeDamage(damage);
                Debug.Log($"[EnemyHitbox] ❤️ HUD cập nhật: Player còn {ps.CurrentHealth}/{ps.MaxHealth}");
            }
            else
            {
                ps.TakeDamage(ps.CurrentHealth);
                Debug.Log($"💀 [EnemyHitbox] Player đã chết → HUD về 0");
            }
        }

        // --- Xác nhận Player chết thực sự ---
        if (isDeadNow && !wasDeadBefore)
        {
            Debug.Log($"☠️ [EnemyHitbox] Player {bs.entityName} CHẾT tại thời điểm {Time.time:F2}");
        }
        else if (!isDeadNow)
        {
            Debug.Log($"🩸 [EnemyHitbox] Player {bs.entityName} vẫn sống sau đòn đánh này.");
        }
    }
}
