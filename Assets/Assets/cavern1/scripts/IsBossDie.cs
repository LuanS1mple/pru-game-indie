using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

// Script này chỉ "chờ" được gọi
public class IsBossDie : MonoBehaviour
{
    [Header("Exit Door Settings")]
    public GameObject exitDoorPrefab;
    public string nextSceneName = "NextLevelScene";
    public Transform doorSpawnPoint;
    public float spawnDelay = 1f;

    [Header("Optional FX")]
    public ParticleSystem spawnEffect;
    public AudioClip spawnSound;

    private bool doorSpawned = false;

    // Hàm này sẽ được gọi bởi Unity Event "OnBossDied"
    public void SpawnTheDoor()
    {
        if (doorSpawned) return;
        doorSpawned = true;
        StartCoroutine(SpawnDoorAfterDelay(spawnDelay));
    }

    // Coroutine spawn cửa (giữ nguyên)
    private IEnumerator SpawnDoorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (exitDoorPrefab == null)
        {
            Debug.LogWarning("ExitDoorPrefab chưa được gán!", this);
            yield break;
        }

        Vector3 spawnPos = doorSpawnPoint != null ? doorSpawnPoint.position : transform.position;
        GameObject door = Instantiate(exitDoorPrefab, spawnPos, Quaternion.identity);
        door.SetActive(true);

        // ... (phần còn lại của code spawn cửa) ...

        // Gán scene đích cho cửa
        DoorController doorController = door.GetComponent<DoorController>();
        if (doorController != null)
        {
            doorController.targetScene = nextSceneName;
        }
        else
        {
            Debug.LogError($"Prefab cửa ({door.name}) thiếu script DoorController!", door);
        }

        // ... (FX và âm thanh) ...
        Debug.Log("Cửa đã xuất hiện!", door);
    }
}