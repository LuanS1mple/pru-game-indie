using UnityEngine;

/// <summary>
/// Script này chịu trách nhiệm giữ cho Player tồn tại khi chuyển map
/// và lưu các chỉ số "meta" (như Souls).
/// Nó KHÔNG quản lý máu/sát thương (BaseStats làm việc đó).
/// </summary>
public class PlayerPersistence : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static PlayerPersistence instance;

    // --- Tham chiếu đến các component khác của Player ---
    // Chúng ta sẽ gán các biến này trong Awake()
    [HideInInspector] public BaseStats stats;
    [HideInInspector] public movement playerMovement;

    [Header("Chỉ số riêng của Player")]
    public int currentSouls = 0;
    // (Thêm các chỉ số khác bạn muốn lưu ở đây, ví dụ: mana, chìa khóa...)

    private void Awake()
    {
        // === BẮT ĐẦU PHẦN SINGLETON ===
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Tự động lấy các component cốt lõi trên cùng GameObject Player
            stats = GetComponent<BaseStats>();
            playerMovement = GetComponent<movement>();

            // Kiểm tra lỗi
            if (stats == null)
            {
                Debug.LogError("PlayerPersistence không tìm thấy BaseStats!", gameObject);
            }
            if (playerMovement == null)
            {
                Debug.LogError("PlayerPersistence không tìm thấy script 'movement'!", gameObject);
            }
        }
        else
        {
            // Nếu đã có Player từ map trước, hủy bản sao này
            Debug.LogWarning("Phát hiện Player trùng lặp. Hủy bản sao.");
            Destroy(gameObject);
        }
        // === KẾT THÚC PHẦN SINGLETON ===
    }

    // --- Các hàm quản lý chỉ số riêng của Player ---

    public void AddSouls(int amount)
    {
        if (amount <= 0) return;
        currentSouls += amount;
        Debug.Log($"Đã nhận {amount} souls. Tổng: {currentSouls}");

        // (Gọi sự kiện cho UI souls cập nhật ở đây nếu cần)
        // OnSoulsChanged?.Invoke(currentSouls);
    }
}