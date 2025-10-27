using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    [Header("Scene đích khi player đi qua cửa")]
    public string targetScene;

    [Header("Layer Player (tự động tìm nếu chưa gán)")]
    public LayerMask playerLayer; // Gán layer Player trong Inspector hoặc sẽ tự động tìm

    void Awake()
    {
        Debug.Log("DoorController Awake, targetScene: " + targetScene);

        // Nếu chưa gán layer Player trong prefab → tự động tìm
        if (playerLayer.value == 0)
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            if (playerLayerIndex >= 0)
            {
                playerLayer = 1 << playerLayerIndex;
                Debug.Log("Tự động gán playerLayer = Player (Layer " + playerLayerIndex + ")");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy layer 'Player' trong project!");
            }
        }
    }

    void Start()
    {
        Debug.Log("DoorController Start, Collider: " + GetComponent<Collider2D>());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Object chạm vào cửa: " + other.name + ", Layer: " + LayerMask.LayerToName(other.gameObject.layer));

        // Kiểm tra xem collider có thuộc playerLayer không
        if ((playerLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            Debug.Log("Không phải Player → bỏ qua");
            return;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("targetScene chưa được gán trong DoorController!");
            return;
        }

        Debug.Log("Player đi qua cửa → chuyển sang scene: " + targetScene);
        SceneManager.LoadScene(targetScene);
    }
}
