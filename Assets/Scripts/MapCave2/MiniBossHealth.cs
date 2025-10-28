using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MiniBossHealth : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMiniBossDied;

    [Header("HP Settings")]
    public int maxHP = 10;
    private int currentHP;

    [Header("Animator Settings")]
    public Animator animatorObject;

    [Header("Animation Triggers")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";
    public string idleState = "IdleMiniBoss";

    [Header("Optional")]
    public float deathDelay = 1f;
    public float hurtRecoverDelay = 0.6f;
    public GameObject explosionVFX;

    private Animator anim;
    private bool isDead = false;
    private bool isInvulnerable = false; // 🛡 Tránh nhận damage liên tục trong cùng lần chém

    public int GetCurrentHealth() => currentHP;

    void Start()
    {
        currentHP = maxHP;
        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();

        if (anim == null)
            Debug.LogError($"[MiniBossHealth] ❌ Không tìm thấy Animator trong {name}!");
        else
            Debug.Log($"[MiniBossHealth] ✅ Animator tìm thấy: {anim.gameObject.name}");

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();
        else
            Debug.LogWarning("[MiniBossHealth] ⚠ Không tìm thấy object có tag 'Audio' trong scene!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable) return;

        currentHP -= damage;
        Debug.Log($"[MiniBossHealth] 💥 Bị đánh! HP: {currentHP}/{maxHP}");

        if (currentHP > 0)
            StartCoroutine(HurtAndRecover());
        else
            Die();
    }

    private IEnumerator HurtAndRecover()
    {
        isInvulnerable = true; // 🛑 Tránh ăn damage thêm khi đang trong hurt state
        PlayHurtAnimation();

        yield return new WaitForSeconds(hurtRecoverDelay);

        if (!isDead && anim != null)
        {
            anim.Play(idleState);
            Debug.Log("[MiniBossHealth] ↩ Quay lại trạng thái IdleMiniBoss.");
        }

        isInvulnerable = false; // ✅ Cho phép nhận damage lại sau khi hồi phục
    }

    private void PlayHurtAnimation()
    {
        if (anim == null) return;

        if (!anim.HasParameterOfType(hurtTrigger, AnimatorControllerParameterType.Trigger))
        {
            Debug.LogError($"[MiniBossHealth] ❌ Animator không có trigger '{hurtTrigger}'!");
            return;
        }

        anim.SetTrigger(hurtTrigger);
        Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {hurtTrigger}");
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        OnMiniBossDied?.Invoke();
        Debug.Log("[MiniBossHealth] ☠ Bắt đầu quy trình chết...");

        if (anim != null && anim.HasParameterOfType(deadTrigger, AnimatorControllerParameterType.Trigger))
        {
            anim.SetTrigger(deadTrigger);
            Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {deadTrigger}");
        }

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.monsterDeath);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Debug.Log("[MiniBossHealth] ❌ Xoá miniboss khỏi Scene.");
        Destroy(gameObject);
    }

    // ✅ CHỈ nhận damage khi trúng hitbox tấn công của player (tag = "TestAttack")
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable) return; // ⛔ Không nhận thêm damage khi đang hurt
        if (!collision.CompareTag("TestAttack")) return;

        PlayerCombat playerCombat = collision.GetComponentInParent<PlayerCombat>();
        if (playerCombat != null && playerCombat.playerStats != null)
        {
            int damage = (int)playerCombat.playerStats.attack;
            Debug.Log($"[MiniBossHealth] ⚔ Bị tấn công bởi {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }
}

// 🔹 Kiểm tra animator parameter
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
