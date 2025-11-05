using UnityEngine;
using System;
using System.Collections;

public class GreenSlimeStats : MonoBehaviour
{
    [SerializeField] private GreenSlimeStatus greenSlimeStatus;
    [SerializeField] private bool autoPullFromSlime = true;
    [SerializeField] private float pullInterval = 0.05f;

    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private bool isDead = false;

    public event Action<int, int> OnHealthChanged;
    public event Action OnSlimeDied;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    void Awake()
    {
        if (!greenSlimeStatus) greenSlimeStatus = GetComponent<GreenSlimeStatus>();

        if (greenSlimeStatus)
        {
            maxHealth = Mathf.RoundToInt(greenSlimeStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(greenSlimeStatus.currentHealth), 0, maxHealth);
            isDead = greenSlimeStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromSlime && greenSlimeStatus)
            StartCoroutine(CoPullFromSlime());
    }

    public void UpdateFromSlime()
    {
        if (!greenSlimeStatus) return;

        int newMax = Mathf.RoundToInt(greenSlimeStatus.maxHealth);
        int newCur = Mathf.Clamp(Mathf.RoundToInt(greenSlimeStatus.currentHealth), 0, newMax);

        bool changed = (newCur != currentHealth) || (newMax != maxHealth);

        maxHealth = newMax;
        currentHealth = newCur;
        isDead = greenSlimeStatus.isDead;

        if (changed)
            RaiseHealthChanged();

        if (isDead)
            OnSlimeDied?.Invoke();
    }

    public void SetHealthForUI(int current, int max)
    {
        currentHealth = Mathf.Clamp(current, 0, max);
        maxHealth = Mathf.Max(1, max);
        RaiseHealthChanged();
    }

    private IEnumerator CoPullFromSlime()
    {
        var wait = new WaitForSeconds(pullInterval);
        while (true)
        {
            if (greenSlimeStatus)
                UpdateFromSlime();

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
