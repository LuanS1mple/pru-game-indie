using UnityEngine;
using Pathfinding; // 🔹 Cần import để dùng AIDestinationSetter

public class MiniBossSummon : MonoBehaviour
{
    [Header("Summon Settings")]
    [Tooltip("Prefab của con Ghost sẽ được triệu hồi.")]
    public GameObject ghostPrefab;

    [Tooltip("Thời gian chờ giữa các lần triệu hồi (giây).")]
    public float summonInterval = 15f;

    [Tooltip("Vị trí offset khi triệu hồi, tính từ miniboss.")]
    public Vector2 summonOffset = new Vector2(2f, 0f);

    [Tooltip("Số lượng ghost tối đa có thể tồn tại cùng lúc.")]
    public int maxActiveGhosts = 3;

    private float summonTimer;
    private Transform _bossTransform;

    void Start()
    {
        _bossTransform = transform;
        summonTimer = summonInterval;

        Debug.Log($"✅ [MiniBossSummon] Khởi tạo thành công — sẽ triệu hồi Ghost mỗi {summonInterval} giây.");
        if (ghostPrefab == null)
            Debug.LogError("❌ [MiniBossSummon] ghostPrefab chưa được gán trong Inspector!");
    }

    void Update()
    {
        if (!ghostPrefab) return;

        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            TrySummon();
            summonTimer = summonInterval;
        }
    }

    void TrySummon()
    {
        // Đếm xem hiện có bao nhiêu ghost đang tồn tại
        int activeGhosts = GameObject.FindGameObjectsWithTag("Ghost").Length;
        Debug.Log($"👻 [MiniBossSummon] Đang có {activeGhosts}/{maxActiveGhosts} ghost trong scene.");

        if (activeGhosts >= maxActiveGhosts)
        {
            Debug.Log($"⚠️ [MiniBossSummon] Đã đạt giới hạn ghost tối đa — không triệu hồi thêm.");
            return;
        }

        // ✅ Xác định hướng boss đang quay
        float dir = _bossTransform.localScale.x >= 0 ? 1f : -1f;

        // ✅ Tính vị trí spawn theo hướng đó
        Vector3 spawnPos = _bossTransform.position + new Vector3(summonOffset.x * dir, summonOffset.y, 0f);

        // 🧭 Ghi log chi tiết vị trí spawn
        Debug.Log($"📍 [MiniBossSummon] MiniBoss pos = {_bossTransform.position}, Ghost spawn pos = {spawnPos}, dir = {dir}");

        // ✅ Triệu hồi ghost
        GameObject ghost = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);

        if (ghost == null)
        {
            Debug.LogError("❌ [MiniBossSummon] Instantiate ghost thất bại (ghost == null)!");
            return;
        }

        // ✅ Tag / Layer check
        if (ghost.tag != "Ghost")
        {
            ghost.tag = "Ghost";
            Debug.Log($"🔖 [MiniBossSummon] Gán tag 'Ghost' cho {ghost.name}.");
        }

        Debug.Log($"✅ [MiniBossSummon] Ghost đã được tạo: {ghost.name} tại {ghost.transform.position}, Layer = {ghost.layer}");

        // ✅ Quay ghost theo hướng miniboss
        if (_bossTransform.localScale.x < 0)
        {
            Vector3 scale = ghost.transform.localScale;
            scale.x *= -1;
            ghost.transform.localScale = scale;
            Debug.Log($"↩ [MiniBossSummon] Đảo hướng ghost do boss đang quay trái (localScale.x < 0).");
        }

        // ✅ Kiểm tra Renderer có đang bị ẩn
        var renderer = ghost.GetComponentInChildren<SpriteRenderer>();
        if (renderer == null)
            Debug.LogWarning($"⚠️ [MiniBossSummon] Ghost '{ghost.name}' không có SpriteRenderer!");
        else if (!renderer.enabled)
            Debug.LogWarning($"⚠️ [MiniBossSummon] Ghost '{ghost.name}' có SpriteRenderer nhưng bị tắt (enabled = false)!");
        else
            Debug.Log($"🟢 [MiniBossSummon] Ghost '{ghost.name}' Renderer OK, SortingLayer = {renderer.sortingLayerName}, Order = {renderer.sortingOrder}");

        // ✅ Gán Player làm target cho ghost (CÁCH 1)
        var destination = ghost.GetComponent<AIDestinationSetter>();
        if (destination != null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                destination.target = playerObj.transform;
                Debug.Log($"🎯 [MiniBossSummon] Gán Player ({playerObj.name}) làm target cho ghost '{ghost.name}'.");
            }
            else
            {
                Debug.LogWarning("⚠️ [MiniBossSummon] Không tìm thấy Player trong scene để gán cho ghost!");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ [MiniBossSummon] Ghost '{ghost.name}' không có AIDestinationSetter!");
        }
    }
}
