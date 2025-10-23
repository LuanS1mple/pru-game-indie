using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathFollower : MonoBehaviour
{
    [Header("References")]
    [Tooltip("WaypointGraph chứa tất cả các Waypoint (Dùng để giới hạn phạm vi đuổi).")]
    public WaypointGraph graph;
    [Tooltip("Transform của người chơi (Target).")]
    public Transform target;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Settings")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.2f;
    public float detectionRadius = 6f;
    public float waypointTolerance = 0.5f;

    [Header("Patrol Settings")]
    [Tooltip("Phạm vi tuần tra (ngang) độc lập quanh điểm hiện tại.")]
    public float patrolRange = 2f;

    private bool isChasing = false;
    private bool facingRight = true;

    private Waypoint currentLimitWaypoint;
    private Vector2 patrolCenter;
    private int patrolDirection = 1;

    // === Dead-End (Idle Timer) Logic ===
    private float deadEndTimer = 0f;
    private float deadEndWaitTime = 3f;
    private bool isIdleAtDeadEnd = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
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
            return nearest;

        foreach (var neighbor in nearest.neighbors)
        {
            if (neighbor != null && neighbor.neighbors.Count >= 2)
                return neighbor;
        }

        return nearest;
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
                isChasing = true;
                currentLimitWaypoint = graph.GetClosestWaypoint(transform.position);
            }

            ChaseTargetWithWaypointLimit();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                rb.velocity = Vector2.zero;

                Waypoint fallbackPoint = FindFallbackPatrolWaypoint(transform.position);
                if (fallbackPoint != null && fallbackPoint.neighbors.Count >= 2)
                    InitializePatrolCenter(fallbackPoint.Position);
                else
                    InitializePatrolCenter((Vector2)transform.position);
            }

            Patrol();
        }
    }

    // === Đuổi có giới hạn + Idle 3s tại dead-end ===
    void ChaseTargetWithWaypointLimit()
    {
        Vector2 currentPos = transform.position;
        currentLimitWaypoint = graph.GetClosestWaypoint(currentPos);

        if (currentLimitWaypoint == null)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            float dirX = target.position.x > transform.position.x ? 1 : -1;
            RotateToDirection(dirX);
            return;
        }

        Waypoint targetNode = graph.GetClosestWaypoint(target.position);
        bool isTargetReachable = false;

        if (targetNode != null && graph.FindPath(currentLimitWaypoint, targetNode) != null)
            isTargetReachable = true;

        bool isDeadEndWaypoint = currentLimitWaypoint.neighbors.Count <= 1;

        // ====== DEAD-END IDLE LOGIC ======
        if (isDeadEndWaypoint)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);

            float dirX = target.position.x > transform.position.x ? 1 : -1;
            RotateToDirection(dirX);

            // Nếu thấy player, reset thời gian chờ (vẫn đứng tại chỗ canh)
            float distToPlayer = Vector2.Distance(transform.position, target.position);
            if (distToPlayer <= detectionRadius)
            {
                deadEndTimer = 0f;
                return;
            }

            // Không thấy player => bắt đầu tính thời gian chờ
            deadEndTimer += Time.deltaTime;

            if (deadEndTimer >= deadEndWaitTime)
            {
                // Sau 3s, tìm waypoint có >= 2 neighbors để quay lại
                Waypoint fallback = FindNearestMultiNeighborWaypoint(currentLimitWaypoint);
                if (fallback != null && fallback != currentLimitWaypoint)
                {
                    MoveTowards(fallback.Position, chaseSpeed * 0.9f);

                    float distToFallback = Vector2.Distance(currentPos, fallback.Position);
                    if (distToFallback <= waypointTolerance)
                    {
                        deadEndTimer = 0f;
                        InitializePatrolCenter(fallback.Position);
                    }
                }
            }

            return;
        }

        // ====== BÌNH THƯỜNG (CÓ ĐƯỜNG) ======
        if (!isDeadEndWaypoint && isTargetReachable)
        {
            deadEndTimer = 0f;
            MoveTowards(target.position, chaseSpeed);
        }
        else
        {
            // Không thể tới player nhưng không phải dead-end
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            float dir = target.position.x > transform.position.x ? 1 : -1;
            RotateToDirection(dir);
        }
    }

    /// <summary>
    /// BFS tìm waypoint gần nhất có >= 2 neighbors (để enemy rút lui)
    /// </summary>
    Waypoint FindNearestMultiNeighborWaypoint(Waypoint start)
    {
        if (start == null || graph == null) return null;
        if (start.neighbors.Count >= 2) return start;

        Queue<Waypoint> queue = new Queue<Waypoint>();
        HashSet<Waypoint> visited = new HashSet<Waypoint>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            Waypoint current = queue.Dequeue();

            foreach (var neighbor in current.neighbors)
            {
                if (neighbor == null || visited.Contains(neighbor)) continue;
                visited.Add(neighbor);

                if (neighbor.neighbors.Count >= 2)
                    return neighbor;

                queue.Enqueue(neighbor);
            }
        }

        return start;
    }

    // === PATROL ===
    void Patrol()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;

        if (Mathf.Abs(currentPos.x - targetPos.x) < waypointTolerance)
        {
            patrolDirection *= -1;
            targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;
        }

        MoveTowards(targetPos, patrolSpeed);
    }

    // === HỖ TRỢ ===
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

        if (!isChasing)
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
        }
    }
}
