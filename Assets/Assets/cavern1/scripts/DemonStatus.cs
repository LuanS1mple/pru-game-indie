using UnityEngine;
using System.Collections;

public class DemonStatus : MonoBehaviour
{
    public string demonName = "Demon";
    public float maxHealth = 300f;
    public float currentHealth;
    public bool isInvulnerable = false;
    public bool isDead = false;
    public float deathDelay = 1.5f;

    private Animator animator;
    private Rigidbody2D rb;
    private DemonMovement demonMovement;
    private Collider2D demonCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        demonMovement = GetComponent<DemonMovement>();
        demonCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"{demonName} nhận {damage} sát thương! (Còn {currentHealth}/{maxHealth})");

        if (currentHealth > 0)
        {
            if (animator != null)
            {
                animator.ResetTrigger("DemonHurt");
                animator.SetTrigger("DemonHurt");
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
        Debug.Log($"⚔️ {demonName} bị tấn công bởi Player, nhận {playerStats.attack} sát thương!");
    }

    private IEnumerator HandleDeath()
    {
        if (isDead) yield break;

        isDead = true;
        isInvulnerable = true;
        currentHealth = 0;

        Debug.Log($"{demonName} đã chết!");

        if (demonMovement != null)
            demonMovement.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (animator != null)
        {
            animator.ResetTrigger("DemonHurt");
            animator.SetTrigger("DemonDie");
        }

        if (demonCollider != null)
            demonCollider.isTrigger = true;

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
