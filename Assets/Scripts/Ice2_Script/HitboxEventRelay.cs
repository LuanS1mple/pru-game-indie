// HitboxEventRelay.cs
using UnityEngine;

public class HitboxEventRelay : MonoBehaviour
{
    [Header("Child hitbox (Collider2D ở object AttackHitbox)")]
    public Collider2D hitbox;               // kéo BoxCollider2D của child vào đây

    void Awake()
    {
        if (hitbox) hitbox.enabled = false; // tắt mặc định
    }

    // Animation Events sẽ gọi 2 hàm này
    public void EnableHitbox() { if (hitbox) hitbox.enabled = true; }
    public void DisableHitbox() { if (hitbox) hitbox.enabled = false; }
}

