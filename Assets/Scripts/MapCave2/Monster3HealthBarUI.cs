using UnityEngine;
using UnityEngine.UI;
using Game.Enemy; // để nhận dạng Monster3Stats

public class Monster3HealthBarUI : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = monster3.transform).")]
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

    [Tooltip("Hiển thị trong X giây sau khi monster3 bị đánh.")]
    public float showSecondsAfterHit = 2f;

    [Header("Monster3 Stats")]
    [Tooltip("Kéo thả component Monster3Stats vào đây.")]
    public Monster3Stats monster3Stats;

    // Internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        // Tự động tìm và gán các component cần thiết
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!monster3Stats)
            monster3Stats = GetComponentInParent<Monster3Stats>();

        if (!followTarget && monster3Stats)
            followTarget = monster3Stats.transform;

        _initialLocalScale = transform.localScale;

        if (monster3Stats)
        {
            _lastHP = monster3Stats.currentHP;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!monster3Stats || !followTarget || !fill || !canvas) return;

        // 1️⃣ Theo dõi vị trí Monster3
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // 2️⃣ Cập nhật fill thanh máu
        UpdateFillImmediate();

        // 3️⃣ Hiển thị thanh máu khi bị đánh
        if (_lastHP != monster3Stats.currentHP)
        {
            _lastHP = monster3Stats.currentHP;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        // 4️⃣ Ẩn nếu đầy máu
        if (hideWhenFull)
        {
            if (monster3Stats.currentHP >= monster3Stats.maxHP && _showTimer <= 0f)
                canvas.enabled = false;
        }

        // 5️⃣ Đếm ngược ẩn thanh máu
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && monster3Stats.currentHP >= monster3Stats.maxHP)
                canvas.enabled = false;
        }

        // 6️⃣ Giữ thanh máu không bị lật khi Monster3 flip X
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    void UpdateFillImmediate()
    {
        float max = Mathf.Max(1, monster3Stats.maxHP);
        float t = Mathf.Clamp01((float)monster3Stats.currentHP / max);
        fill.fillAmount = t;
    }

    // 🔹 Cho phép hiển thị thủ công khi spawn
    public void ForceShow(float seconds = 2f)
    {
        _showTimer = seconds;
        if (canvas)
            canvas.enabled = true;
    }
}
