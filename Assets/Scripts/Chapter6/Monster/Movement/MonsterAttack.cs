using System;
using System.Collections;
using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 1f; // Thời gian chờ giữa 2 lần attack
    private float lastAttackTime = -999f;

    [SerializeField] private LayerMask playerLayer;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // chỉ xét nếu collider thuộc playerLayer
        if (((1 << collision.gameObject.layer) & playerLayer) == 0) return;
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            AttackPlayer(collision);
        }
    }


    private void AttackPlayer(Collider2D collision)
    {
        BaseStats playerStats = collision.GetComponent<BaseStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(10);
            Debug.LogWarning("Attack");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy BaseStats trên Player!");
        }
    }
}
