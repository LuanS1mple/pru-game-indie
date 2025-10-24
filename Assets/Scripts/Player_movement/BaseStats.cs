using UnityEngine;

public class BaseStats : MonoBehaviour
{
    [Header("Basic Stats")]
    public string entityName = "Unknown";
    public float maxHP = 100f;
    public float currentHP;
    public float attack = 15f;
    public float defense = 5f;
    public bool isInvulnerable = false; // 👈 dùng cho roll
    public bool isDead => currentHP <= 0;
    public float attackCooldown = 0.3f;
    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isInvulnerable)
        {
            Debug.Log($"{entityName} đang bất tử (roll) → không nhận sát thương!");
            return;
        }

        float finalDamage = Mathf.Max(1, damage - defense);
        currentHP -= finalDamage;

        Debug.Log($"{entityName} nhận {finalDamage} sát thương! (Còn {currentHP})");

        if (currentHP <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"{entityName} đã chết!");
    }
    public void TakeDamage(int damage) => TakeDamage((float)damage);

    public void TakeDamage(int damage, Vector2 from, float kb, float stun)
        => TakeDamage(damage);
}
