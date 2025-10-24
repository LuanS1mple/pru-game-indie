using UnityEngine;

public class BossHitBox : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 25f; // Sát thương gây ra mỗi cú đánh

    private bool canDamage = false; // Trạng thái có thể gây sát thương
    private Collider2D hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
        {
            Debug.LogError("Không tìm thấy Collider2D trên BossHitBox!");
            return;
        }

        // Collider chỉ bật khi Boss đang ra đòn
        hitCollider.enabled = false;
    }

    // Gọi trong AnimationEvent khi Boss bắt đầu chém
    public void EnableDamage()
    {
        canDamage = true;
        hitCollider.enabled = true;
        Debug.Log("Boss HitBox Enabled");
    }

    // Gọi trong AnimationEvent khi Boss kết thúc cú chém
    public void DisableDamage()
    {
        canDamage = false;
        hitCollider.enabled = false;
        Debug.Log("Boss HitBox Disabled");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return; // Chỉ xử lý khi có thể gây sát thương

        // Kiểm tra layer Player (không cần tag)
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Boss hit Player!");

            // Gây sát thương cho Player
            BaseStats playerStats = other.GetComponent<BaseStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"Player nhận {damage} sát thương từ Boss!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy BaseStats trên Player!");
            }

            // Chỉ gây sát thương một lần mỗi cú đánh
            canDamage = false;
            hitCollider.enabled = false;
        }
    }
}
