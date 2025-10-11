using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxMultiplier = 0.3f;
    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate() // phải là LateUpdate!
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier, deltaMovement.y * parallaxMultiplier, 0);
        lastCameraPosition = cameraTransform.position;
    }
}