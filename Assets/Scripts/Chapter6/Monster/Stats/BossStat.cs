using UnityEngine;

public class BossStat : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 100;
    private int currentHP;

    [Header("References")]
    public Animator animator;
    public BossBehaviors bossBehaviors;
    private Collider2D bossCollider;

    [HideInInspector] public bool isDead = false;

    void Start()
    {
        currentHP = maxHP;
        if (animator == null)
            animator = GetComponent<Animator>();
        if (bossBehaviors == null)
            bossBehaviors = GetComponent<BossBehaviors>();
        bossCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TestAttack") && !isDead)
            TakeDamage(20);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

        if (bossBehaviors != null && !bossBehaviors.IsHurt)
            StartCoroutine(bossBehaviors.HurtRoutine());

        if (currentHP <= 0)
            Die();
    }

    void Die()
    {
        isDead = true;

        // Ngắt toàn bộ hành động
        if (bossBehaviors != null)
        {
            bossBehaviors.StopAllCoroutines();
            bossBehaviors.IsHurt = false;
            bossBehaviors.IsAttack = false;
            bossBehaviors.IsCast = false;
            bossBehaviors.IsWalk = false;

            if (bossBehaviors.aiPath != null)
                bossBehaviors.aiPath.canMove = false;
        }

        // Tắt collider để không bị đánh thêm
        if (bossCollider != null)
            bossCollider.enabled = false;

        // Phát animation chết
        animator.SetTrigger("Death");

        // Hủy boss sau 2 giây
        Destroy(gameObject, 2f);
    }
}
