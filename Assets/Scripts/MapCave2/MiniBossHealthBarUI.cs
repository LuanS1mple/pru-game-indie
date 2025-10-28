using UnityEngine;
using UnityEngine.UI;
using Game.Enemy; // để nhận dạng MiniBossStats

public class MiniBossHealthBarUI : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = miniboss.transform).")]
    public Transform followTarget;

    [Tooltip("Vị trí offset (tính bằng đơn vị thế giới).")]
    public Vector2 worldOffset = new Vector2(0f, 1.5f);

    [Header("UI Components")]
    [Tooltip("Hình ảnh phần fill của thanh máu.")]
    public Image fill;
    [Tooltip("Canvas world-space chứa thanh máu.")]
    public Canvas canvas;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi miniboss bị đánh.")]
    public float showSecondsAfterHit = 3f;

    [Header("MiniBoss Stats")]
    [Tooltip("Kéo thả component MiniBossStats vào đây.")]
    public MiniBossStats miniBossStats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        // Nếu chưa gán, tự tìm component cần thiết
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!miniBossStats)
            miniBossStats = GetComponentInParent<MiniBossStats>();

        if (!followTarget && miniBossStats)
            followTarget = miniBossStats.transform;

        _initialLocalScale = transform.localScale;

        if (miniBossStats)
        {
            _lastHP = miniBossStats.currentHP;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!miniBossStats || !followTarget || !fill || !canvas) return;

        // 1️⃣ Thanh máu bám theo đầu miniboss
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật thanh máu
        UpdateFillImmediate();

        // 3️⃣ Hiện thanh máu khi bị đánh
        if (_lastHP != miniBossStats.currentHP)
        {
            _lastHP = miniBossStats.currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        // 4️⃣ Ẩn nếu đầy máu
        if (hideWhenFull)
        {
            if (miniBossStats.currentHP >= miniBossStats.maxHP && _showTimer <= 0f)
                canvas.enabled = false;
        }

        // 5️⃣ Đếm ngược ẩn thanh máu
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && miniBossStats.currentHP >= miniBossStats.maxHP)
                canvas.enabled = false;
        }

        // 6️⃣ Giữ thanh máu không bị lật khi miniboss flip
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, miniBossStats.maxHP);
        float t = Mathf.Clamp01((float)miniBossStats.currentHP / max);
        fill.fillAmount = t;
    }

    // 🔹 Có thể gọi thủ công khi spawn miniboss
    public void ForceShow(float seconds = 2f)
    {
        _showTimer = seconds;
        if (canvas)
            canvas.enabled = true;
    }
}
