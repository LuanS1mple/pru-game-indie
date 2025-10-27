using UnityEngine;

public class WindBlast : MonoBehaviour
{
    // ❌ ĐÃ XÓA: public float speed;
    // ❌ ĐÃ XÓA: private Rigidbody2D rb;

    public float lifetime = 0.03f; // Thời gian hiệu ứng tồn tại (nên ngắn)
    public float damageAmount = 100f;

    // Hàm này sẽ được gọi khi Prefab được sinh ra
    public void Initialize(float scaleX) // Giờ chỉ cần scaleX để xoay hình
    {
        // Xoay hình ảnh (Sprite) của kỹ năng theo hướng nhìn của Player
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);

        // Tự hủy sau thời gian tồn tại
        Destroy(gameObject, lifetime);
    }

    // Logic gây sát thương (giữ nguyên)
    void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }
    void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }

    private void TryDealDamage(Collider2D collision)
    {
        // Gây sát thương chỉ lên Layer "Enemy"
        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        BaseStats enemyStats = collision.GetComponentInParent<BaseStats>();
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(damageAmount);
            // Sau khi gây sát thương, bạn có thể muốn hiệu ứng tự hủy ngay
            // Destroy(gameObject); 
        }
    }
}