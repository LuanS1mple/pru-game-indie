
using UnityEngine;
using System;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    [Header("Link to REAL stats")]
    [SerializeField] private BaseStats baseStats;          // Kéo BaseStats (thật) của Player vào đây
    [SerializeField] private bool autoPullFromBase = true; // Tự đồng bộ từ BaseStats -> HUD
    [SerializeField] private float pullInterval = 0.05f;   // 20Hz (mịn nhưng nhẹ)

    [Header("HUD values (mirrors)")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public event Action<int, int> OnHealthChanged; // (current,max)
    public event Action OnDied;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // cache để tránh spam event
    private int _lastPushedCur, _lastPushedMax;

    void Awake()
    {
        if (!baseStats) baseStats = GetComponent<BaseStats>();

        // Nếu có BaseStats thật → lấy nó làm nguồn khởi tạo
        if (baseStats)
        {
            maxHealth = Mathf.Max(1, Mathf.RoundToInt(baseStats.maxHP));
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(baseStats.currentHP), 0, maxHealth);
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

    // ===================== PUBLIC API (HUD side) =====================

    // Gọi khi nhặt item hồi máu
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        PushHudToBase();          // ↔ cập nhật chỉ số thật
        RaiseHealthChanged();
    }

    // Gọi khi nhặt item tăng máu tối đa
    public void IncreaseMaxHealth(int amount, bool healToFull = true)
    {
        if (amount <= 0) return;

        maxHealth = Mathf.Max(1, maxHealth + amount);
        currentHealth = healToFull ? maxHealth : Mathf.Clamp(currentHealth, 0, maxHealth);

        PushHudToBase();          // ↔ cập nhật chỉ số thật
        RaiseHealthChanged();
    }

    // Khi HUD bị trừ máu (ví dụ bạn chủ động gọi từ đâu đó)
    public void TakeDamage(int dmg)
    {
        if (dmg <= 0) return;

        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
        PushHudToBase();          // ↔ cập nhật chỉ số thật
        RaiseHealthChanged();

        if (currentHealth == 0) OnDied?.Invoke();
    }

    // ===================== SYNC HELPERS =====================

    // Đẩy HUD -> BaseStats (khi ăn item / Heal / IncreaseMaxHealth / TakeDamage gọi từ HUD)
    private void PushHudToBase()
    {
        if (!baseStats) return;

        // Ghi trực tiếp sang stats thật
        baseStats.maxHP = maxHealth;
        baseStats.currentHP = currentHealth;

        // Nếu bạn muốn đẩy attack/defense khi có các item khác cũng làm tương tự ở đây
    }

    // Kéo BaseStats -> HUD (khi bị quái đánh làm BaseStats thay đổi ở nơi khác)
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

                if (bMax != maxHealth)
                {
                    maxHealth = bMax;
                    changed = true;
                }
                if (bCur != currentHealth)
                {
                    currentHealth = bCur;
                    changed = true;
                }

                if (changed)
                    RaiseHealthChanged();
            }

            yield return wait;
        }
    }

    private void RaiseHealthChanged()
    {
        // Chặn spam sự kiện nếu giá trị không đổi
        if (_lastPushedCur == currentHealth && _lastPushedMax == maxHealth) return;

        _lastPushedCur = currentHealth;
        _lastPushedMax = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}

