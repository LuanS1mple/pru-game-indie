using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    public BaseStats playerStats;
    public Collider2D attackCollider;
    public float knockbackForce = 3f; // lực đẩy nhẹ, có thể chỉnh từ 2–5f

    private bool canDealDamage = false;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    void Start()
    {
        playerStats = GetComponent<BaseStats>();
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    // --- Animation Event: bắt đầu đòn đánh ---
    public void EnableAttackCollider()
    {
        if (attackCollider == null) return;

        canDealDamage = true;
        hitEnemies.Clear();
        StartCoroutine(RefreshCollider());
    }

    IEnumerator RefreshCollider()
    {
        attackCollider.enabled = false;
        yield return null;

        // Dịch nhẹ hitbox 0.01f để kích OnTriggerEnter
        Vector3 originalPos = attackCollider.transform.localPosition;
        attackCollider.transform.localPosition += new Vector3(0.01f, 0, 0);
        attackCollider.enabled = true;
        yield return null;
        attackCollider.transform.localPosition = originalPos;
    }

    // --- Animation Event: kết thúc đòn đánh ---
    public void DisableAttackCollider()
    {
        if (attackCollider == null) return;

        canDealDamage = false;
        attackCollider.enabled = false;
    }

    // --- Gây sát thương ---
    private void TryDealDamage(Collider2D collision)
    {
        if (!canDealDamage) return;

      
        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        if (hitEnemies.Contains(collision)) return;

        BaseStats enemyStats = collision.GetComponentInParent<BaseStats>();

        if (enemyStats != null)
        {
            enemyStats.TakeDamage(playerStats.attack);
            hitEnemies.Add(collision);
            Rigidbody2D enemyRb = collision.attachedRigidbody;
            if (enemyRb != null)
            {
                Vector2 dir = (collision.transform.position - transform.position).normalized;

                // Chỉ đẩy ngang, không đẩy lên
                dir.y = 0f;

                // Reset vận tốc cũ trước khi đẩy
                enemyRb.velocity = Vector2.zero;
                enemyRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }

            Debug.Log($"Hit enemy: {collision.name}");
        }
    }


    // --- OnTrigger Events ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void OnDrawGizmos()
    {
        if (attackCollider != null)
        {
            Gizmos.color = attackCollider.enabled ? Color.red : Color.gray;
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
    }
}
