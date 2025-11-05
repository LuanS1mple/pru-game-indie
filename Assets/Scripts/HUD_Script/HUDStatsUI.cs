using UnityEngine;
using TMPro;

public class HUDStatsUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats playerStats;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;

    [Header("Format")]
    [Tooltip("Ví dụ: \"ATK: {0}\" sẽ thay bằng giá trị ATK")]
    public string atkFormat = "ATK: {0}";
    public string defFormat = "DEF: {0}";

    void Reset()
    {
        // Thử tự tìm PlayerStats trên scene
        if (!playerStats) playerStats = FindObjectOfType<PlayerStats>();
    }

    void OnEnable()
    {
        if (!playerStats) return;

        // đăng ký event
        playerStats.OnAttackChanged += HandleAttackChanged;
        playerStats.OnDefenseChanged += HandleDefenseChanged;

        // vẽ lần đầu
        RefreshAll();
    }

    void OnDisable()
    {
        if (!playerStats) return;
        playerStats.OnAttackChanged -= HandleAttackChanged;
        playerStats.OnDefenseChanged -= HandleDefenseChanged;
    }

    void Start()
    {
        // phòng trường hợp OnEnable chạy trước khi PlayerStats init xong
        RefreshAll();
    }

    void HandleAttackChanged(int attack, int defense)
    {
        if (atkText) atkText.text = string.Format(atkFormat, attack);
        // Có thể update DEF kèm theo cho chắc
        if (defText) defText.text = string.Format(defFormat, defense);
    }

    void HandleDefenseChanged(int attack, int defense)
    {
        if (defText) defText.text = string.Format(defFormat, defense);
        if (atkText) atkText.text = string.Format(atkFormat, attack);
    }

    public void RefreshAll()
    {
        if (!playerStats) return;
        if (atkText) atkText.text = string.Format(atkFormat, playerStats.Attack);
        if (defText) defText.text = string.Format(defFormat, playerStats.Defense);
    }
}
