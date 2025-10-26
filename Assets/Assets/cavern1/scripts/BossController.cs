using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Combat References")]
    public BossHitBox hitBox;
    private BossStatus bossStatus;

    [Header("Exit Door Settings")]
    public GameObject exitDoorPrefab;
    public string nextSceneName = "NextLevelScene";
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
        if (bossStatus != null && bossStatus.isDead && !doorSpawned)
        {
            doorSpawned = true;
            StartCoroutine(SpawnDoorAfterDelay(spawnDelay));
        }
    }

    private IEnumerator SpawnDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (exitDoorPrefab == null)
        {
            Debug.LogWarning("⚠️ ExitDoorPrefab chưa gán!");
            yield break;
        }

        Vector3 spawnPos;

        if (doorSpawnPoint != null)
            spawnPos = doorSpawnPoint.position; // Spawn tại điểm bạn muốn
        else
            spawnPos = transform.position; // Nếu không có điểm → spawn tại boss (không khuyến khích)

        GameObject door = Instantiate(exitDoorPrefab, spawnPos, Quaternion.identity);

        var doorController = door.GetComponent<DoorController>();
        if (doorController != null)
            doorController.targetScene = nextSceneName;

        if (spawnEffect != null)
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);

        if (spawnSound != null)
            AudioSource.PlayClipAtPoint(spawnSound, spawnPos);

        Debug.Log("🚪 Cửa đã xuất hiện tại điểm bạn chọn!");
    }

    // Animation Event
    public void EnableHitBox()
    {
        if (hitBox != null) hitBox.EnableDamage();
    }

    public void DisableHitBox()
    {
        if (hitBox != null) hitBox.DisableDamage();
    }
}
