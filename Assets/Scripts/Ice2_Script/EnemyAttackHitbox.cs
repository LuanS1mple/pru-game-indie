using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayers;   // layer Player, hoặc để 0 = mọi layer
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        col.enabled = false;
    }

    // Animation Events
    public void EnableHitbox() { col.enabled = true; }
    public void DisableHitbox() { col.enabled = false; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (targetLayers != 0 && ((1 << other.gameObject.layer) & targetLayers) == 0) return;

        var stats = other.GetComponent<PlayerStats>() ?? other.GetComponentInParent<PlayerStats>();
        if (stats != null) stats.TakeDamage(damage);
    }
}

