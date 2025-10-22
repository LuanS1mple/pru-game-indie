using UnityEngine;
using Pathfinding;
using System.Collections;

public class EnemyBehavior : BaseStats
{
    private AIPath aiPath;
    private Animator anim;
    private Transform target;

    [Header("Detection Settings")]
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float chaseLimitRange = 10f;

    private Vector2 spawnPosition;
    private bool isAttacking = false;
    private float lastAttackTime = 0f;

    private enum EnemyState { Idle, Move, Attack, Dead }
    private EnemyState currentState = EnemyState.Idle;
    private Vector3 lastPosition;


    protected override void Awake()
    {
        base.Awake();
        aiPath = GetComponent<AIPath>();
        anim = GetComponentInChildren<Animator>(true);
        spawnPosition = transform.position;
    }

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player")?.transform;
        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        if (isDead || target == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        float distanceToSpawn = Vector2.Distance(transform.position, spawnPosition);

        if (distanceToSpawn > chaseLimitRange)
        {
            aiPath.destination = spawnPosition;
            aiPath.canMove = true;
            return;
        }

        if (distanceToPlayer <= attackRange)
        {
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
            ChangeState(EnemyState.Idle);
        }
    }

    private void LateUpdate()
    {
        if (isDead || anim == null || aiPath == null) return;

        if (!isAttacking)
        {
            // Tính toán vận tốc thực tế dựa trên vị trí
            float speed = (transform.position - lastPosition).magnitude / Time.deltaTime;

            if (speed > 0.05f)
                ChangeState(EnemyState.Move);
            else
                ChangeState(EnemyState.Idle);

            lastPosition = transform.position;
        }
    }


    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState || isAttacking) return;
        currentState = newState;

        switch (newState)
        {
            case EnemyState.Idle:
                anim.ResetTrigger("Move");
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Idle");
                break;

            case EnemyState.Move:
                anim.ResetTrigger("Idle");
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Move");
                break;

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
        lastAttackTime = Time.time;

        anim.ResetTrigger("Move");
        anim.ResetTrigger("Idle");
        anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.5f);

        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange + 0.2f)
        {
            BaseStats playerStats = target.GetComponent<BaseStats>();
            if (playerStats != null)
                playerStats.TakeDamage(attack);
        }

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        if (!isDead)
            ChangeState(EnemyState.Idle);
    }

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        base.TakeDamage(damage);
        anim.SetTrigger("Hit");

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

        GameObject root = transform.root.gameObject;
        Destroy(root, 2.5f);
    }
}
