using UnityEngine;
using UnityEngine.UI;

public enum PickupType
{
    Heal,
    MaxHealthUp,
    AttackUp,
    DefenseUp
}

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    [Header("Pickup Settings")]
    public PickupType type = PickupType.Heal;
    public int amount = 2;
    public bool healToFullOnMaxUp = true;
    public Sprite iconToShow;
    public AudioClip sfx;

    [Header("UI Prompt")]
    public GameObject pressFIndicator; // Text hoặc icon "Press F"

    private bool playerInRange = false;
    private PlayerStats playerStats;

    void Reset()
    {
        // Collider tự động là trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // ✅ Gắn tag và layer tự động nếu chưa có
        gameObject.tag = "Item";

        int pickupLayer = LayerMask.NameToLayer("Pickup");
        if (pickupLayer == -1)
        {
            Debug.LogWarning("⚠️ Layer 'Pickup' chưa tồn tại! Hãy tạo layer 'Pickup' trong Unity.");
        }
        else
        {
            gameObject.layer = pickupLayer;
        }
    }

    void Start()
    {
        if (pressFIndicator != null)
            pressFIndicator.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            Pickup();
        }
    }

    private void Pickup()
    {
        if (playerStats == null) return;

        // Áp dụng hiệu ứng
        switch (type)
        {
            case PickupType.Heal:
                playerStats.Heal(amount);
                break;
            case PickupType.MaxHealthUp:
                playerStats.IncreaseMaxHealth(amount, healToFullOnMaxUp);
                break;
            case PickupType.AttackUp:
                playerStats.IncreaseAttack(amount);
                break;
            case PickupType.DefenseUp:
                playerStats.IncreaseDefense(amount);
                break;
        }

        // ✅ Thêm icon vào HUDInventory5
        if (HUDInventory5.Instance != null && iconToShow != null)
        {
            HUDInventory5.Instance.Add(iconToShow);
        }

        // Âm thanh nhặt đồ
        if (sfx != null)
        {
            AudioSource.PlayClipAtPoint(sfx, transform.position);
        }

        // Ẩn thông báo & xóa vật phẩm
        if (pressFIndicator != null)
            pressFIndicator.SetActive(false);

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        playerInRange = true;
        playerStats = stats;

        if (pressFIndicator != null)
            pressFIndicator.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.GetComponentInParent<PlayerStats>() == playerStats)
        {
            playerInRange = false;
            playerStats = null;

            if (pressFIndicator != null)
                pressFIndicator.SetActive(false);
        }
    }
}
