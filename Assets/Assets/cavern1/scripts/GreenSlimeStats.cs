using UnityEngine;
using System;
using System.Collections;

public class GreenSlimeStats : MonoBehaviour, EnemySta_IDamage
{
    [Header("Nguồn dữ liệu máu (GreenSlimeStatus)")]
    [SerializeField] private GreenSlimeStatus slimeStatus; // Thay thế DemonStatus
    [SerializeField] private bool autoPullFromSlime = true; // Thay thế autoPullFromDemon
    [SerializeField] private float pullInterval = 0.05f;

    [Header("Thông số máu")]
    [SerializeField] private int maxHealth = 100; // Bạn có thể thay đổi giá trị mặc định
    [SerializeField] private int currentHealth = 100;

    [Header("Trạng thái")]
    [SerializeField] private bool isDead = false;

    // Sự kiện
    public event Action<int, int> OnHealthChanged;
    public event Action OnSlimeDied; // Thay thế OnDemonDied

    // Triển khai interface
    int EnemySta_IDamage.MaxHealth => maxHealth;
    int EnemySta_IDamage.CurrentHealth => currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    private void Awake()
    {
        if (!slimeStatus)
            slimeStatus = GetComponent<GreenSlimeStatus>();

        if (slimeStatus)
        {
            maxHealth = Mathf.RoundToInt(slimeStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(slimeStatus.currentHealth), 0, maxHealth);
            isDead = slimeStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    private void Start()
    {
        // Kích hoạt sự kiện ban đầu để UI cập nhật
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromSlime && slimeStatus)
            StartCoroutine(CoPullFromSlime()); // Thay thế CoPullFromDemon
    }

    private IEnumerator CoPullFromSlime() // Thay thế CoPullFromDemon
    {
        var wait = new WaitForSeconds(pullInterval);

        while (true)
        {
            if (slimeStatus)
                UpdateFromSlime(); // Thay thế UpdateFromDemon

            yield return wait;
        }
    }

    public void UpdateFromSlime() // Thay thế UpdateFromDemon
    {
        if (!slimeStatus) return;

        int newMax = Mathf.RoundToInt(slimeStatus.maxHealth);
        int newCur = Mathf.Clamp(Mathf.RoundToInt(slimeStatus.currentHealth), 0, newMax);

        bool changed = (newCur != currentHealth) || (newMax != maxHealth);

        maxHealth = newMax;
        currentHealth = newCur;
        isDead = slimeStatus.isDead;

        if (changed)
            RaiseHealthChanged();

        if (isDead)
            OnSlimeDied?.Invoke(); 
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