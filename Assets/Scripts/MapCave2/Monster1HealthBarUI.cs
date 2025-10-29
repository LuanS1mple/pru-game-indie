using UnityEngine;
using UnityEngine.UI;
using Game.Enemy; // để nhận dạng Monster1Stats

public class Monster1HealthBarUI : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = monster1.transform).")]
    public Transform followTarget;

    [Tooltip("Vị trí offset (tính bằng đơn vị thế giới).")]
    public Vector2 worldOffset = new Vector2(0f, 1.2f);

    [Header("UI Components")]
    [Tooltip("Hình ảnh phần fill của thanh máu.")]
    public Image fill;
    [Tooltip("Canvas world-space chứa thanh máu.")]
    public Canvas canvas;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi monster1 bị đánh.")]
    public float showSecondsAfterHit = 2f;

    [Header("Monster1 Stats")]
    [Tooltip("Kéo thả component Monster1Stats vào đây.")]
    public Monster1Stats monster1Stats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        // Nếu chưa gán, tự tìm component cần thiết
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!monster1Stats)
            monster1Stats = GetComponentInParent<Monster1Stats>();

        if (!followTarget && monster1Stats)
            followTarget = monster1Stats.transform;

        _initialLocalScale = transform.localScale;

        if (monster1Stats)
        {
            _lastHP = monster1Stats.currentHP;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!monster1Stats || !followTarget || !fill || !canvas) return;

        // 1️⃣ Thanh máu bám theo đầu monster1
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật thanh máu
        UpdateFillImmediate();

        // 3️⃣ Hiện thanh máu khi bị đánh
        if (_lastHP != monster1Stats.currentHP)
        {
            _lastHP = monster1Stats.currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        // 4️⃣ Ẩn nếu đầy máu
        if (hideWhenFull)
        {
            if (monster1Stats.currentHP >= monster1Stats.maxHP && _showTimer <= 0f)
                canvas.enabled = false;
        }

        // 5️⃣ Đếm ngược ẩn thanh máu
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && monster1Stats.currentHP >= monster1Stats.maxHP)
                canvas.enabled = false;
        }

        // 6️⃣ Giữ thanh máu không bị lật khi monster1 flip
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, monster1Stats.maxHP);
        float t = Mathf.Clamp01((float)monster1Stats.currentHP / max);
        fill.fillAmount = t;
    }

    // 🔹 Có thể gọi thủ công khi spawn monster1
    public void ForceShow(float seconds = 2f)
    {
        _showTimer = seconds;
        if (canvas)
            canvas.enabled = true;
    }
}
