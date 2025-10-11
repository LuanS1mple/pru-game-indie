using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogBehavior : MonoBehaviour
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
            aiPath.canMove = true;

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

        Debug.Log("Dog bắt đầu tấn công!");

        // Lưu tốc độ gốc để trả lại sau
        float originalSpeed = aiPath.maxSpeed;
        float attackSpeed = originalSpeed * 2.5f; // tăng tốc khi attack
        aiPath.maxSpeed = attackSpeed;
        aiPath.canMove = true;

        // Cập nhật điểm đến liên tục trong 1.5s để Dog lao về Player
        float attackDuration = 1.5f;
        float elapsed = 0f;
        while (elapsed < attackDuration)
        {
            if (Target != null)
            {
                aiPath.destination = Target.transform.position; // luôn nhắm tới Player
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Trả lại trạng thái ban đầu
        aiPath.maxSpeed = originalSpeed;
        isAttack = false;
        isIdle = true;
        animator.SetBool("IsAttack", false);
        animator.SetBool("IsIdle", true);

        isAttackingNow = false;
        Debug.Log("Dog kết thúc tấn công, tốc độ trở lại bình thường.");
    }



}
