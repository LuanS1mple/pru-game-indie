using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform target;          // Nhân vật mà camera sẽ theo dõi
    public float smoothing = 5f;      // Độ mượt khi di chuyển

    [Header("Giới hạn map tự động")]
    public BoxCollider2D mapBounds;   // Collider bao quanh bản đồ

    private Vector3 offset;           // Khoảng cách giữa camera và nhân vật
    private float minX, maxX, minY, maxY;
    private float camHalfHeight, camHalfWidth;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("⚠️ CameraFollow: Chưa gán target!");
            return;
        }

        offset = transform.position - target.position;

        // Lấy kích thước của camera
        Camera cam = GetComponent<Camera>();
        camHalfHeight = cam.orthographicSize;
        camHalfWidth = cam.aspect * camHalfHeight;

        // Tính giới hạn theo collider của map
        if (mapBounds != null)
        {
            minX = mapBounds.bounds.min.x + camHalfWidth;
            maxX = mapBounds.bounds.max.x - camHalfWidth;
            minY = mapBounds.bounds.min.y + camHalfHeight;
            maxY = mapBounds.bounds.max.y - camHalfHeight;
        }
        else
        {
            Debug.LogWarning("⚠️ CameraFollow: Chưa gán mapBounds (BoxCollider2D)!");
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 targetCamPos = target.position + offset;
        Vector3 smoothPos = Vector3.Lerp(transform.position, targetCamPos, smoothing * Time.deltaTime);

        // Nếu có mapBounds → giới hạn camera
        if (mapBounds != null)
        {
            float clampX = Mathf.Clamp(smoothPos.x, minX, maxX);
            float clampY = Mathf.Clamp(smoothPos.y, minY, maxY);
            transform.position = new Vector3(clampX, clampY, smoothPos.z);
        }
        else
        {
            transform.position = smoothPos;
        }
    }
}
