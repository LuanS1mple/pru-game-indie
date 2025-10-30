using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropITem : MonoBehaviour
{
    [System.Serializable]
    public class DropEntry
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float chance = 1f;
        public Vector2Int countRange = new Vector2Int(1, 1);
        [Header("Physics (nếu prefab có Rigidbody2D)")]
        public Vector2 impulse = new Vector2(1.5f, 3.5f);
        public float spawnSpreadX = 0.2f;
        public float torque = 0f;
    }

    public enum TriggerMode { Manual, OnDisable, OnDestroy }

    [Header("Drop Settings")]
    public TriggerMode triggerMode = TriggerMode.OnDestroy; // ✔ rơi đồ khi enemy bị Destroy
    public DropEntry[] drops;

    [Header("Spawn")]
    // ⭐ Dùng để gán vị trí sinh loot chính xác (ví dụ: chân quái)
    public Transform spawnPointOverride;
    public Transform spawnRoot;
    public Vector3 spawnOffset = new Vector3(0f, 0.05f, 0f);

    // ⭐ PHẦN THÊM VÀO: Ground Snapping
    [Header("Ground Snapping")]
    public LayerMask groundLayer;
    public float raycastDistance = 5f;

    [Header("One-shot & Cleanup")]
    public bool dropOnce = true;
    public bool destroyAfterDrop = false;
    public float destroyDelay = 2f;

    bool _dropped;
    bool isQuitting = false;

    void OnApplicationQuit()
    {
        // Unity sắp thoát Play Mode / đóng app
        isQuitting = true;
    }

    void Awake()
    {
        if (!spawnRoot) spawnRoot = transform;
        if (!spawnPointOverride) spawnPointOverride = transform; // Mặc định là transform của Enemy

        // Cố gắng thiết lập mặc định cho Ground Layer
        if (groundLayer.value == 0)
        {
            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying || isQuitting) return;
        if (triggerMode == TriggerMode.OnDisable) Drop();
    }

    void OnDestroy()
    {
        if (!Application.isPlaying || isQuitting) return;
        if (triggerMode == TriggerMode.OnDestroy) Drop();
    }

    public void Drop()
    {
        if (_dropped && dropOnce) return;
        _dropped = true;

        if (drops == null || drops.Length == 0) return;

        // Sử dụng spawnPointOverride làm vị trí cơ sở cho Raycast và X.
        Vector3 baseWorldPos = spawnPointOverride.position;

        // --- LOGIC RAYCAST TÌM MẶT ĐẤT ---
        Vector3 groundPos = Vector3.zero;

        // Cố định offset chiều cao Raycast (ví dụ: 1.0f)
        const float RAYCAST_HEIGHT_OFFSET = 1.0f;

        // ⭐ ĐIỀU CHỈNH: Chiều cao loot sẽ nhô lên khỏi mặt đất (1 unit)
        const float LOOT_RISE_HEIGHT = 1.0f;

        // Raycast bắt đầu từ trên vị trí spawnPointOverride 
        Vector3 raycastStart = baseWorldPos + Vector3.up * RAYCAST_HEIGHT_OFFSET;
        float totalRaycastDistance = raycastDistance + RAYCAST_HEIGHT_OFFSET;

        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, totalRaycastDistance, groundLayer);

        if (hit.collider != null)
        {
            // Tìm thấy mặt đất: giữ X của Enemy (baseWorldPos.x), đặt Y là điểm chạm của Raycast.
            // groundPos lúc này là TỌA ĐỘ MẶT ĐẤT (Y) tại vị trí X của quái.
            groundPos = new Vector3(baseWorldPos.x, hit.point.y, baseWorldPos.z);
        }
        else
        {
            // Không tìm thấy mặt đất: sử dụng vị trí của spawnPointOverride làm dự phòng
            groundPos = baseWorldPos;
            Debug.LogWarning($"[{gameObject.name}] Không tìm thấy Layer 'Ground' ({LayerMask.LayerToName(groundLayer)}) dưới vật thể. Loot spawn tại vị trí Enemy (Không dính xuống đất).");
        }
        // --- KẾT THÚC LOGIC RAYCAST ---

        int totalLootCount = 0; // Biến đếm số lượng loot được drop

        foreach (var d in drops)
        {
            if (!d.prefab) continue;
            if (Random.value > d.chance) continue;

            int min = Mathf.Max(0, d.countRange.x);
            int max = Mathf.Max(min, d.countRange.y);
            int count = Random.Range(min, max + 1);
            if (count <= 0) continue;

            for (int i = 0; i < count; i++)
            {
                // ⭐ ÁP DỤNG LOOT_RISE_HEIGHT (1f)
                // Lấy groundPos (mặt đất), thêm X ngẫu nhiên và nâng Y lên 1f.
                Vector3 pos = groundPos + new Vector3(
                    Random.Range(-d.spawnSpreadX, d.spawnSpreadX),
                    LOOT_RISE_HEIGHT,
                    0f);

                var go = Instantiate(d.prefab, pos, Quaternion.identity);

                var rb = go.GetComponent<Rigidbody2D>();
                if (rb)
                {
                    float dirX = Random.value < 0.5f ? -1f : 1f;
                    float vx = dirX * Random.Range(0.4f * d.impulse.x, d.impulse.x);
                    float vy = Random.Range(0.6f * d.impulse.y, d.impulse.y);
                    rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
                    if (Mathf.Abs(d.torque) > 0f) rb.AddTorque(Random.Range(-d.torque, d.torque), ForceMode2D.Impulse);
                }
                totalLootCount++; // Tăng biến đếm
            }
        }

        // LOG THÔNG BÁO DROP
        if (totalLootCount > 0)
        {
            // groundPos.y + LOOT_RISE_HEIGHT là vị trí Y thực tế của loot
            Debug.Log($"[{gameObject.name}] ĐÃ TẠO {totalLootCount} vật phẩm loot. Vị trí spawn Y: {groundPos.y + LOOT_RISE_HEIGHT}.");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Kích hoạt Drop nhưng không có vật phẩm nào được tạo ra (do tỷ lệ rơi hoặc Count Range bằng 0).");
        }
        // KẾT THÚC LOG

        if (destroyAfterDrop) Destroy(gameObject, destroyDelay);
    }

    public void DropAndDestroy()
    {
        Drop();
        if (!destroyAfterDrop) Destroy(gameObject, destroyDelay);
    }

    // Gizmos để Debug Raycast
    private void OnDrawGizmosSelected()
    {
        // Sử dụng spawnPointOverride cho Gizmos
        Vector3 baseWorldPos = (spawnPointOverride ? spawnPointOverride.position : transform.position);

        const float RAYCAST_HEIGHT_OFFSET = 1.0f;

        Vector3 raycastStart = baseWorldPos + Vector3.up * RAYCAST_HEIGHT_OFFSET;
        float totalRaycastDistance = raycastDistance + RAYCAST_HEIGHT_OFFSET;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(raycastStart, 0.1f);

        // Thử Raycast để vẽ
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, totalRaycastDistance, groundLayer);

        if (hit.collider != null)
        {
            // Raycast thành công
            Gizmos.color = Color.green;
            Gizmos.DrawLine(raycastStart, hit.point);
            Gizmos.DrawWireSphere(hit.point, 0.05f);
        }
        else
        {
            // Raycast thất bại
            Gizmos.color = Color.red;
            Gizmos.DrawLine(raycastStart, raycastStart + Vector3.down * totalRaycastDistance);
        }
    }
}
