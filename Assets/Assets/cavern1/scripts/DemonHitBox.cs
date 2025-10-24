using UnityEngine;

public class DemonHitBox : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 15f; // Sát thương mỗi đòn tấn công

    private bool canDamage = false; // Trạng thái có thể gây sát thương
    private Collider2D hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
        {
            Debug.LogError("Không tìm thấy Collider2D trên DemonHitBox!");
            return;
        }

        // Collider chỉ bật khi Demon đang ra đòn
        hitCollider.enabled = false;
    }

    // Gọi trong Animation Event khi Demon bắt đầu tấn công
    public void EnableDamage()
    {
        canDamage = true;
        hitCollider.enabled = true;
        Debug.Log("Demon HitBox Enabled");
    }

    // Gọi trong Animation Event khi Demon kết thúc tấn công
    public void DisableDamage()
    {
        canDamage = false;
        hitCollider.enabled = false;
        Debug.Log("Demon HitBox Disabled");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        // Kiểm tra layer của Player thay vì tag
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("🔥 Demon hit Player!");

            BaseStats playerStats = other.GetComponent<BaseStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"Player nhận {damage} sát thương từ Demon!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy BaseStats trên Player!");
            }

            // Chỉ gây sát thương một lần mỗi đòn đánh
            canDamage = false;
            hitCollider.enabled = false;
        }
    }
}
