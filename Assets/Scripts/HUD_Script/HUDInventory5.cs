//using UnityEngine;
//using UnityEngine.UI;

//public class HUDInventory5 : MonoBehaviour
//{
//    public static HUDInventory5 Instance { get; private set; }

//    [SerializeField] private Image[] slots = new Image[5];
//    [SerializeField] private Sprite emptySprite;

//    int count = 0;

//    void Awake()
//    {
//        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//        Instance = this;
//        ClearAll();
//    }

//    public void Add(Sprite icon)
//    {
//        if (icon == null) return;
//        if (count < slots.Length) { slots[count++].sprite = icon; }
//        else
//        {
//            for (int i = 1; i < slots.Length; i++) slots[i - 1].sprite = slots[i].sprite;
//            slots[^1].sprite = icon;
//        }
//    }

//    public void ClearAll()
//    {
//        foreach (var img in slots) if (img) img.sprite = emptySprite;
//        count = 0;
//    }
//}
using System;
using UnityEngine;
using UnityEngine.UI;

public class HUDInventory5 : MonoBehaviour
{
    public static HUDInventory5 Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;      // Kéo Player (có PlayerStats) vào đây
    [SerializeField] private Image[] slotIcons = new Image[5];
    [SerializeField] private Sprite emptySprite;

    // Dữ liệu mỗi slot
    [Serializable]
    public class SlotData
    {
        public bool occupied;
        public Sprite icon;
        public PickupType type;
        public int amount;
    }

    private SlotData[] slots;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (slotIcons == null || slotIcons.Length == 0)
            slotIcons = new Image[5];

        slots = new SlotData[slotIcons.Length];
        for (int i = 0; i < slots.Length; i++) slots[i] = new SlotData();

        RefreshUI();
    }

    void Update()
    {
        // Demo: bấm phím 1..5 để remove slot tương ứng
        if (Input.GetKeyDown(KeyCode.Alpha1)) RemoveAt(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) RemoveAt(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) RemoveAt(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) RemoveAt(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) RemoveAt(4);
    }

    // ======= API chính =======

    /// <summary>
    /// Thêm item. Với Heal -> tiêu thụ ngay, KHÔNG vào HUD.
    /// Với MaxHealthUp/AttackUp/DefenseUp -> cộng chỉ số + đưa vào HUD (nếu còn slot).
    /// </summary>
    public bool AddItem(PickupType type, int amount, Sprite icon)
    {
        if (!playerStats) { Debug.LogWarning("HUDInventory5: thiếu PlayerStats."); return false; }

        switch (type)
        {
            case PickupType.Heal:
                playerStats.Heal(amount);
                // Heal là consumable -> không giữ trong HUD
                return true;

            case PickupType.MaxHealthUp:
                playerStats.IncreaseMaxHealth(amount, healToFull: true);
                break;

            case PickupType.AttackUp:
                playerStats.IncreaseAttack(amount);
                break;

            case PickupType.DefenseUp:
                playerStats.IncreaseDefense(amount);
                break;
        }

        // tìm slot trống
        int free = FindFreeSlot();
        if (free < 0)
        {
            // Nếu hết chỗ, có thể đẩy trái -> phải (tuỳ bạn muốn)
            ShiftLeft();
            free = slots.Length - 1;
        }

        slots[free].occupied = true;
        slots[free].icon = icon;
        slots[free].type = type;
        slots[free].amount = amount;

        RefreshUI();
        return true;
    }

    /// <summary> Xoá 1 slot, hoàn tác chỉ số. </summary>
    public void RemoveAt(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        var s = slots[index];
        if (!s.occupied) return;
        if (!playerStats) return;

        // hoàn tác
        switch (s.type)
        {
            case PickupType.MaxHealthUp:
                playerStats.DecreaseMaxHealth(s.amount, clampCurrent: true);
                break;
            case PickupType.AttackUp:
                playerStats.DecreaseAttack(s.amount);
                break;
            case PickupType.DefenseUp:
                playerStats.DecreaseDefense(s.amount);
                break;
                // Heal không nằm trong HUD -> không có revert
        }

        // clear slot + dồn trái để không bị lỗ
        for (int i = index; i < slots.Length - 1; i++)
            CopySlot(slots[i + 1], slots[i]);
        ClearSlot(slots[^1]);

        RefreshUI();
    }

    /// <summary> Xoá toàn bộ HUD, hoàn tác toàn bộ buff. </summary>
    public void ClearAllAndRevert()
    {
        if (playerStats)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var s = slots[i];
                if (!s.occupied) continue;

                switch (s.type)
                {
                    case PickupType.MaxHealthUp: playerStats.DecreaseMaxHealth(s.amount, true); break;
                    case PickupType.AttackUp: playerStats.DecreaseAttack(s.amount); break;
                    case PickupType.DefenseUp: playerStats.DecreaseDefense(s.amount); break;
                }
            }
        }

        for (int i = 0; i < slots.Length; i++) ClearSlot(slots[i]);
        RefreshUI();
    }

    // ======= Helpers =======

    int FindFreeSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].occupied) return i;
        return -1;
    }

    void ShiftLeft()
    {
        for (int i = 1; i < slots.Length; i++)
            CopySlot(slots[i], slots[i - 1]);
        ClearSlot(slots[^1]);
    }

    void CopySlot(SlotData from, SlotData to)
    {
        to.occupied = from.occupied;
        to.icon = from.icon;
        to.type = from.type;
        to.amount = from.amount;
    }

    void ClearSlot(SlotData s)
    {
        s.occupied = false;
        s.icon = null;
        s.type = default;
        s.amount = 0;
    }

    void RefreshUI()
    {
        for (int i = 0; i < slotIcons.Length; i++)
        {
            var img = slotIcons[i];
            if (!img) continue;

            if (slots[i].occupied && slots[i].icon != null)
            {
                img.sprite = slots[i].icon;
                img.color = Color.white;
                img.enabled = true;
            }
            else
            {
                img.sprite = emptySprite;
                img.color = emptySprite ? Color.white : new Color(1, 1, 1, 0);
                img.enabled = true; // để thấy ô trống nếu có emptySprite
            }
        }
    }
}
