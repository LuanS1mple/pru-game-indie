using UnityEngine;

public class BossGroundAOE : MonoBehaviour
{
    [Header("AOE Settings")]
    public float damage = 20f;               // Sát thương mỗi cú nhảy

    private Collider2D aoeCollider;
    private bool canDamage = false;

    private void Awake()
    {
        aoeCollider = GetComponent<Collider2D>();
        if (aoeCollider == null)
        {
            Debug.LogError("BossGroundAOE cần Collider2D!");
            return;
        }

        aoeCollider.enabled = false; // ban đầu tắt
    }

    // 🔥 Gọi khi Boss bắt đầu nhảy → bật collider
    public void ActivateAOE()
    {
        canDamage = true;
        if (aoeCollider != null)
            aoeCollider.enabled = true; // bật collider ngay

        Debug.Log("🦘 Boss nhảy → AOE collider bật, có thể gây damage");
    }

    // 🔥 Gọi khi Boss chạm đất → tắt collider
    public void DeactivateAOE()
    {
        canDamage = false;
        if (aoeCollider != null)
            aoeCollider.enabled = false; // tắt collider

        Debug.Log("💥 Boss chạm đất → AOE collider tắt");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return; // chỉ gây damage khi bật

        // Gây sát thương cho Player
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var stats = other.GetComponent<BaseStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
                Debug.Log($"[BossAOE] Player trúng Jump AOE! (-{damage})");
            }

            // Chỉ gây damage một lần mỗi cú
            canDamage = false;
            if (aoeCollider != null)
                aoeCollider.enabled = false;
        }
    }
}
