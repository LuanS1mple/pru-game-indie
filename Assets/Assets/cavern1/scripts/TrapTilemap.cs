using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class TrapTilemap : MonoBehaviour
{
    [Header("Tilemap & Damage Settings")]
    public Tilemap trapTilemap;       // Tilemap chứa trap
    public int damage = 2;
    public float damageInterval = 2f;

    private Dictionary<GameObject, float> playerTimers = new Dictionary<GameObject, float>();

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playerTimers.ContainsKey(other.gameObject))
                playerTimers[other.gameObject] = 0f;

            playerTimers[other.gameObject] += Time.deltaTime;

            if (playerTimers[other.gameObject] >= damageInterval)
            {
                BaseStats playerStats = other.GetComponent<BaseStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamageTrap(damage);  
                }

                playerTimers[other.gameObject] = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && playerTimers.ContainsKey(other.gameObject))
        {
            playerTimers.Remove(other.gameObject);
        }
    }
}
