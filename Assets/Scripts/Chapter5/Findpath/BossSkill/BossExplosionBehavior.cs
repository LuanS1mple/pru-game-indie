using UnityEngine;
using System.Collections.Generic;

public class BossExplosionBehavior : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float damageAmount = 75f; // Sát thương cố định
    public float duration = 0.7f;    // Thời gian tồn tại
    [HideInInspector] public LayerMask targetLayer;

    private const float FixedAreaSize = 2.5f;

    private Collider2D explosionCollider;
    private HashSet<BaseStats> hitTargets = new HashSet<BaseStats>();

    void Awake()
    {
        explosionCollider = GetComponent<Collider2D>();
        if (explosionCollider != null)
            explosionCollider.isTrigger = true;

        // Cấu hình kích thước collider bằng giá trị cố định
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.radius = FixedAreaSize;
        }

        // 🔥 Tự hủy sau khi hết thời gian hiệu lực
        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) == 0)
            return;

        BaseStats targetStats = other.GetComponentInParent<BaseStats>();
        if (targetStats != null && !hitTargets.Contains(targetStats))
        {
            targetStats.TakeDamage(damageAmount);
            hitTargets.Add(targetStats);
            Debug.Log($"💥 Vòng xoáy gây {damageAmount} sát thương lên {targetStats.entityName}.");

            // Không cần clear vì prefab sẽ bị hủy ngay sau duration
            // Nhưng nếu bạn muốn tái sử dụng prefab lâu hơn, vẫn có thể giữ lại:
            // StartCoroutine(ClearHitTarget(targetStats, 0.5f));
        }
    }

    private System.Collections.IEnumerator ClearHitTarget(BaseStats stats, float delay)
    {
        yield return new WaitForSeconds(delay);
        hitTargets.Remove(stats);
    }
}
