using UnityEngine;
using Pathfinding;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyBehavior : BaseStats
{
    private AIPath aiPath;
    private Animator anim;
    private Transform target;
    private Rigidbody2D rb;
    private CircleCollider2D detectTrigger;

    [Header("Detection Settings")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chaseLimitRange = 10f;
    [SerializeField] private LayerMask playerLayer;

    private Vector2 spawnPosition;
    private bool isAttacking = false;
    private bool playerInTrigger = false;
    private bool isPausedChase = false;

    private enum EnemyState { Idle, Move, Attack, Dead }
    private EnemyState currentState = EnemyState.Idle;

    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        anim = GetComponentInChildren<Animator>(true);
        rb = GetComponent<Rigidbody2D>();
        detectTrigger = GetComponent<CircleCollider2D>();

        detectTrigger.isTrigger = true;
        detectTrigger.radius = detectRange;
        spawnPosition = transform.position;
    }

    private void Update()
    {
        if (isDead || aiPath == null || isPausedChase)
        {
            if (isDead) aiPath.canMove = false;
            return;
        }

        if (target == null)
        {
            aiPath.canMove = false;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float distanceToSpawn = Vector2.Distance(transform.position, spawnPosition);

        // Giới hạn phạm vi đuổi
        if (distanceToSpawn > chaseLimitRange)
        {
            aiPath.destination = spawnPosition;
            aiPath.canMove = true;
            if (distanceToSpawn < 0.1f)
                aiPath.canMove = false;
            return;
        }

        // Tấn công khi player vào range
        if (playerInTrigger && distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            aiPath.canMove = false;
            ChangeState(EnemyState.Attack);
        }
        else if (distanceToPlayer <= detectRange)
        {
            aiPath.destination = target.position;
            aiPath.canMove = true;
        }
        else
        {
            aiPath.canMove = false;
        }
    }

    private void LateUpdate()
    {
        if (isDead || anim == null || aiPath == null) return;

        float speed = aiPath.desiredVelocity.magnitude;

        if (!isAttacking && aiPath.desiredVelocity.x != 0)
            RotateToDirection(aiPath.desiredVelocity.x);

        anim.SetFloat("Speed", speed);

        if (speed > 0.05f)
            currentState = EnemyState.Move;
        else if (currentState != EnemyState.Attack && currentState != EnemyState.Dead)
            currentState = EnemyState.Idle;
    }

    private void ChangeState(EnemyState newState)
    {
        if (isAttacking && newState != EnemyState.Attack) return;
        if (currentState == newState && newState == EnemyState.Attack) return;

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Attack:
                StartCoroutine(AttackRoutine());
                break;

            case EnemyState.Dead:
                anim.SetTrigger("Dead");
                break;
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Dừng hoàn toàn
        rb.velocity = Vector2.zero;
        aiPath.canMove = false;

        // Về Idle animation trước khi tấn công
        anim.Play("Idle");
        yield return new WaitForSeconds(0.2f); // chuẩn bị trước khi chém

        // Quay về player
        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        // Trigger attack animation
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.3f); // delay trước khi gây sát thương

        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange + 0.2f)
        {
            BaseStats playerStats = target.GetComponent<BaseStats>();
            if (playerStats != null)
                playerStats.TakeDamage(attack);
        }

        float remainingCooldown = attackCooldown - 0.5f;
        if (remainingCooldown > 0)
            yield return new WaitForSeconds(remainingCooldown);

        isAttacking = false;
        if (!isDead)
            currentState = EnemyState.Idle;
    }

    private void RotateToDirection(float directionX)
    {
        if (Mathf.Abs(directionX) > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Sign(directionX) * Mathf.Abs(transform.localScale.x),
                                               transform.localScale.y,
                                               transform.localScale.z);
        }
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
        anim.SetTrigger("Hit");

        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
        }

        if (!isDead)
            aiPath.canMove = true;

        if (isDead)
            Die();
    }

    protected override void Die()
    {
        base.Die();
        aiPath.canMove = false;
        ChangeState(EnemyState.Dead);
        StopAllCoroutines();
        Destroy(transform.root.gameObject, 2.5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            playerInTrigger = true;
            target = other.transform;
            Debug.Log($"{name}: Player entered detection zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            playerInTrigger = false;
            target = null;
            Debug.Log($"{name}: Player exited detection zone.");
        }
    }

    // ------------------ PAUSE CHASE ------------------
    public void PauseChase(float duration)
    {
        if (!isPausedChase)
            StartCoroutine(PauseChaseCoroutine(duration));
    }

    private IEnumerator PauseChaseCoroutine(float duration)
    {
        isPausedChase = true;
        aiPath.canMove = false;

        yield return new WaitForSeconds(duration);

        isPausedChase = false;
        if (target != null)
            aiPath.canMove = true;
    }

    // ------------------ Gizmos ------------------
    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawWireSphere(center, detectRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.75f);
        Gizmos.DrawWireSphere(center, attackRange);

        if (spawnPosition != Vector2.zero)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
            Gizmos.DrawWireSphere(spawnPosition, chaseLimitRange);
        }
    }
}
