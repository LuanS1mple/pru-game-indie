using UnityEngine;

namespace Game.Enemy
{
    public class Monster3Stats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 8;
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 3;
        public int defense = 2;

        // Khởi tạo khi spawn hoặc bắt đầu game
        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
            // Giảm damage theo chỉ số phòng thủ
            damage = Mathf.Max(0, damage - defense);
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        public bool IsDead() => currentHP <= 0;

        // Hồi máu
        public void Heal(int amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }

        // Cập nhật lại maxHP
        public void SetMaxHP(int newMax)
        {
            maxHP = Mathf.Max(1, newMax);
            currentHP = Mathf.Min(currentHP, maxHP);
        }
    }
}
