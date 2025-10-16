using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public BaseStats playerStats;
    public Collider2D attackCollider;     // hitbox (isTrigger = true)
    private attack attackScript;          // script combo bạn đã có

    private bool canDealDamage = false;   // chỉ bật khi đang tấn công

    void Start()
    {
        playerStats = GetComponent<BaseStats>();
        attackScript = GetComponent<attack>();

        if (attackCollider != null)
            attackCollider.enabled = false;  // tắt khi chưa tấn công
    }

    void Update()
    {
        if (attackScript != null && attackScript.isAttacking)
        {
            if (!canDealDamage)
            {
                EnableAttackCollider();
            }
        }
        else
        {
            if (canDealDamage)
            {
                DisableAttackCollider();
            }
        }
    }

    void EnableAttackCollider()
    {
        canDealDamage = true;
        attackCollider.enabled = true;
    }

    void DisableAttackCollider()
    {
        canDealDamage = false;
        attackCollider.enabled = false;
    }

    // Khi hitbox va chạm quái
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canDealDamage) return;
        if (collision.CompareTag("Enemy"))
        {
            BaseStats enemyStats = collision.GetComponent<BaseStats>();
            if (enemyStats != null && !enemyStats.isDead)
            {
                enemyStats.TakeDamage(playerStats.attack);
            }
        }
    }
}
