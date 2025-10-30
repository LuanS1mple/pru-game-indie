using UnityEngine;

public class BossRoomManager : MonoBehaviour
{
    // Kéo thả các Collider của tường ảo vào đây
    [Header("Tường Chắn")]
    public Collider2D entryBarrierCollider; // Ngăn vào phòng Boss
    public Collider2D exitBarrierCollider;  // Ngăn quay ra khỏi phòng Boss

    // Kéo thả Trigger kích hoạt vào đây
    [Header("Kích Hoạt")]
    public GameObject bossFightTrigger; // Trigger ở cửa phòng Boss

    // Số lượng quái cần phải tiêu diệt để mở lối vào
    [Header("Quản lý Quái")]
    public int enemiesRemaining = 0; // Đặt số lượng quái ban đầu trong Inspector

    // Trạng thái Boss
    private bool isBossActive = false;
    private bool bossDefeated = false;

    void Start()
    {
        // 1. Khởi tạo: Tường chắn VÀO phải BẬT để ngăn Player
        if (entryBarrierCollider != null)
        {
            entryBarrierCollider.enabled = true;
        }
        // Tường chắn RA ban đầu phải TẮT
        if (exitBarrierCollider != null)
        {
            exitBarrierCollider.enabled = false;
        }

        // Tắt Trigger kích hoạt Boss cho đến khi cần thiết (thường là để kích hoạt Boss)
        if (bossFightTrigger != null)
        {
            bossFightTrigger.SetActive(false);
        }

        Debug.Log("Phòng Boss đã sẵn sàng. Cần tiêu diệt " + enemiesRemaining + " quái.");
    }

    // Hàm này được Quái gọi khi nó bị tiêu diệt
    public void EnemyDied()
    {
        if (enemiesRemaining > 0)
        {
            enemiesRemaining--;
            Debug.Log("Quái còn lại: " + enemiesRemaining);

            // 2. Logic Mở Tường Lần 1 (Vào phòng Boss)
            if (enemiesRemaining <= 0 && !isBossActive && !bossDefeated)
            {
                UnlockEntry();
            }
        }
    }

    private void UnlockEntry()
    {
        // Vô hiệu hóa tường chắn VÀO
        if (entryBarrierCollider != null)
        {
            entryBarrierCollider.enabled = false;
        }

        // Bật Trigger để Player đi vào phòng Boss và kích hoạt Boss Fight
        if (bossFightTrigger != null)
        {
            bossFightTrigger.SetActive(true);
        }
        Debug.Log("✅ Tất cả quái đã chết! Tường chắn đã mở. Mời Player vào.");
    }

    // Hàm này được BossFightTrigger gọi khi Player chạm vào
    public void StartBossFight()
    {
        if (!isBossActive && !bossDefeated)
        {
            isBossActive = true;

            // 3. Logic Đóng Tường Lần 2 (Bắt đầu đánh Boss)
            // Kích hoạt lại tường chắn VÀO để Player không quay ra
            if (entryBarrierCollider != null)
            {
                entryBarrierCollider.enabled = true;
            }

            // Kích hoạt tường chắn RA (nếu cần thiết, để chắn lối đi tiếp)
            if (exitBarrierCollider != null)
            {
                exitBarrierCollider.enabled = true;
            }

            // Vô hiệu hóa Trigger này sau khi đã dùng
            if (bossFightTrigger != null)
            {
                bossFightTrigger.SetActive(false);
            }

            // Kích hoạt Boss (ví dụ: Boss.GetComponent<Boss>().ActivateBoss();)
            Debug.Log("⚔️ Boss Fight ĐÃ BẮT ĐẦU! Tường chắn đã đóng.");
        }
    }

    // Hàm này được Boss gọi khi nó bị tiêu diệt
    public void BossDied()
    {
        // 4. Logic Mở Tường Lần 3 (Boss chết)
        if (isBossActive)
        {
            isBossActive = false;
            bossDefeated = true;

            // Vô hiệu hóa TẤT CẢ tường chắn vĩnh viễn
            if (entryBarrierCollider != null)
            {
                entryBarrierCollider.enabled = false;
            }
            if (exitBarrierCollider != null)
            {
                exitBarrierCollider.enabled = false;
            }

            Debug.Log("🏆 Boss đã bị tiêu diệt! Tất cả tường chắn đã gỡ bỏ.");
        }
    }
}