using UnityEngine;
using System;
using System.Collections;

public class DemonStats : MonoBehaviour
{
    [SerializeField] private DemonStatus demonStatus;  // Script chứa thông tin gốc (VD: currentHP, maxHP)
    [SerializeField] private bool autoPullFromDemon = true;
    [SerializeField] private float pullInterval = 0.05f;

    [SerializeField] private int maxHealth = 150;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDemonDied;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    void Awake()
    {
        if (!demonStatus) demonStatus = GetComponent<DemonStatus>();

        if (demonStatus)
        {
            maxHealth = Mathf.RoundToInt(demonStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(demonStatus.currentHealth), 0, maxHealth);
            isDead = demonStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromDemon && demonStatus)
            StartCoroutine(CoPullFromDemon());
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

    public void SetHealthForUI(int current, int max)
    {
        currentHealth = Mathf.Clamp(current, 0, max);
        maxHealth = Mathf.Max(1, max);
        RaiseHealthChanged();
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

    private void RaiseHealthChanged()
    {
        if (_lastCur == currentHealth && _lastMax == maxHealth)
            return;

        _lastCur = currentHealth;
        _lastMax = maxHealth;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
