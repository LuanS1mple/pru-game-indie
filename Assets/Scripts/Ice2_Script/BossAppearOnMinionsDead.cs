using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BossAppearOnMinionsDead : MonoBehaviour
{
    [Header("Minions phải chết trước")]
    public List<GameObject> requiredMinions = new List<GameObject>(); // kéo 4 con vào đây

    [Header("Boss sẽ xuất hiện")]
    [Tooltip("Nếu có sẵn boss trong scene thì kéo vào đây (script sẽ tự SetActive(false) lúc Start).")]
    public GameObject existingBoss;

    [Tooltip("Nếu không dùng boss có sẵn, đặt Prefab để spawn khi đủ điều kiện.")]
    public GameObject bossPrefab;
    public Transform spawnPoint; // để trống thì spawn tại vị trí this

    [Header("Hiệu ứng xuất hiện (tuỳ chọn)")]
    public string appearTrigger = "Appear";   // để rỗng nếu không dùng anim
    public float enableAIDelay = 0.5f;        // delay bật AI sau khi xuất hiện

    [Header("Debug")]
    public bool logDebug = true;

    bool done = false;

    void Start()
    {
        // Tắt boss sẵn trong scene (nếu có)
        if (existingBoss)
        {
            if (logDebug) Debug.Log("[BossAppear] Deactivate existing boss at start.", existingBoss);
            existingBoss.SetActive(false);
        }

        if (!existingBoss && !bossPrefab)
        {
            Debug.LogWarning("[BossAppear] Chưa cấu hình existingBoss hoặc bossPrefab.", this);
        }
    }

    void Update()
    {
        if (done) return;

        if (requiredMinions.Count == 0)
        {
            // Không cấu hình minion -> cho boss xuất hiện luôn
            AppearBoss();
            return;
        }

        int dead = 0;
        for (int i = 0; i < requiredMinions.Count; i++)
        {
            if (IsMinionDead(requiredMinions[i])) dead++;
        }

        if (dead >= requiredMinions.Count)
        {
            AppearBoss();
        }
    }

    bool IsMinionDead(GameObject m)
    {
        if (m == null) return true;               // Destroyed
        if (!m.activeInHierarchy) return true;    // SetActive(false)
        var bs = m.GetComponentInParent<BaseStats>();
        if (bs != null && bs.isDead) return true; // có BaseStats và đã chết
        return false;
    }

    void AppearBoss()
    {
        done = true;

        GameObject bossGo = existingBoss;
        if (!bossGo)
        {
            if (!bossPrefab)
            {
                if (logDebug) Debug.LogWarning("[BossAppear] Không có boss để hiện.", this);
                return;
            }
            Vector3 pos = spawnPoint ? spawnPoint.position : transform.position;
            bossGo = Instantiate(bossPrefab, pos, Quaternion.identity);
            if (logDebug) Debug.Log("[BossAppear] Spawn boss từ prefab.", bossGo);
        }
        else
        {
            bossGo.SetActive(true);
            if (logDebug) Debug.Log("[BossAppear] Activate existing boss.", bossGo);
        }

        // Gửi trigger Appear nếu có Animator
        var anim = bossGo.GetComponentInChildren<Animator>();
        if (anim && !string.IsNullOrEmpty(appearTrigger))
        {
            anim.SetTrigger(appearTrigger);
        }

        // Tắt AI 1 nhịp cho đẹp (nếu có EnemyAI2D_Animator)
        var ai = bossGo.GetComponentInChildren<EnemyAI2D_Animator>();
        if (ai)
        {
            ai.enabled = false;
            bossGo.GetComponent<MonoBehaviour>().StartCoroutine(ReEnableAIAfter(ai, enableAIDelay));
        }
    }

    System.Collections.IEnumerator ReEnableAIAfter(EnemyAI2D_Animator ai, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ai) ai.enabled = true;
    }
}
