using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    // 🔹 Dữ liệu được lưu giữa các scene
    [Header("Persistent Player Stats")]
    public int currentHealth;
    public int maxHealth;
    public int attack;
    public int defense;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ✅ Giữ lại khi load scene mới
        }
        else
        {
            Destroy(gameObject); // ❌ Xóa nếu có bản trùng
        }
    }

    // Gọi khi chuẩn bị chuyển scene
    public void SaveFrom(PlayerStats stats)
    {
        if (stats == null) return;

        currentHealth = stats.CurrentHealth;
        maxHealth = stats.MaxHealth;
        attack = stats.Attack;
        defense = stats.Defense;

        Debug.Log($"[PlayerManager] Saved: HP {currentHealth}/{maxHealth}, ATK {attack}, DEF {defense}");
    }

    // Gọi sau khi scene mới load
    public void LoadTo(PlayerStats stats)
    {
        if (stats == null) return;

        stats.SetStatsFromManager(currentHealth, maxHealth, attack, defense);

        Debug.Log($"[PlayerManager] Loaded to PlayerStats (HUD synced)");
    }
}