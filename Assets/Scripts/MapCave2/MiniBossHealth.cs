using System.Collections;
using UnityEngine;

public class MiniBossHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 10;
    private int currentHP;

    [Header("Animator Settings")]
    [Tooltip("Kéo trực tiếp object có Animator (ví dụ: MiniBossMapCave2)")]
    public Animator animatorObject;

    [Header("Animation Triggers (phải trùng với Animator)")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";

    [Header("Optional")]
    public float deathDelay = 1f;
    public GameObject explosionVFX;

    private Animator anim;
    private bool isDead = false;

    void Start()
    {
        currentHP = maxHP;

        // 🔍 Lấy animator từ object được gán hoặc tự tìm trong con
        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();

        if (anim == null)
        {
            Debug.LogError($"[MiniBossHealth] ❌ Không tìm thấy Animator trong {name} hoặc con của nó!");
        }
        else
        {
            Debug.Log($"[MiniBossHealth] ✅ Animator tìm thấy: {anim.gameObject.name}");
        }
    }

    // 💥 Nhận damage từ Player
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"[MiniBossHealth] 💥 Bị đánh! HP: {currentHP}/{maxHP}");

        if (currentHP > 0)
            PlayHurtAnimation();
        else
            Die();
    }

    private void PlayHurtAnimation()
    {
        if (anim == null)
        {
            Debug.LogError("[MiniBossHealth] ❌ Không có Animator để chạy animation Hurt!");
            return;
        }

        if (!anim.HasParameterOfType(hurtTrigger, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogError($"[MiniBossHealth] ❌ Animator không có trigger tên '{hurtTrigger}'!");
            return;
        }

        Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {hurtTrigger}");
        anim.SetTrigger(hurtTrigger);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[MiniBossHealth] ☠ Bắt đầu quy trình chết...");

        if (anim != null && anim.HasParameterOfType(deadTrigger, AnimatorControllerParameterType.Trigger))
        {
            Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {deadTrigger}");
            anim.SetTrigger(deadTrigger);
        }

        // 🔇 Tắt collider để không bị đánh thêm
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // 💣 Hiệu ứng nổ (nếu có)
        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Debug.Log("[MiniBossHealth] ❌ Xoá miniboss khỏi Scene.");
        Destroy(gameObject);
    }

    // 🔥 Khi Player Attack collider chạm miniboss
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("TestAttack"))
        {
            Debug.Log($"[MiniBossHealth] ⚔ Bị tấn công bởi {collision.name}");
            TakeDamage(1); // hoặc damage khác tùy game
        }
    }
}


// ===============================
// 🔧 Hàm mở rộng kiểm tra Animator
// ===============================
public static class AnimatorExtensions
{
    public static bool HasParameterOfType(this Animator animator, string name, AnimatorControllerParameterType type)
    {
        foreach (var param in animator.parameters)
            if (param.type == type && param.name == name)
                return true;
        return false;
    }
}
