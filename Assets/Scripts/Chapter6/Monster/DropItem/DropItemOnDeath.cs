using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemOnDeath : MonoBehaviour
{
    [Header("Prefab của item sẽ sinh ra")]
    public GameObject itemPrefab;

    [Header("Độ trễ trước khi sinh (tùy chọn)")]
    public float delay = 0.2f;

    private bool isQuitting = false; // Dùng để tránh sinh item khi thoát game

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        // Nếu đang thoát game thì không sinh item
        if (isQuitting) return;

        if (itemPrefab != null)
        {
            if (delay > 0)
            {
                Invoke(nameof(SpawnItem), delay);
            }
            else
            {
                SpawnItem();
            }
        }
    }

    void SpawnItem()
    {
        Instantiate(itemPrefab, transform.position, Quaternion.identity);
    }
}
