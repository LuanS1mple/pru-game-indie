using UnityEngine;
using UnityEngine.SceneManagement; // Quan trọng để lấy tên Scene

public class GameProgressManager : MonoBehaviour
{
    // ⭐ SỬ DỤNG SINGLETON HOẶC STATIC để dễ dàng truy cập từ mọi nơi
    public static GameProgressManager Instance;

    // Trạng thái của các kỹ năng
    public bool isWindBlastUnlocked = false;
    public bool isCrossBlastUnlocked = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // ⭐ Giữ đối tượng này giữa các Scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // ⭐ Có thể Load trạng thái từ Save File tại đây (cho game hoàn chỉnh)
    }

    // --- HÀM CÔNG KHAI ĐỂ GỌI TỪ LOGIC BOSS ---

    public void UnlockWindBlast()
    {
        if (!isWindBlastUnlocked)
        {
            isWindBlastUnlocked = true;
            Debug.Log("🎉 Kỹ năng WindBlast đã được Unlock!");
            // ⭐ Tùy chọn: Thêm hiệu ứng/thông báo
        }
    }

    public void UnlockCrossBlast()
    {
        if (!isCrossBlastUnlocked)
        {
            isCrossBlastUnlocked = true;
            Debug.Log("🎉 Kỹ năng CrossBlast đã được Unlock!");
            // ⭐ Tùy chọn: Thêm hiệu ứng/thông báo
        }
    }
}