using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public BaseStats playerStats;
    public Collider2D attackCollider;


    private bool canDealDamage = false;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    void Start()
    {
        playerStats = GetComponent<BaseStats>();
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // Gọi từ Animation Event
    public void EnableAttackCollider()
    {
        if (attackCollider == null) return;
        canDealDamage = true;

        attackCollider.enabled = false; // reset collider để OnTriggerEnter2D hoạt động lại
        attackCollider.enabled = true;

        hitEnemies.Clear();
    }

    // Gọi từ Animation Event
    public void DisableAttackCollider()
    {
        if (attackCollider == null) return;
        canDealDamage = false;
        attackCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDealDamage) return;

        if (collision.CompareTag("Enemy") && !hitEnemies.Contains(collision))
        {
            hitEnemies.Add(collision);
            BaseStats enemyStats = collision.GetComponent<BaseStats>();
            if (enemyStats != null)
            {
                enemyStats.TakeDamage(playerStats.attack);
            }
        }
    }
}
