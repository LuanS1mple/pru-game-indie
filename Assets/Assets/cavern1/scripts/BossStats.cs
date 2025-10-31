using UnityEngine;
using System;
using System.Collections;

public class BossStats : MonoBehaviour, EnemySta_IDamage
{
    [SerializeField] private BossStatus bossStatus;
    [SerializeField] private bool autoPullFromBoss = true;
    [SerializeField] private float pullInterval = 0.05f;

    [SerializeField] private int maxHealth = 500;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnBossDied;

    int EnemySta_IDamage.MaxHealth => maxHealth;
    int EnemySta_IDamage.CurrentHealth => currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    void Awake()
    {
        if (!bossStatus) bossStatus = GetComponent<BossStatus>();

        if (bossStatus)
        {
            maxHealth = Mathf.RoundToInt(bossStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(bossStatus.currentHealth), 0, maxHealth);
            isDead = bossStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromBoss && bossStatus)
            StartCoroutine(CoPullFromBoss());
    }

    public void UpdateFromBoss()
    {
        if (!bossStatus) return;

        int newMax = Mathf.RoundToInt(bossStatus.maxHealth);
        int newCur = Mathf.Clamp(Mathf.RoundToInt(bossStatus.currentHealth), 0, newMax);

        bool changed = (newCur != currentHealth) || (newMax != maxHealth);

        maxHealth = newMax;
        currentHealth = newCur;
        isDead = bossStatus.isDead;

        if (changed)
            RaiseHealthChanged();

        if (isDead)
            OnBossDied?.Invoke();
    }

    public void SetHealthForUI(int current, int max)
    {
        currentHealth = Mathf.Clamp(current, 0, max);
        maxHealth = Mathf.Max(1, max);
        RaiseHealthChanged();
    }

    private IEnumerator CoPullFromBoss()
    {
        var wait = new WaitForSeconds(pullInterval);
        while (true)
        {
            if (bossStatus)
                UpdateFromBoss();

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
