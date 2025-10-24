using UnityEngine;

public class DemonController : MonoBehaviour
{
    public DemonHitBox hitBox; // Gán object con chứa collider đánh của Demon vào đây (trong Inspector)

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
