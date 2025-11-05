//using UnityEngine;

//public class LootDropper2D : MonoBehaviour
//{
//    [System.Serializable]
//    public class DropEntry
//    {
//        public GameObject prefab;
//        [Range(0f, 1f)] public float chance = 1f;
//        public Vector2Int countRange = new Vector2Int(1, 1);
//        [Header("Physics (nếu prefab có Rigidbody2D)")]
//        public Vector2 impulse = new Vector2(1.5f, 3.5f);
//        public float spawnSpreadX = 0.2f;
//        public float torque = 0f;
//    }

//    public enum TriggerMode { Manual, OnDisable, OnDestroy }

//    [Header("Drop Settings")]
//    public TriggerMode triggerMode = TriggerMode.OnDestroy; // ✔ rơi đồ khi enemy bị Destroy
//    public DropEntry[] drops;

//    [Header("Spawn")]
//    public Transform spawnRoot;
//    public Vector3 spawnOffset = new Vector3(0f, 0.05f, 0f);

//    [Header("One-shot & Cleanup")]
//    public bool dropOnce = true;
//    public bool destroyAfterDrop = false;
//    public float destroyDelay = 2f;

//    bool _dropped;
//    bool isQuitting = false;

//    void OnApplicationQuit()
//    {
//        // Unity sắp thoát Play Mode / đóng app
//        isQuitting = true;
//    }

//    void Awake()
//    {
//        if (!spawnRoot) spawnRoot = transform;
//    }

//    void OnDisable()
//    {
//        if (!Application.isPlaying || isQuitting) return;
//        if (triggerMode == TriggerMode.OnDisable) Drop();
//    }

//    void OnDestroy()
//    {
//        if (!Application.isPlaying || isQuitting) return;
//        if (triggerMode == TriggerMode.OnDestroy) Drop();
//    }

//    public void Drop()
//    {
//        if (_dropped && dropOnce) return;
//        _dropped = true;

//        if (drops == null || drops.Length == 0) return;

//        Vector3 origin = (spawnRoot ? spawnRoot.position : transform.position) + spawnOffset;

//        foreach (var d in drops)
//        {
//            if (!d.prefab) continue;
//            if (Random.value > d.chance) continue;

//            int min = Mathf.Max(0, d.countRange.x);
//            int max = Mathf.Max(min, d.countRange.y);
//            int count = Random.Range(min, max + 1);
//            if (count <= 0) continue;

//            for (int i = 0; i < count; i++)
//            {
//                Vector3 pos = origin + new Vector3(Random.Range(-d.spawnSpreadX, d.spawnSpreadX), 0f, 0f);
//                var go = Instantiate(d.prefab, pos, Quaternion.identity);

//                var rb = go.GetComponent<Rigidbody2D>();
//                if (rb)
//                {
//                    float dirX = Random.value < 0.5f ? -1f : 1f;
//                    float vx = dirX * Random.Range(0.4f * d.impulse.x, d.impulse.x);
//                    float vy = Random.Range(0.6f * d.impulse.y, d.impulse.y);
//                    rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
//                    if (Mathf.Abs(d.torque) > 0f) rb.AddTorque(Random.Range(-d.torque, d.torque), ForceMode2D.Impulse);
//                }
//            }
//        }

//        if (destroyAfterDrop) Destroy(gameObject, destroyDelay);
//    }

//    public void DropAndDestroy()
//    {
//        Drop();
//        if (!destroyAfterDrop) Destroy(gameObject, destroyDelay);
//    }
//}

using UnityEngine;

public class LootDropper2D : MonoBehaviour
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
    public TriggerMode triggerMode = TriggerMode.OnDestroy;
    public DropEntry[] drops;

    [Header("Spawn")]
    public Transform spawnRoot;
    public Vector3 spawnOffset = new Vector3(0f, 0.05f, 0f);

    [Header("One-shot & Cleanup")]
    public bool dropOnce = true;
    public bool destroyAfterDrop = false;
    public float destroyDelay = 2f;

    // ===== Safety flags =====
    static bool s_AppQuitting = false;     // static để mọi instance đều biết
    bool _dropped = false;

    void Awake()
    {
        if (!spawnRoot) spawnRoot = transform;
    }

    void OnEnable()
    {
        // Khi reload domain, static có thể về false, đảm bảo reset mỗi lần chạy game
        if (!Application.isPlaying) s_AppQuitting = false;
    }

    void OnApplicationQuit()
    {
        // Unity đang chuẩn bị thoát Play Mode / đóng app
        s_AppQuitting = true;
    }

    void OnDisable()
    {
        // Đừng spawn khi không ở Play Mode, hoặc đang quit, hoặc scene đã/unloaded
        if (!Application.isPlaying || s_AppQuitting) return;
        if (!IsSceneSafe()) return;

        if (triggerMode == TriggerMode.OnDisable)
            Drop();
    }

    void OnDestroy()
    {
        // Đừng spawn khi không ở Play Mode, hoặc đang quit, hoặc scene đã/unloaded
        if (!Application.isPlaying || s_AppQuitting) return;
        if (!IsSceneSafe()) return;

        if (triggerMode == TriggerMode.OnDestroy)
            Drop();
    }

    bool IsSceneSafe()
    {
        // Tránh spawn trong lúc scene đang bị unload
        var sc = gameObject.scene;
        return sc.IsValid() && sc.isLoaded;
    }

    // ====== API ======
    public void Drop()
    {
        if (_dropped && dropOnce) return;
        _dropped = true;

        if (drops == null || drops.Length == 0) return;

        Vector3 origin = (spawnRoot ? spawnRoot.position : transform.position) + spawnOffset;

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
                Vector3 pos = origin + new Vector3(Random.Range(-d.spawnSpreadX, d.spawnSpreadX), 0f, 0f);
                var go = Instantiate(d.prefab, pos, Quaternion.identity);

                var rb = go.GetComponent<Rigidbody2D>();
                if (rb)
                {
                    float dirX = Random.value < 0.5f ? -1f : 1f;
                    float vx = dirX * Random.Range(0.4f * d.impulse.x, d.impulse.x);
                    float vy = Random.Range(0.6f * d.impulse.y, d.impulse.y);
                    rb.AddForce(new Vector2(vx, vy), ForceMode2D.Impulse);
                    if (Mathf.Abs(d.torque) > 0f)
                        rb.AddTorque(Random.Range(-d.torque, d.torque), ForceMode2D.Impulse);
                }
            }
        }

        if (destroyAfterDrop)
            Destroy(gameObject, destroyDelay);
    }

    public void DropAndDestroy()
    {
        Drop();
        if (!destroyAfterDrop)
            Destroy(gameObject, destroyDelay);
    }
}

