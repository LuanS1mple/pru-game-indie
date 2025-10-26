using UnityEngine;
using System.Collections;

public class SpellDamage : MonoBehaviour
{
    private Collider2D spellCollider;

    void Start()
    {
        // Lấy collider 2D gắn trên chiêu
        spellCollider = GetComponent<Collider2D>();

        if (spellCollider == null)
        {
            Debug.LogError("Không tìm thấy Collider2D trên SpellDamage!");
            return;
        }

        // Tạm tắt collider ngay khi khởi tạo
        spellCollider.enabled = false;

        // Bắt đầu coroutine để bật collider sau 1 giây
        StartCoroutine(EnableColliderAfterDelay());
    }

    IEnumerator EnableColliderAfterDelay()
    {
        yield return new WaitForSeconds(0.3f); // đợi 0.3 giây

        spellCollider.enabled = true;
        spellCollider.isTrigger = true;

        Debug.Log("Collider của spell đã được bật sau 1 giây!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            BaseStats baseStats = other.GetComponent<BaseStats>();
            baseStats.TakeDamage(20);
        }
    }
}
