using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Miniboss_EnemyStatUI : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;
    public Vector2 worldOffset = new Vector2(0f, 2.2f); // cao hơn đầu miniboss

    [Header("UI")]
    public Image fill;
    public Canvas canvas;

    [Header("Visibility")]
    public bool hideWhenFull = true;
    public float showSecondsAfterHit = 3f;

    [Header("Refs")]
    public MinibossBehaviour enemyStats; // liên kết tới MinibossBehaviour

    // Internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!enemyStats) enemyStats = GetComponentInParent<MinibossBehaviour>();
        if (!followTarget && enemyStats) followTarget = enemyStats.transform;
        _initialLocalScale = transform.localScale;

        if (enemyStats != null)
        {
            _lastHP = enemyStats.GetCurrentHealth();
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!enemyStats || !followTarget || !fill || !canvas) return;

        // 1️⃣ Theo dõi vị trí
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật fill
        UpdateFillImmediate();

        // 3️⃣ Hiện/ẩn khi bị đánh
        int currentHP = enemyStats.GetCurrentHealth();
        if (_lastHP != currentHP)
        {
            _lastHP = currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull)
        {
            if (currentHP >= enemyStats.GetMaxHealth() && _showTimer <= 0f)
                canvas.enabled = false;
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && currentHP >= enemyStats.GetMaxHealth())
                canvas.enabled = false;
        }

        // 4️⃣ Giữ hướng thanh máu
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        if (enemyStats == null) return;
        float max = Mathf.Max(1, enemyStats.GetMaxHealth());
        float t = Mathf.Clamp01((float)enemyStats.GetCurrentHealth() / max);
        fill.fillAmount = t;
    }

    // Cho phép gọi thủ công khi spawn
    public void ForceShow(float seconds = 2f)
    {
        _showTimer = seconds;
        if (canvas) canvas.enabled = true;
    }
}
