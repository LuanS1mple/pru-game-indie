using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Enemy;

public class Monster1Health : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMonsterDied;

    [Header("Monster Stats")]
    public Monster1Stats stats; // Kéo thả vào trong Inspector

    [Header("Animator Settings")]
    public Animator animatorObject;

    [Header("Animation Triggers")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";
    public string idleState = "IdleMonster1";

    [Header("Optional Settings")]
    public float deathDelay = 0.8f;
    public float hurtRecoverDelay = 0.5f;
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
            stats = GetComponent<Monster1Stats>();
            if (stats == null)
            {
                Debug.LogError("[Monster1Health] ❌ Thiếu tham chiếu Monster1Stats! Hãy gắn hoặc kéo vào Inspector.");
                enabled = false;
                return;
            }
        }

        stats.Init();

        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError($"[Monster1Health] ❌ Không tìm thấy Animator trong {name}!");
        else
            Debug.Log($"[Monster1Health] ✅ Animator tìm thấy: {anim.gameObject.name}");

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();
        else
            Debug.LogWarning("[Monster1Health] ⚠ Không tìm thấy object có tag 'Audio' trong scene!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable || stats == null) return;

        stats.TakeDamage(damage);
        Debug.Log($"[Monster1Health] 💥 Bị đánh! HP: {stats.currentHP}/{stats.maxHP}");

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
            anim.Play(idleState);
            Debug.Log("[Monster1Health] ↩ Quay lại trạng thái IdleMonster1.");
        }

        isInvulnerable = false;
    }

    private void PlayHurtAnimation()
    {
        if (anim == null) return;

        try
        {
            anim.SetTrigger(hurtTrigger);
            Debug.Log($"[Monster1Health] ▶ Gọi trigger: {hurtTrigger}");
        }
        catch
        {
            Debug.LogWarning($"[Monster1Health] ⚠ Animator không có trigger '{hurtTrigger}'.");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDied?.Invoke();
        Debug.Log("[Monster1Health] ☠ Quái chết...");

        try
        {
            if (anim != null)
            {
                anim.SetTrigger(deadTrigger);
                Debug.Log($"[Monster1Health] ▶ Gọi trigger: {deadTrigger}");
            }
        }
        catch
        {
            Debug.LogWarning($"[Monster1Health] ⚠ Animator không có trigger '{deadTrigger}'.");
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
        Debug.Log("[Monster1Health] ❌ Xoá Monster1 khỏi Scene.");
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
            Debug.Log($"[Monster1Health] ⚔ Bị tấn công bởi {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }
}
