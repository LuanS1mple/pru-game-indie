using UnityEngine;
using System.Collections;

public class BlueSlimeStatus : MonoBehaviour
{
    [Header("Thông số cơ bản")]
    public string slimeName = "Blue Slime";
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Trạng thái chiến đấu")]
    public bool isInvulnerable = false;
    public bool isDead = false;

    [Header("Cấu hình thời gian")]
    public float deathDelay = 1.5f;

    // Tham chiếu component
    private Animator animator;
    private Rigidbody2D rb;
    private BlueSlimeMovement slimeMovement;
    private Collider2D slimeCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        slimeMovement = GetComponent<BlueSlimeMovement>();
        slimeCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"{slimeName} nhận {damage} sát thương! (Còn {currentHealth}/{maxHealth})");

        if (currentHealth > 0)
        {
            if (animator != null)
            {
                animator.ResetTrigger("SlimeHurt");
                animator.SetTrigger("SlimeHurt");
            }
        }
        else
        {
            StartCoroutine(HandleDeath());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isInvulnerable) return;

        PlayerCombat playerCombat = other.GetComponentInParent<PlayerCombat>();
        if (playerCombat == null) return;

        BaseStats playerStats = playerCombat.playerStats;
        if (playerStats == null) return;

        TakeDamage(playerStats.attack);
        Debug.Log($"⚔️ {slimeName} bị chém, nhận {playerStats.attack} sát thương!");
    }

    private IEnumerator HandleDeath()
    {
        if (isDead) yield break;

        isDead = true;
        isInvulnerable = true;
        currentHealth = 0;

        Debug.Log($"{slimeName} đã chết!");

        if (slimeMovement != null)
            slimeMovement.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (animator != null)
        {
            animator.ResetTrigger("SlimeHurt");
            animator.SetTrigger("SlimeDie");
        }

        if (slimeCollider != null)
            slimeCollider.isTrigger = true;

        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    public void SetInvulnerable(float duration)
    {
        StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float time)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(time);
        isInvulnerable = false;
    }

    public bool IsAlive() => !isDead && currentHealth > 0;
}
