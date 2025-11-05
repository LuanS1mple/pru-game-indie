using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Enemy;

public class MiniBossHealth : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMiniBossDied;

    [Header("MiniBoss Stats")]
    public MiniBossStats stats; // kéo thả vào Inspector (recommended)

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
    private bool isInvulnerable = false;

    // tiện ích đọc HP
    public int GetCurrentHealth() => stats != null ? stats.currentHP : 0;

    void Start()
    {
        // nếu bạn quên kéo stats trong Inspector, cố lấy component tự động
        if (stats == null)
        {
            stats = GetComponent<MiniBossStats>();
            if (stats == null)
            {
                Debug.LogError("[MiniBossHealth] ❌ Thiếu tham chiếu MiniBossStats! Gắn MiniBossStats hoặc kéo vào Inspector.");
                enabled = false;
                return;
            }
        }

        // init HP
        stats.Init();

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
        if (isDead || isInvulnerable || stats == null) return;

        // Apply damage to stats
        stats.TakeDamage(damage);
        Debug.Log($"[MiniBossHealth] 💥 Bị đánh! HP: {stats.currentHP}/{stats.maxHP}");

        if (!stats.IsDead())
            StartCoroutine(HurtAndRecover());
        else
            Die();
    }

    private IEnumerator HurtAndRecover()
    {
        isInvulnerable = true;
        PlayHurtAnimation();

        yield return new WaitForSeconds(hurtRecoverDelay);

        if (!isDead && anim != null)
        {
            // Play idle state by name (ensure state exists)
            anim.Play(idleState);
            Debug.Log("[MiniBossHealth] ↩ Quay lại trạng thái IdleMiniBoss.");
        }

        isInvulnerable = false;
    }

    private void PlayHurtAnimation()
    {
        if (anim == null) return;

        // Set trigger safely (if trigger doesn't exist, just warn)
        try
        {
            anim.SetTrigger(hurtTrigger);
            Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {hurtTrigger}");
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"[MiniBossHealth] ⚠ Animator có thể không có trigger '{hurtTrigger}'.");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        OnMiniBossDied?.Invoke();
        Debug.Log("[MiniBossHealth] ☠ Bắt đầu quy trình chết...");

        try
        {
            if (anim != null)
            {
                anim.SetTrigger(deadTrigger);
                Debug.Log($"[MiniBossHealth] ▶ Gọi trigger: {deadTrigger}");
            }
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"[MiniBossHealth] ⚠ Animator có thể không có trigger '{deadTrigger}'.");
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

    // Nhận damage khi trúng hitbox tấn công player (tag = "TestAttack")
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable || stats == null) return;

        if (collision.CompareTag("TestAttack"))
        {
            // ✅ Lấy damage theo mẫu FlyingEyeBehaviors
            BaseStats attacker = collision.GetComponentInParent<BaseStats>();
            int damage = attacker != null ? Mathf.RoundToInt(attacker.attack) : 5;

            Debug.Log($"[MiniBossHealth] ⚔ Bị tấn công bởi {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }

}
