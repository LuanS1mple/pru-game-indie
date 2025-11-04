using UnityEngine;
using Pathfinding; // Dùng cho AIDestinationSetter
using System.Collections;

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
        int activeGhosts = GameObject.FindGameObjectsWithTag("Ghost").Length;
        Debug.Log($"👻 [MiniBossSummon] Đang có {activeGhosts}/{maxActiveGhosts} ghost trong scene.");

        if (activeGhosts >= maxActiveGhosts)
        {
            Debug.Log($"⚠️ [MiniBossSummon] Đã đạt giới hạn ghost tối đa — không triệu hồi thêm.");
            return;
        }

        float dir = _bossTransform.localScale.x >= 0 ? 1f : -1f;
        Vector3 spawnPos = _bossTransform.position + new Vector3(summonOffset.x * dir, summonOffset.y, 0f);

        Debug.Log($"📍 [MiniBossSummon] MiniBoss pos = {_bossTransform.position}, Ghost spawn pos = {spawnPos}, dir = {dir}");

        GameObject ghost = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
        if (ghost == null)
        {
            Debug.LogError("❌ [MiniBossSummon] Instantiate ghost thất bại!");
            return;
        }

        if (ghost.tag != "Ghost")
        {
            ghost.tag = "Ghost";
            Debug.Log($"🔖 [MiniBossSummon] Gán tag 'Ghost' cho {ghost.name}.");
        }

        if (_bossTransform.localScale.x < 0)
        {
            Vector3 scale = ghost.transform.localScale;
            scale.x *= -1;
            ghost.transform.localScale = scale;
            Debug.Log($"↩ [MiniBossSummon] Đảo hướng ghost do boss quay trái.");
        }

        Debug.Log($"✅ [MiniBossSummon] Ghost '{ghost.name}' đã spawn tại {ghost.transform.position}.");

        // Gán target sau một chút để đảm bảo Player đã tồn tại
        StartCoroutine(AssignTargetToGhost(ghost));
    }

    IEnumerator AssignTargetToGhost(GameObject ghost)
    {
        // chờ một frame để đảm bảo mọi object trong scene đã load
        yield return new WaitForSeconds(0.2f);

        if (ghost == null)
        {
            Debug.LogWarning("⚠️ [MiniBossSummon] Ghost bị hủy trước khi gán target!");
            yield break;
        }

        var destination = ghost.GetComponent<AIDestinationSetter>();
        if (destination == null)
        {
            Debug.LogWarning($"⚠️ [MiniBossSummon] Ghost '{ghost.name}' không có AIDestinationSetter!");
            yield break;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogWarning("⚠️ [MiniBossSummon] Không tìm thấy Player trong scene để gán target!");
            yield break;
        }

        // 🔹 Kiểm tra Layer để chắc chắn Player thuộc Layer "Player"
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerObj.layer != playerLayer)
        {
            Debug.LogWarning($"⚠️ [MiniBossSummon] Player có tag 'Player' nhưng không nằm trong Layer 'Player'. (Layer hiện tại: {LayerMask.LayerToName(playerObj.layer)})");
        }

        // ✅ Gán target thành công
        destination.target = playerObj.transform;
        Debug.Log($"🎯 [MiniBossSummon] Gán Player ({playerObj.name}) làm target cho ghost '{ghost.name}'.");
    }
}
