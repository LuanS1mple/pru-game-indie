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

        // Lưu vị trí ban đầu của collider
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogWarning("Không tìm thấy Collider2D trên Slime!");
            yield break;
        }

        Vector2 originalOffset = col.offset;

        // Xác định hướng thật từ slime tới target
        Vector2 direction = (Target.transform.position - transform.position).normalized;

        // Tiến collider ra theo hướng đó (0.5 đơn vị)
        float pushDistance = 0.5f;
        col.offset = originalOffset + direction * pushDistance;

        Debug.Log($"Collider tiến theo hướng {direction}, offset = {col.offset}");

        // Giữ collider ở phía trước 1 giây (thời gian ra đòn)
        yield return new WaitForSeconds(1f);

        // Trả collider về ban đầu
        col.offset = originalOffset;

        // Kết thúc animation attack
        isAttack = false;
        isIdle = true;
        animator.SetBool("IsAttack", false);
        animator.SetBool("IsIdle", true);

        isAttackingNow = false;
        Debug.Log("Slime kết thúc tấn công, collider đã trở về vị trí ban đầu.");
    }


}
