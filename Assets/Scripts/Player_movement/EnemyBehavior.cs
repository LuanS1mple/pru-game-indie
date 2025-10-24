using UnityEngine;
using Pathfinding;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBehavior : BaseStats
{
    private AIPath aiPath;
    private Animator anim;
    private Transform target;
    private Rigidbody2D rb;

    [Header("Detection Settings")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chaseLimitRange = 10f;

    private Vector2 spawnPosition;
    private bool isAttacking = false;

    // Chỉ dùng cho logic code, không tương tác trực tiếp với Animator
    private enum EnemyState { Idle, Move, Attack, Dead }
    private EnemyState currentState = EnemyState.Idle;

    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        anim = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // 🛑 Ngừng xử lý nếu chết, không có Target, hoặc đang trong chuỗi tấn công
        if (isDead || target == null || aiPath == null)
        {
            if (isDead) aiPath.canMove = false;
            return;
        }

        // Nếu đang tấn công, chỉ xoay nhân vật và KHÔNG thay đổi vị trí/hướng đi
        if (isAttacking)
        {
            aiPath.canMove = false;
            RotateToDirection(target.position.x - transform.position.x);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float distanceToSpawn = Vector2.Distance(transform.position, spawnPosition);

        // 1. Kiểm tra Giới hạn phạm vi đuổi (Backtrack)
        if (distanceToSpawn > chaseLimitRange)
        {
            aiPath.destination = spawnPosition;
            aiPath.canMove = true;
            if (distanceToSpawn < 0.1f)
            {
                aiPath.canMove = false;
                // Chuyển sang Idle thông qua LateUpdate
            }
            return;
        }

        // 2. Kiểm tra Tấn công (Ưu tiên cao nhất)
        if (distanceToPlayer <= attackRange)
        {
            aiPath.canMove = false;
            ChangeState(EnemyState.Attack);
        }
        // 3. Kiểm tra Đuổi
        else if (distanceToPlayer <= detectRange)
        {
            aiPath.destination = target.position;
            aiPath.canMove = true;
        }
        // 4. Kiểm tra Idle
        else
        {
            aiPath.canMove = false;
            // Chuyển sang Idle thông qua LateUpdate
        }
    }

    private void LateUpdate()
    {
        if (isDead || anim == null || aiPath == null) return;

        // Dùng vận tốc mong muốn của AIPath
        float speed = aiPath.desiredVelocity.magnitude;

        // 1. Xoay nhân vật (nếu không đang tấn công, xoay theo hướng di chuyển)
        if (!isAttacking && aiPath.desiredVelocity.x != 0)
        {
            RotateToDirection(aiPath.desiredVelocity.x);
        }

        // 2. 🔑 Cập nhật Float Speed cho Animator (Điều khiển Idle <-> Move)
        anim.SetFloat("Speed", speed);

        // Cập nhật trạng thái code (dành cho logic, không phải Animator)
        if (speed > 0.05f)
        {
            currentState = EnemyState.Move;
        }
        else
        {
            if (currentState != EnemyState.Attack && currentState != EnemyState.Dead)
                currentState = EnemyState.Idle;
        }
    }


    private void ChangeState(EnemyState newState)
    {
        // Ngăn chặn trạng thái Move/Idle ghi đè khi đang Attack
        if (isAttacking && newState != EnemyState.Attack) return;

        // Tránh chạy lại Coroutine khi đã ở trạng thái Attack
        if (currentState == newState && newState == EnemyState.Attack) return;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Attack:
                StartCoroutine(AttackRoutine());
                break;

            case EnemyState.Dead:
                anim.SetTrigger("Dead"); // Sử dụng Trigger
                break;

                // Idle và Move được điều khiển bởi anim.SetFloat("Speed") trong LateUpdate
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Quay mặt về Target ngay trước khi tấn công
        if (target != null)
        {
            RotateToDirection(target.position.x - transform.position.x);
        }

        // 🔑 Kích hoạt animation Attack bằng Trigger
        anim.SetTrigger("Attack");

        // Chờ thời gian để khớp với khung gây sát thương (0.5s)
        yield return new WaitForSeconds(0.5f);

        // Gây sát thương
        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange + 0.2f)
        {
            BaseStats playerStats = target.GetComponent<BaseStats>();
            if (playerStats != null)
                // Giả định 'attack' là thuộc tính sát thương (damage value) trong BaseStats
                playerStats.TakeDamage(attack);
        }

        // Chờ thời gian cooldown còn lại (attackCooldown - 0.5s đã chờ)
        float remainingCooldown = attackCooldown - 0.5f;
        if (remainingCooldown > 0)
        {
            yield return new WaitForSeconds(remainingCooldown);
        }

        isAttacking = false;
        if (!isDead)
        {
            // Trạng thái sẽ được LateUpdate đưa về Idle hoặc Move (nếu Enemy bắt đầu di chuyển)
            currentState = EnemyState.Idle;
        }
    }

    // Hàm xoay nhân vật (dùng transform.localScale)
    private void RotateToDirection(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            if (directionX > 0 && transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (directionX < 0 && transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
        anim.SetTrigger("Hit"); // 🔑 Kích hoạt animation Hit bằng Trigger

        // Hủy tấn công nếu đang diễn ra
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
        }

        // Cho phép di chuyển lại nếu chưa chết
        if (!isDead)
            aiPath.canMove = true;

        if (isDead)
        {
            Die();
        }
    }

    protected override void Die()
    {
        base.Die();
        aiPath.canMove = false;
        ChangeState(EnemyState.Dead);

        StopAllCoroutines();

        GameObject root = transform.root.gameObject;
        Destroy(root, 2.5f);
    }

    // --- HIỂN THỊ TẦM ĐÁNH VÀ PHÁT HIỆN BẰNG GIZMOS ---
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;

        // 1. Bán kính Phát hiện (Detection Range)
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, detectRange);

        // 2. Bán kính Tấn công (Attack Range)
        Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
        Gizmos.DrawWireSphere(center, attackRange);

        // 3. Giới hạn tầm đuổi (Chase Limit Range)
        if (spawnPosition != Vector2.zero)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
            Gizmos.DrawWireSphere(spawnPosition, chaseLimitRange);
        }
    }
}