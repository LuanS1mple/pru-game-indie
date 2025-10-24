using UnityEngine;

public class SlimeController : MonoBehaviour
{
    public SlimeHitBox hitBox; // Gán object con chứa collider tấn công của Slime (trong Inspector)

    // Gọi từ Animation Event khi bắt đầu ra đòn
    public void EnableHitBox()
    {
        if (hitBox != null)
            hitBox.EnableDamage();
    }

    // Gọi từ Animation Event khi kết thúc đòn đánh
    public void DisableHitBox()
    {
        if (hitBox != null)
            hitBox.DisableDamage();
    }
}
