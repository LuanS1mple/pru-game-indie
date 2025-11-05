using UnityEngine;
using System.Collections.Generic;

public class LightningPillarBehavior : MonoBehaviour
{
    // ⭐ CÁC BIẾN NÀY ĐƯỢC THIẾT LẬP TRÊN INSPECTOR CỦA PREFAB ⭐
    [Header("Pillar Settings")]
    public float damageAmount = 20f;     // Sát thương gây ra
    public float duration = 0.5f;        // Thời gian cột sét tồn tại

    [HideInInspector] public int targetLayer;

    // Dùng HashSet để đảm bảo chỉ gây sát thương 1 lần cho mỗi đối tượng
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    void Start()
    {
        // TỰ ĐỘNG HỦY SAU KHI HẾT THỜI GIAN
        Destroy(gameObject, duration);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void TryDealDamage(Collider2D collision)
    {
        // Kiểm tra Layer (Layer được Boss truyền vào: Layer Player)
        if (collision.gameObject.layer != targetLayer) return;
        // Kiểm tra đã gây sát thương chưa
        if (hitTargets.Contains(collision)) return;

        // Gây sát thương
        BaseStats targetStats = collision.GetComponentInParent<BaseStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(damageAmount);
            hitTargets.Add(collision);
            Debug.Log($"[Cột Sét] gây {damageAmount} sát thương lên {collision.name}");
        }
    }
}