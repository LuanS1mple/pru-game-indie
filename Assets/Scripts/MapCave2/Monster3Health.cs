using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Game.Enemy;

public class Monster3Health : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Events")]
    public UnityEvent OnMonsterDied;

    [Header("Monster Stats")]
    public Monster3Stats stats; // Kéo th? trong Inspector

    [Header("Animator Settings")]
    public Animator animatorObject;

    [Header("Animation Triggers")]
    public string hurtTrigger = "Hurt";
    public string deadTrigger = "Dead";
    public string idleState = "IdleMonster3";

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
        // Gán t? ð?ng n?u quên kéo trong Inspector
        if (stats == null)
        {
            stats = GetComponent<Monster3Stats>();
            if (stats == null)
            {
                Debug.LogError("[Monster3Health] ? Thi?u tham chi?u Monster3Stats! H?y g?n ho?c kéo vào Inspector.");
                enabled = false;
                return;
            }
        }

        stats.Init();

        anim = animatorObject != null ? animatorObject : GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError($"[Monster3Health] ? Không t?m th?y Animator trong {name}!");
        else
            Debug.Log($"[Monster3Health] ? Animator t?m th?y: {anim.gameObject.name}");

        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
            audioManager = audioObj.GetComponent<AudioManager>();
        else
            Debug.LogWarning("[Monster3Health] ? Không t?m th?y object có tag 'Audio' trong scene!");
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvulnerable || stats == null) return;

        stats.TakeDamage(damage);
        Debug.Log($"[Monster3Health] ?? B? ðánh! HP: {stats.currentHP}/{stats.maxHP}");

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
            Debug.Log("[Monster3Health] ? Quay l?i tr?ng thái IdleMonster3.");
        }

        isInvulnerable = false;
    }

    private void PlayHurtAnimation()
    {
        if (anim == null) return;

        try
        {
            anim.SetTrigger(hurtTrigger);
            Debug.Log($"[Monster3Health] ? G?i trigger: {hurtTrigger}");
        }
        catch
        {
            Debug.LogWarning($"[Monster3Health] ? Animator không có trigger '{hurtTrigger}'.");
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        OnMonsterDied?.Invoke();
        Debug.Log("[Monster3Health] ? Quái ch?t...");

        try
        {
            if (anim != null)
            {
                anim.SetTrigger(deadTrigger);
                Debug.Log($"[Monster3Health] ? G?i trigger: {deadTrigger}");
            }
        }
        catch
        {
            Debug.LogWarning($"[Monster3Health] ? Animator không có trigger '{deadTrigger}'.");
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
        Debug.Log("[Monster3Health] ? Xoá Monster3 kh?i Scene.");
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
            Debug.Log($"[Monster3Health] ? B? t?n công b?i {collision.name}, Damage: {damage}");
            TakeDamage(damage);
        }
    }
}
