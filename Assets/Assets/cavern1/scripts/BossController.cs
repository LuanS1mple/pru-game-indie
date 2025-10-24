using UnityEngine;

public class BossController : MonoBehaviour
{
    public BossHitBox hitBox; // Gán trong Inspector

    // Gọi từ Animation Event
    public void EnableHitBox()
    {
        if (hitBox != null)
            hitBox.EnableDamage();
    }

    // Gọi từ Animation Event
    public void DisableHitBox()
    {
        if (hitBox != null)
            hitBox.DisableDamage();
    }
}
