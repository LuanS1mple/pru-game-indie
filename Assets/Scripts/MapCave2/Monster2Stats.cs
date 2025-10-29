using UnityEngine;

namespace Game.Enemy
{
    public class Monster2Stats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 15; // mạnh hơn Monster1
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 4;
        public int defense = 2;

        // Gọi khi bắt đầu hoặc khi spawn quái
        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
            // Trừ theo chỉ số phòng thủ
            damage = Mathf.Max(0, damage - defense);
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        public bool IsDead() => currentHP <= 0;

        // Hồi máu
        public void Heal(int amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }

        // Cập nhật HP tối đa
        public void SetMaxHP(int newMax)
        {
            maxHP = Mathf.Max(1, newMax);
            currentHP = Mathf.Min(currentHP, maxHP);
        }
    }
}
