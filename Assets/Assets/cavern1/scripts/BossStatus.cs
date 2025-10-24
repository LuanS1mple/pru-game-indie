using UnityEngine;
using System.Collections;

public class BossStatus : MonoBehaviour
{
    [Header("Thông số cơ bản")]
    public string bossName = "Boss";
    public float maxHealth = 500f;
    public float currentHealth;

    [Header("Trạng thái chiến đấu")]
    public bool isInvulnerable = false;   // Không thể bị đánh trong một số animation
    public bool isDead = false;           // Boss đã chết hay chưa

    [Header("Cấu hình thời gian")]
    public float deathDelay = 2f;         // Thời gian boss biến mất sau khi chết

    // Tham chiếu tới các component
    private Animator animator;
    private Rigidbody2D rb;
    private BossMovement bossMovement;
    private Collider2D bossCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bossMovement = GetComponent<BossMovement>();
        bossCollider = GetComponent<Collider2D>();

        currentHealth = maxHealth;
    }

    // Nhận sát thương 
    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"{bossName} nhận {damage} sát thương! (Còn {currentHealth}/{maxHealth})");

        if (currentHealth > 0)
        {
            if (animator != null)
            {
                animator.ResetTrigger("BossHurt");
                animator.SetTrigger("BossHurt");
            }
        }
        else
        {
            StartCoroutine(HandleDeath());
        }
    }

    // Khi va chạm với đòn đánh của Player
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isInvulnerable) return;

        PlayerCombat playerCombat = other.GetComponentInParent<PlayerCombat>();
        if (playerCombat == null) return;

        BaseStats playerStats = playerCombat.playerStats;
        if (playerStats == null) return;

        TakeDamage(playerStats.attack);
        Debug.Log($"⚔️ {bossName} bị chém bởi Player, nhận {playerStats.attack} sát thương!");
    }

    // Xử lý cái chết của Boss
    private IEnumerator HandleDeath()
    {
        if (isDead) yield break;

        isDead = true;
        isInvulnerable = true;
        currentHealth = 0;

        Debug.Log($"{bossName} đã chết!");

        // Ngắt di chuyển và vật lý, nhưng không tắt collider
        if (bossMovement != null)
            bossMovement.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Không rơi
        }

        // Chạy animation chết
        if (animator != null)
        {
            animator.ResetTrigger("BossHurt");
            animator.SetTrigger("BossDie");
        }

        // Giữ collider để boss không rơi, nhưng không còn va chạm vật lý
        if (bossCollider != null)
            bossCollider.isTrigger = true;

        // Chờ thời gian animation
        yield return new WaitForSeconds(deathDelay);

        // Biến mất
        Destroy(gameObject);
    }

    // Hiệu ứng bất tử tạm thời
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
