using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPathFollower : MonoBehaviour
{
    [Header("References")]
    public WaypointGraph graph;
    public Transform target;

    private Rigidbody2D rb;
    private Animator anim;

    [Header("Movement Settings")]
    public float chaseSpeed = 2f;
    public float patrolSpeed = 1.2f;
    public float detectionRadius = 6f;
    public float repathDelay = 0.4f;
    public float patrolRange = 2f;

    private float repathTimer = 0f;
    private bool isChasing = false;
    private bool facingRight = true;

    private List<Waypoint> currentPath = new List<Waypoint>();
    private int currentWaypointIndex = 0;

    // Tuần tra
    private Vector2 patrolCenter;
    private int patrolDirection = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        patrolCenter = transform.position; // tuần tra ban đầu quanh chỗ spawn
    }

    private void Update()
    {
        if (target == null || graph == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= detectionRadius)
        {
            // ✅ Player trong phạm vi phát hiện → bắt đầu đuổi
            if (!isChasing)
            {
                isChasing = true;
                currentPath.Clear();
            }

            repathTimer += Time.deltaTime;
            if (repathTimer >= repathDelay)
            {
                UpdatePathToTarget();
                repathTimer = 0f;
            }

            FollowPathToPlayer();
        }
        else
        {
            // ❌ Player ra khỏi tầm → dừng đuổi, tạo vùng tuần tra mới
            if (isChasing)
            {
                isChasing = false;
                currentPath.Clear();
                rb.velocity = Vector2.zero;

                Waypoint nearest = graph.GetClosestWaypoint(transform.position);
                if (nearest != null)
                    patrolCenter = nearest.Position;
            }

            Patrol();
        }
    }

    /// <summary>
    /// Tính đường đi trong phạm vi waypoint
    /// </summary>
    void UpdatePathToTarget()
    {
        var startNode = graph.GetClosestWaypoint(transform.position);
        var endNode = graph.GetClosestWaypoint(target.position);

        // ⚠️ Nếu player không nằm trong vùng waypoint nào → chỉ đuổi đến waypoint gần nhất
        if (startNode == null || endNode == null)
        {
            currentPath.Clear();
            return;
        }

        currentPath = graph.FindPath(startNode, endNode);
        currentWaypointIndex = 0;
    }

    /// <summary>
    /// Đuổi player nhưng chỉ trong vùng waypoint
    /// </summary>
    void FollowPathToPlayer()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = currentPath[currentWaypointIndex].Position;
        float distance = Vector2.Distance(currentPos, targetPos);

        // Nếu đến waypoint hiện tại → chuyển sang waypoint kế tiếp
        if (distance <= 0.2f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= currentPath.Count)
            {
                // Đến cuối đường → dừng ở biên waypoint
                rb.velocity = Vector2.zero;
                if (anim) anim.SetFloat("Speed", 0);
                return;
            }
            targetPos = currentPath[currentWaypointIndex].Position;
        }

        MoveTowards(targetPos, chaseSpeed);
    }

    /// <summary>
    /// Tuần tra qua lại quanh vùng waypoint hiện tại
    /// </summary>
    void Patrol()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;

        float distance = Vector2.Distance(currentPos, targetPos);
        if (distance < 0.2f)
        {
            patrolDirection *= -1; // đổi hướng
            targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;
        }

        MoveTowards(targetPos, patrolSpeed);
    }

    /// <summary>
    /// Hàm di chuyển + quay hướng theo trục X
    /// </summary>
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
        if (dirX > 0 && !facingRight)
        {
            facingRight = true;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (dirX < 0 && facingRight)
        {
            facingRight = false;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void OnDrawGizmos()
    {
        // Vẽ bán kính phát hiện
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Vẽ đường path hiện tại
        if (currentPath != null && currentPath.Count > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < currentPath.Count - 1; i++)
                Gizmos.DrawLine(currentPath[i].Position, currentPath[i + 1].Position);
        }

        // Vẽ vùng tuần tra
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(patrolCenter - Vector2.right * patrolRange, patrolCenter + Vector2.right * patrolRange);
    }
}
