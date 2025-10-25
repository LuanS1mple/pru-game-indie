using UnityEngine;
using System.Collections;
using System;

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
        if (isDead) return;
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
        animator.SetBool("IsDeath",true);

        // Chờ animation chạy xong rồi mới hủy object
        StartCoroutine(WaitForDeathAnimation());
    }

    private IEnumerator WaitForDeathAnimation()
    {
        // Đợi cho đến khi animation "Death" bắt đầu
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));

        // Lấy thời lượng clip death
        float deathDuration = animator.GetCurrentAnimatorStateInfo(0).length;

        // Đợi hết animation
        yield return new WaitForSeconds(deathDuration);

        // Hủy object sau khi animation kết thúc
        Destroy(gameObject);
    }

    internal int GetCurrentHealth()
    {
        return currentHP;
    }
}
