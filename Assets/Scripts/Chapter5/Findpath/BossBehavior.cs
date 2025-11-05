using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BossBehavior : BaseStats
{
    [Header("Events")]
    public UnityEvent OnBossDied;

    [Header("References")]
    public WaypointGraph graph;
    public Transform target;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Death & Cutscene Settings")]
    public string cutsceneSceneName = "CutSence1";
    public float deathWaitTime = 3f;

    [Header("Skill Points")]
    public Transform skill2SpawnPoint; // Kéo GameObject con (Child) vào đây

    [Header("Skill Prefabs")]
    public GameObject lightningPillarPrefab; // Skill 1
    public GameObject explosionAreaPrefab; // Skill 2

    // ⭐ SKILL 3: BẤT TỬ ⭐
    [Header("Skill 3 (Invulnerability)")]
    public GameObject skill3CastPrefab;
    public GameObject invulnerabilityEffectPrefab;
    public float skill3MinHealthPercentage = 0.6f;
    public float skill3Duration = 4f;
    public float skill3Cooldown = 30f;
    private bool isSkill3Ready = true;
    private Coroutine skill3CooldownCoroutine;
    private bool isInvulnerable = false;
    private GameObject invulnEffectInstance;
    private GameObject castEffectInstance;
    private bool isCastingSkill3 = false;

    // ... (Movement Settings) ...
    [Header("Movement Settings")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.2f;
    public Vector2 detectionBoxSize = new Vector2(12f, 4f);
    public float waypointTolerance = 0.5f;

    // ... (Attack Settings) ...
    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public Collider2D attackCollider;
    public float knockbackForce = 3f;
    // Đảm bảo bạn đã khai báo attackCooldown trong BaseStats hoặc ở đây
    public float attackCooldown = 1.0f;

    [Header("Counter Attack Settings")]
    [Tooltip("Số lần Boss bị đánh trước khi thực hiện đòn phản công.")]
    public int hitsForCounterAttack = 4;
    private int currentHitCounter = 0;
    private bool isCounterAttacking = false;
    // ⭐ CỜ KHÓA MỚI: Ngăn chặn logic đếm hit khi đang bắt đầu phản công ⭐
    private bool isCounterAttackWindUp = false;

    [Header("Skill Settings")]
    // Skill 1 (Cột Sét)
    public float skill1Cooldown = 5f;
    public float skill1Range = 10f;
    private bool isSkill1Ready = true;
    private Coroutine skill1CooldownCoroutine;

    // Skill 2 (Vòng Xoáy Năng Lượng)
    public float skill2Cooldown = 8f;
    public float skill2Range = 3f;
    private bool isSkill2Ready = true;
    private Coroutine skill2CooldownCoroutine;

    private bool isUsingSkill = false;
    private LayerMask playerLayerMask;

    private GameObject explosionInstance;
    private const float Skill2ActiveDuration = 1.0f;

    // ... (Các biến private khác không đổi) ...
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

    private BossRoomManager manager;
    private bool managerNotified = false;

    //------------------------------------------
    // UNITY LIFECYCLE VÀ SETUP
    //------------------------------------------

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        if (GetComponent<Animator>() != null) anim = GetComponent<Animator>();
        InitializePatrolCenter((Vector2)transform.position);
        if (attackCollider != null)
            attackCollider.enabled = false;

        playerLayerMask = 1 << LayerMask.NameToLayer("Player");

        if (attackCooldown <= 0.01f)
        {
            attackCooldown = 1.0f;
        }
    }

    void Start()
    {
        manager = FindObjectOfType<BossRoomManager>();
        if (manager != null)
        {
            manager.enemiesRemaining++;
        }
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

    void StopWaitAndBeginChase()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        isAttacking = false;
        isCounterAttacking = false;
        isCounterAttackWindUp = false; // ⭐ Reset cờ khóa ⭐

        // LƯU THAM CHIẾU COOLDOWN TRƯỚC KHI HỦY
        Coroutine tempCooldown3 = skill3CooldownCoroutine;
        Coroutine tempCooldown1 = skill1CooldownCoroutine;
        Coroutine tempCooldown2 = skill2CooldownCoroutine;

        StopAllCoroutines();

        // KHỞI ĐỘNG LẠI COOLDOWN NẾU BỊ HỦY VÀ ĐANG KHÔNG READY
        if (!isSkill3Ready && tempCooldown3 != null)
        {
            skill3CooldownCoroutine = StartCoroutine(Skill3CooldownRoutine());
        }
        if (!isSkill1Ready && tempCooldown1 != null)
        {
            skill1CooldownCoroutine = StartCoroutine(Skill1CooldownRoutine());
        }
        if (!isSkill2Ready && tempCooldown2 != null)
        {
            skill2CooldownCoroutine = StartCoroutine(Skill2CooldownRoutine());
        }

        // Hủy hiệu ứng skill bị ngắt khi chuyển trạng thái
        if (explosionInstance != null) { Destroy(explosionInstance); explosionInstance = null; }
        if (castEffectInstance != null) { Destroy(castEffectInstance); castEffectInstance = null; }
        if (invulnEffectInstance != null) { Destroy(invulnEffectInstance); invulnEffectInstance = null; }
        isInvulnerable = false;

        isUsingSkill = false;
        isCastingSkill3 = false;
        if (attackCollider != null) attackCollider.enabled = false;


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
        isCounterAttacking = false;
        isCounterAttackWindUp = false; // ⭐ Reset cờ khóa ⭐

        // LƯU THAM CHIẾU COOLDOWN TRƯỚC KHI HỦY
        Coroutine tempCooldown3 = skill3CooldownCoroutine;
        Coroutine tempCooldown1 = skill1CooldownCoroutine;
        Coroutine tempCooldown2 = skill2CooldownCoroutine;

        StopAllCoroutines();

        // KHỞI ĐỘNG LẠI COOLDOWN NẾU BỊ HỦY VÀ ĐANG KHÔNG READY
        if (!isSkill3Ready && tempCooldown3 != null)
        {
            skill3CooldownCoroutine = StartCoroutine(Skill3CooldownRoutine());
        }
        if (!isSkill1Ready && tempCooldown1 != null)
        {
            skill1CooldownCoroutine = StartCoroutine(Skill1CooldownRoutine());
        }
        if (!isSkill2Ready && tempCooldown2 != null)
        {
            skill2CooldownCoroutine = StartCoroutine(Skill2CooldownRoutine());
        }

        // Hủy hiệu ứng skill bị ngắt khi chuyển trạng thái
        if (explosionInstance != null) { Destroy(explosionInstance); explosionInstance = null; }
        if (castEffectInstance != null) { Destroy(castEffectInstance); castEffectInstance = null; }
        if (invulnEffectInstance != null) { Destroy(invulnEffectInstance); invulnEffectInstance = null; }
        isInvulnerable = false;

        isUsingSkill = false;
        isCastingSkill3 = false;
        if (attackCollider != null) attackCollider.enabled = false;

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


    private void Update()
    {
        // 1. NGĂN CHẶN NẾU ĐANG CHẾT/TẤN CÔNG/DÙNG SKILL CAST / PHẢN CÔNG
        if (isDead || isAttacking || isCastingSkill3 || isCounterAttacking)
        {
            rb.velocity = Vector2.zero;
            if (!isAttacking && !isCastingSkill3 && !isCounterAttacking)
                if (anim) anim.SetFloat("Speed", 0);
            if ((isAttacking || isCastingSkill3 || isCounterAttacking) && target)
                RotateToDirection(target.position.x - transform.position.x);
            return;
        }

        // ... (Logic kiểm tra Target và Patrol) ...

        if (target == null || graph == null)
        {
            if (isGuardBound) StopWaitAndBeginPatrol();
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            Patrol();
            return;
        }

        bool playerInDetectionBox = IsPlayerInDetectionBox();

        if (playerInDetectionBox)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);

            if (!isChasing)
            {
                StopWaitAndBeginChase();
                fallbackPatrolPoint = FindFallbackPatrolWaypoint(transform.position);
            }

            // ⭐ ƯU TIÊN 1: SKILL 3 (Máu thấp)
            if (currentHP / maxHP <= skill3MinHealthPercentage && isSkill3Ready)
            {
                UseSkill3();
                return;
            }

            // ⭐ ƯU TIÊN 2: SKILL 2 
            if (!isUsingSkill && isSkill2Ready && distanceToPlayer <= skill2Range)
            {
                UseSkill2();
                return;
            }

            // ⭐ ƯU TIÊN 3: SKILL 1
            if (!isUsingSkill && isSkill1Ready && distanceToPlayer <= skill1Range)
            {
                UseSkill1();
                return;
            }


            // ⭐ ƯU TIÊN 4: TẤN CÔNG THƯỜNG
            // Chặn attack thường nếu đang phản công (Counter) hoặc đang trong giai đoạn khóa (WindUp)
            if (!isUsingSkill && !isAttacking && !isCounterAttacking && !isCounterAttackWindUp && isChasing && distanceToPlayer <= attackRange)
            {
                StartAttack();
                return;
            }

            // ⭐ ƯU TIÊN 5: ĐUỔI THEO
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

    private bool IsPlayerInDetectionBox()
    {
        if (target == null) return false;

        Vector2 enemyPos = transform.position;
        Vector2 playerPos = target.position;

        float halfWidth = detectionBoxSize.x / 2f;
        float halfHeight = detectionBoxSize.y / 2f;

        float minX = enemyPos.x - halfWidth;
        float maxX = enemyPos.x + halfWidth;
        float minY = enemyPos.y - halfHeight;
        float maxY = enemyPos.y + halfHeight;

        bool withinX = playerPos.x >= minX && playerPos.x <= maxX;
        bool withinY = playerPos.y >= minY && playerPos.y <= maxY;

        return withinX && withinY;
    }


    // --- HÀM TẤN CÔNG THƯỜNG ---
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

    // --- HÀM PHẢN CÔNG MỚI (Đã sửa đổi để thêm Khóa Khởi Động) ---
    void StartCounterAttack()
    {
        if (isDead || isCounterAttacking || isUsingSkill || isCastingSkill3)
        {
            return;
        }

        // Ngắt di chuyển/tấn công thường đang chạy
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        if (waitCoroutine != null) StopCoroutine(waitCoroutine);
        if (rb != null) rb.velocity = Vector2.zero;

        isAttacking = false;
        isWaiting = false;

        StartCoroutine(CounterAttackRoutine());
    }

    private IEnumerator CounterAttackRoutine()
    {
        // ⭐ BƯỚC 1: KHỞI ĐỘNG CỜ KHÓA ⭐
        isCounterAttackWindUp = true;
        isCounterAttacking = true;

        // 2. Dừng mọi di chuyển và xoay mặt về Player
        rb.velocity = Vector2.zero;
        if (anim) anim.SetFloat("Speed", 0);
        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        // 3. Kích hoạt Animation Attack thường
        if (anim) anim.SetTrigger("Attack");

        // ⭐ BƯỚC 4: CHỜ QUA GIAI ĐOẠN KHỞI ĐỘNG (0.3s) ⭐
        // Khoảng thời gian này Boss không thể bị đếm hit
        yield return new WaitForSeconds(0.3f);

        isCounterAttackWindUp = false; // ⭐ Mở khóa ⭐

        // 5. Chờ thời gian đòn đánh phản công còn lại kết thúc
        float remainingAttackDuration = attackCooldown - 0.3f;
        if (remainingAttackDuration < 0) remainingAttackDuration = 0;

        yield return new WaitForSeconds(remainingAttackDuration);

        // 6. Kết thúc phản công
        isCounterAttacking = false;

        HandleAnimation_DisableAttackCollider();
    }
    // --- HẾT PHẢN CÔNG ---

    public void HandleAnimation_EnableAttackCollider()
    {
        if (attackCollider == null) return;
        if (isAttacking || isCounterAttacking)
        {
            canDealDamage = true;
            hitPlayers.Clear();
            StartCoroutine(RefreshCollider());
        }
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
    public void HandleAnimation_DisableAttackCollider()
    {
        if (attackCollider == null) return;
        canDealDamage = false;
        attackCollider.enabled = false;
    }
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
    private void OnTriggerEnter2D(Collider2D collision) { TryDealDamage(collision); }
    private void OnTriggerStay2D(Collider2D collision) { TryDealDamage(collision); }

    public void HandleAnimation_TriggerTakeDamage(float damage) { TakeDamage(damage); }


    // --- LOGIC SKILL (Giữ nguyên) ---

    void UseSkill1()
    {
        if (isUsingSkill || !isSkill1Ready) return;
        if (lightningPillarPrefab == null)
        {
            Debug.LogError("Lightning Pillar Prefab chưa được gán!");
            return;
        }

        isUsingSkill = true;
        isSkill1Ready = false;

        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        if (anim) anim.SetTrigger("Skill1");

        StartCoroutine(Skill1Routine());
        skill1CooldownCoroutine = StartCoroutine(Skill1CooldownRoutine());
    }

    private IEnumerator Skill1Routine()
    {
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.3f);

        if (target != null)
        {
            Vector3 pillarPosition = target.position;
            GameObject pillar = Instantiate(lightningPillarPrefab, pillarPosition, Quaternion.identity);
            var pillarScript = pillar.GetComponent<LightningPillarBehavior>();
            if (pillarScript != null)
            {
                pillarScript.targetLayer = LayerMask.NameToLayer("Player");
            }
        }

        yield return new WaitForSeconds(0.7f);
        isUsingSkill = false;
    }

    private IEnumerator Skill1CooldownRoutine()
    {
        yield return new WaitForSeconds(skill1Cooldown);
        isSkill1Ready = true;
        skill1CooldownCoroutine = null;
    }


    void UseSkill2()
    {
        if (isUsingSkill || !isSkill2Ready) return;
        if (explosionAreaPrefab == null)
        {
            Debug.LogError("Explosion Area Prefab chưa được gán!");
            return;
        }

        isUsingSkill = true;
        isSkill2Ready = false;

        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        if (anim) anim.SetTrigger("Skill2");

        StartCoroutine(Skill2Routine());
        skill2CooldownCoroutine = StartCoroutine(Skill2CooldownRoutine());
    }

    private IEnumerator Skill2Routine()
    {
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.7f);

        if (skill2SpawnPoint == null)
        {
            isUsingSkill = false;
            yield break;
        }

        explosionInstance = Instantiate(explosionAreaPrefab, skill2SpawnPoint.position, Quaternion.identity);

        explosionInstance.transform.SetParent(skill2SpawnPoint);
        explosionInstance.transform.localPosition = Vector3.zero;

        var explosionScript = explosionInstance.GetComponent<BossExplosionBehavior>();
        if (explosionScript != null)
        {
            explosionScript.targetLayer = playerLayerMask;
        }

        yield return new WaitForSeconds(Skill2ActiveDuration);

        if (explosionInstance != null)
        {
            Destroy(explosionInstance);
            explosionInstance = null;
        }

        yield return new WaitForSeconds(0.6f);

        isUsingSkill = false;
    }

    private IEnumerator Skill2CooldownRoutine()
    {
        yield return new WaitForSeconds(skill2Cooldown);
        isSkill2Ready = true;
        skill2CooldownCoroutine = null;
    }


    void UseSkill3()
    {
        if (!isSkill3Ready) return;

        isSkill3Ready = false;
        isCastingSkill3 = true;

        isInvulnerable = true;

        if (anim) anim.SetTrigger("Skill3");

        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        if (invulnerabilityEffectPrefab != null && invulnEffectInstance == null)
        {
            invulnEffectInstance = Instantiate(invulnerabilityEffectPrefab, transform.position, Quaternion.identity);
            invulnEffectInstance.transform.SetParent(transform);
            invulnEffectInstance.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(Skill3Routine());
        skill3CooldownCoroutine = StartCoroutine(Skill3CooldownRoutine());
    }

    private IEnumerator Skill3Routine()
    {
        rb.velocity = Vector2.zero;
        float castDuration = 0.8f;

        if (skill3CastPrefab != null)
        {
            castEffectInstance = Instantiate(skill3CastPrefab, transform.position, Quaternion.identity);
            castEffectInstance.transform.SetParent(transform);
            castEffectInstance.transform.localPosition = Vector3.zero;
        }

        yield return new WaitForSeconds(castDuration);

        if (castEffectInstance != null)
        {
            Destroy(castEffectInstance);
            castEffectInstance = null;
        }

        isCastingSkill3 = false;

        float activeDuration = skill3Duration - castDuration;
        if (activeDuration > 0)
        {
            yield return new WaitForSeconds(activeDuration);
        }

        isInvulnerable = false;

        if (invulnEffectInstance != null)
        {
            Destroy(invulnEffectInstance);
            invulnEffectInstance = null;
        }

        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator Skill3CooldownRoutine()
    {
        yield return new WaitForSeconds(skill3Cooldown);
        isSkill3Ready = true;
        skill3CooldownCoroutine = null;
    }

    // ... (Logic Di Chuyển/Patrol và Gizmos giữ nguyên) ...
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
        if (isTargetReachable) { isGuardBound = false; } else { isGuardBound = true; }

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


    // ⭐ HÀM TAKE DAMAGE ĐÃ CẬP NHẬT ⭐
    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        // 1. KIỂM TRA BẤT TỬ
        if (isInvulnerable)
        {
            if (anim) anim.SetTrigger("Hit");
            return;
        }

        base.TakeDamage(damage);
        if (anim) anim.SetTrigger("Hit");

        // 2. LOGIC NGẮT ĐÒN ĐÁNH THƯỜNG
        if (isAttacking)
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            isAttacking = false;
            canDealDamage = false;
            hitPlayers.Clear();
            if (attackCollider != null)
            {
                attackCollider.enabled = false;
            }
        }

        // ⭐ 3. XỬ LÝ BIẾN ĐẾM VÀ KÍCH HOẠT COUNTER ATTACK ⭐
        // Bỏ qua việc đếm hit nếu đang trong giai đoạn khóa WindUp!
        if (!isCounterAttacking && !isUsingSkill && !isCastingSkill3 && !isCounterAttackWindUp)
        {
            currentHitCounter++;

            if (currentHitCounter >= hitsForCounterAttack)
            {
                currentHitCounter = 0;
                StartCounterAttack();
            }
        }

        if (isDead)
            Die();
    }

    protected override void Die()
    {
        base.Die();
        if (manager != null && !managerNotified)
        {
            manager.EnemyDied();
            managerNotified = true;
        }

        if (explosionInstance != null) { Destroy(explosionInstance); }
        if (invulnEffectInstance != null) { Destroy(invulnEffectInstance); }
        if (castEffectInstance != null) { Destroy(castEffectInstance); }

        if (anim) anim.SetTrigger("Dead");
        StopAllCoroutines();

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathWaitTime);

        Destroy(gameObject);

        SceneManager.LoadScene(cutsceneSceneName);
    }

    private void OnDrawGizmos()
    {
        // Vẽ vùng phát hiện hình chữ nhật
        Gizmos.color = (target != null && IsPlayerInDetectionBox()) ? new Color(1f, 0f, 0f, 0.25f) : new Color(0f, 1f, 0f, 0.25f);
        Vector3 boxCenter = transform.position;
        Vector3 boxSize = new Vector3(detectionBoxSize.x, detectionBoxSize.y, 0.1f);
        Gizmos.DrawWireCube(boxCenter, boxSize);

        // Giữ nguyên Gizmos cho Attack Collider, Waypoint Limit, Patrol Range
        if (attackCollider != null)
        {
            Gizmos.color = attackCollider.enabled ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireCube(attackCollider.bounds.center, attackCollider.bounds.size);
        }

        // ⭐ VẼ PHẠM VI SKILL 1 (TẦM DÙNG CHIÊU) ⭐
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, skill1Range);

        // ⭐ VẼ PHẠM VI SKILL 2 ⭐
        // Vòng tròn TẦM DÙNG chiêu (skill2Range)
        Gizmos.color = new Color(1f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, skill2Range);

        // Vòng tròn PHẠM VI NỔ (Giả định giá trị 2.5f cố định từ Prefab)
        Gizmos.color = new Color(1f, 0f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, 2.5f);

        if (currentLimitWaypoint != null)
        {
            Vector3 waypointPos3D = currentLimitWaypoint.Position;
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            Vector3 limitLeft = waypointPos3D + Vector3.left * maxDistanceFromWaypointX;
            Vector3 limitRight = waypointPos3D + Vector3.right * maxDistanceFromWaypointX;
            Gizmos.DrawLine(limitLeft + Vector3.up * 10f, limitLeft + Vector3.down * 10f);
            Gizmos.DrawLine(limitRight + Vector3.up * 10f, limitRight + Vector3.down * 10f);
        }
    }
}