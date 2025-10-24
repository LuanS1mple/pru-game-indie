using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;                 // Mặc định = enemy.transform
    public Vector2 worldOffset = new Vector2(0f, 1.0f); // cao hơn đầu quái

    [Header("UI")]
    public Image fill;                             // Image 'Fill'
    public Canvas canvas;                          // Canvas world-space chứa bar

    [Header("Visibility")]
    public bool hideWhenFull = true;
    public float showSecondsAfterHit = 2f;         // hiện ra X giây sau khi bị đánh

    [Header("Refs")]
    public EnemyStats_IDamageable enemyStats;      // đọc current/max từ đây

    // internal
    int _lastHP = -1;
    float _showTimer = 0f;
    Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!enemyStats) enemyStats = GetComponentInParent<EnemyStats_IDamageable>();
        if (!followTarget && enemyStats) followTarget = enemyStats.transform;
        _initialLocalScale = transform.localScale;

        if (enemyStats)
        {
            _lastHP = enemyStats.currentHealth;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!enemyStats || !followTarget || !fill || !canvas) return;

        // 1) Follow vị trí
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2) Cập nhật fill
        UpdateFillImmediate();

        // 3) Hiện/ẩn
        if (_lastHP != enemyStats.currentHealth)
        {
            _lastHP = enemyStats.currentHealth;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull)
        {
            if (enemyStats.currentHealth >= enemyStats.maxHealth && _showTimer <= 0f)
                canvas.enabled = false;
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && enemyStats.currentHealth >= enemyStats.maxHealth)
                canvas.enabled = false;
        }

        // 4) Giữ bar không bị lật khi enemy flip X
        // (nếu bar là child của enemy sẽ bị scale âm theo X)
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x); // buộc dương theo X
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, enemyStats.maxHealth);
        float t = Mathf.Clamp01((float)enemyStats.currentHealth / max);
        fill.fillAmount = t;
    }

    // Cho phép gọi thủ công khi spawn
    public void ForceShow(float seconds = 1.5f)
    {
        _showTimer = seconds;
        if (canvas) canvas.enabled = true;
    }
}
