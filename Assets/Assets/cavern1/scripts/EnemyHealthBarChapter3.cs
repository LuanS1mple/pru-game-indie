using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarChapter3 : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform followTarget;                  // Mặc định = enemy.transform
    public Vector2 worldOffset = new Vector2(0f, 1.0f); // Cao hơn đầu quái / boss

    [Header("UI Settings")]
    public Image fill;                              // Image "Fill"
    public Canvas canvas;                           // Canvas world-space chứa thanh máu

    [Header("Visibility Settings")]
    public bool hideWhenFull = true;                // Ẩn khi đầy máu
    public float showSecondsAfterHit = 2f;          // Hiện X giây sau khi bị đánh

    [Header("References")]
    public EnemySta_IDamage enemyStats;             // Đọc current/max từ đây

    // Nội bộ
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        if (!fill) fill = GetComponentInChildren<Image>(); // ✅ tự tìm nếu quên gán

        if (enemyStats == null)
            enemyStats = GetComponentInParent<EnemySta_IDamage>();

        if (!followTarget && enemyStats is MonoBehaviour mb)
            followTarget = mb.transform;

        _initialLocalScale = transform.localScale;

        if (enemyStats != null && fill != null)
        {
            _lastHP = enemyStats.CurrentHealth;
            UpdateFillImmediate();
        }
        else
        {
            if (enemyStats == null)
                Debug.LogWarning($"{name}: Không tìm thấy EnemySta_IDamage trong cha!");
            if (fill == null)
                Debug.LogWarning($"{name}: Chưa gán image Fill!");
        }
    }


    void LateUpdate()
    {
        if (enemyStats == null)
        {
            Debug.LogWarning($"{name}: Không tìm thấy EnemySta_IDamage trong cha!");
            return;
        }

        // 1️⃣ Theo dõi vị trí
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật thanh máu
        UpdateFillImmediate();

        // 3️⃣ Xử lý hiển thị
        if (_lastHP != enemyStats.CurrentHealth)
        {
            _lastHP = enemyStats.CurrentHealth;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull)
        {
            if (enemyStats.CurrentHealth >= enemyStats.MaxHealth && _showTimer <= 0f)
                canvas.enabled = false;
        }

        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && enemyStats.CurrentHealth >= enemyStats.MaxHealth)
                canvas.enabled = false;
        }

        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);

        // 4️⃣ Giữ thanh máu không bị lật khi nhân vật flip X
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        if (enemyStats == null || followTarget == null || fill == null || canvas == null)
        {
            Debug.LogWarning($"{name}: Missing refs → enemyStats={enemyStats}, followTarget={followTarget}, fill={fill}, canvas={canvas}");
            return;
        }

    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, enemyStats.MaxHealth);
        float t = Mathf.Clamp01((float)enemyStats.CurrentHealth / max);
        fill.fillAmount = t;
    }

    // 🟢 Gọi thủ công để buộc hiển thị (ví dụ khi spawn)
    public void ForceShow(float seconds = 1.5f)
    {
        _showTimer = seconds;
        if (canvas) canvas.enabled = true;
    }
}
