using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // ✅ BẮT BUỘC PHẢI THÊM DÒNG NÀY

public class DoorController : MonoBehaviour
{
    [Header("Scene đích khi player đi qua cửa")]
    public string targetScene;

    [Header("Layer Player (tự động tìm nếu chưa gán)")]
    public LayerMask playerLayer;

    // ✅ Thêm biến này để tránh gọi load nhiều lần
    private bool isLoading = false;

    void Awake()
    {
        Debug.Log("DoorController Awake, targetScene: " + targetScene);

        // Tự động gán layer Player
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
        // Nếu đã bắt đầu load scene thì không làm gì cả
        if (isLoading) return;

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

        // ✅ Đánh dấu là đang load
        isLoading = true;

        // ✅ Thay vì load ngay, gọi Coroutine để chờ
        StartCoroutine(LoadSceneAfterDelay(targetScene));
    }

    // ✅ Đây là Coroutine thực hiện việc chờ và load scene
    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        Debug.Log($"Player chạm cửa. Bắt đầu chờ 3 giây trước khi tải {sceneName}...");

        // (Tùy chọn: Bạn có thể bắt đầu hiệu ứng mờ dần màn hình (fade-out) tại đây)

        // ✅ Chờ 3 giây (sử dụng Realtime để không bị ảnh hưởng bởi Time.timeScale)
        yield return new WaitForSecondsRealtime(3f);

        Debug.Log("Đã chờ 3 giây. Bắt đầu tải scene (bất đồng bộ)...");

        // ✅ Sử dụng LoadSceneAsync để không bị giật lag
        SceneManager.LoadSceneAsync(sceneName);

        // Bạn không cần 'yield return' hay 'while' ở đây
        // vì chúng ta chỉ cần ra lệnh cho nó tải, và nó sẽ tự động kích hoạt khi xong.
    }
}