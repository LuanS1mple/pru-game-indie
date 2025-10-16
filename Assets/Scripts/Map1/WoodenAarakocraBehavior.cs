using System.Collections;
using UnityEngine;
using Pathfinding;

public class WoodenAarakocraBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject target;
    private Animator animator;

    [Header("Ranges")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1.5f;  // thời gian nghỉ giữa các đòn
    [SerializeField] private float attackDuration = 1.0f;   // thời gian animation đánh
    [SerializeField] private float attackSpeed = 1.0f;      // tốc độ đánh mặc định

    private bool isAttacking = false;
    private bool canAttack = true;
    private int attackCount = 0;

    [Header("HP")]
    public float hp = 100f;
    private bool isTakingHit = false;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (aiPath == null)
            aiPath = GetComponent<AIPath>();

        aiPath.canMove = true;
        animator.SetFloat("AttackSpeed", attackSpeed); // tốc độ animation
    }

    void Update()
    {
        if (isDead) return;

        animator.SetFloat("Hp", hp);

        // 💀 Kiểm tra chết
        if (hp <= 0.01f)
        {
            StartCoroutine(DieRoutine());
            return;
        }

        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.transform.position);

        // 🧭 Lật hướng theo hướng di chuyển
        if (aiPath.desiredVelocity.x >= 0.01f)
            transform.localScale = new Vector3(1, 1, 1);
        else if (aiPath.desiredVelocity.x <= -0.01f)
            transform.localScale = new Vector3(-1, 1, 1);

        // --- Hành vi ---
        if (distance <= detectRange)
        {
            aiPath.destination = target.transform.position;

            if (distance <= attackRange)
            {
                aiPath.canMove = false;

                if (!isAttacking && canAttack && !isTakingHit)
                {
                    StartCoroutine(AttackRoutine());
                }
            }
            else
            {
                // chỉ di chuyển, không đánh
                aiPath.canMove = true;
            }
        }
        else
        {
            aiPath.canMove = false;
        }
    }
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        canAttack = false;
        aiPath.canMove = false;

        // 🚫 Kiểm tra lại khoảng cách trước khi đánh
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance > attackRange)
        {
            // Nếu target đã chạy ra xa thì huỷ tấn công
            isAttacking = false;
            canAttack = true;
            aiPath.canMove = true;
            yield break;
        }

        // ⚔️ Nếu chưa đủ 3 combo → đánh thường
        if (attackCount < 3)
        {
            animator.SetTrigger("trig_attack");
            attackCount++;
            Debug.Log($"🗡️ Attack thường ({attackCount}/3)");
        }
        else
        {
            // 🔥 Lần 4 → Special Attack
            animator.SetTrigger("trig_special");
            Debug.Log("🔥 Special Attack!");
            attackCount = 0; // reset combo
        }

        // ⏱ chờ hết animation tấn công
        yield return new WaitForSeconds(attackDuration / attackSpeed);

        // ⚙️ chờ cooldown giữa 2 đòn
        yield return new WaitForSeconds(attackCooldown);

        aiPath.canMove = true;
        canAttack = true;
        isAttacking = false;
    }


    // 💥 Bị đánh trúng
    public void TakeDamage(float damage)
    {
        if (isTakingHit || isDead) return;

        hp -= damage;
        animator.SetFloat("Hp", hp);

        StartCoroutine(TakeHitRoutine());
    }

    IEnumerator TakeHitRoutine()
    {
        isTakingHit = true;
        animator.SetTrigger("trig_hit");
        yield return new WaitForSeconds(0.4f);
        isTakingHit = false;
    }

    // ☠️ Chết
    IEnumerator DieRoutine()
    {
        isDead = true;
        aiPath.canMove = false;

        animator.SetTrigger("trig_die");
        Debug.Log("☠️ Aarakocra has died!");

        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
