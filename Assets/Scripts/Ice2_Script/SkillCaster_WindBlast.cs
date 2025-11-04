using UnityEngine;

public class SkillCaster_WindBlast : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public WindBlast windBlastPrefab;      // Prefab phải có script WindBlast
    public Transform spawnPoint;           // Thường là attackpoint
    public Vector2 localOffset = new Vector2(0.4f, 0f);

    [Header("Debug")]
    public bool debugLog = true;

    /// <summary>
    /// Gọi từ Controller. Trả về true nếu spawn thành công.
    /// </summary>
    public bool Cast()
    {
        if (!windBlastPrefab)
        {
            if (debugLog) Debug.LogWarning("[WindBlastCaster] Prefab rỗng!", this);
            return false;
        }

        Transform root = spawnPoint ? spawnPoint : transform;
        Vector3 pos = root.TransformPoint(localOffset);

        // xác định hướng theo scale X
        float scaleX = Mathf.Sign(root.lossyScale.x);
        if (scaleX == 0) scaleX = 1;

        WindBlast inst = Instantiate(windBlastPrefab, pos, Quaternion.identity);
        inst.Initialize(scaleX);

        if (debugLog) Debug.Log($"[WindBlastCaster] CAST at {pos} (scaleX={scaleX})", this);
        return true;
    }
}


