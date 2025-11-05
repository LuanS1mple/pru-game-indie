using UnityEngine;
using UnityEngine.UI;

public class GreenSlimeHealthBarChapter3 : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = greenSlime.transform).")]
    public Transform followTarget;

    [Tooltip("Vị trí offset (tính bằng đơn vị thế giới).")]
    public Vector2 worldOffset = new Vector2(0f, 1.5f);

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
    [Range(0.02f, 3f)]
    public float scaleMultiplier = 1f;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi bị đánh.")]
    public float showSecondsAfterHit = 2f;

    [Header("GreenSlime Stats")]
    [Tooltip("Kéo thả component GreenSlimeStats vào đây.")]
    public GreenSlimeStats greenSlimeStats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;

    void Awake()
    {
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!greenSlimeStats)
            greenSlimeStats = GetComponentInParent<GreenSlimeStats>();

        if (!followTarget && greenSlimeStats)
            followTarget = greenSlimeStats.transform;

        if (greenSlimeStats)
        {
            greenSlimeStats.OnHealthChanged += OnHealthChangedHandler;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!greenSlimeStats || !followTarget || !fill || !canvas)
            return;

        // 1️⃣ Thanh máu bám theo đầu slime
        Vector3 pos = followTarget.position + (Vector3)worldOffset + Vector3.up * heightOffset;
        transform.position = pos;

        // 2️⃣ Giữ kích thước cố định
        transform.localScale = Vector3.one * scaleMultiplier;

        // 3️⃣ Ẩn sau thời gian hiển thị
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
            if (_showTimer <= 0f && hideWhenFull && greenSlimeStats.CurrentHealth >= greenSlimeStats.MaxHealth)
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
        if (!greenSlimeStats || !fill) return;
        UpdateFill(greenSlimeStats.CurrentHealth, greenSlimeStats.MaxHealth);
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
        if (greenSlimeStats != null)
            greenSlimeStats.OnHealthChanged -= OnHealthChangedHandler;
    }
}
