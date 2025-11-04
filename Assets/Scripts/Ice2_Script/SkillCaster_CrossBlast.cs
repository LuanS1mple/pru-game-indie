// SkillCaster_CrossBlast.cs
using UnityEngine;

public class SkillCaster_CrossBlast : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public GameObject crossBlastPrefab;
    public Transform spawnPoint;                  // kéo AttackPoint/crossSpawnPoint vào
    public Vector2 localOffset;                   // offset nhỏ nếu cần

    public void Cast()
    {
        if (!crossBlastPrefab) { Debug.LogWarning("[CrossCaster] Missing prefab"); return; }

        // Vị trí sinh
        Vector3 pos = spawnPoint ? spawnPoint.position
                                 : transform.position + (Vector3)localOffset;
        if (spawnPoint) pos += (Vector3)spawnPoint.TransformVector(localOffset);
        else pos += transform.TransformVector(localOffset);

        // Hướng theo scale X của Player
        float scaleX = Mathf.Sign(transform.localScale.x);

        GameObject go = Instantiate(crossBlastPrefab, pos, Quaternion.identity);

        // Gọi Initialize nếu có
        var cross = go.GetComponent<CrossBlast>();
        if (cross) cross.Initialize(scaleX);
    }
}
