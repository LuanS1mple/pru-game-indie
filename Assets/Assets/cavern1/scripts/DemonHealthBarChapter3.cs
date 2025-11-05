using UnityEngine;
using UnityEngine.UI;

public class DemonHealthBarChapter3 : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = demon.transform).")]
    public Transform followTarget;

    [Tooltip("Vị trí offset (tính bằng đơn vị thế giới).")]
    public Vector2 worldOffset = new Vector2(0f, 2.5f);

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
    [Range(0.5f, 4f)]
    public float scaleMultiplier = 1f;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi demon bị đánh.")]
    public float showSecondsAfterHit = 2.5f;

    [Header("Demon Stats")]
    [Tooltip("Kéo thả component DemonStats vào đây.")]
    public DemonStats demonStats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!demonStats)
            demonStats = GetComponentInParent<DemonStats>();

        if (!followTarget && demonStats)
            followTarget = demonStats.transform;

        _initialLocalScale = transform.localScale;

        if (demonStats)
        {
            demonStats.OnHealthChanged += OnHealthChangedHandler;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!demonStats || !followTarget || !fill || !canvas)
            return;

        // 1️⃣ Thanh máu bám theo đầu demon + offset tùy chỉnh
        Vector3 pos = followTarget.position + (Vector3)worldOffset + Vector3.up * heightOffset;
        transform.position = pos;

        // 2️⃣ Điều chỉnh kích thước thanh máu theo scaleMultiplier
        Vector3 s = _initialLocalScale * scaleMultiplier;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        // 3️⃣ Ẩn thanh máu sau thời gian chỉ định
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && demonStats.CurrentHealth >= demonStats.MaxHealth)
                canvas.enabled = false;
        }
    }

    private void OnHealthChangedHandler(int current, int max)
    {
        UpdateFill(current, max);

        if (_lastHP != current)
        {
            _lastHP = current;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        if (hideWhenFull && current >= max && _showTimer <= 0f)
            canvas.enabled = false;
    }

    private void UpdateFillImmediate()
    {
        if (!demonStats || !fill) return;
        UpdateFill(demonStats.CurrentHealth, demonStats.MaxHealth);
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
        if (demonStats != null)
            demonStats.OnHealthChanged -= OnHealthChangedHandler;
    }
}
