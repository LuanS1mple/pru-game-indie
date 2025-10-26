using UnityEngine;

public class WindBlast : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 1.5f; // Thời gian hiệu ứng tồn tại
    public float damageAmount = 10f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Đảm bảo Rigidbody tồn tại
        if (rb == null)
            Debug.LogError("WindBlast thiếu Rigidbody2D!", this);

        Destroy(gameObject, lifetime);
    }

    // Hàm khởi tạo, thiết lập hướng bắn
    public void Initialize(Vector2 direction, float scaleX)
    {
        // Xoay hình ảnh (Sprite) của kỹ năng theo hướng bắn
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);

        // Thiết lập vận tốc (chỉ theo phương ngang)
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }

        // Cần có logic để hiệu ứng không bị rơi (nếu Rigidbody có Gravity)
        if (rb != null)
        {
            rb.gravityScale = 0;
        }
    }

    // Logic gây sát thương (giống như logic hitbox của bạn)
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
        // Ví dụ: Kỹ năng chỉ đánh trúng Layer "Enemy"
        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        BaseStats enemyStats = collision.GetComponentInParent<BaseStats>();
        if (enemyStats != null)
        {
            enemyStats.TakeDamage(damageAmount);
            // Sau khi gây sát thương, bạn có thể muốn hiệu ứng tan biến hoặc không
            // Ví dụ: Destroy(gameObject); // Tùy chọn: Tự hủy sau khi trúng 1 mục tiêu
        }
    }
}