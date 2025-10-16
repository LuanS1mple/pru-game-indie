//using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
//public class EnemyAI2D_Animator : MonoBehaviour
//{
//    [Header("Target")]
//    public Transform player;

//    [Header("Ranges")]
//    public float patrolRadius = 6f;
//    public float leashRange = 10f;     // bán kính "nhà" theo TRỤC X (platformer)

//    [Header("Attack (Proximity)")]
//    public float attackCooldown = 0.9f;
//    public float proximityRadius = 1.2f; // giữ lại cho gizmo cũ nếu muốn
//    public Vector2 proximityCenterOffset = new Vector2(0f, -0.2f);

//    [Header("Attack Shape (axis-aligned)")]
//    [Tooltip("Nửa bề rộng vùng tấn công theo trục X")]
//    public float attackHalfWidthX = 1.1f;
//    [Tooltip("Nửa bề cao vùng tấn công theo trục Y (cho phép lệch cao/thấp)")]
//    public float attackHalfHeightY = 1.4f; // nới cao hơn để KHÔNG cần nhảy

//    [Range(0, 1)] public float pAttack1 = 0.5f;
//    [Range(0, 1)] public float pAttack2 = 0.3f;
//    [Range(0, 1)] public float pAttack3 = 0.2f;

//    [Header("Move")]
//    public float patrolSpeed = 1.5f;
//    public float chaseSpeed = 2.7f;
//    public float arriveDist = 0.2f;    // ngưỡng coi như tới mục tiêu theo X

//    [Header("Chase Stop")]
//    [Tooltip("Khoảng dừng đuổi theo trục X để không tỳ/đẩy Player")]
//    public float stopChaseDistX = 0.6f;

//    [Header("Ability (optional)")]
//    public bool useAbility = false;
//    public float abilityCooldown = 6f;

//    [Header("Animator")]
//    public Animator animator;
//    public string locomotionStateName = "Enemy Idle";

//    Rigidbody2D rb;
//    Vector2 homePos, patrolTarget;
//    float lastAttackTime = -999f, lastAbilityTime = -999f, attackEnterTime = -999f;
//    int locomotionHash;

//    enum State { Patrol, Chase, Attack, Return }
//    State state = State.Patrol;

//    [SerializeField] float attackHardTimeout = 2.0f;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody2D>();
//        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
//        homePos = transform.position;

//        if (!player)
//        {
//            var p = GameObject.FindGameObjectWithTag("Player");
//            if (p) player = p.transform;
//        }
//        if (!animator) animator = GetComponentInChildren<Animator>();
//        locomotionHash = Animator.StringToHash(locomotionStateName);
//    }

//    void Start() => ChooseNewPatrolPoint();

//    void Update()
//    {
//        float dxHome = (player ? Mathf.Abs(player.position.x - homePos.x) : -1f);
//        // Debug.Log($"state={state} insideHomeX={PlayerInsideHomeX()} prox={InProximity()} dx={dxHome:0.00}");

//        switch (state)
//        {
//            case State.Patrol:
//                if (PlayerInsideHomeX()) { state = State.Chase; ResetAttackAnim(); }
//                break;

//            case State.Chase:
//                if (!PlayerInsideHomeX()) { state = State.Return; ResetAttackAnim(); break; }
//                if (InProximity()) { state = State.Attack; attackEnterTime = Time.time; }
//                break;

//            case State.Attack:
//                if (!PlayerInsideHomeX()) { CancelAttackAnd(goReturn: true); break; }
//                if (!InProximity()) { CancelAttackAnd(goReturn: false); break; }
//                if (Time.time - attackEnterTime > attackHardTimeout) { CancelAttackAnd(false); }
//                break;

//            case State.Return:
//                if (Mathf.Abs(transform.position.x - homePos.x) <= arriveDist) state = State.Patrol;
//                else if (PlayerInsideHomeX()) { state = State.Chase; ResetAttackAnim(); }
//                break;
//        }

//        animator?.SetBool("Run", Mathf.Abs(rb.velocity.x) > 0.01f);
//        if (useAbility && Time.time - lastAbilityTime > abilityCooldown && PlayerInsideHomeX())
//        {
//            lastAbilityTime = Time.time;
//            animator?.SetTrigger("Ability");
//        }
//    }

//    void FixedUpdate()
//    {
//        switch (state)
//        {
//            case State.Patrol:
//                Face(patrolTarget);
//                MoveTowards(patrolTarget, patrolSpeed);
//                if (Vector2.Distance(transform.position, patrolTarget) <= arriveDist)
//                    ChooseNewPatrolPoint();
//                break;

//            case State.Chase:
//                if (player)
//                {
//                    // Đứng cách ra để không đẩy Player
//                    float dxToPlayer = Mathf.Abs(player.position.x - transform.position.x);
//                    if (dxToPlayer <= stopChaseDistX)
//                    {
//                        rb.velocity = new Vector2(0f, rb.velocity.y);
//                        Face(player.position);
//                        break;
//                    }

//                    Face(player.position);
//                    MoveTowards(new Vector2(player.position.x, transform.position.y), chaseSpeed);
//                }
//                break;

//            case State.Attack:
//                rb.velocity = new Vector2(0f, rb.velocity.y);
//                if (player) Face(player.position);
//                TryAttack();
//                break;

//            case State.Return:
//                Face(homePos); // <-- QUAY ĐẦU VỀ NHÀ
//                MoveTowards(homePos, patrolSpeed);
//                break;
//        }
//    }

//    // ================= Helpers =================
//    // Offset theo LOCAL, có xét flip scale (sửa lỗi hitbox lệch khi quay đầu)
//    Vector2 ProxCenter() => (Vector2)transform.TransformPoint(proximityCenterOffset);

//    bool PlayerInsideHomeX()
//    {
//        if (!player) return false;
//        return Mathf.Abs(player.position.x - homePos.x) <= leashRange;
//    }

//    // HCN tấn công: chỉ cần |ΔX| <= width, |ΔY| <= height (không cần nhảy)
//    bool InProximity()
//    {
//        if (!player) return false;
//        if (!PlayerInsideHomeX()) return false;

//        Vector2 c = ProxCenter();
//        float dx = Mathf.Abs(player.position.x - c.x);
//        float dy = Mathf.Abs(player.position.y - c.y);
//        return dx <= attackHalfWidthX && dy <= attackHalfHeightY;
//    }

//    void MoveTowards(Vector2 target, float speed)
//    {
//        Vector2 delta = target - (Vector2)transform.position;
//        if (Mathf.Abs(delta.x) < 0.001f)
//        {
//            rb.velocity = new Vector2(0f, rb.velocity.y);
//            return;
//        }
//        float dirX = Mathf.Sign(delta.x);
//        rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
//    }

//    void Face(Vector2 target)
//    {
//        if (target.x > transform.position.x) transform.localScale = new Vector3(1, 1, 1);
//        else if (target.x < transform.position.x) transform.localScale = new Vector3(-1, 1, 1);
//    }

//    void ChooseNewPatrolPoint()
//    {
//        patrolTarget = homePos + Random.insideUnitCircle * patrolRadius;
//        patrolTarget.y = transform.position.y; // tuần tra ngang
//    }

//    void TryAttack()
//    {
//        if (!InProximity()) return;
//        if (Time.time - lastAttackTime < attackCooldown) return;
//        lastAttackTime = Time.time;

//        float r = Random.value;
//        if (r < pAttack1) animator?.SetTrigger("Attack");
//        else if (r < pAttack1 + pAttack2) animator?.SetTrigger("Attack 2");
//        else animator?.SetTrigger("Attack 3");
//    }

//    void CancelAttackAnd(bool goReturn)
//    {
//        ResetAttackAnim();
//        if (animator && locomotionHash != 0) animator.Play(locomotionStateName, 0, 0f);
//        state = goReturn ? State.Return : State.Chase;
//    }

//    void ResetAttackAnim()
//    {
//        if (!animator) return;
//        animator.ResetTrigger("Attack");
//        animator.ResetTrigger("Attack 2");
//        animator.ResetTrigger("Attack 3");
//    }

//    public void PlayHit() => animator?.SetTrigger("Hit");

//    public void PlayDeath()
//    {
//        animator?.SetTrigger("Death");
//        enabled = false;
//        rb.velocity = Vector2.zero;
//    }

//    void OnDrawGizmosSelected()
//    {
//        Vector3 cHome = Application.isPlaying ? (Vector3)homePos : transform.position;
//        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(cHome, leashRange);
//        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(cHome, patrolRadius);

//        // Vẽ HCN attack theo offset đã transform (để thấy đúng bên trái/phải)
//        if (Application.isPlaying)
//        {
//            Gizmos.color = Color.green;
//            Vector2 c = ProxCenter();
//            Vector3 p1 = new Vector3(c.x - attackHalfWidthX, c.y - attackHalfHeightY, 0);
//            Vector3 p2 = new Vector3(c.x + attackHalfWidthX, c.y - attackHalfHeightY, 0);
//            Vector3 p3 = new Vector3(c.x + attackHalfWidthX, c.y + attackHalfHeightY, 0);
//            Vector3 p4 = new Vector3(c.x - attackHalfWidthX, c.y + attackHalfHeightY, 0);
//            Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p3); Gizmos.DrawLine(p3, p4); Gizmos.DrawLine(p4, p1);
//        }
//    }
//}








using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI2D_Animator : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Ranges")]
    public float patrolRadius = 6f;
    public float leashRange = 10f;     // bán kính "nhà" theo TRỤC X (platformer)

    [Header("Attack (Proximity)")]
    public float attackCooldown = 0.9f;
    public float proximityRadius = 1.2f; // (giữ cho gizmo cũ nếu muốn)
    public Vector2 proximityCenterOffset = new Vector2(0f, -0.2f);

    [Header("Attack Shape (axis-aligned)")]
    public float attackHalfWidthX = 1.1f;
    public float attackHalfHeightY = 0.9f;

    [Range(0, 1)] public float pAttack1 = 0.5f;
    [Range(0, 1)] public float pAttack2 = 0.3f;
    [Range(0, 1)] public float pAttack3 = 0.2f;

    [Header("Move")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 2.7f;
    public float arriveDist = 0.2f;    // ngưỡng coi như tới mục tiêu theo X

    [Header("Chase Stop")]
    public float stopChaseDistX = 0.6f;

    [Header("Ability (optional)")]
    public bool useAbility = false;
    public float abilityCooldown = 6f;

    [Header("Animator")]
    public Animator animator;
    public string locomotionStateName = "Enemy Idle";

    [Header("Facing Control")]
    [Tooltip("Chỉ lật khi đang di chuyển hoặc mục tiêu cách xa hơn idleFaceDeadzone")]
    public bool lockFacingWhenIdle = true;
    [Tooltip("Khoảng chết khi đứng yên: |Δx| nhỏ hơn giá trị này sẽ không lật")]
    public float idleFaceDeadzone = 0.25f;
    [Tooltip("Khoảng chết tối thiểu khi đang tấn công để tránh rung")]
    public float attackFaceDeadzone = 0.06f;

    // === DEBUG FACING ===
    [Header("Facing Debug")]
    public bool debugFacing = false;           // bật lên để xem log lật mặt
    [Tooltip("Thời gian tối thiểu giữa hai lần lật (chống rung)")]
    public float minFlipInterval = 0.35f;      // tăng lên nếu vẫn lật
    float _lastFlipTime = -999f;

    Rigidbody2D rb;
    Vector2 homePos, patrolTarget;
    float lastAttackTime = -999f, lastAbilityTime = -999f, attackEnterTime = -999f;
    int locomotionHash;

    enum State { Patrol, Chase, Attack, Return }
    State state = State.Patrol;

    [SerializeField] float attackHardTimeout = 2.0f;

    // hướng hiện tại: 1 = phải, -1 = trái
    int facingDir = 1;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // tránh xoay
        homePos = transform.position;

        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (!animator) animator = GetComponentInChildren<Animator>();
        locomotionHash = Animator.StringToHash(locomotionStateName);

        facingDir = (transform.localScale.x >= 0f) ? 1 : -1;
        ApplyFacingScale();
    }

    void Start() => ChooseNewPatrolPoint();

    void Update()
    {
        // float dxHome = (player ? Mathf.Abs(player.position.x - homePos.x) : -1f);
        // Debug.Log($"state={state} insideHomeX={PlayerInsideHomeX()} prox={InProximity()} dx={dxHome:0.00}");

        switch (state)
        {
            case State.Patrol:
                if (PlayerInsideHomeX()) { state = State.Chase; ResetAttackAnim(); }
                break;

            case State.Chase:
                if (!PlayerInsideHomeX()) { state = State.Return; ResetAttackAnim(); break; } // ra khỏi nhà -> quay về
                if (InProximity()) { state = State.Attack; attackEnterTime = Time.time; }
                break;

            case State.Attack:
                if (!PlayerInsideHomeX()) { CancelAttackAnd(goReturn: true); break; }
                if (!InProximity()) { CancelAttackAnd(goReturn: false); break; }
                if (Time.time - attackEnterTime > attackHardTimeout) { CancelAttackAnd(false); }
                break;

            case State.Return:
                if (Mathf.Abs(transform.position.x - homePos.x) <= arriveDist)
                {
                    state = State.Patrol;
                    rb.velocity = Vector2.zero; // đứng yên thật sự khi về đến nhà
                }
                else if (PlayerInsideHomeX()) { state = State.Chase; ResetAttackAnim(); }
                break;
        }

        animator?.SetBool("Run", Mathf.Abs(rb.velocity.x) > 0.01f);

        if (useAbility && Time.time - lastAbilityTime > abilityCooldown && PlayerInsideHomeX())
        {
            lastAbilityTime = Time.time;
            animator?.SetTrigger("Ability");
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Patrol:
                // chỉ lật khi di chuyển; khi gần tới điểm -> dừng hẳn và KHÔNG đổi mặt
                if (Vector2.Distance(transform.position, patrolTarget) <= arriveDist)
                {
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    MoveTowards(patrolTarget, patrolSpeed);
                    Face(patrolTarget, idleMode: true);
                }
                break;

            case State.Chase:
                if (player)
                {
                    float dxToPlayer = Mathf.Abs(player.position.x - transform.position.x);
                    if (dxToPlayer <= stopChaseDistX)
                    {
                        rb.velocity = new Vector2(0f, rb.velocity.y);
                        Face(player.position, idleMode: true); // có deadzone nên sẽ không lật nếu rất gần
                        break;
                    }

                    MoveTowards(new Vector2(player.position.x, transform.position.y), chaseSpeed);
                    Face(player.position, idleMode: false);
                }
                break;

            case State.Attack:
                rb.velocity = new Vector2(0f, rb.velocity.y);
                FaceAttack(player ? player.position : (Vector2)transform.position);
                TryAttack();
                break;

            case State.Return:
                if (Mathf.Abs(transform.position.x - homePos.x) <= arriveDist)
                {
                    rb.velocity = Vector2.zero;
                }
                else
                {
                    MoveTowards(homePos, patrolSpeed);
                    Face(homePos, idleMode: false);
                }
                break;
        }
    }

    // ================= Helpers =================
    Vector2 ProxCenter() => (Vector2)transform.TransformPoint(proximityCenterOffset); // theo hướng lật

    bool PlayerInsideHomeX()
    {
        if (!player) return false;
        return Mathf.Abs(player.position.x - homePos.x) <= leashRange;
    }

    // HCN tấn công: |ΔX| <= width, |ΔY| <= height
    bool InProximity()
    {
        if (!player) return false;
        if (!PlayerInsideHomeX()) return false;

        Vector2 c = ProxCenter();
        float dx = Mathf.Abs(player.position.x - c.x);
        float dy = Mathf.Abs(player.position.y - c.y);
        return dx <= attackHalfWidthX && dy <= attackHalfHeightY;
    }

    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 delta = target - (Vector2)transform.position;
        if (Mathf.Abs(delta.x) < 0.001f)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }
        float dirX = Mathf.Sign(delta.x);
        rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
    }

    // ==== Facing with deadzone + hysteresis ====
    void Face(Vector2 target, bool idleMode)
    {
        float dx = target.x - transform.position.x;

        // deadzone theo ngữ cảnh
        float dz = idleMode ? idleFaceDeadzone : 0.05f;

        // Không lật nếu mục tiêu quá gần theo X
        if (Mathf.Abs(dx) < dz) return;

        // Nếu idleMode và đứng yên -> khoan lật để tránh rung
        if (idleMode && lockFacingWhenIdle && Mathf.Abs(rb.velocity.x) <= 0.01f) return;

        // Hysteresis: không cho lật quá dày
        if (Time.time - _lastFlipTime < minFlipInterval) return;

        int desired = dx >= 0f ? 1 : -1;
        if (desired == facingDir) return;

        DebugFlip("Face", dx, idleMode);

        facingDir = desired;
        ApplyFacingScale();
        _lastFlipTime = Time.time;
    }

    void FaceAttack(Vector2 target)
    {
        float dx = target.x - transform.position.x;

        if (Mathf.Abs(dx) < attackFaceDeadzone) return;
        if (Time.time - _lastFlipTime < minFlipInterval) return;

        int desired = dx >= 0f ? 1 : -1;
        if (desired == facingDir) return;

        DebugFlip("Attack", dx, idleMode: false);

        facingDir = desired;
        ApplyFacingScale();
        _lastFlipTime = Time.time;
    }

    void DebugFlip(string tag, float dx, bool idleMode)
    {
        if (!debugFacing) return;
        string st = state.ToString();
        float vx = rb ? rb.velocity.x : 0f;
        bool insideHome = PlayerInsideHomeX();
        bool prox = InProximity();
        Debug.Log($"[Flip-{tag}] t={Time.time:0.00} state={st} idleMode={idleMode} dx={dx:0.000} vx={vx:0.000} " +
                  $"insideHomeX={insideHome} prox={prox} lastFlipAgo={Time.time - _lastFlipTime:0.00}");
    }

    void ApplyFacingScale()
    {
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingDir >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    void ChooseNewPatrolPoint()
    {
        // tránh chọn quá sát → khi tới nơi sẽ không lật nữa
        const float minStepX = 0.5f;
        for (int i = 0; i < 8; i++)
        {
            Vector2 candidate = homePos + Random.insideUnitCircle * patrolRadius;
            candidate.y = transform.position.y; // tuần tra ngang
            if (Mathf.Abs(candidate.x - transform.position.x) >= minStepX)
            {
                patrolTarget = candidate;
                return;
            }
        }
        // fallback
        patrolTarget = new Vector2(homePos.x + (Random.value < 0.5f ? -minStepX : minStepX), transform.position.y);
    }

    void TryAttack()
    {
        if (!InProximity()) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        float r = Random.value;
        if (r < pAttack1) animator?.SetTrigger("Attack");
        else if (r < pAttack1 + pAttack2) animator?.SetTrigger("Attack 2");
        else animator?.SetTrigger("Attack 3");
    }

    void CancelAttackAnd(bool goReturn)
    {
        ResetAttackAnim();
        if (animator && locomotionHash != 0) animator.Play(locomotionStateName, 0, 0f);
        state = goReturn ? State.Return : State.Chase;
    }

    void ResetAttackAnim()
    {
        if (!animator) return;
        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Attack 2");
        animator.ResetTrigger("Attack 3");
    }

    public void PlayHit() => animator?.SetTrigger("Hit");

    public void PlayDeath()
    {
        animator?.SetTrigger("Death");
        enabled = false;
        rb.velocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 cHome = Application.isPlaying ? (Vector3)homePos : transform.position;
        Gizmos.color = Color.magenta; Gizmos.DrawWireSphere(cHome, leashRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(cHome, patrolRadius);

        // Vẽ HCN attack theo offset đã transform
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Vector2 c = ProxCenter();
            Vector3 p1 = new Vector3(c.x - attackHalfWidthX, c.y - attackHalfHeightY, 0);
            Vector3 p2 = new Vector3(c.x + attackHalfWidthX, c.y - attackHalfHeightY, 0);
            Vector3 p3 = new Vector3(c.x + attackHalfWidthX, c.y + attackHalfHeightY, 0);
            Vector3 p4 = new Vector3(c.x - attackHalfWidthX, c.y + attackHalfHeightY, 0);
            Gizmos.DrawLine(p1, p2); Gizmos.DrawLine(p2, p3); Gizmos.DrawLine(p3, p4); Gizmos.DrawLine(p4, p1);
        }
    }
}

