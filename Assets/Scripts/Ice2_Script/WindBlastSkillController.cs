using UnityEngine;

/// HUD-only: KHÔNG đọc input, KHÔNG cast. Chỉ hiển thị icon + cooldown.
/// Hãy gọi NotifyCast(...) khi chiêu bắt đầu được tung (từ movement hoặc Animation Event).
public class WindBlastSkillController : MonoBehaviour
{
    [Header("HUD Binding")]
    public SkillSlotUI slotUI;           // kéo SkillSlot_1
    public Sprite skillIcon;             // icon

    [Header("Cooldown")]
    [Tooltip("Thời gian hồi mặc định (nếu NotifyCast không truyền vào).")]
    public float cooldown = 5f;

    [Tooltip("Đếm theo unscaled time (hữu ích khi pause/timeScale=0).")]
    public bool useUnscaledTime = false;

    [Header("Unlock")]
    public bool unlockedAtStart = true;

    [Header("Debug")]
    public bool debugLog = false;

    // trạng thái
    float cdRemain = 0f;
    bool unlocked = false;

    void Awake()
    {
        unlocked = unlockedAtStart;

        if (slotUI)
        {
            if (skillIcon) slotUI.SetIcon(skillIcon);
            // Nếu bạn muốn hiện phím ở HUD, set từ ngoài: slotUI.SetKey("E");
            slotUI.SetCooldown(0f, Mathf.Max(0.0001f, cooldown)); // hiển thị sẵn sàng
        }
    }

    void Update()
    {
        // giảm CD
        if (cdRemain > 0f)
        {
            cdRemain -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (cdRemain < 0f) cdRemain = 0f;
        }

        // cập nhật HUD
        if (slotUI) slotUI.SetCooldown(cdRemain, Mathf.Max(0.0001f, cooldown));
    }

    // ====== API để movement/AnimationEvent báo "đã tung chiêu" ======

    /// Gọi khi chiêu WindBlast VỪA được tung thành công.
    /// Thường gọi ngay sau khi bạn set nextWindBlastTime trong movement
    /// hoặc đặt Animation Event ở frame cast.
    public void NotifyCast(float customCooldown = -1f)
    {
        if (!unlocked)
        {
            if (debugLog) Debug.Log("[WindBlast] NotifyCast bị bỏ qua (skill locked)", this);
            return;
        }

        cdRemain = (customCooldown > 0f) ? customCooldown : cooldown;

        if (debugLog) Debug.Log($"[WindBlast] Bắt đầu cooldown = {cdRemain:0.00}s", this);

        if (slotUI) slotUI.SetCooldown(cdRemain, Mathf.Max(0.0001f, cooldown));
    }

    /// Mở khóa chiêu (vd sau khi giết boss)
    public void Unlock()
    {
        unlocked = true;
        cdRemain = 0f;
        if (slotUI) slotUI.SetCooldown(0f, Mathf.Max(0.0001f, cooldown));
        if (debugLog) Debug.Log("[WindBlast] Unlocked", this);
    }

    /// Khóa lại chiêu (tùy chọn)
    public void Lock()
    {
        unlocked = false;
        if (debugLog) Debug.Log("[WindBlast] Locked", this);
    }

    /// Cho HUD biết còn bao nhiêu s hồi chiêu (nếu cần nơi khác đọc)
    public float GetRemainingCooldown() => cdRemain;
}
