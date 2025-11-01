using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameAnimationController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    void Start()
    {
        animator = GetComponent<Animator>();
        rb= GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra tag
        if (collision.CompareTag("Player") || collision.CompareTag("Ground"))
        {
            animator.SetTrigger("IsBom");
            // Ngừng mọi chuyển động
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Tắt trọng lực
            rb.gravityScale = 0f;

            // Giữ nguyên vị trí, không rơi nữa
            rb.bodyType = RigidbodyType2D.Kinematic;
            Destroy(gameObject, 0.18f);
        }
    }
}
