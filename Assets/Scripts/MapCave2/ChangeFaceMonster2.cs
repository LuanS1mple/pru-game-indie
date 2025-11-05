using Pathfinding;
using UnityEngine;

public class ChangeFaceMonster2 : MonoBehaviour
{
    [Header("References")]
    public AIPath aiPath;
    [Tooltip("Nếu sprite mặc định quay sang trái, bật tùy chọn này.")]
    public bool flip = false;

    [Tooltip("Object cần xoay thêm cùng hướng (vd: GunTip)")]
    public Transform extraObjectToFlip; // Gắn GunTip vào đây trong Inspector

    private bool facingRight = true;

    void Update()
    {
        if (aiPath == null) return;

        float direction = aiPath.desiredVelocity.x;

        if (direction >= 0.01f && !facingRight)
        {
            Flip(true);
        }
        else if (direction <= -0.01f && facingRight)
        {
            Flip(false);
        }
    }

    private void Flip(bool faceRight)
    {
        facingRight = faceRight;

        // ✅ Lật thân quái
        transform.localScale = flip
            ? (faceRight ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1))
            : (faceRight ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1));

        // ✅ Lật luôn GunTip (nếu có)
        if (extraObjectToFlip != null)
        {
            // Thay vì lật scale, ta xoay hẳn GunTip
            if (faceRight)
                extraObjectToFlip.localRotation = Quaternion.Euler(0, 0, 0);
            else
                extraObjectToFlip.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }
}
