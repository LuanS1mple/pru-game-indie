using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 10;
    public float knockbackToPlayer = 6f;
    public float stunToPlayer = 0.15f;

    private bool hasHit;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Debug kiểm tra Rigidbody cha
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        if (rb == null)
            Debug.LogWarning($"⚠️ [Hitbox] Không tìm thấy Rigidbody2D trong cha của {name}. Va chạm có thể KHÔNG hoạt động!");
        else
            Debug.Log($"✅ [Hitbox] Rigidbody2D cha: {rb.gameObject.name}, type = {rb.bodyType}");
    }

    private void OnEnable() => hasHit = false;
    private void OnDisable() => hasHit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ xử lý nếu đối tượng có tag Player
        if (!other.CompareTag("Player")) return;
        if (hasHit) return;

        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;
        Debug.Log($"[Hitbox] 💥 Va chạm với Player: {root.name}");

        // Gây damage qua PlayerStats nếu có
        PlayerStats ps = root.GetComponent<PlayerStats>();
        if (ps != null)
        {
            hasHit = true;
            ps.TakeDamage(damage, (Vector2)transform.position, knockbackToPlayer, stunToPlayer);
            Debug.Log($"💥 [Hitbox] Gây {damage} damage (PlayerStats)");
            return;
        }

        // Hoặc BaseStats nếu có
        BaseStats bs = root.GetComponent<BaseStats>();
        if (bs != null)
        {
            hasHit = true;
            bs.TakeDamage(damage);
            Debug.Log($"💥 [Hitbox] Gây {damage} damage (BaseStats)");
            return;
        }

        Debug.LogWarning($"⚠️ [Hitbox] Player không có script nhận damage trên {root.name}");
    }
}
