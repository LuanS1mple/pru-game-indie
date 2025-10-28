using UnityEngine;

namespace Game.Enemy
{
    public class MiniBossStats : MonoBehaviour
    {
        [Header("HP Settings")]
        public int maxHP = 10;
        [HideInInspector] public int currentHP;

        [Header("Combat Stats")]
        public int attack = 5;
        public int defense = 2;

        // Called from MiniBossHealth.Start() or manually in inspector via script
        public void Init()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(int damage)
        {
            // you could factor in defense here if you want: damage = Mathf.Max(0, damage - defense);
            currentHP = Mathf.Max(0, currentHP - damage);
        }

        public bool IsDead() => currentHP <= 0;

        // Optional helpers
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
