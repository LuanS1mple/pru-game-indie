using System.Collections;
using UnityEngine;

public class BossGroundAOE : MonoBehaviour
{
    public int damage = 20;
    public float activeTime = 0.5f; // Thời gian vùng sát thương tồn tại

    private Collider2D aoeCollider;
    private bool canDamage = false;

    void Awake()
    {
        aoeCollider = GetComponent<Collider2D>();
        aoeCollider.enabled = false; // ẩn khi bắt đầu
    }

    // Gọi khi Boss tiếp đất
    public void ActivateAOE()
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(ActivateRoutine());
    }

    private IEnumerator ActivateRoutine()
    {
        canDamage = true;
        aoeCollider.enabled = true;

        Debug.Log("JumpAttack bật collider!");

        yield return new WaitForSeconds(activeTime);

        canDamage = false;
        aoeCollider.enabled = false;

        Debug.Log("JumpAttack tắt collider!");
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
