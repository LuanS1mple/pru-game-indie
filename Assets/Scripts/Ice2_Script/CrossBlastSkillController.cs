// CrossBlastSkillController.cs
using UnityEngine;

[RequireComponent(typeof(SkillCaster_CrossBlast))]
public class CrossBlastSkillController : MonoBehaviour
{
    [Header("Binding")]
    public SkillCaster_CrossBlast caster;     // auto-find nếu để trống
    public SkillSlotUI slotUI;                // kéo SkillSlot cho Cross vào
    public Sprite skillIcon;

    [Header("Gameplay")]
    public KeyCode key = KeyCode.Mouse1;      // chuột phải (tuỳ set)
    public float cooldown = 8f;
    public bool unlockedAtStart = true;
    public bool useUnscaledTime = false;

    [Header("Debug")]
    public bool debugLog = false;

    float cdRemain = 0f;
    bool unlocked;

    void Awake()
    {
        if (!caster) caster = GetComponent<SkillCaster_CrossBlast>();
        unlocked = unlockedAtStart;

        if (slotUI)
        {
            if (skillIcon) slotUI.SetIcon(skillIcon);
            slotUI.SetKey(key.ToString());
            slotUI.SetCooldown(0f, Mathf.Max(0.0001f, cooldown)); // hiển thị ready
        }
    }

    void Update()
    {
        // 1) Giảm CD
        if (cdRemain > 0f)
        {
            cdRemain -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (cdRemain < 0f) cdRemain = 0f;
        }

        // 2) Cập nhật HUD
        if (slotUI) slotUI.SetCooldown(cdRemain, Mathf.Max(0.0001f, cooldown));

        // 3) Khoá nếu chưa unlock
        if (!unlocked) return;

        // 4) Input
        if (Input.GetKeyDown(key))
        {
            if (cdRemain <= 0f)
            {
                if (caster) caster.Cast();
                cdRemain = cooldown;                 // bắt đầu hồi chiêu
                if (slotUI) slotUI.SetCooldown(cdRemain, cooldown);
                if (debugLog) Debug.Log("[CrossBlast] CAST -> start cooldown");
            }
            else if (debugLog)
            {
                Debug.Log($"[CrossBlast] Blocked, cdRemain={cdRemain:0.00}s");
            }
        }
    }

    // Cho phép script khác báo bắt đầu hồi chiêu (nếu bạn gọi từ Animation Event)
    public void NotifyCast(float customCooldown = -1f)
    {
        cdRemain = customCooldown > 0f ? customCooldown : cooldown;
        if (slotUI) slotUI.SetCooldown(cdRemain, Mathf.Max(0.0001f, cooldown));
    }

    public void Unlock()
    {
        unlocked = true;
        cdRemain = 0f;
        if (slotUI) slotUI.SetCooldown(0f, Mathf.Max(0.0001f, cooldown));
    }

    public void Lock() => unlocked = false;
}
