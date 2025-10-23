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
    public float waypointTolerance = 0.3f;

    [Header("Patrol Settings")]
    [Tooltip("Phạm vi tuần tra (ngang) độc lập quanh điểm hiện tại.")]
    public float patrolRange = 2f;

    private bool isChasing = false;
    private bool facingRight = true;

    // Logic Giới hạn Waypoint
    private Waypoint currentLimitWaypoint; // Waypoint giới hạn gần Enemy nhất

    // Logic Tuần tra Độc lập
    private Vector2 patrolCenter; // Điểm trung tâm tuần tra (Lấy từ vị trí hiện tại)
    private int patrolDirection = 1;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Khởi tạo Patrol Center tại vị trí khởi đầu của Enemy
        InitializePatrolCenter((Vector2)transform.position);
    }

    /// <summary>
    /// Đặt lại điểm trung tâm tuần tra TẠI VỊ TRÍ ĐƯỢC CHỈ ĐỊNH (không cần Waypoint)
    /// </summary>
    void InitializePatrolCenter(Vector2 position)
    {
        // 🛑 Đặt trung tâm tuần tra là vị trí hiện tại của Enemy, bỏ qua Waypoint
        patrolCenter = position;
        // Đặt hướng tuần tra ngẫu nhiên
        patrolDirection = (Random.value > 0.5f) ? 1 : -1;
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
            // ✅ CHASE: Target trong tầm
            if (!isChasing)
            {
                isChasing = true;
            }

            // Cập nhật Waypoint giới hạn gần nhất (luôn cập nhật khi đuổi)
            currentLimitWaypoint = graph.GetClosestWaypoint(transform.position);

            // THỰC HIỆN LOGIC ĐUỔI THẲNG + KIỂM TRA GIỚI HẠN
            ChaseTargetWithWaypointLimit();
        }
        else
        {
            // ❌ PATROL: Target ra khỏi tầm
            if (isChasing)
            {
                isChasing = false;
                rb.velocity = Vector2.zero;

                // TẠO VÙNG TUẦN TRA MỚI tại vị trí dừng (VỊ TRÍ HIỆN TẠI của Enemy)
                InitializePatrolCenter((Vector2)transform.position);
            }

            Patrol();
        }
    }

    // --- LOGIC CHASE (Đuổi Thẳng + Giới hạn Waypath) ---

    /// <summary>
    /// Đuổi thẳng đến Target, nhưng dừng lại nếu Target ở khu vực không thể tiếp cận.
    /// </summary>
    void ChaseTargetWithWaypointLimit()
    {
        Vector2 currentPos = transform.position;

        // 1. Kiểm tra giới hạn: Target có Waypoint gần không, và Waypoint đó có cùng Region với Enemy không
        Waypoint targetNode = graph.GetClosestWaypoint(target.position);
        bool isTargetReachable = false;

        if (targetNode != null && currentLimitWaypoint != null)
        {
            // Sử dụng FindPath (hoặc AreInSameRegion) để kiểm tra kết nối giữa hai Waypoint.
            // Nếu có đường đi (path != null), Target được coi là có thể tiếp cận qua Waypath.
            // Chúng ta không cần Path, chỉ cần biết có kết nối hay không.
            if (graph.FindPath(currentLimitWaypoint, targetNode) != null)
            {
                isTargetReachable = true;
            }
        }

        // 2. Quyết định di chuyển
        if (isTargetReachable || targetNode == null)
        {
            // TH1: Target có thể tiếp cận (hoặc Target quá xa Waypoint, nên đuổi thẳng trong tầm detection)
            // Kẻ địch ĐUỔI THẲNG TỚI TARGET
            MoveTowards(target.position, chaseSpeed);
        }
        else
        {
            // TH2: Target KHÔNG THỂ tiếp cận (Target vượt qua vực/biên Waypath)

            // Kẻ địch di chuyển đến Waypoint cuối cùng có thể tiếp cận (currentLimitWaypoint) và DỪNG LẠI.
            if (currentLimitWaypoint != null)
            {
                MoveTowards(currentLimitWaypoint.Position, chaseSpeed);
                // Nếu đã đến điểm giới hạn, dừng lại
                if (Vector2.Distance(currentPos, currentLimitWaypoint.Position) <= waypointTolerance)
                {
                    rb.velocity = Vector2.zero;
                    if (anim) anim.SetFloat("Speed", 0);
                }
            }
            else
            {
                // Không có Waypoint nào gần, chỉ đứng yên
                rb.velocity = Vector2.zero;
                if (anim) anim.SetFloat("Speed", 0);
            }
        }
    }

    // --- LOGIC PATROL (Độc lập) ---

    void Patrol()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;

        // Kiểm tra xem đã đến điểm giới hạn chưa
        if (Mathf.Abs(currentPos.x - targetPos.x) < waypointTolerance)
        {
            patrolDirection *= -1; // đổi hướng
            targetPos = patrolCenter + Vector2.right * patrolRange * patrolDirection;
        }

        MoveTowards(targetPos, patrolSpeed);
    }

    // --- HÀM HỖ TRỢ & GIZMOS (Giữ nguyên) ---

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
        // Bán kính phát hiện
        Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Vẽ vùng tuần tra độc lập
        if (!isChasing)
        {
            Gizmos.color = Color.cyan;
            Vector2 pointA = patrolCenter + Vector2.right * patrolRange;
            Vector2 pointB = patrolCenter - Vector2.right * patrolRange;
            Gizmos.DrawLine(pointA, pointB);
            Gizmos.DrawWireSphere(pointA, 0.2f);
            Gizmos.DrawWireSphere(pointB, 0.2f);
        }

        // Vẽ Waypoint giới hạn
        if (isChasing && currentLimitWaypoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentLimitWaypoint.Position, 0.5f);
            Gizmos.DrawLine(transform.position, currentLimitWaypoint.Position);
        }
    }
}