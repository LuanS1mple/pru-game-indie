using UnityEngine;

[ExecuteAlways]
public class SnowFollowCam : MonoBehaviour
{
    public Camera cam;
    [Range(0f, 1f)] public float parallax = 1f;
    public float margin = 2f;
    Vector3 startCam;

    ParticleSystem ps; ParticleSystem.ShapeModule shape;

    void OnEnable()
    {
        if (!cam) cam = Camera.main;
        if (cam) startCam = cam.transform.position;
        ps = GetComponent<ParticleSystem>();
        shape = ps.shape;
    }

    void LateUpdate()
    {
        if (!cam) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;

        // Hộp phát hạt luôn phủ khung hình
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3((w * 2f) + margin, 0.1f, 1f);

        // Trung tâm emitter = tâm camera (không bao giờ lệch)
        Vector3 center = new Vector3(cam.transform.position.x,
                                     cam.transform.position.y + h + 0.5f, 0f);

        // Offset parallax chỉ để "nhìn" (không làm hụt phủ)
        float visualOffsetX = (cam.transform.position.x - startCam.x) * (parallax - 1f);
        transform.position = center + new Vector3(visualOffsetX, 0f, 0f);
    }
}
