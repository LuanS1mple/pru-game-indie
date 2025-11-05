using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedItemData
{
    public string iconName;
    public PickupType type;
    public int amount;
}

[System.Serializable]
public class PlayerData
{
    public float currentHP;
    public float maxHP;
    public float attack;
    public float defense;
    public List<SavedItemData> savedItems = new List<SavedItemData>();
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Global Player Data")]
    public PlayerData playerData = new PlayerData();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // =====================================================
    // 🔹 TỰ ĐỘNG GỌI KHI SCENE MỚI LOAD
    // =====================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(LoadAllWhenReady());
    }

    // =====================================================
    // 🔹 TỰ ĐỘNG GỌI KHI SCENE CŨ SẮP ĐÓNG
    // =====================================================
    private void OnSceneUnloaded(Scene scene)
    {
        var player = FindObjectOfType<PlayerStats>();
        if (player != null)
        {
            SavePlayerData(player);
            Debug.Log($"[GameDataManager] 💾 Auto-save khi rời scene '{scene.name}'");
        }
    }

    // =====================================================
    // 🔹 LOAD PLAYER + HUD SAU KHI SCENE MỚI MỞ
    // =====================================================
    private System.Collections.IEnumerator LoadAllWhenReady()
    {
        // 🔸 Đợi Player xuất hiện
        float timeout = 3f;
        PlayerStats playerStats = null;

        while (timeout > 0f)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerStats = playerObj.GetComponent<PlayerStats>();
                if (playerStats != null)
                    break;
            }
            timeout -= 0.2f;
            yield return new WaitForSecondsRealtime(0.2f);
        }

        // 🔸 Load dữ liệu Player
        if (playerStats != null)
            LoadPlayerData(playerStats);

        // 🔸 Đợi HUD spawn
        yield return new WaitForSecondsRealtime(0.5f);

        var hud = FindObjectOfType<HUDInventory5>();
        if (hud != null)
        {
            LoadHUDInventory(hud);
            Debug.Log($"[GameDataManager] ✅ HUD khôi phục thành công trong scene '{SceneManager.GetActiveScene().name}'");
        }
        else
        {
            Debug.LogWarning("[GameDataManager] ⚠ Không tìm thấy HUDInventory5 trong scene mới!");
        }
    }

    // =====================================================
    // 🔹 LƯU PLAYER DATA
    // =====================================================
    public void SavePlayerData(PlayerStats player)
    {
        if (player == null) return;

        playerData.currentHP = player.CurrentHealth;
        playerData.maxHP = player.MaxHealth;
        playerData.attack = player.Attack;
        playerData.defense = player.Defense;

        // 🔹 Lưu cả HUD
        var hud = FindObjectOfType<HUDInventory5>();
        if (hud != null)
            SaveHUDInventory(hud);

        Debug.Log($"[GameDataManager] 💾 Lưu Player + {playerData.savedItems.Count} item HUD thành công.");
    }

    // =====================================================
    // 🔹 LOAD PLAYER DATA
    // =====================================================
    public void LoadPlayerData(PlayerStats player)
    {
        if (player == null || playerData.maxHP <= 0)
        {
            Debug.LogWarning("[GameDataManager] ⚠ PlayerData chưa hợp lệ, bỏ qua.");
            return;
        }

        int curHP = Mathf.RoundToInt(playerData.currentHP);
        int maxHP = Mathf.RoundToInt(playerData.maxHP);
        int atk = Mathf.RoundToInt(playerData.attack);
        int def = Mathf.RoundToInt(playerData.defense);

        if (curHP <= 0) curHP = maxHP;
        player.SetStatsFromManager(curHP, maxHP, atk, def);

        Debug.Log($"[GameDataManager] ✅ Load Player: HP {curHP}/{maxHP}, ATK {atk}, DEF {def}");
    }

    // =====================================================
    // 🔹 LƯU HUD
    // =====================================================
    public void SaveHUDInventory(HUDInventory5 hud)
    {
        playerData.savedItems.Clear();

        var field = typeof(HUDInventory5).GetField("slots",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            Debug.LogWarning("[GameDataManager] ❌ Không tìm thấy field 'slots' trong HUDInventory5!");
            return;
        }

        var slotArray = field.GetValue(hud) as System.Array;
        if (slotArray == null) return;

        foreach (var slotObj in slotArray)
        {
            var occupiedField = slotObj.GetType().GetField("occupied");
            bool occupied = (bool)occupiedField.GetValue(slotObj);
            if (!occupied) continue;

            var iconField = slotObj.GetType().GetField("icon");
            var typeField = slotObj.GetType().GetField("type");
            var amountField = slotObj.GetType().GetField("amount");

            Sprite icon = (Sprite)iconField.GetValue(slotObj);
            PickupType type = (PickupType)typeField.GetValue(slotObj);
            int amount = (int)amountField.GetValue(slotObj);

            playerData.savedItems.Add(new SavedItemData
            {
                iconName = icon ? icon.name : "",
                type = type,
                amount = amount
            });
        }

        Debug.Log($"[GameDataManager] 💾 Lưu {playerData.savedItems.Count} item vào HUD.");
    }

    // =====================================================
    // 🔹 LOAD HUD
    // =====================================================
    public void LoadHUDInventory(HUDInventory5 hud)
    {
        if (hud == null)
        {
            Debug.LogWarning("[GameDataManager] ⚠ Không thể load HUD: HUDInventory5 null!");
            return;
        }

        hud.ClearAllAndRevert();

        foreach (var item in playerData.savedItems)
        {
            // 🔹 Tìm sprite chuẩn
            Sprite found = Resources.Load<Sprite>(item.iconName);
            if (found == null)
            {
                // Nếu không thấy trong Resources, fallback tìm trong scene
                var all = Resources.FindObjectsOfTypeAll<Sprite>();
                foreach (var s in all)
                {
                    if (s.name == item.iconName)
                    {
                        found = s;
                        break;
                    }
                }
            }

            hud.AddItem(item.type, item.amount, found);
        }

        Debug.Log($"[GameDataManager] 🔁 Load lại {playerData.savedItems.Count} item vào HUD.");
    }
}
