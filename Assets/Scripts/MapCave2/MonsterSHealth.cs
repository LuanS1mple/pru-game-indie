using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Enemy;

public class MonsterSHealth : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMonsterSDied;

    [Header("MonsterS Stats")]
    public MonsterSStats stats; // Kéo thả vào Inspector (hoặc tự động tìm)

    [Header("Animator Settings")]
    public Animator animatorObject;

    [Header("Animation Triggers")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";
    public string idleState = "MonsterSIdle"; // giống cách MiniBoss dùng IdleMiniBoss

    [Header("Optional")]
    public float deathDelay = 1f;
    public float hurtRecoverDelay = 0.6f;
    public GameObject deathVFX;

    private Animator anim;
    private bool isDead = false;
    private bool isInvulnerable = false;

    // Đọc HP hiện tại
    public int GetCurrentHealth() => stats != null ? stats.currentHP : 0;

    void Start()
    {
        // nếu quên kéo Stats, tự tìm
        if (stats == null)
        {
            stats = GetComponent<MonsterSStats>();
            if (stats == null)
            {
                Debug.LogError("[MonsterSHealth] ❌ Thiếu MonsterSStats! Gắn hoặc kéo vào Inspector.");
                enabled = false;
                return;
            }
        }

        // khởi tạo HP
        stats.Init();

        // Lấy Animator
        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError($"[MonsterSHealth] ❌ Không tìm thấy Animator trong {name}!");
        else
            Debug.Log($"[MonsterSHealth] ✅ Animator tìm thấy: {anim.gameObject.name}");

        // Lấy Audio Manager
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();
        else
            Debug.LogWarning("[MonsterSHealth] ⚠ Không tìm thấy object có tag 'Audio' trong scene!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable || stats == null) return;

        // Apply damage
        stats.TakeDamage(damage);
        Debug.Log($"[MonsterSHealth] 💥 Bị đánh! HP: {stats.currentHP}/{stats.maxHP}");

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
            // Quay lại Idle state
            anim.Play(idleState);
            Debug.Log("[MonsterSHealth] ↩ Quay lại trạng thái Idle.");
        }

        isInvulnerable = false;
    }

    private void PlayHurtAnimation()
    {
        if (anim == null) return;

        try
        {
            anim.SetTrigger(hurtTrigger);
            Debug.Log($"[MonsterSHealth] ▶ Gọi trigger: {hurtTrigger}");
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"[MonsterSHealth] ⚠ Animator có thể không có trigger '{hurtTrigger}'.");
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        OnMonsterSDied?.Invoke();
        Debug.Log("[MonsterSHealth] ☠ Bắt đầu quy trình chết...");

        try
        {
            if (anim != null)
            {
                anim.SetTrigger(deadTrigger);
                Debug.Log($"[MonsterSHealth] ▶ Gọi trigger: {deadTrigger}");
            }
        }
        catch (System.Exception)
        {
            Debug.LogWarning($"[MonsterSHealth] ⚠ Animator có thể không có trigger '{deadTrigger}'.");
        }

        // Tắt collider
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        // Spawn VFX
        if (deathVFX != null)
            Instantiate(deathVFX, transform.position, Quaternion.identity);

        // Âm thanh
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.monsterDeath);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Debug.Log("[MonsterSHealth] ❌ Xoá MonsterS khỏi Scene.");
        Destroy(gameObject);
    }

    // Nhận damage khi trúng hitbox
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead || isInvulnerable || stats == null) return;

        if (collision.CompareTag("TestAttack"))
        {
            // ✅ Lấy damage theo mẫu FlyingEyeBehaviors
            BaseStats attacker = collision.GetComponentInParent<BaseStats>();
            int damage = attacker != null ? Mathf.RoundToInt(attacker.attack) : 5;

            Debug.Log($"[Monster1Health] ⚔ Bị tấn công bởi {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }

}
