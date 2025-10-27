using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Scene đích khi player đi qua cửa")]
    public string targetScene;

    [Header("Layer Player (tự động tìm nếu chưa gán)")]
    public LayerMask playerLayer;

    [Header("Thời gian chờ trước khi chuyển scene (giây)")]
    public float delayBeforeLoad = 1.5f;

    private bool isLoading = false;

    void Awake()
    {
        Debug.Log("DoorController Awake, targetScene: " + targetScene);

        // Tự động gán layer Player nếu chưa gán
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading) return;

        Debug.Log("Object chạm vào cửa: " + other.name + ", Layer: " + LayerMask.LayerToName(other.gameObject.layer));

        // Kiểm tra collider có phải player không
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

        // ✅ SỬA LỖI #2: Gán cờ ngay lập tức
        isLoading = true;

        // Bắt đầu coroutine load scene
        StartCoroutine(LoadSceneAsyncWithDelay());
    }

    private IEnumerator LoadSceneAsyncWithDelay()
    {
        // GỢI Ý: Đây là lúc tốt để kích hoạt UI Loading
        // ví dụ: LoadingScreenUI.SetActive(true);

        // Tạm dừng game logic (optional)
        Time.timeScale = 0.5f;

        Debug.Log($"Đang chuẩn bị load scene {targetScene} sau {delayBeforeLoad} giây...");
        yield return new WaitForSecondsRealtime(delayBeforeLoad);

        // Load scene bất đồng bộ
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        asyncLoad.allowSceneActivation = false;

        // Chờ đến khi load gần xong (90%)
        while (asyncLoad.progress < 0.9f)
        {
            // GỢI Ý: Cập nhật thanh loading bar tại đây
            // loadingBar.value = asyncLoad.progress;
            yield return null;
        }

        Debug.Log("Scene gần load xong, kích hoạt scene mới...");

        // ✅ SỬA LỖI #1: Phải khôi phục Time.timeScale TRƯỚC KHI kích hoạt scene
        Time.timeScale = 1f;

        // Kích hoạt scene mới
        asyncLoad.allowSceneActivation = true;
    }
}