//using UnityEngine;

//[RequireComponent(typeof(Collider2D))]
//public class EnemyAttackHitbox : MonoBehaviour
//{
//    [Header("Damage To Player")]
//    public int damage = 12;
//    public float knockbackToPlayer = 6f;
//    public float stunToPlayer = 0.15f;

//    [Header("Filters")]
//    public string playerTag = "Player";     // tag Player
//    public LayerMask playerLayers;          // =0 thì bỏ check layer

//    Collider2D col;

//    void Awake()
//    {
//        col = GetComponent<Collider2D>();
//        col.isTrigger = true;
//        col.enabled = false; // Bật/tắt bằng Animation Event
//        Debug.Log($"[Hitbox/Awake] {name} trigger={col.isTrigger} enabled={col.enabled} layer={gameObject.layer}");
//    }

//    // ===== Animation Events =====
//    public void EnableHitbox()
//    {
//        if (!col) { Debug.LogWarning("[Hitbox] Collider missing"); return; }
//        col.enabled = true;
//        Debug.Log("[Hitbox] ENABLE");
//    }

//    public void DisableHitbox()
//    {
//        if (!col) { Debug.LogWarning("[Hitbox] Collider missing"); return; }
//        col.enabled = false;
//        Debug.Log("[Hitbox] DISABLE");
//    }

//    void OnTriggerEnter2D(Collider2D other)
//    {
//        if (!col.enabled)
//        {
//            Debug.Log($"[Hitbox] OnTriggerEnter2D while disabled with {other.name} -> ignore");
//            return;
//        }

//        Debug.Log($"[Hitbox] ENTER -> {other.name} (layer={other.gameObject.layer}, tag={other.tag})");

//        // Layer filter (optional)
//        if (playerLayers != 0 && (playerLayers.value & (1 << other.gameObject.layer)) == 0)
//        {
//            Debug.Log("[Hitbox] Layer filtered out -> ignore");
//            return;
//        }

//        // Tag filter
//        if (!other.CompareTag(playerTag))
//        {
//            Debug.Log("[Hitbox] Tag mismatch -> ignore");
//            return;
//        }

//        // Tìm component có TakeDamage
//        var target = other.GetComponentInParent<Component>() ?? (Component)other;
//        if (!target)
//        {
//            Debug.Log("[Hitbox] No component on target -> ignore");
//            return;
//        }

//        // Ưu tiên chữ ký đầy đủ
//        var mFull = target.GetType().GetMethod("TakeDamage",
//            new System.Type[] { typeof(int), typeof(Vector2), typeof(float), typeof(float) });
//        if (mFull != null)
//        {
//            Debug.Log($"[Hitbox] CALL {target.GetType().Name}.TakeDamage(int,Vector2,float,float) dmg={damage}");
//            mFull.Invoke(target, new object[] { damage, (Vector2)transform.position, knockbackToPlayer, stunToPlayer });
//            return;
//        }

//        // Fallback: chỉ có int
//        var mSimple = target.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
//        if (mSimple != null)
//        {
//            Debug.Log($"[Hitbox] CALL {target.GetType().Name}.TakeDamage(int) dmg={damage}");
//            mSimple.Invoke(target, new object[] { damage });
//            return;
//        }

//        Debug.LogWarning("[Hitbox] Player không có TakeDamage phù hợp. Hãy thêm:\n" +
//                         "public void TakeDamage(int dmg, Vector2 from, float kb, float stun) { ... }");
//    }

//    // ===== Optional: giữ log khi đang chồng hitbox =====
//    void OnTriggerStay2D(Collider2D other)
//    {
//        // Uncomment nếu cần soi “đang chồng”
//        // if (col.enabled && other.CompareTag(playerTag)) Debug.Log("[Hitbox] STAY with " + other.name);
//    }

//#if UNITY_EDITOR
//    // Vẽ vùng hitbox khi bật
//    void OnDrawGizmos()
//    {
//        var c = GetComponent<Collider2D>();
//        if (!c) return;
//        Gizmos.color = (Application.isPlaying && c.enabled) ? Color.red : new Color(1,0,0,0.25f);
//        if (c is BoxCollider2D b)
//        {
//            var t = b.transform;
//            var pos = (Vector2)t.TransformPoint(b.offset);
//            var size = Vector2.Scale(b.size, t.lossyScale);
//            Gizmos.DrawWireCube(pos, size);
//        }
//    }
//#endif
//}
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int dmg, Vector2 from, float knockback, float stun);
}

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage To Player")]
    public int damage = 12;
    public float knockbackToPlayer = 6f;
    public float stunToPlayer = 0.15f;

    [Header("Filters")]
    public string playerTag = "Player";   // Tag của Player
    public LayerMask playerLayers;        // =0 thì bỏ lọc layer

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false; // bật/tắt bằng Animation Event
        Debug.Log($"[Hitbox/Awake] {name} trigger={col.isTrigger} enabled={col.enabled}");
    }

    // ==== Animation Events ====
    public void EnableHitbox()
    {
        if (!col) return;
        col.enabled = true;
        Debug.Log("[Hitbox] WINDOW OPEN (collider enabled)");
    }

    public void DisableHitbox()
    {
        if (!col) return;
        col.enabled = false;
        Debug.Log("[Hitbox] WINDOW CLOSE (collider disabled)");
    }

    void TryDamage(Collider2D other)
    {
        if (!col.enabled) return;

        // 1) Lọc layer (nếu đã set)
        if (playerLayers != 0 && (playerLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            // Debug.Log("[Hitbox] layer filtered");
            return;
        }

        // 2) Lọc tag (nên để tag ở root Player, còn hurtbox là child)
        if (!other.CompareTag(playerTag) && !(other.attachedRigidbody && other.attachedRigidbody.CompareTag(playerTag)))
        {
            // Debug.Log("[Hitbox] tag mismatch");
            return;
        }

        // 3) Lấy ROOT của player (nếu va vào child collider)
        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform;

        // 4) Ưu tiên gọi PlayerStats (để HUD sync), sau đó BaseStats, cuối cùng IDamageable
        var ps = root.GetComponent<PlayerStats>();
        if (ps != null)
        {
            // Cần có overload này trong PlayerStats:
            // public void TakeDamage(int dmg, Vector2 from, float knockback, float stun) { TakeDamage(dmg); ... }
            ps.TakeDamage(damage, (Vector2)transform.position, knockbackToPlayer, stunToPlayer);
            // Debug.Log("[Hitbox] -> PlayerStats.TakeDamage(...)");
            return;
        }

        var bs = root.GetComponent<BaseStats>();
        if (bs != null)
        {
            // nếu chỉ có hàm float/int thì gọi cái có sẵn
            bs.TakeDamage((float)damage);
            // Debug.Log("[Hitbox] -> BaseStats.TakeDamage(float)");
            return;
        }

        var id = root.GetComponent<IDamageable>();
        if (id != null)
        {
            id.TakeDamage(damage, (Vector2)transform.position, knockbackToPlayer, stunToPlayer);
            // Debug.Log("[Hitbox] -> IDamageable.TakeDamage(...)");
            return;
        }

        Debug.LogWarning($"[Hitbox] Không tìm thấy script nhận dame trên {root.name}. Hãy gắn PlayerStats/BaseStats/IDamageable.");
    }

    void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    // Nếu muốn mỗi đòn chỉ tính 1 lần, giữ nguyên như trên (Enter đủ).
    // Nếu muốn “găm” sát thương trong suốt cửa sổ đòn, có thể bật Stay:
    // void OnTriggerStay2D(Collider2D other) => TryDamage(other);

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var c = GetComponent<Collider2D>();
        if (!c) return;
        Gizmos.color = (Application.isPlaying && c.enabled) ? Color.red : new Color(1,0,0,0.25f);
        if (c is BoxCollider2D b)
        {
            var t = b.transform;
            var pos = (Vector2)t.TransformPoint(b.offset);
            var size = Vector2.Scale(b.size, t.lossyScale);
            Gizmos.DrawWireCube(pos, size);
        }
    }
#endif
}
