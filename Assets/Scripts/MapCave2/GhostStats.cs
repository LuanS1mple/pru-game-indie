using UnityEngine;

namespace Game.Enemy
{
    public class GhostStats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 3;
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 1;
        public int defense = 0;

        // Gọi khi spawn
        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
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
