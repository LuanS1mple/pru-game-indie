using UnityEngine;

public class CrossBlast : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 20f;
    public float maxDistance = 10f; // ⭐ MỚI: Giới hạn khoảng cách
    private Rigidbody2D rb;
    private Vector3 startPosition;  // ⭐ MỚI: Vị trí bắt đầu
    private float flyDirectionX;

    [Header("Damage Settings")]
    public float initialHitDamage = 25f;

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;

    // Animator và Fly Animation Name không cần thiết nếu animation chạy sẵn

    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position; // Lấy vị trí bắt đầu

        if (rb == null) Debug.LogError("CrossBlast thiếu Rigidbody2D!");

        // ⭐ XÓA Dòng kiểm tra Animator
    }

    public void Initialize(float scaleX)
    {
        // Xoay hình ảnh
        transform.localScale = new Vector3(scaleX, transform.localScale.y, transform.localScale.z);
        flyDirectionX = scaleX;

        // Bắt đầu bay ngang theo trục X
        if (rb != null)
        {
            rb.velocity = new Vector2(flyDirectionX * speed, 0f);
        }

        // ⭐ XÓA Dòng kích hoạt Animation (vì nó chạy sẵn)
    }

    private void Update()
    {
        // ⭐ LOGIC MỚI: Kiểm tra khoảng cách để kích hoạt nổ nếu không va chạm
        if (!hasHit && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            hasHit = true; // Đánh dấu đã kích hoạt
            ActivateExplosion(null); // Kích hoạt nổ (không có Enemy va chạm)
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra va chạm với vật cản hoặc Enemy... (Logic giữ nguyên)
        if (hasHit) return;

        // Nếu chạm vào vật cản (không phải Enemy)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            hasHit = true;
            // Dừng bay
            if (rb != null) rb.velocity = Vector2.zero;
            // Kích hoạt nổ ngay tại tường/vật cản
            ActivateExplosion(null);
            return;
        }

        // Chỉ xử lý Enemy
        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy")) return;

        hasHit = true;

        // Dừng vật lý và tắt collider
        if (rb != null) rb.velocity = Vector2.zero;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Gây sát thương lên kẻ địch đầu tiên
        BaseStats initialEnemyStats = collision.GetComponentInParent<BaseStats>();
        if (initialEnemyStats != null)
        {
            initialEnemyStats.TakeDamage(initialHitDamage);
        }

        // Kích hoạt vụ nổ và truyền đối tượng Enemy đã bị trúng
        ActivateExplosion(initialEnemyStats?.gameObject);
    }

    // Hàm kích hoạt nổ (Giữ nguyên)
    private void ActivateExplosion(GameObject initialHitEnemy)
    {
        // ... (Logic sinh Prefab Nổ và Destroy(gameObject))
        if (explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            ExplosionHandler handler = explosionInstance.GetComponent<ExplosionHandler>();
            if (handler != null)
            {
                handler.SetInitialHitEnemy(initialHitEnemy);
            }
        }
        Destroy(gameObject);
    }
}