using UnityEngine;
using System;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Link to REAL stats")]
    [SerializeField] private BaseStats baseStats;
    [SerializeField] private bool autoPullFromBase = true;
    [SerializeField] private float pullInterval = 0.05f;

    [Header("HUD values (mirrors)")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attack = 10;
    [SerializeField] private int defense = 5;

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnAttackChanged;
    public event Action<int, int> OnDefenseChanged;
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Attack => attack;
    public int Defense => defense;

    private int _lastPushedCur, _lastPushedMax;

    // cache để tránh spam event
    private int _lastPushedCur, _lastPushedMax;

    void Awake()
    {
        if (!baseStats) baseStats = GetComponent<BaseStats>();

        if (baseStats)
        {
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, maxHealth);
            attack = Mathf.RoundToInt(baseStats.attack);
            defense = Mathf.RoundToInt(baseStats.defense);
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth == 0 ? maxHealth : currentHealth, 0, maxHealth);
        }

        _lastPushedCur = currentHealth;
        _lastPushedMax = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (autoPullFromBase && baseStats)
            StartCoroutine(CoPullFromBase());
    }

    // ===================== PUBLIC API =====================
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        PushHudToBase();
        RaiseHealthChanged();
    }

    // Gọi khi nhặt item tăng máu tối đa
    public void IncreaseMaxHealth(int amount, bool healToFull = true)
    {
        if (amount == 0) return;

        maxHealth = Mathf.Max(1, maxHealth + amount);
        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);

        PushHudToBase();
        RaiseHealthChanged();
    }

    // Khi HUD bị trừ máu (ví dụ bạn chủ động gọi từ đâu đó)
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
        PushHudToBase();
        RaiseHealthChanged();

        if (currentHealth == 0) OnDied?.Invoke();
    }

    public void IncreaseAttack(int amount)
    {
        if (amount <= 0) return;
        attack += amount;
        PushHudToBase();
        OnAttackChanged?.Invoke(attack, defense);
    }

    public void IncreaseDefense(int amount)
    {
        if (amount <= 0) return;
        defense += amount;
        PushHudToBase();
        OnDefenseChanged?.Invoke(attack, defense);
    }

    public void DecreaseAttack(int amount)
    {
        if (amount <= 0) return;
        attack = Mathf.Max(0, attack - amount);
        PushHudToBase();
        OnAttackChanged?.Invoke(attack, defense);
    }

    public void DecreaseDefense(int amount)
    {
        if (amount <= 0) return;
        defense = Mathf.Max(0, defense - amount);
        PushHudToBase();
        OnDefenseChanged?.Invoke(attack, defense);
    }

    // ===================== SYNC HELPERS =====================
    private void PushHudToBase()
    {
        if (!baseStats) return;

        baseStats.maxHP = maxHealth;
        baseStats.currentHP = currentHealth;
        baseStats.attack = attack;
        baseStats.defense = defense;
    }

    private IEnumerator CoPullFromBase()
    {
        var wait = new WaitForSeconds(pullInterval);
        while (true)
        {
            if (baseStats)
            {
                int bMax = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
                int bCur = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, bMax);

                bool changed = false;

                if (bMax != maxHealth) { maxHealth = bMax; changed = true; }
                if (bCur != currentHealth) { currentHealth = bCur; changed = true; }

                if (changed) RaiseHealthChanged();
            }
            yield return wait;
        }
    }

    private void RaiseHealthChanged()
    {
        if (_lastPushedCur == currentHealth && _lastPushedMax == maxHealth) return;
        _lastPushedCur = currentHealth;
        _lastPushedMax = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

