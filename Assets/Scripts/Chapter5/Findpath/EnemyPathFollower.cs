using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathFollower : MonoBehaviour
{
    [Header("References")]
    public WaypointGraph graph;
    public Transform target;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Settings")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.2f;
    public float detectionRadius = 6f;
    [Tooltip("Khoảng cách gần nhất để coi là Enemy đã tới Waypoint/Endpoint.")]
    public float waypointTolerance = 0.5f;

    [Header("Waypath Limit")]
    [Tooltip("Khoảng cách tối đa (theo phương ngang) cho phép Enemy cách xa Waypoint gần nhất.")]
    public float maxDistanceFromWaypointX = 1.5f;

    [Header("Endpoint Wait")]
    public float endpointWaitTime = 3f;

    [Header("Patrol Settings")]
    public float patrolRange = 2f;

    private bool isChasing = false;
    private bool facingRight = true;
    private bool isWaiting = false;
    private Coroutine waitCoroutine;
    private bool hasReachedLimit = false;
    private bool isGuardBound = false; // TRẠNG THÁI CANH GÁC: Đứng yên nhìn Player

    private Waypoint currentLimitWaypoint;
    private Waypoint fallbackPatrolPoint;
    private Vector2 patrolCenter;
    private int patrolDirection = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (GetComponent<Animator>() != null) anim = GetComponent<Animator>();
        InitializePatrolCenter((Vector2)transform.position);
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

        if (nearest.neighbors.Count >= 2)
        {
            return nearest;
        }

        foreach (var neighbor in nearest.neighbors)
        {
            if (neighbor != null && neighbor.neighbors.Count >= 2)
            {
                return neighbor;
            }
        }
        return nearest;
    }

    // ----------------------- CHỈNH 1 -----------------------
    void StopWaitAndBeginChase()
    {
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        isWaiting = false;
        isChasing = true;
        hasReachedLimit = false;
        isGuardBound = false;

        // MỞ KHÓA DI CHUYỂN KHI BẮT ĐẦU ĐUỔI
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    // ----------------------- CHỈNH 2 -----------------------
    void StopWaitAndBeginPatrol()
    {
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        isWaiting = false;
        isChasing = false;
        rb.velocity = Vector2.zero;
        hasReachedLimit = false;
        isGuardBound = false;

        // MỞ KHÓA DI CHUYỂN KHI QUAY LẠI TUẦN TRA
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

    private void Update()
    {
        if (target == null || graph == null)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

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

    // ----------------------------------------------------------------------------------
    // --- LOGIC CHASE (Đuổi) ---
    // ----------------------------------------------------------------------------------
    void ChaseTargetWithWaypointLimit()
    {
        Vector2 currentPos = transform.position;
        currentLimitWaypoint = graph.GetClosestWaypoint(currentPos);
        Waypoint targetNode = graph.GetClosestWaypoint(target.position);
        bool isTargetReachable = (targetNode != null && graph.FindPath(currentLimitWaypoint, targetNode) != null);

        float distanceToPlayer = Vector2.Distance(currentPos, target.position);

        // --- 1. Nếu đang ở trạng thái "canh gác" ---
        if (isGuardBound)
        {
            // Player rời xa khỏi phạm vi detection => quay về tuần tra
            if (distanceToPlayer > detectionRadius)
            {
                StopWaitAndBeginPatrol();
                return;
            }

            // Player đã quay lại vùng hợp lệ => tiếp tục chase
            if (isTargetReachable)
            {
                StopWaitAndBeginChase();
                return;
            }

            // Vẫn ở ngoài vùng => đứng yên và nhìn
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            RotateToDirection(target.position.x - transform.position.x);
            return;
        }

        // --- 2. Không có waypoint hợp lệ ---
        if (currentLimitWaypoint == null)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            return;
        }

        float xDifference = Mathf.Abs(currentPos.x - currentLimitWaypoint.Position.x);

        // --- 3. Enemy vượt giới hạn ---
        if (xDifference > maxDistanceFromWaypointX)
        {
            MoveTowards(currentLimitWaypoint.Position, chaseSpeed);
            RotateToDirection(target.position.x - transform.position.x);
            return;
        }

        // --- 4. Player trong vùng hợp lệ ---
        if (isTargetReachable)
        {
            MoveTowards(target.position, chaseSpeed);
            if (anim) anim.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            return;
        }

        // --- 5. Player KHÔNG nằm trong vùng hợp lệ ---
        float distToLimit = Vector2.Distance(currentPos, currentLimitWaypoint.Position);

        // ----------------------- CHỈNH 3 -----------------------
        if (distToLimit <= waypointTolerance)
        {
            // DỪNG HOÀN TOÀN KHI CHẠM GIỚI HẠN
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            // ÉP ANIMATOR VỀ IDLE
            if (anim)
            {
                anim.SetFloat("Speed", 0);
                anim.Play("idle");
            }

            // Quay hướng về phía player
            RotateToDirection(target.position.x - transform.position.x);

            // Bật trạng thái canh gác
            isGuardBound = true;
            return;
        }
        else
        {
            MoveTowards(currentLimitWaypoint.Position, chaseSpeed);
            RotateToDirection(target.position.x - transform.position.x);
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

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
