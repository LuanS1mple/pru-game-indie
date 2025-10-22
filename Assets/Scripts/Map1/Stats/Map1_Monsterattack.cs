using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map1_Monsterattack : MonoBehaviour
{
    [SerializeField] private float attackCooldown = 1f; // Thời gian chờ giữa 2 lần attack
    private float lastAttackTime = -999f;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                lastAttackTime = Time.time;
                AttackPlayer();
            }
        }
    }

    private void AttackPlayer()
    {
        Debug.LogWarning("Attack");
    }
}
