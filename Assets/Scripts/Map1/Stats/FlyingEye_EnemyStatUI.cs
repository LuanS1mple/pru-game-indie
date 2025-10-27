using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlyingEye_EnemyStatUI : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;
    public Vector2 worldOffset = new Vector2(0f, 1.2f);

    [Header("UI")]
    public Image fill;
    public Canvas canvas;

    [Header("Visibility")]
    public bool hideWhenFull = true;
    public float showSecondsAfterHit = 2f;

    [Header("Refs")]
    public FlyingEyeBehaviors enemyStats;  // ✅ Liên kết với FlyingEyeBehaviors

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!enemyStats) enemyStats = GetComponentInParent<FlyingEyeBehaviors>();
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

        // 1️⃣ Theo dõi vị trí world → UI
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật thanh máu
        UpdateFillImmediate();

        // 3️⃣ Xử lý hiện/ẩn khi bị đánh
        int currentHP = enemyStats.GetCurrentHealth();
        if (_lastHP != currentHP)
        {
            _lastHP = currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull)
        {
            if (currentHP >= enemyStats.maxHealth && _showTimer <= 0f)
                canvas.enabled = false;
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && currentHP >= enemyStats.maxHealth)
                canvas.enabled = false;
        }

        // 4️⃣ Giữ hướng thanh máu đúng chiều
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        if (enemyStats == null) return;
        float max = Mathf.Max(1, enemyStats.maxHealth);
        float t = Mathf.Clamp01((float)enemyStats.GetCurrentHealth() / max);
        fill.fillAmount = t;
    }

    // Cho phép ép hiện thanh máu khi spawn
    public void ForceShow(float seconds = 1.5f)
    {
        _showTimer = seconds;
        if (canvas) canvas.enabled = true;
    }
}
