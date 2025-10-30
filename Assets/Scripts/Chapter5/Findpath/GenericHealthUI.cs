using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Script này có thể được gắn vào bất kỳ HUD thanh máu nào là con của đối tượng có BaseStats
public class GenericHealthUI : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;
    public Vector2 worldOffset = new Vector2(0f, 1.2f); // Offset so với vị trí của nhân vật

    [Header("UI")]
    public Image fill;
    public Canvas canvas;

    [Header("Visibility")]
    public bool hideWhenFull = true;
    public float showSecondsAfterHit = 2f; // Thời gian hiển thị sau khi bị đánh

    [Header("Refs")]
    // Tham chiếu chung đến lớp BaseStats
    public BaseStats entityStats;

    // internal
    // ⭐ Đã đổi _lastHP thành private float _cachedHP để dùng cho việc kiểm tra thay đổi trong LateUpdate
    private float _cachedHP = -1f;
    private float _showTimer = 0f;
    private Vector3 _initialLocalScale;

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        // Tự động tìm BaseStats trong đối tượng cha
        if (!entityStats) entityStats = GetComponentInParent<BaseStats>();
        if (!followTarget && entityStats) followTarget = entityStats.transform;
        _initialLocalScale = transform.localScale;

        if (entityStats != null)
        {
            _cachedHP = entityStats.currentHP;
            UpdateFillImmediate();
        }

        // Ban đầu tắt Canvas nếu máu đầy và yêu cầu ẩn
        if (canvas && hideWhenFull && entityStats != null && entityStats.currentHP >= entityStats.maxHP)
        {
            canvas.enabled = false;
        }
    }

    void LateUpdate()
    {
        if (!entityStats || !followTarget || !fill || !canvas) return;

        // 1️⃣ Theo dõi vị trí world → UI
        Vector3 pos = followTarget.position + (Vector3)worldOffset;
        transform.position = pos;

        // ⭐ THÊM: Đặt lại góc quay của HUD để nó luôn nhìn thẳng (không bị lật theo Enemy)
        // Đặt rotation về Quaternion.identity (góc quay 0, 0, 0)
        transform.rotation = Quaternion.identity;
        // --------------------------------------------------------------------------------

        // 2️⃣ Xử lý hiện/ẩn khi máu thay đổi (TỰ ĐỘNG PHÁT HIỆN SÁT THƯƠNG)
        if (_cachedHP != entityStats.currentHP)
        {
            // Nếu máu hiện tại nhỏ hơn máu cache (tức là vừa bị mất máu)
            if (entityStats.currentHP < _cachedHP)
            {
                ForceShow(showSecondsAfterHit); // Buộc hiển thị
            }

            _cachedHP = entityStats.currentHP; // Cập nhật cache
        }
        // -------------------------------------------------------------------

        // 3️⃣ Cập nhật thanh máu (cần phải gọi liên tục)
        UpdateFillImmediate();

        // 4️⃣ Xử lý thời gian hiển thị và ẩn
        if (_showTimer > 0f)
        {
            _showTimer -= Time.deltaTime;
        }

        bool isFullHealth = Mathf.Approximately(entityStats.currentHP, entityStats.maxHP); // Dùng Approximately cho float

        if (hideWhenFull)
        {
            // Ẩn nếu máu đầy VÀ timer đã hết
            if (isFullHealth && _showTimer <= 0f)
            {
                canvas.enabled = false;
            }
            // Nếu máu không đầy hoặc timer đang chạy, đảm bảo nó được bật
            else if (!isFullHealth || _showTimer > 0f)
            {
                canvas.enabled = true;
            }
        }

        // ⭐ Giữ hướng thanh máu đúng chiều (Giữ nguyên logic cũ để tránh lật scale X)
        Vector3 s = _initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    // Cập nhật giá trị fill ngay lập tức
    void UpdateFillImmediate()
    {
        if (entityStats == null) return;
        // Sử dụng entityStats.maxHP và entityStats.currentHP
        float max = Mathf.Max(1f, entityStats.maxHP);
        float t = Mathf.Clamp01(entityStats.currentHP / max);
        fill.fillAmount = t;
    }

    // ⭐ HÀM PUBLIC: Được gọi từ BaseStats để buộc hiển thị UI khi có sự kiện (nhận sát thương)
    public void ForceShow(float seconds = -1f)
    {
        // Nếu không có tham số, sử dụng thời gian mặc định của script
        if (seconds < 0) seconds = showSecondsAfterHit;

        _showTimer = seconds;
        if (canvas)
        {
            canvas.enabled = true;
        }
        // Cập nhật ngay lập tức sau khi bị đánh
        UpdateFillImmediate();
    }

    // HÀM PUBLIC: Được gọi từ BaseStats khi thực thể chết
    public void Hide()
    {
        _showTimer = 0f;
        if (canvas) canvas.enabled = false;
    }
}
