using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarChapter3 : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = boss.transform).")]
    public Transform followTarget;

    [Tooltip("Vị trí offset (tính bằng đơn vị thế giới).")]
    public Vector2 worldOffset = new Vector2(0f, 3.0f);

    [Tooltip("Khoảng cách điều chỉnh thêm trên trục Y (bổ sung cho worldOffset).")]
    [Range(-2f, 5f)]
    public float heightOffset = 0f;

    [Header("UI Components")]
    [Tooltip("Hình ảnh phần fill của thanh máu.")]
    public Image fill;
    [Tooltip("Canvas world-space chứa thanh máu.")]
    public Canvas canvas;

    [Header("Visual Settings")]
    [Tooltip("Tỉ lệ phóng to / thu nhỏ thanh máu.")]
    [Range(0.5f, 5f)]
    public float scaleMultiplier = 1f;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi boss bị đánh.")]
    public float showSecondsAfterHit = 3f;

    [Header("Boss Stats")]
    [Tooltip("Kéo thả component BossStats vào đây.")]
    public BossStats bossStats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!bossStats)
            bossStats = GetComponentInParent<BossStats>();

        if (!followTarget && bossStats)
            followTarget = bossStats.transform;

        _initialLocalScale = transform.localScale;

        if (bossStats)
        {
            bossStats.OnHealthChanged += OnHealthChangedHandler;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!bossStats || !followTarget || !fill || !canvas)
            return;

        // 1️⃣ Thanh máu bám theo đầu boss + offset tùy chỉnh
        Vector3 pos = followTarget.position + (Vector3)worldOffset + Vector3.up * heightOffset;
        transform.position = pos;

        // 2️⃣ Điều chỉnh kích thước thanh máu theo scaleMultiplier
        Vector3 s = _initialLocalScale * scaleMultiplier;
        s.x = Mathf.Abs(s.x); // tránh bị lật
        transform.localScale = s;

        // 3️⃣ Ẩn thanh máu nếu hết thời gian hiển thị
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && bossStats.CurrentHealth >= bossStats.MaxHealth)
                canvas.enabled = false;
        }
    }

    private void OnHealthChangedHandler(int current, int max)
    {
        UpdateFill(current, max);

        // Nếu bị đánh → bật thanh máu và reset timer
        if (_lastHP != current)
        {
            _lastHP = current;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        // Ẩn khi đầy máu (nếu tùy chọn)
        if (hideWhenFull && current >= max && _showTimer <= 0f)
            canvas.enabled = false;
    }

    private void UpdateFillImmediate()
    {
        if (!bossStats || !fill) return;
        UpdateFill(bossStats.CurrentHealth, bossStats.MaxHealth);
    }

    private void UpdateFill(int current, int max)
    {
        float ratio = Mathf.Clamp01((float)current / Mathf.Max(1f, max));
        fill.fillAmount = ratio;
    }

    public void ForceShow(float seconds = 2f)
    {
        _showTimer = seconds;
        if (canvas)
            canvas.enabled = true;
    }

    void OnDestroy()
    {
        if (bossStats != null)
            bossStats.OnHealthChanged -= OnHealthChangedHandler;
    }
}
