    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEyeStats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 50;
    private int currentHealth;

    [Header("State")]
    public bool IsDeath = false;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (IsDeath)
        {
            animator.SetTrigger("die");
        }
    }

    // Gọi khi bị tấn công
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("TestAttack") && !IsDeath)
        {
            TakeDamage(10);
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        IsDeath = true;
        animator.SetTrigger("die");

        // Ngăn di chuyển, collider...
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Gọi hàm xóa sau khi animation death chạy hết
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        // Lấy thời gian animation death (nếu có)
        float deathAnimLength = 0f;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains("Death"))
                {
                    deathAnimLength = clip.length;
                    break;
                }
            }
        }

        yield return new WaitForSeconds(deathAnimLength > 0 ? deathAnimLength : 1.0f);
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
