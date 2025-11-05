using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DoorController : MonoBehaviour
{
    [Header("Scene đích khi player đi qua cửa")]
    public string targetScene;

    [Header("Layer Player (tự động tìm nếu chưa gán)")]
    public LayerMask playerLayer;

    private bool isLoading = false;

    void Awake()
    {
        Debug.Log("DoorController Awake, targetScene: " + targetScene);

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
        if (isLoading) return;

        Debug.Log("Object chạm vào cửa: " + other.name + ", Layer: " + LayerMask.LayerToName(other.gameObject.layer));

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

        isLoading = true;
        StartCoroutine(LoadSceneAfterDelay(targetScene));
    }

    private IEnumerator LoadSceneAfterDelay(string sceneName)
    {
        Debug.Log($"[SceneTransition] Player chạm cửa. Bắt đầu chờ 3 giây trước khi tải scene '{sceneName}'...");

        yield return new WaitForSecondsRealtime(3f);
        Debug.Log("[SceneTransition] Đã chờ 3 giây. Bắt đầu chuẩn bị chuyển scene...");

        // 🔹 1. Lưu dữ liệu Player trước khi chuyển
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var stats = player.GetComponent<PlayerStats>();

            if (stats != null && GameDataManager.Instance != null)
            {
                GameDataManager.Instance.SavePlayerData(stats);
                Debug.Log($"[SceneTransition] ✅ Đã lưu trạng thái player: HP {stats.CurrentHealth}/{stats.MaxHealth}, ATK {stats.Attack}, DEF {stats.Defense}");
            }
            else
            {
                Debug.LogWarning("[SceneTransition] ⚠ Không thể lưu dữ liệu Player — thiếu PlayerStats hoặc GameDataManager!");
            }
        }

        // 🔹 2. Chuyển scene
        Debug.Log($"[SceneTransition] Đang tải scene '{sceneName}'...");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOp.isDone)
        {
            yield return null;
        }

        Debug.Log("[SceneTransition] ✅ Scene mới đã được tải thành công.");
    }
}
