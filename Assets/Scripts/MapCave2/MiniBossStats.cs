using UnityEngine;

[System.Serializable]
public class MiniBossStats : MonoBehaviour
{
    [Header("MiniBoss Stats")]
    public int maxHP = 100;
    [HideInInspector] public int currentHP;

    void Awake()
    {
        currentHP = maxHP;
    }

    // 📉 Giảm máu
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
    }

    // ❤️ Hồi máu
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    // ☠ Kiểm tra chết
    public bool IsDead => currentHP <= 0;
}
