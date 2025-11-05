using UnityEngine;
using UnityEngine.UI;

public class BlueSlimeHealthBarChapter3 : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Transform để thanh máu bám theo (mặc định = blueSlime.transform).")]
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
    [Range(0.01f, 3f)]
    public float scaleMultiplier = 1f;

    [Header("Visibility Settings")]
    [Tooltip("Ẩn khi HP đầy.")]
    public bool hideWhenFull = true;
    [Tooltip("Hiển thị trong X giây sau khi bị đánh.")]
    public float showSecondsAfterHit = 2f;

    [Header("BlueSlime Stats")]
    [Tooltip("Kéo thả component BlueSlimeStats vào đây.")]
    public BlueSlimeStats blueSlimeStats;

    // internal
    private int _lastHP = -1;
    private float _showTimer = 0f;

    void Awake()
    {
        if (!canvas)
            canvas = GetComponentInParent<Canvas>();

        if (!blueSlimeStats)
            blueSlimeStats = GetComponentInParent<BlueSlimeStats>();

        if (!followTarget && blueSlimeStats)
            followTarget = blueSlimeStats.transform;

        if (blueSlimeStats)
        {
            blueSlimeStats.OnHealthChanged += OnHealthChangedHandler;
            UpdateFillImmediate();
        }
    }

    void LateUpdate()
    {
        if (!blueSlimeStats || !followTarget || !fill || !canvas)
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
            if (_showTimer <= 0f && hideWhenFull && blueSlimeStats.CurrentHealth >= blueSlimeStats.MaxHealth)
                canvas.enabled = false;
        }
    }

    private void OnHealthChangedHandler(int current, int max)
    {
        UpdateFill(current, max);

        // Khi bị đánh
        if (_lastHP != current)
        {
            _lastHP = current;
            _showTimer = showSecondsAfterHit;
            canvas.enabled = true;
        }

        // Ẩn khi đầy máu
        if (hideWhenFull && current >= max && _showTimer <= 0f)
            canvas.enabled = false;
    }

    private void UpdateFillImmediate()
    {
        if (!blueSlimeStats || !fill) return;
        UpdateFill(blueSlimeStats.CurrentHealth, blueSlimeStats.MaxHealth);
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
        if (blueSlimeStats != null)
            blueSlimeStats.OnHealthChanged -= OnHealthChangedHandler;
    }
}
