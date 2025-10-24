using UnityEngine;
using System.Collections;

public class HitboxEventRelayBoss : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        public string key = "A";                  // tên tuỳ ý: "A", "B", "Uppercut", ...
        public EnemyAttackHitbox hitbox;          // gắn EnemyAttackHitbox con
        [Tooltip("Bật/tắt nhanh collider mỗi lần enable để chắc chắn OnTriggerEnter kích hoạt kể cả khi player đứng yên")]
        public bool refreshOnEnable = true;
    }

    [Header("Hitboxes (kéo các object con vào)")]
    public Slot[] slots;

    // ==== API gọi từ Animation Event (không tham số) ====
    public void EnableA() => EnableByKey("A");
    public void DisableA() => DisableByKey("A");
    public void EnableB() => EnableByKey("B");
    public void DisableB() => DisableByKey("B");

    // ==== API gọi từ Animation Event (int) ====
    public void EnableByIndex(int i) => ToggleByIndex(i, true);
    public void DisableByIndex(int i) => ToggleByIndex(i, false);

    // ==== API gọi từ Animation Event (string) ====
    public void EnableByKey(string key) => ToggleByKey(key, true);
    public void DisableByKey(string key) => ToggleByKey(key, false);

    // ----------------------------------------------------

    void ToggleByIndex(int i, bool on)
    {
        if (i < 0 || i >= slots.Length) return;
        Toggle(slots[i], on);
    }

    void ToggleByKey(string key, bool on)
    {
        if (string.IsNullOrEmpty(key)) return;
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].key == key) { Toggle(slots[i], on); return; }
    }

    void Toggle(Slot s, bool on)
    {
        if (!s.hitbox) return;
        if (on)
        {
            if (s.refreshOnEnable) StartCoroutine(RefreshAndEnable(s.hitbox));
            else s.hitbox.EnableHitbox();
        }
        else
        {
            s.hitbox.DisableHitbox();
        }
    }

    IEnumerator RefreshAndEnable(EnemyAttackHitbox hb)
    {
        // đảm bảo mỗi nhát vung đều tạo cửa sổ mới, kể cả khi chồng collider
        var col = hb.GetComponent<Collider2D>();
        if (col)
        {
            col.enabled = false;
            yield return null; // chờ 1 frame
        }
        hb.EnableHitbox();
    }

    // Tuỳ chọn: tắt TẤT CẢ hitbox (gọi ở đầu/ cuối Attack để tránh double-hit)
    public void DisableAll()
    {
        foreach (var s in slots) if (s.hitbox) s.hitbox.DisableHitbox();
    }
}
