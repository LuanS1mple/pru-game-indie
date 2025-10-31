using UnityEngine;
using System;
using System.Collections;

public class DemonStats : MonoBehaviour, EnemySta_IDamage
{
    [Header("Nguồn dữ liệu máu (DemonStatus)")]
    [SerializeField] private DemonStatus demonStatus;
    [SerializeField] private bool autoPullFromDemon = true;
    [SerializeField] private float pullInterval = 0.05f;

    [Header("Thông số máu")]
    [SerializeField] private int maxHealth = 300;
    [SerializeField] private int currentHealth = 300;

    [Header("Trạng thái")]
    [SerializeField] private bool isDead = false;

    // Sự kiện
    public event Action<int, int> OnHealthChanged;
    public event Action OnDemonDied;

    // Triển khai interface
    int EnemySta_IDamage.MaxHealth => maxHealth;
    int EnemySta_IDamage.CurrentHealth => currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    private void Awake()
    {
        if (!demonStatus)
            demonStatus = GetComponent<DemonStatus>();

        if (demonStatus)
        {
            maxHealth = Mathf.RoundToInt(demonStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(demonStatus.currentHealth), 0, maxHealth);
            isDead = demonStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    private void Start()
    {
        // Kích hoạt sự kiện ban đầu để UI cập nhật
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromDemon && demonStatus)
            StartCoroutine(CoPullFromDemon());
    }

    private IEnumerator CoPullFromDemon()
    {
        var wait = new WaitForSeconds(pullInterval);

        while (true)
        {
            if (demonStatus)
                UpdateFromDemon();

            yield return wait;
        }
    }

    public void UpdateFromDemon()
    {
        if (!demonStatus) return;

        int newMax = Mathf.RoundToInt(demonStatus.maxHealth);
        int newCur = Mathf.Clamp(Mathf.RoundToInt(demonStatus.currentHealth), 0, newMax);

        bool changed = (newCur != currentHealth) || (newMax != maxHealth);

        maxHealth = newMax;
        currentHealth = newCur;
        isDead = demonStatus.isDead;

        if (changed)
            RaiseHealthChanged();

        if (isDead)
            OnDemonDied?.Invoke();
    }

    private void RaiseHealthChanged()
    {
        if (_lastCur == currentHealth && _lastMax == maxHealth)
            return;

        _lastCur = currentHealth;
        _lastMax = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
