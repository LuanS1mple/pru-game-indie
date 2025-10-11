using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Camera cam;
    [Range(0f, 1f)] public float strength = 0.3f; // xa: 0.2–0.3, gần: 0.5–0.7
    public bool parallaxY = false;

    Vector3 startCam, startPos;

    void OnEnable()
    {
        if (!cam) cam = Camera.main;
        Recenter();                 // ghi nhớ vị trí hiện tại làm gốc
    }

    void LateUpdate()
    {
        // CHỈ chạy khi Play để bạn kéo player/camera trong Editor mà BG không bị dịch
        if (!Application.isPlaying || !cam) return;

        var d = cam.transform.position - startCam;
        transform.position = startPos + new Vector3(d.x * strength,
                                                    parallaxY ? d.y * strength : 0f,
                                                    0f);
    }

    // Gọi khi bạn teleport player/camera hoặc muốn đặt lại gốc parallax
    [ContextMenu("Recenter to current camera")]
    public void Recenter()
    {
        if (!cam) cam = Camera.main;
        startCam = cam ? cam.transform.position : Vector3.zero;
        startPos = transform.position;
    }
}

