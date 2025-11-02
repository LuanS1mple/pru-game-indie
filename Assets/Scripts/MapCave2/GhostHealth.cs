using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Enemy;

public class GhostHealth : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnGhostDied;

    [Header("Ghost Stats")]
    public GhostStats stats; // Kéo thả vào trong Inspector

    [Header("Animator Settings")]
    public Animator animatorObject;

    [Header("Animation States")]
    public string runState = "GhostRun";
    public string deadTrigger = "Dead";

    [Header("Optional Settings")]
    public float deathDelay = 0.8f;
    public float hurtRecoverDelay = 0.4f;
    public GameObject deathVFX;

    private Animator anim;
    private bool isDead = false;
    private bool isInvulnerable = false;

    public int GetCurrentHealth() => stats != null ? stats.currentHP : 0;

    void Start()
    {
        // Gán tự động nếu quên kéo trong Inspector
        if (stats == null)
        {
            stats = GetComponent<GhostStats>();
            if (stats == null)
            {
                Debug.LogError("[GhostHealth] ❌ Thiếu GhostStats! Hãy gắn hoặc kéo vào Inspector.");
                enabled = false;
                return;
            }
        }

        stats.Init();

        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError($"[GhostHealth] ❌ Không tìm thấy Animator trong {name}!");
        else
            Debug.Log($"[GhostHealth] ✅ Animator tìm thấy: {anim.gameObject.name}");

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();
        else
            Debug.LogWarning("[GhostHealth] ⚠ Không tìm thấy object có tag 'Audio' trong scene!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable || stats == null) return;

        stats.TakeDamage(damage);
        Debug.Log($"[GhostHealth] 💥 Bị đánh! HP: {stats.currentHP}/{stats.maxHP}");

        if (!stats.IsDead())
            StartCoroutine(HurtAndRecover());
        else
            Die();
    }

    private IEnumerator HurtAndRecover()
    {
        isInvulnerable = true;

        // Ghost không có animation Hurt riêng → chỉ cần tiếp tục animation chạy
        if (anim != null)
            anim.Play(runState);

        yield return new WaitForSeconds(hurtRecoverDelay);

        if (!isDead && anim != null)
        {
            anim.Play(runState);
            Debug.Log("[GhostHealth] ↩ Quay lại trạng thái GhostRun.");
        }

        isInvulnerable = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnGhostDied?.Invoke();
        Debug.Log("[GhostHealth] ☠ Ghost chết...");

        try
        {
            if (anim != null)
            {
                anim.SetTrigger(deadTrigger);
                Debug.Log($"[GhostHealth] ▶ Gọi trigger: {deadTrigger}");
            }
        }
        catch
        {
            Debug.LogWarning($"[GhostHealth] ⚠ Animator không có trigger '{deadTrigger}'.");
        }

        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        if (deathVFX != null)
            Instantiate(deathVFX, transform.position, Quaternion.identity);

        if (audioManager != null)
            audioManager.PlaySFX(audioManager.monsterDeath);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Debug.Log("[GhostHealth] ❌ Xoá Ghost khỏi Scene.");
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable || stats == null) return;
        if (!collision.CompareTag("TestAttack")) return;

        PlayerCombat playerCombat = collision.GetComponentInParent<PlayerCombat>();
        if (playerCombat != null && playerCombat.playerStats != null)
        {
            int damage = (int)playerCombat.playerStats.attack;
            Debug.Log($"[GhostHealth] ⚔ Bị tấn công bởi {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }
}
