using Pathfinding;
using System.Collections;
using UnityEngine;

public class SlimeBehavior : MonoBehaviour
{
    [SerializeField] private AIPath aiPath;
    [SerializeField] private GameObject Target;
    private Animator animator;

    private bool isIdle;
    private bool isAttack;
    private bool isChasing;
    private bool isAttackingNow = false; // ngăn spam coroutine

    [Header("Ranges")]
    [SerializeField] private float DetectRange = 6f;
    [SerializeField] private float AttackRange = 2f;

    [Header("Attack Cooldown")]
    [SerializeField] private float AttackCooldown = 3f;
    private float lastAttackTime;

    void Start()
    {
        animator = GetComponent<Animator>();

        isIdle = true;
        isAttack = false;
        isChasing = false;

        aiPath.canMove = true;
        lastAttackTime = -AttackCooldown;
    }

    void Update()
    {
        if (Target == null) return;

        float distance = Vector2.Distance(transform.position, Target.transform.position);
        float timeSinceLastAttack = Time.time - lastAttackTime;

        // Trong tầm phát hiện nhưng chưa đủ gần để tấn công
        if (distance <= DetectRange && distance > AttackRange)
        {
            if (!isChasing)
            {
                isChasing = true;
                isIdle = false;
                isAttack = false;
                Debug.Log("Slime phát hiện Player -> bắt đầu đuổi!");
            }

            aiPath.canMove = true;
            aiPath.destination = Target.transform.position;
        }
        // Trong tầm tấn công
        else if (distance <= AttackRange)
        {
            aiPath.canMove = false;

            if (!isAttackingNow && timeSinceLastAttack >= AttackCooldown)
            {
                StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
        // Ngoài tầm phát hiện
        else
        {
            if (!isIdle)
            {
                isIdle = true;
                isChasing = false;
                isAttack = false;
                Debug.Log("Player rời khỏi phạm vi, Slime quay về Idle.");
            }

            aiPath.canMove = false;
        }

        // Cập nhật animation
        animator.SetBool("IsIdle", isIdle);
        animator.SetBool("IsAttack", isAttack);

        Debug.Log($"Distance: {distance:F2}, Move: {aiPath.canMove}, Cooldown: {timeSinceLastAttack:F2}, State: Idle={isIdle}, Chase={isChasing}, Attack={isAttack}");
    }

    IEnumerator AttackRoutine()
    {
        isAttackingNow = true;
        isAttack = true;
        isIdle = false;

        animator.SetBool("IsAttack", true);
        animator.SetBool("IsIdle", false);

        Debug.Log("Slime bắt đầu tấn công!");

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning("Không tìm thấy Collider2D trên Slime!");
            yield break;
        }

        Vector2 originalOffset = col.offset;

        // === Dựa trên scale để xác định hướng (EnemyGFX đã lật) ===
        float facingDir = (transform.localScale.x < 0) ? 1f : -1f;

        float pushDistance = 0.7f;
        col.offset = originalOffset + new Vector2(facingDir * pushDistance, 0);

        Debug.Log($"Collider tiến về phía {(facingDir > 0 ? "phải" : "trái")}, offset = {col.offset}");

        yield return new WaitForSeconds(1f);

        col.offset = originalOffset;

        isAttack = false;
        isIdle = true;
        animator.SetBool("IsAttack", false);
        animator.SetBool("IsIdle", true);

        isAttackingNow = false;
        Debug.Log("Slime kết thúc tấn công, collider đã trở về vị trí ban đầu.");
    }




}
