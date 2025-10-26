using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Scene đích khi player đi qua cửa")]
    public string targetScene;

    [Header("Layer Player")]
    public LayerMask playerLayer; // Gán layer Player trong Inspector

    void Awake()
    {
        Debug.Log("DoorController Awake, targetScene: " + targetScene);
    }

    void Start()
    {
        Debug.Log("DoorController Start, Collider: " + GetComponent<Collider2D>());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem collider có thuộc playerLayer không
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
            return; // Không phải player → bỏ qua

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("⚠️ targetScene chưa được gán trong DoorController!");
            return;
        }

        Debug.Log($"🚪 Player đi qua cửa → chuyển sang scene: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }
}
