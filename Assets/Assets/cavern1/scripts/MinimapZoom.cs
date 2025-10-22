using UnityEngine;

public class MinimapZoom : MonoBehaviour
{
    public Camera minimapCamera;
    public float zoomSpeed = 10f;
    public float minSize = 10f;
    public float maxSize = 50f;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            minimapCamera.orthographicSize -= scroll * zoomSpeed;
            minimapCamera.orthographicSize = Mathf.Clamp(minimapCamera.orthographicSize, minSize, maxSize);
        }
    }
}
