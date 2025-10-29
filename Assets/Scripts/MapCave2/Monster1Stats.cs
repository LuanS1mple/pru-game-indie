using UnityEngine;

namespace Game.Enemy
{
    public class Monster1Stats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 5;
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 2;
        public int defense = 1;

        // Gọi khi bắt đầu hoặc spawn quái
        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
            // Trừ damage có tính phòng thủ
            damage = Mathf.Max(0, damage - defense);
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        public bool IsDead() => currentHP <= 0;

        // Hồi máu
        public void Heal(int amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }

        // Đặt lại maxHP mới
        public void SetMaxHP(int newMax)
        {
            maxHP = Mathf.Max(1, newMax);
            currentHP = Mathf.Min(currentHP, maxHP);
        }
    }
}
