using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public event Action<int, int> OnHealthChanged; // (current,max)
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth == 0 ? maxHealth : currentHealth, 0, maxHealth);
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void IncreaseMaxHealth(int amount, bool healToFull = true)
    {
        if (amount <= 0) return;
        maxHealth = Mathf.Max(1, maxHealth + amount);
        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth == 0) OnDied?.Invoke();
    }
}
