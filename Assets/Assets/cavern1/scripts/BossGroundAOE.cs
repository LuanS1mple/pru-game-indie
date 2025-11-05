using System.Collections;
using UnityEngine;

public class BossGroundAOE : MonoBehaviour
{
    [Header("AOE Settings")]
    public int damage = 20;
    public float activeTime = 0.5f; // Thời gian vùng sát thương tồn tại

    [Header("Effects")]
    public GameObject leftDustPrefab;   // 💨 Prefab bụi bên trái
    public GameObject rightDustPrefab;  // 💨 Prefab bụi bên phải
    public Transform effectSpawnPoint;  // Vị trí sinh bụi (thường là chân Boss)
    public Animator bossAnimator;       // Animator của Boss (ví dụ: Idle sau khi chạm đất)

    public GameObject landDustEffect;  

    private Collider2D aoeCollider;
    private bool canDamage = false;

    void Awake()
    {
        aoeCollider = GetComponent<Collider2D>();
        aoeCollider.enabled = false;
    }

    // Gọi khi Boss tiếp đất
    public void ActivateAOE()
    {
        // Gọi animation bụi
        if (landDustEffect != null)
        {
            GameObject dust = Instantiate(
                landDustEffect,
                effectSpawnPoint != null ? effectSpawnPoint.position : transform.position,
                Quaternion.identity
            );

            // Hủy sau 1s để dọn rác
            Destroy(dust, 1f);
        }

        Debug.Log("💨 Hiệu ứng bụi được kích hoạt!");
    }

    private IEnumerator ActivateRoutine()
    {
        canDamage = true;
        aoeCollider.enabled = true;

        // Chuyển animation về Idle (sau khi chạm đất)
        if (bossAnimator != null)
            bossAnimator.Play("BossIdle");

        // 💥 Hiệu ứng bụi khi chạm đất
        Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;

        // Tạo bụi bên trái
        if (leftDustPrefab != null)
        {
            Instantiate(leftDustPrefab, spawnPos, Quaternion.identity);
        }

        // Tạo bụi bên phải
        if (rightDustPrefab != null)
        {
            Instantiate(rightDustPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log("💥 Boss tiếp đất → kích hoạt AOE + bụi trái phải!");

        yield return new WaitForSeconds(activeTime);

        canDamage = false;
        aoeCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canDamage) return;

        if (other.CompareTag("Player"))
        {
            var stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
                Debug.Log($"[BossAOE] Player trúng đòn dậm đất! (-{damage})");
            }
        }
    }
}
