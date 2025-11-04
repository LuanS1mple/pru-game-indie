using UnityEngine;
using System.Collections.Generic;

public class LightningPillarBehavior : MonoBehaviour
{
    [HideInInspector] public float damageAmount;
    [HideInInspector] public int targetLayer;

    // Dùng HashSet để đảm bảo chỉ gây sát thương 1 lần cho mỗi Player
    private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }
    private void TryDealDamage(Collider2D collision)
    {
        if (collision.gameObject.layer != targetLayer) return;
        if (hitTargets.Contains(collision)) return;
        BaseStats targetStats = collision.GetComponentInParent<BaseStats>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(damageAmount);
            hitTargets.Add(collision);
            Debug.Log($"Cột sét gây {damageAmount} sát thương lên {collision.name}");
        }
    }
}