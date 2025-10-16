using UnityEngine;

public class BaseStats : MonoBehaviour
{
    [Header("Basic Stats")]
    public string entityName = "Unknown";
    public float maxHP = 100f;
    public float currentHP;
    public float attack = 15f;
    public float defense = 5f;

    public bool isDead => currentHP <= 0;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(float damage)
    {
        float finalDamage = Mathf.Max(1, damage - defense);
        currentHP -= finalDamage;

        Debug.Log($"{entityName} nhận {finalDamage} sát thương! (Còn {currentHP})");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{entityName} đã chết!");
    }
}
