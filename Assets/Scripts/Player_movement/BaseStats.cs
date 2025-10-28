using UnityEngine;

public class BaseStats : MonoBehaviour
{
    [Header("Basic Stats")]
    public string entityName = "Unknown";
    public float maxHP = 100f;
    public float currentHP;
    public float attack = 15f;
    public float defense = 5f;
    public bool isInvulnerable = false;
    public bool isDead => currentHP <= 0;
    public float attackCooldown = 0.3f;

    // ⭐ THÊM LẠI: Tham chiếu movement (chỉ Player mới có)
    private movement playerMovement;
    // Tham chiếu Animator (cho Enemy)
    private Animator anim;

    protected virtual void Awake()
    {
        currentHP = maxHP;
        // Cố gắng lấy movement
        playerMovement = GetComponent<movement>();
        // Lấy Animator (cho cả Player và Enemy)
        anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>(); // Thử tìm ở con nếu không thấy

        // Log cảnh báo nếu thiếu movement (nhưng không phải lỗi nghiêm trọng)
        if (GetComponent<movement>() != null && playerMovement == null)
        {
            Debug.LogWarning($"[{entityName}] Có script 'movement' nhưng không lấy được tham chiếu!", this.gameObject);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isInvulnerable || isDead) return;

        float finalDamage = Mathf.Max(1, damage - defense);
        currentHP -= finalDamage;
        Debug.Log($"{entityName} nhận {finalDamage} sát thương! (Còn {currentHP})");

        // ⭐ SỬA: Gọi animation Hit
        if (playerMovement != null) // Nếu là Player
        {
            playerMovement.PlayHitAnimation();
        }
        else if (anim != null) // Nếu là Enemy (hoặc Player không có script movement riêng)
        {
            // Dùng tên trigger "Hit" (chữ hoa) như EnemyPathFollower đang dùng
            anim.SetTrigger("Hit");
        }

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (currentHP > 0) currentHP = 0;
        Debug.Log($"{entityName} đã chết!");

        // ⭐ SỬA: Gọi animation Dead
        if (playerMovement != null) // Nếu là Player
        {
            playerMovement.HandleDeath();
        }
        else if (anim != null) // Nếu là Enemy
        {
            // Dùng tên trigger "Dead" (chữ hoa) như EnemyPathFollower đang dùng
            anim.SetTrigger("Dead");
            // Enemy nên có logic dừng riêng trong script của nó (EnemyPathFollower.Die())
            // Destroy(gameObject, 2.5f); // Ví dụ
        }
    }

    // Đặt hàm này gần hoặc sau hàm TakeDamage(float damage) hiện tại
    public virtual void TakeDamageTrap(float damage)
    {
        // 1. Kiểm tra trạng thái miễn nhiễm/chết trước
        if (isInvulnerable || isDead) return;

        // 2. Sát thương TRỰC TIẾP (Bỏ qua Defense)
        float finalDamage = damage;

        currentHP -= finalDamage;
        Debug.Log($"{entityName} nhận {finalDamage} sát thương Bẫy! (Còn {currentHP})");

        // 3. Gọi animation Hit
        if (playerMovement != null)
        {
            playerMovement.PlayHitAnimation();
        }
        else if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        // 4. Kiểm tra chết
        if (currentHP <= 0)
            Die();
    }

    public void TakeDamage(int damage) => TakeDamage((float)damage);
    public void TakeDamage(int damage, Vector2 from, float kb, float stun) => TakeDamage(damage);
}