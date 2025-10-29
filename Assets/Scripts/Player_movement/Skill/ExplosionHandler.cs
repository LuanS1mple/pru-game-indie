using UnityEngine;
using System.Collections.Generic;

public class ExplosionHandler : MonoBehaviour
{
    [Header("Explosion Damage")]
    public float explosionDamage = 50f;
    public float lifetime = 0.5f;

    // HashSet để theo dõi Enemy đã bị trúng sát thương nổ
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    // Biến để lưu Enemy đã bị WindBlast va chạm lần đầu
    private GameObject initialHitEnemy = null;

    private bool damageDealt = false; // Đảm bảo logic sát thương chỉ chạy 1 lần

    private void Start()
    {
        // Tự hủy sau khi animation kết thúc
        Destroy(gameObject, lifetime);

        // Logic gây sát thương nổ lan được gọi ngay lập tức
        if (!damageDealt)
        {
            DealExplosionDamage();
            damageDealt = true;
        }
    }

    // ⭐ HÀM MỚI: Nhận Enemy đã bị trúng từ WindBlast
    public void SetInitialHitEnemy(GameObject enemy)
    {
        initialHitEnemy = enemy;
    }

    private void DealExplosionDamage()
    {
        // Giả sử Explosion Prefab có CircleCollider2D để định nghĩa phạm vi
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null || !circleCollider.isTrigger)
        {
            Debug.LogError("ExplosionHandler cần một CircleCollider2D được set Is Trigger để xác định phạm vi nổ!");
            return;
        }

        float radius = circleCollider.radius;
        Vector3 center = transform.position + (Vector3)circleCollider.offset;

        // Tìm tất cả Collider2D trong bán kính nổ
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(center, radius, LayerMask.GetMask("Enemy"));

        foreach (Collider2D hit in hitObjects)
        {
            BaseStats enemyStats = hit.GetComponentInParent<BaseStats>();
            if (enemyStats != null)
            {
                // Lấy GameObject gốc của Enemy
                GameObject enemyObject = enemyStats.gameObject;

                

                // 2. Gây sát thương nổ (và chỉ 1 lần)
                if (!hitEnemies.Contains(enemyObject))
                {
                    enemyStats.TakeDamage(explosionDamage);
                    hitEnemies.Add(enemyObject);
                }
            }
        }
    }

    // Tùy chọn: Thêm OnDrawGizmosSelected để thấy phạm vi nổ trong Scene view
    private void OnDrawGizmosSelected()
    {
        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null && circleCollider.enabled)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
        }
    }
}