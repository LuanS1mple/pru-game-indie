using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private BossRoomManager manager;

    void Start()
    {
        manager = FindObjectOfType<BossRoomManager>();
    }

    // Hàm này được gọi khi một Collider khác (ví dụ: Player) chạm vào Trigger này
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Giả sử Player có Tag là "Player"
        if (other.CompareTag("Player"))
        {
            if (manager != null)
            {
                manager.StartBossFight(); // Bắt đầu Boss Fight và đóng tường
            }

            // (Tùy chọn) Vô hiệu hóa Trigger sau khi đã kích hoạt
            gameObject.SetActive(false);
        }
    }
}