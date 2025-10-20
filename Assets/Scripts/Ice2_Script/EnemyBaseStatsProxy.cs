using UnityEngine;

// GẮN script này lên đúng GameObject có Collider bị Player chém (Hurtbox).
// GameObject đó phải Tag = "Enemy" để khớp PlayerCombat.
public class EnemyBaseStatsProxy : BaseStats
{
    [Header("Forward Target (HP thật ở root)")]
    public EnemyStats_IDamageable target;        // kéo script HP thật (ở root enemy) vào đây
    public bool autoFindTargetInParent = true;   // nếu không kéo thì auto tìm ở parent

    [Header("Behaviour")]
    [Tooltip("Nếu tick, vừa forward, vừa trừ HP local của BaseStats. Thường để OFF để tránh trừ đôi.")]
    public bool alsoApplyLocalHP = false;

    protected override void Awake()
    {
        base.Awake(); // gọi Awake của BaseStats (khởi tạo currentHP = maxHP)
        if (!target && autoFindTargetInParent)
            target = GetComponentInParent<EnemyStats_IDamageable>();
#if UNITY_EDITOR
        if (!target)
            Debug.LogWarning($"[EnemyBaseStatsProxy] Không tìm thấy EnemyStats_IDamageable ở parent của {name}");
#endif
    }

    // OVERRIDE ĐÚNG CHỮ KÝ float
    public override void TakeDamage(float damage)
    {
        int dmgInt = Mathf.Max(1, Mathf.RoundToInt(damage));
        Vector2 hitFrom = (Vector2)transform.position;

        Debug.Log($"[Proxy] {name} TakeDamage float={damage} -> int={dmgInt}, target={(target ? target.name : "null")}");

        if (target)
        {
            target.TakeDamage(dmgInt, hitFrom);
            // KHÔNG gọi base để tránh trừ HP kép
            return;
        }

        Debug.LogWarning("[Proxy] target NULL → fallback base.TakeDamage (HP local)");
        base.TakeDamage(damage);
    }

}

