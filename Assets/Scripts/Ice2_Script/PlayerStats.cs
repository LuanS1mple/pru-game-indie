//using UnityEngine;
//using System;
//using System.Collections;

//public class PlayerStats : MonoBehaviour
//{
//    [Header("Link to REAL stats")]
//    [SerializeField] private BaseStats baseStats;
//    [SerializeField] private bool autoPullFromBase = true;
//    [SerializeField] private float pullInterval = 0.05f;

//    [Header("HUD values (mirrors)")]
//    [SerializeField] private int maxHealth = 100;
//    [SerializeField] private int currentHealth;
//    [SerializeField] private int attack = 10;
//    [SerializeField] private int defense = 5;

//    public event Action<int, int> OnHealthChanged;
//    public event Action<int, int> OnAttackChanged;
//    public event Action<int, int> OnDefenseChanged;
//    public event Action OnDied;

//    public int CurrentHealth => currentHealth;
//    public int MaxHealth => maxHealth;
//    public int Attack => attack;
//    public int Defense => defense;



//    // cache để tránh spam event
//    private int _lastPushedCur, _lastPushedMax;

//    void Awake()
//    {
//        if (!baseStats) baseStats = GetComponent<BaseStats>();

//        if (baseStats)
//        {
//            maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
//            currentHealth = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, maxHealth);
//            attack = Mathf.RoundToInt(baseStats.attack);
//            defense = Mathf.RoundToInt(baseStats.defense);
//        }
//        else
//        {
//            currentHealth = Mathf.Clamp(currentHealth == 0 ? maxHealth : currentHealth, 0, maxHealth);
//        }

//        _lastPushedCur = currentHealth;
//        _lastPushedMax = maxHealth;
//    }

//    void Start()
//    {
//        OnHealthChanged?.Invoke(currentHealth, maxHealth);
//        if (autoPullFromBase && baseStats)
//            StartCoroutine(CoPullFromBase());
//    }

//    // ===================== PUBLIC API =====================
//    public void Heal(int amount)
//    {
//        if (amount <= 0) return;
//        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
//        PushHudToBase();
//        RaiseHealthChanged();
//    }

//    // Gọi khi nhặt item tăng máu tối đa
//    public void IncreaseMaxHealth(int amount, bool healToFull = true)
//    {
//        if (amount == 0) return;

//        maxHealth = Mathf.Max(1, maxHealth + amount);
//        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);

//        PushHudToBase();
//        RaiseHealthChanged();
//    }
//    // Giảm Max HP, có thể kẹp current về max mới
//    public void DecreaseMaxHealth(int amount, bool clampCurrent = true)
//    {
//        if (amount <= 0) return;

//        maxHealth = Mathf.Max(1, maxHealth - amount);

//        if (clampCurrent)
//            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

//        PushHudToBase();
//        RaiseHealthChanged();
//    }

//    // Khi HUD bị trừ máu (ví dụ bạn chủ động gọi từ đâu đó)
//    public void TakeDamage(int dmg)
//    {
//        if (dmg <= 0) return;

//        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
//        PushHudToBase();
//        RaiseHealthChanged();

//        if (currentHealth == 0) OnDied?.Invoke();
//    }

//    public void IncreaseAttack(int amount)
//    {
//        if (amount <= 0) return;
//        attack += amount;
//        PushHudToBase();
//        OnAttackChanged?.Invoke(attack, defense);
//    }

//    public void IncreaseDefense(int amount)
//    {
//        if (amount <= 0) return;
//        defense += amount;
//        PushHudToBase();
//        OnDefenseChanged?.Invoke(attack, defense);
//    }

//    public void DecreaseAttack(int amount)
//    {
//        if (amount <= 0) return;
//        attack = Mathf.Max(0, attack - amount);
//        PushHudToBase();
//        OnAttackChanged?.Invoke(attack, defense);
//    }

//    public void DecreaseDefense(int amount)
//    {
//        if (amount <= 0) return;
//        defense = Mathf.Max(0, defense - amount);
//        PushHudToBase();
//        OnDefenseChanged?.Invoke(attack, defense);
//    }

//    // ===================== SYNC HELPERS =====================
//    private void PushHudToBase()
//    {
//        if (!baseStats) return;

//        baseStats.maxHP = maxHealth;
//        baseStats.currentHP = currentHealth;
//        baseStats.attack = attack;
//        baseStats.defense = defense;
//    }

//    private IEnumerator CoPullFromBase()
//    {
//        var wait = new WaitForSeconds(pullInterval);
//        while (true)
//        {
//            if (baseStats)
//            {
//                int bMax = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
//                int bCur = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, bMax);

//                bool changed = false;

//                if (bMax != maxHealth) { maxHealth = bMax; changed = true; }
//                if (bCur != currentHealth) { currentHealth = bCur; changed = true; }

//                if (changed) RaiseHealthChanged();
//            }
//            yield return wait;
//        }
//    }

//    private void RaiseHealthChanged()
//    {
//        if (_lastPushedCur == currentHealth && _lastPushedMax == maxHealth) return;
//        _lastPushedCur = currentHealth;
//        _lastPushedMax = maxHealth;
//        OnHealthChanged?.Invoke(currentHealth, maxHealth);
//    }

//    public void TakeDamage(int dmg, Vector2 from, float knockback, float stun)
//    {
//        // Nếu có i-frame riêng thì check ở đây (optional)
//        TakeDamage(dmg);          // dùng hàm int sẵn có -> HUD giảm & PushHudToBase()
//                                  // TODO: áp knockback, stun vào controller của Player nếu bạn muốn
//    }
//}
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
        PushHudToBase();                 // tăng/heal -> đẩy sang Base
        RaiseHealthChanged();
    }

    public void IncreaseMaxHealth(int amount, bool healToFull = true)
    {
        if (amount == 0) return;

        maxHealth = Mathf.Max(1, maxHealth + amount);
        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);

        PushHudToBase();                 // tăng chỉ số -> đẩy sang Base
        RaiseHealthChanged();
    }

    // Giảm Max HP
    public void DecreaseMaxHealth(int amount, bool clampCurrent = true)
    {
        if (amount <= 0) return;

        maxHealth = Mathf.Max(1, maxHealth - amount);
        if (clampCurrent) currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        PushHudToBase();                 // thay đổi chỉ số -> đẩy sang Base
        RaiseHealthChanged();
    }

    // Trừ máu từ HUD (chỉ dùng nếu bạn CHỦ ĐỘNG muốn trừ HUD và Base theo)
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
        PushHudToBase();                 // bạn chủ động chỉnh -> đẩy sang Base
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

    // ==== NHẬN ĐÒN TỪ QUÁI (gọi từ EnemyAttackHitbox) ====
    public void TakeDamage(int dmg, Vector2 from, float knockback, float stun)
    {
        if (dmg <= 0) return;

        if (baseStats != null)
        {
            // 1) Cho BaseStats xử lý (gồm i-frame, Hit/Dead animation)
            baseStats.TakeDamage((float)dmg);

            // 2) Kéo NGƯỢC số liệu từ Base về HUD ngay lập tức
            PullFromBaseOnce();

            // 3) Báo HUD cập nhật
            RaiseHealthChanged();

            // (Optional) áp knockback/stun lên controller của Player tại đây nếu bạn muốn
            // vd: playerController.ApplyKnockback(from, knockback, stun);
        }
        else
        {
            // Fallback nếu chưa gắn BaseStats
            currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
            RaiseHealthChanged();
            if (currentHealth == 0) OnDied?.Invoke();
        }
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

    private void PullFromBaseOnce()
    {
        if (!baseStats) return;
        maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
        currentHealth = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, maxHealth);
        attack = Mathf.RoundToInt(baseStats.attack);
        defense = Mathf.RoundToInt(baseStats.defense);
    }

    private void RaiseHealthChanged()
    {
        if (_lastPushedCur == currentHealth && _lastPushedMax == maxHealth) return;
        _lastPushedCur = currentHealth;
        _lastPushedMax = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

