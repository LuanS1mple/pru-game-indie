using UnityEngine;
using System.Collections;
using Cinemachine;

public class BossRoomManager : MonoBehaviour
{
    [Header("1. Barriers & Trigger")]
    public Collider2D entryBarrierCollider; // Tường vào
    public Collider2D exitBarrierCollider;  // Tường ra
    public GameObject bossFightTrigger;     // Trigger để bắt đầu Boss fight

    [Header("2. Camera Confiner")]
    public CinemachineConfiner2D cameraConfiner;
    public Collider2D normalRoomConfiner;   // Vùng camera chỉ phòng thường
    public Collider2D combinedConfiner;     // Vùng camera bao 2 phòng
    public Collider2D bossRoomConfiner;     // Vùng camera chỉ phòng boss

    [Header("3. Game Logic")]
    public int enemiesRemaining = 0;
    private bool isBossActive = false;
    private bool bossDefeated = false;

    void Start()
    {
        // Ban đầu: tường vào đóng, tường ra mở
        if (entryBarrierCollider != null) entryBarrierCollider.enabled = true;
        if (exitBarrierCollider != null) exitBarrierCollider.enabled = false;
        if (bossFightTrigger != null) bossFightTrigger.SetActive(false);

        // Giới hạn camera trong phòng thường
        if (cameraConfiner != null && normalRoomConfiner != null)
        {
            cameraConfiner.m_BoundingShape2D = normalRoomConfiner;
            cameraConfiner.InvalidateCache();
        }
    }

    // --- Khi 1 con quái chết ---
    private bool isFirstEnemyKilled = false;
    public void EnemyDied()
    {
        if (enemiesRemaining > 0)
        {
            if (!isFirstEnemyKilled)
            {
                // Lần đầu tiên: Trừ 2 quái vật
                enemiesRemaining -= 2;
                isFirstEnemyKilled = true;
                Debug.Log("❗ Quái đầu tiên chết! Trừ 2 quái còn lại.");
            }
            else
            {
                // Các lần sau: Chỉ trừ 1 quái vật
                enemiesRemaining--;
            }

            // Đảm bảo số quái còn lại không âm
            if (enemiesRemaining < 0) enemiesRemaining = 0;

            // Kiểm tra mở đường sang phòng Boss
            if (enemiesRemaining <= 0 && !isBossActive && !bossDefeated)
            {
                UnlockEntry();
            }

            Debug.Log($"Số quái còn lại: {enemiesRemaining}");
        }
    }

    private void UnlockEntry()
    {
        StartCoroutine(OpenEntryRoutine());
    }

    private IEnumerator OpenEntryRoutine()
    {
        yield return null;

        // Mở tường chắn vào boss
        if (entryBarrierCollider != null) entryBarrierCollider.enabled = false;

        // Mở rộng vùng camera bao cả 2 phòng
        if (cameraConfiner != null && combinedConfiner != null)
        {
            cameraConfiner.m_BoundingShape2D = combinedConfiner;
            cameraConfiner.InvalidateCache();
        }

        // Cho phép player đi vào vùng boss
        if (bossFightTrigger != null) bossFightTrigger.SetActive(true);

        Debug.Log("✅ Quái chết hết. Đang mở đường sang phòng Boss!");
    }

    // --- Khi player bước vào vùng Boss ---
    public void StartBossFight()
    {
        if (isBossActive || bossDefeated) return;
        isBossActive = true;

        StartCoroutine(StartBossRoutine());
    }

    private IEnumerator StartBossRoutine()
    {
        yield return null;

        // Đóng lối vào và ra
        if (entryBarrierCollider != null) entryBarrierCollider.enabled = true;
        if (exitBarrierCollider != null) exitBarrierCollider.enabled = true;

        // Giới hạn camera chỉ trong phòng boss
        if (cameraConfiner != null && bossRoomConfiner != null)
        {
            cameraConfiner.m_BoundingShape2D = bossRoomConfiner;
            cameraConfiner.InvalidateCache();
        }

        Debug.Log("⚔️ BOSS FIGHT BẮT ĐẦU!");
    }

    // --- Khi Boss chết ---
    public void BossDied()
    {
        if (!isBossActive) return;

        isBossActive = false;
        bossDefeated = true;

        StartCoroutine(BossDefeatedRoutine());
    }

    private IEnumerator BossDefeatedRoutine()
    {
        yield return null;

        // Mở lại tường và mở rộng camera
        if (entryBarrierCollider != null) entryBarrierCollider.enabled = false;
        if (exitBarrierCollider != null) exitBarrierCollider.enabled = false;

        if (cameraConfiner != null && combinedConfiner != null)
        {
            cameraConfiner.m_BoundingShape2D = combinedConfiner;
            cameraConfiner.InvalidateCache();
        }

        Debug.Log("🏆 Boss đã bị tiêu diệt! Tường đã gỡ bỏ và camera mở rộng.");
    }
}
