using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathFollower : BaseStats
{
    // ... (Giữ nguyên các biến) ...
    [Header("References")]
    public WaypointGraph graph;
    public Transform target;
    private Rigidbody2D rb;
    private Animator anim;
    [Header("Movement Settings")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.2f;
    public float detectionRadius = 6f;
    public float waypointTolerance = 0.5f;
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public Collider2D attackCollider;
    public float knockbackForce = 3f;
    [Header("Waypath Limit")]
    public float maxDistanceFromWaypointX = 1.5f;
    [Header("Endpoint Wait")]
    public float endpointWaitTime = 3f;
    [Header("Patrol Settings")]
    public float patrolRange = 2f;
    private bool isChasing = false;
    private bool facingRight = true;
    private bool isWaiting = false;
    private Coroutine waitCoroutine;
    private bool isGuardBound = false;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private bool canDealDamage = false;
    private HashSet<Collider2D> hitPlayers = new HashSet<Collider2D>();
    private Waypoint currentLimitWaypoint;
    private Waypoint fallbackPatrolPoint;
    private Vector2 patrolCenter;
    private int patrolDirection = 1;


    // ... (Giữ nguyên Awake, InitializePatrolCenter, FindFallbackPatrolWaypoint) ...
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (GetComponent<Animator>() != null) anim = GetComponent<Animator>();
        InitializePatrolCenter((Vector2)transform.position);
        if (attackCollider != null)
            attackCollider.enabled = false;
    }
    void InitializePatrolCenter(Vector2 position)
    {
        patrolCenter = position;
        patrolDirection = (Random.value > 0.5f) ? 1 : -1;
    }
    Waypoint FindFallbackPatrolWaypoint(Vector2 currentPosition)
    {
        Waypoint nearest = graph.GetClosestWaypoint(currentPosition);
        if (nearest == null) return null;
        if (nearest.neighbors.Count >= 2) return nearest;
        foreach (var neighbor in nearest.neighbors)
        {
            if (neighbor != null && neighbor.neighbors.Count >= 2)
            {
                return neighbor;
            }
        }
        return nearest;
    }


    // ... (Giữ nguyên StopWaitAndBeginChase, StopWaitAndBeginPatrol) ...
    void StopWaitAndBeginChase()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        isAttacking = false;
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        isWaiting = false;
        isChasing = true;
        isGuardBound = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
    void StopWaitAndBeginPatrol()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        isAttacking = false;
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        isWaiting = false;
        isChasing = false;
        rb.velocity = Vector2.zero;
        isGuardBound = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        fallbackPatrolPoint = FindFallbackPatrolWaypoint(transform.position);
        if (fallbackPatrolPoint != null && fallbackPatrolPoint.neighbors.Count >= 2)
        {
            InitializePatrolCenter(fallbackPatrolPoint.Position);
        }
        else
        {
            InitializePatrolCenter((Vector2)transform.position);
        }
    }


    // ... (Giữ nguyên Update, StartAttack, AttackRoutine) ...
    private void Update()
    {
        if (isDead || isAttacking)
        {
            rb.velocity = Vector2.zero;
            if (!isAttacking)
                if (anim) anim.SetFloat("Speed", 0);
            if (isAttacking && target)
                RotateToDirection(target.position.x - transform.position.x);
            return;
        }
        if (target == null || graph == null)
        {
            if (isGuardBound) StopWaitAndBeginPatrol();
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            return;
        }
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (isChasing && distanceToPlayer <= attackRange)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            StartAttack();
            return;
        }
        if (distanceToPlayer <= detectionRadius)
        {
            if (!isChasing)
            {
                StopWaitAndBeginChase();
                fallbackPatrolPoint = FindFallbackPatrolWaypoint(transform.position);
            }
            ChaseTargetWithWaypointLimit();
        }
        else
        {
            if (isChasing || isWaiting || isGuardBound)
            {
                StopWaitAndBeginPatrol();
            }
            Patrol();
        }
    }
    void StartAttack()
    {
        if (isAttacking) return;

        attackCoroutine = StartCoroutine(AttackRoutine());
    }
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero;
        if (anim) anim.SetFloat("Speed", 0);
        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);
        if (anim) anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackCoroutine = null;
    }


    // ⭐⭐⭐ LOGIC TẤN CÔNG (ĐÃ ĐỔI TÊN HÀM) ⭐⭐⭐

    // --- Animation Event: Bắt đầu khung sát thương ---
    // SỬ DỤNG HÀM NÀY:
    public void HandleAnimation_EnableAttackCollider()
    {
        if (attackCollider == null) return;
        Debug.Log($"[{entityName}] --- BẬT Hitbox Tấn công!", this.gameObject);
        canDealDamage = true;
        hitPlayers.Clear();
        StartCoroutine(RefreshCollider());
    }

    IEnumerator RefreshCollider()
    {
        attackCollider.enabled = false;
        yield return null;
        Vector3 originalPos = attackCollider.transform.localPosition;
        attackCollider.transform.localPosition += new Vector3(0.01f, 0, 0);
        attackCollider.enabled = true;
        yield return null;
        attackCollider.transform.localPosition = originalPos;
    }

    // --- Animation Event: Kết thúc khung sát thương ---
    // SỬ DỤNG HÀM NÀY:
    public void HandleAnimation_DisableAttackCollider()
    {
        if (attackCollider == null) return;
        Debug.Log($"[{entityName}] --- TẮT Hitbox Tấn công.", this.gameObject);
        canDealDamage = false;
        attackCollider.enabled = false;
    }

    // --- HÀM CŨ (Đã đổi tên) ---
    // public void AnimationEvent_EnableAttackCollider() { ... }
    // public void AnimationEvent_DisableAttackCollider() { ... }


    // ... (Giữ nguyên TryDealDamage, OnTriggerEnter2D, OnTriggerStay2D) ...
    private void TryDealDamage(Collider2D collision)
    {
        if (!canDealDamage) return;
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (hitPlayers.Contains(collision)) return;
        BaseStats playerStats = collision.GetComponentInParent<BaseStats>();
        if (playerStats != null)
        {
            playerStats.TakeDamage(attack);
            hitPlayers.Add(collision);
            Debug.Log($"[{entityName}] đã đánh trúng Player ({collision.name})! Gây {attack} sát thương.");
            Rigidbody2D playerRb = collision.attachedRigidbody;
            if (playerRb != null)
            {
                Vector2 dir = (collision.transform.position - transform.position).normalized;
                dir.y = 0f;
                playerRb.velocity = Vector2.zero;
                playerRb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        TryDealDamage(collision);
    }


    // ⭐⭐⭐ LOGIC HÀM GỐC (ĐỂ TRÁNH LỖI) ⭐⭐⭐

    // HÀM MỚI: Dùng hàm này trong Animation "Dead" (nếu bạn cần gọi Die() từ animation)
    public void HandleAnimation_TriggerDie()
    {
        Die(); // Gọi hàm Die() đã có
    }

    // HÀM MỚI: Dùng hàm này nếu bạn muốn animation tự gây sát thương
    public void HandleAnimation_TriggerTakeDamage(float damage)
    {
        TakeDamage(damage); // Gọi hàm TakeDamage() đã có
    }

    // ... (Giữ nguyên ChaseTargetWithWaypointLimit, Patrol, MoveTowards, RotateToDirection) ...
    void ChaseTargetWithWaypointLimit()
    {
        Vector2 currentPos = transform.position;
        currentLimitWaypoint = graph.GetClosestWaypoint(currentPos);
        if (currentLimitWaypoint == null)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            return;
        }
        Waypoint targetNode = graph.GetClosestWaypoint(target.position);
        bool isTargetReachable = (targetNode != null && graph.FindPath(currentLimitWaypoint, targetNode) != null);
        if (isTargetReachable)
        {
            isGuardBound = false;
        }
        else
        {
            isGuardBound = true;
        }
        if (isGuardBound)
        {
            float distToLimit = Vector2.Distance(currentPos, currentLimitWaypoint.Position);
            if (distToLimit > waypointTolerance)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                MoveTowards(currentLimitWaypoint.Position, chaseSpeed);
                RotateToDirection(target.position.x - transform.position.x);
            }
            else
            {
                rb.velocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
                if (anim) anim.SetFloat("Speed", 0);
                RotateToDirection(target.position.x - transform.position.x);
            }
            return;
        }
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        float xDifference = Mathf.Abs(currentPos.x - currentLimitWaypoint.Position.x);
        if (xDifference > maxDistanceFromWaypointX)
        {
            MoveTowards(currentLimitWaypoint.Position, chaseSpeed);
            RotateToDirection(target.position.x - transform.position.x);
            return;
        }
        MoveTowards(target.position, chaseSpeed);
    }
    private IEnumerator WaitAndPatrol()
    {
        isWaiting = true;
        yield return new WaitForSeconds(endpointWaitTime);
        isWaiting = false;
        waitCoroutine = null;
    }
    void Patrol()
    {
        if (isWaiting || isGuardBound) return;
        Vector2 currentPos = transform.position;
        Vector2 targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;
        if (Mathf.Abs(currentPos.x - targetPos.x) < waypointTolerance)
        {
            patrolDirection *= -1;
            targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;
        }
        MoveTowards(targetPos, patrolSpeed);
    }
    void MoveTowards(Vector2 targetPos, float speed)
    {
        Vector2 currentPos = transform.position;
        Vector2 dir = (targetPos - currentPos).normalized;
        rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
        RotateToDirection(dir.x);
        if (anim) anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
    void RotateToDirection(float dirX)
    {
        if (dirX > 0.01f && !facingRight)
        {
            facingRight = true;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (dirX < -0.01f && facingRight)
        {
            facingRight = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }


    // ... (Giữ nguyên TakeDamage, Die, OnDrawGizmos) ...
    public override void TakeDamage(float damage)
    {
        if (isDead) return;
        base.TakeDamage(damage);
        if (anim) anim.SetTrigger("Hit");
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            attackCoroutine = null;
            if (attackCollider != null) attackCollider.enabled = false;
        }
        if (isDead)
            Die();
    }
    protected override void Die()
    {
        base.Die();
        if (anim) anim.SetTrigger("Dead");
        StopAllCoroutines();
        Destroy(gameObject, 2.5f);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (attackCollider != null)
        {
            Gizmos.color = attackCollider.enabled ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }
        if (currentLimitWaypoint != null)
        {
            Vector3 waypointPos3D = currentLimitWaypoint.Position;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Vector3 limitLeft = waypointPos3D + Vector3.left * maxDistanceFromWaypointX;
            Vector3 limitRight = waypointPos3D + Vector3.right * maxDistanceFromWaypointX;
            Gizmos.DrawLine(limitLeft + Vector3.up * 10f, limitLeft + Vector3.down * 10f);
            Gizmos.DrawLine(limitRight + Vector3.up * 10f, limitRight + Vector3.down * 10f);
        }
        if (!isChasing && !isWaiting && !isGuardBound)
        {
            Gizmos.color = Color.cyan;
            Vector2 pointA = patrolCenter + Vector2.right * patrolRange;
            Vector2 pointB = patrolCenter - Vector2.right * patrolRange;
            Gizmos.DrawLine(pointA, pointB);
            Gizmos.DrawWireSphere(pointA, 0.2f);
            Gizmos.DrawWireSphere(pointB, 0.2f);
        }
        if (isChasing && currentLimitWaypoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentLimitWaypoint.Position, 0.5f);
            Gizmos.DrawLine(transform.position, currentLimitWaypoint.Position);
            if (fallbackPatrolPoint != null && currentLimitWaypoint != fallbackPatrolPoint)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(fallbackPatrolPoint.Position, 0.4f);
            }
        }
    }
}