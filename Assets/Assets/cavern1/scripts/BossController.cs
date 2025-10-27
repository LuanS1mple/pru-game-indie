using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Combat References")]
    public BossHitBox hitBox;
    private BossStatus bossStatus;

    [Header("Exit Door Settings")]
    public GameObject exitDoorPrefab;

    public string nextSceneName = "NextLevelScene"; // ⭐ Tên scene đích sẽ được chuyển đến
    public Transform doorSpawnPoint; // Gán DoorSpawnPoint ở đây
    public float spawnDelay = 1f;

    [Header("Optional FX")]
    public ParticleSystem spawnEffect;
    public AudioClip spawnSound;

    private bool doorSpawned = false;

    void Awake()
    {
        bossStatus = GetComponent<BossStatus>();
    }

    void Update()
    {
        // ⭐ Kiểm tra khi Boss chết để chuẩn bị mở cửa (bắt đầu quá trình chuyển màn)
        if (bossStatus != null && bossStatus.isDead && !doorSpawned)
        {
            doorSpawned = true;
            StartCoroutine(SpawnDoorAfterDelay(spawnDelay)); // ⭐ Gọi hàm spawn cửa (chuẩn bị chuyển scene)
        }
    }

    private IEnumerator SpawnDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (exitDoorPrefab == null)
        {
            Debug.LogWarning("ExitDoorPrefab chưa được gán!");
            yield break;
        }

        Vector3 spawnPos = doorSpawnPoint != null ? doorSpawnPoint.position : transform.position;

        GameObject door = Instantiate(exitDoorPrefab, spawnPos, Quaternion.identity);
        door.SetActive(true);

        // Đảm bảo collider có isTrigger = true
        Collider2D doorCollider = door.GetComponent<Collider2D>();
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("Door clone không có Collider2D!");
        }

        // Đảm bảo có Rigidbody2D để trigger hoạt động
        if (door.GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = door.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.gravityScale = 0;
        }

        // ⭐ Gán scene đích cho cửa — phần quan trọng giúp cửa biết sẽ load scene nào
        DoorController doorController = door.GetComponent<DoorController>();
        if (doorController != null)
        {
            doorController.targetScene = nextSceneName; // ⭐ Liên kết scene đích
        }

        // Hiệu ứng spawn (không ảnh hưởng đến việc chuyển scene)
        if (spawnEffect != null)
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);

        // Âm thanh spawn (chỉ là hiệu ứng)
        if (spawnSound != null)
            AudioSource.PlayClipAtPoint(spawnSound, spawnPos);

        Debug.Log("Cửa đã xuất hiện tại vị trí spawn!"); // ⭐ Thông báo đã sẵn sàng để chuyển màn (qua cửa)
    }

    // Animation Event
    public void EnableHitBox()
    {
        if (hitBox != null)
            hitBox.EnableDamage();
    }

    public void DisableHitBox()
    {
        if (hitBox != null)
            hitBox.DisableDamage();
    }
}
