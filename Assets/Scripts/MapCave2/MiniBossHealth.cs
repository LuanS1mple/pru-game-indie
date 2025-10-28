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
    public string idleState = "IdleMiniBoss"; // 👈 Tên state Idle trong Animator

    [Header("Optional")]
    public float deathDelay = 1f;
    public float hurtRecoverDelay = 0.6f; // 👈 Thời gian chờ sau Hurt để quay lại tấn công
    public GameObject explosionVFX;

    private Animator anim;
    private bool isDead = false;

    public int GetCurrentHealth() => currentHP;

    void Start()
    {
        currentHP = maxHP;
        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();

        if (anim == null)
            Debug.LogError($"[MiniBossHealth] ❌ Không tìm thấy Animator trong {name}!");
        else
            Debug.Log($"[MiniBossHealth] ✅ Animator tìm thấy: {anim.gameObject.name}");

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;
        Debug.Log($"[MiniBossHealth] 💥 Bị đánh! HP: {currentHP}/{maxHP}");

        if (currentHP > 0)
            StartCoroutine(HurtAndRecover());
        else
            Die();
    }

    private IEnumerator HurtAndRecover()
    {
        PlayHurtAnimation();

        // ⏳ Chờ một chút cho animation Hurt chạy xong
        yield return new WaitForSeconds(hurtRecoverDelay);

        // 👇 Quay lại trạng thái tấn công (Idle hoặc tự logic tấn công)
        if (!isDead && anim != null)
        {
            anim.Play(idleState);
            Debug.Log("[MiniBossHealth] ↩ Quay lại trạng thái IdleMiniBoss (chuẩn bị tấn công lại).");
        }
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
        //Âm thanh quái chết
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.monsterDeath);
        }
        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Debug.Log("[MiniBossHealth] ❌ Xoá miniboss khỏi Scene.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("TestAttack"))
        {
            Debug.Log($"[MiniBossHealth] ⚔ Bị tấn công bởi {collision.name}");
            TakeDamage(1);
        }
    }
}

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
