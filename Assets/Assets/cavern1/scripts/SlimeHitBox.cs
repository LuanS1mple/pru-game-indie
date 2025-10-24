using UnityEngine;

public class SlimeHitBox : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damage = 10f; // Gán khác nhau tùy loại slime

    private bool canDamage = false;
    private Collider2D hitCollider;

    private void Awake()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider == null)
        {
            Debug.LogError("Không tìm thấy Collider2D trên SlimeHitBox!");
            return;
        }

        hitCollider.enabled = false;
    }

    // Gọi từ Animation Event khi slime bắt đầu tấn công
    public void EnableDamage()
    {
        canDamage = true;
        hitCollider.enabled = true;
        Debug.Log($"{name}: HitBox Enabled");
    }

    // Gọi từ Animation Event khi slime kết thúc tấn công
    public void DisableDamage()
    {
        canDamage = false;
        hitCollider.enabled = false;
        Debug.Log($"{name}: HitBox Disabled");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log($"{name} hit Player!");

            BaseStats playerStats = other.GetComponent<BaseStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
                Debug.Log($"Player nhận {damage} sát thương từ {name}!");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy BaseStats trên Player!");
            }

            // Gây sát thương một lần mỗi đòn
            canDamage = false;
            hitCollider.enabled = false;
        }
    }
}
