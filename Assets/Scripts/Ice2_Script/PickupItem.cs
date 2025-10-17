using UnityEngine;

public enum PickupType { Heal, MaxHealthUp }

[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    public PickupType type = PickupType.Heal;
    public int amount = 2;                 // Heal +2 HP hoặc +2 Max HP
    public bool healToFullOnMaxUp = true;  // khi tăng Max có hồi đầy không
    public Sprite iconToShow;
    public AudioClip sfx;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerStats>(out var stats)) return;

        if (type == PickupType.Heal) stats.Heal(amount);
        else stats.IncreaseMaxHealth(amount, healToFullOnMaxUp);

        if (HUDInventory5.Instance) HUDInventory5.Instance.Add(iconToShow);
        if (sfx) AudioSource.PlayClipAtPoint(sfx, transform.position);
        Destroy(gameObject);
    }
}
