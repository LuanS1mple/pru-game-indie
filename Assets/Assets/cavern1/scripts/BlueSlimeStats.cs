using UnityEngine;
using System;
using System.Collections;

public class BlueSlimeStats : MonoBehaviour, EnemySta_IDamage
{
    [Header("Nguồn dữ liệu máu (BlueSlimeStatus)")]
    [SerializeField] private BlueSlimeStatus blueSlimeStatus; // Đổi từ GreenSlimeStatus
    [SerializeField] private bool autoPullFromBlueSlime = true; // Đổi từ autoPullFromSlime
    [SerializeField] private float pullInterval = 0.05f;

    [Header("Thông số máu")]
    [SerializeField] private int maxHealth = 120; // Bạn có thể thay đổi giá trị mặc định
    [SerializeField] private int currentHealth = 120;

    [Header("Trạng thái")]
    [SerializeField] private bool isDead = false;

    // Sự kiện
    public event Action<int, int> OnHealthChanged;
    public event Action OnBlueSlimeDied; // Đổi từ OnSlimeDied

    // Triển khai interface
    int EnemySta_IDamage.MaxHealth => maxHealth;
    int EnemySta_IDamage.CurrentHealth => currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private int _lastCur, _lastMax;

    private void Awake()
    {
        if (!blueSlimeStatus)
            blueSlimeStatus = GetComponent<BlueSlimeStatus>();

        if (blueSlimeStatus)
        {
            maxHealth = Mathf.RoundToInt(blueSlimeStatus.maxHealth);
            currentHealth = Mathf.Clamp(Mathf.RoundToInt(blueSlimeStatus.currentHealth), 0, maxHealth);
            isDead = blueSlimeStatus.isDead;
        }

        _lastCur = currentHealth;
        _lastMax = maxHealth;
    }

    private void Start()
    {
        // Kích hoạt sự kiện ban đầu để UI cập nhật
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (autoPullFromBlueSlime && blueSlimeStatus)
            StartCoroutine(CoPullFromBlueSlime()); // Đổi từ CoPullFromSlime
    }

    private IEnumerator CoPullFromBlueSlime() // Đổi từ CoPullFromSlime
    {
        var wait = new WaitForSeconds(pullInterval);

        while (true)
        {
            if (blueSlimeStatus)
                UpdateFromBlueSlime(); // Đổi từ UpdateFromSlime

            yield return wait;
        }
    }

    public void UpdateFromBlueSlime() // Đổi từ UpdateFromSlime
    {
        if (!blueSlimeStatus) return;

        int newMax = Mathf.RoundToInt(blueSlimeStatus.maxHealth);
        int newCur = Mathf.Clamp(Mathf.RoundToInt(blueSlimeStatus.currentHealth), 0, newMax);

        bool changed = (newCur != currentHealth) || (newMax != maxHealth);

        maxHealth = newMax;
        currentHealth = newCur;
        isDead = blueSlimeStatus.isDead;

        if (changed)
            RaiseHealthChanged();

        if (isDead)
            OnBlueSlimeDied?.Invoke(); // Đổi từ OnSlimeDied
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