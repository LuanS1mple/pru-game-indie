using UnityEngine;

namespace Game.Enemy
{
    public class MonsterSStats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 5;
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 2;
        public int defense = 1;

        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
            // Tính toán damage có thể dựa vào defense
            damage = Mathf.Max(0, damage - defense);
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        public bool IsDead() => currentHP <= 0;

        public void Heal(int amount)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }

        public void SetMaxHP(int newMax)
        {
            maxHP = Mathf.Max(1, newMax);
            currentHP = Mathf.Min(currentHP, maxHP);
        }
    }
}
