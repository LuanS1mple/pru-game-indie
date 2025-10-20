using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    [Header("Damage To Player")]
    public int damage = 12;
    public float knockbackToPlayer = 6f;
    public float stunToPlayer = 0.15f;

    [Header("Filters")]
    public string playerTag = "Player";     // tag Player
    public LayerMask playerLayers;          // =0 thì bỏ check layer

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false; // Bật/tắt bằng Animation Event
        Debug.Log($"[Hitbox/Awake] {name} trigger={col.isTrigger} enabled={col.enabled} layer={gameObject.layer}");
    }

    // ===== Animation Events =====
    public void EnableHitbox()
    {
        if (!col) { Debug.LogWarning("[Hitbox] Collider missing"); return; }
        col.enabled = true;
        Debug.Log("[Hitbox] ENABLE");
    }

    public void DisableHitbox()
    {
        if (!col) { Debug.LogWarning("[Hitbox] Collider missing"); return; }
        col.enabled = false;
        Debug.Log("[Hitbox] DISABLE");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!col.enabled)
        {
            Debug.Log($"[Hitbox] OnTriggerEnter2D while disabled with {other.name} -> ignore");
            return;
        }

        Debug.Log($"[Hitbox] ENTER -> {other.name} (layer={other.gameObject.layer}, tag={other.tag})");

        // Layer filter (optional)
        if (playerLayers != 0 && (playerLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            Debug.Log("[Hitbox] Layer filtered out -> ignore");
            return;
        }

        // Tag filter
        if (!other.CompareTag(playerTag))
        {
            Debug.Log("[Hitbox] Tag mismatch -> ignore");
            return;
        }

        // Tìm component có TakeDamage
        var target = other.GetComponentInParent<Component>() ?? (Component)other;
        if (!target)
        {
            Debug.Log("[Hitbox] No component on target -> ignore");
            return;
        }

        // Ưu tiên chữ ký đầy đủ
        var mFull = target.GetType().GetMethod("TakeDamage",
            new System.Type[] { typeof(int), typeof(Vector2), typeof(float), typeof(float) });
        if (mFull != null)
        {
            Debug.Log($"[Hitbox] CALL {target.GetType().Name}.TakeDamage(int,Vector2,float,float) dmg={damage}");
            mFull.Invoke(target, new object[] { damage, (Vector2)transform.position, knockbackToPlayer, stunToPlayer });
            return;
        }

        // Fallback: chỉ có int
        var mSimple = target.GetType().GetMethod("TakeDamage", new System.Type[] { typeof(int) });
        if (mSimple != null)
        {
            Debug.Log($"[Hitbox] CALL {target.GetType().Name}.TakeDamage(int) dmg={damage}");
            mSimple.Invoke(target, new object[] { damage });
            return;
        }

        Debug.LogWarning("[Hitbox] Player không có TakeDamage phù hợp. Hãy thêm:\n" +
                         "public void TakeDamage(int dmg, Vector2 from, float kb, float stun) { ... }");
    }

    // ===== Optional: giữ log khi đang chồng hitbox =====
    void OnTriggerStay2D(Collider2D other)
    {
        // Uncomment nếu cần soi “đang chồng”
        // if (col.enabled && other.CompareTag(playerTag)) Debug.Log("[Hitbox] STAY with " + other.name);
    }

#if UNITY_EDITOR
    // Vẽ vùng hitbox khi bật
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
