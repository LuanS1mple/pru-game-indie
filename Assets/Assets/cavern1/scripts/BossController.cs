using UnityEngine;

// Script này giờ chỉ chịu trách nhiệm điều khiển các sự kiện combat (từ Animation)
public class BossController : MonoBehaviour
{
    [Header("Combat References")]
    public BossHitBox hitBox;

    // Các hàm này được gọi từ Animation Events
    public void EnableHitBox()
    {
        if (hitBox != null)
            hitBox.EnableDamage();
    }

    public void DisableHitBox()
    {
        if (hitBox != null)
            hitBox.DisableDamage();
    }
}