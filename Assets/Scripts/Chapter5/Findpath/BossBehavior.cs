using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossBehavior : BaseStats
{
    [Header("Events")]
    public UnityEvent OnBossDied;

    [Header("References")]
    public WaypointGraph graph;
    public Transform target;
    private Rigidbody2D rb;
    private Animator anim;

    [Header("Skill Points")]
    public Transform skill2SpawnPoint; // Kéo GameObject con (Child) vào đây

    [Header("Skill Prefabs")]
    public GameObject lightningPillarPrefab; // Skill 1
    public GameObject explosionAreaPrefab; // Skill 2

    // ⭐ SKILL 3: BẤT TỬ (ĐÃ SỬA LỖI HỒI CHIÊU VÀ CAST) ⭐
    [Header("Skill 3 (Invulnerability)")]
    public GameObject skill3CastPrefab;
    public GameObject invulnerabilityEffectPrefab;
    public float skill3MinHealthPercentage = 0.6f;
    public float skill3Duration = 4f;
    public float skill3Cooldown = 30f;
    private bool isSkill3Ready = true;
    private bool isInvulnerable = false; // Trạng thái bất tử (MIỄN NHIỄM SÁT THƯƠNG)
    private GameObject invulnEffectInstance;
    private GameObject castEffectInstance;
    private bool isCastingSkill3 = false; // Chỉ True trong 0.8s Cast ban đầu

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

    [Header("Skill Settings")]
    // Skill 1 (Cột Sét)
    public float skill1Cooldown = 5f;
    public float skill1Range = 10f;
    private bool isSkill1Ready = true;

    // Skill 2 (Vòng Xoáy Năng Lượng)
    public float skill2Cooldown = 8f;
    public float skill2Range = 3f;
    private bool isSkill2Ready = true;

    private bool isUsingSkill = false; // Dùng cho Skill 1 và Skill 2
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
    }

    void Start()
    {
        manager = FindObjectOfType<BossRoomManager>();
        if (manager != null)
        {
            manager.enemiesRemaining++;
            Debug.Log($"[{entityName}] đã đăng ký với Boss Room Manager. Tổng quái hiện tại: {manager.enemiesRemaining}");
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

        if (!isInvulnerable && !isCastingSkill3) StopAllCoroutines();

        isUsingSkill = false;
        isCastingSkill3 = false;

        isSkill1Ready = true;
        isSkill2Ready = true;

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

        if (!isInvulnerable && !isCastingSkill3) StopAllCoroutines();

        isUsingSkill = false;
        isCastingSkill3 = false;

        isSkill1Ready = true;
        isSkill2Ready = true;

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
        // 1. NGĂN CHẶN NẾU ĐANG CHẾT/TẤN CÔNG/DÙNG SKILL CAST
        // Lưu ý: isInvulnerable (Bất tử) KHÔNG chặn hành động, chỉ isCastingSkill3 (Tụ chiêu) chặn
        if (isDead || isAttacking || isCastingSkill3)
        {
            rb.velocity = Vector2.zero;
            if (!isAttacking && !isCastingSkill3)
                if (anim) anim.SetFloat("Speed", 0);
            if ((isAttacking || isCastingSkill3) && target)
                RotateToDirection(target.position.x - transform.position.x);
            return;
        }

        // ⭐ 2. KIỂM TRA LOGIC SKILL 3 (ƯU TIÊN HÀNG ĐẦU) ⭐
        if (currentHP / maxHP <= skill3MinHealthPercentage && isSkill3Ready)
        {
            Debug.Log($"[{entityName}] Kích hoạt Skill 3 do máu thấp và đã sẵn sàng lại!");
            rb.velocity = Vector2.zero;
            if (anim) anim.SetFloat("Speed", 0);
            UseSkill3();
            return;
        }

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

            // ⭐ 3. LOGIC SKILL 2 (ƯU TIÊN 2) ⭐
            // Boss có thể dùng S1/S2 khi đang BẤT TỬ (isInvulnerable=true), vì isUsingSkill chỉ ngăn S1/S2 chồng chéo.
            if (!isUsingSkill && isSkill2Ready && distanceToPlayer <= skill2Range)
            {
                rb.velocity = Vector2.zero;
                if (anim) anim.SetFloat("Speed", 0);
                UseSkill2();
                return;
            }

            // ⭐ 4. LOGIC SKILL 1 (ƯU TIÊN 3) ⭐
            if (!isUsingSkill && isSkill1Ready && distanceToPlayer <= skill1Range)
            {
                rb.velocity = Vector2.zero;
                if (anim) anim.SetFloat("Speed", 0);
                UseSkill1();
                return;
            }


            // ⭐ 5. LOGIC TẤN CÔNG THƯỜNG (ƯU TIÊN 4) ⭐
            if (isChasing && distanceToPlayer <= attackRange)
            {
                rb.velocity = Vector2.zero;
                if (anim) anim.SetFloat("Speed", 0);
                StartAttack();
                return;
            }

            // ⭐ 6. LOGIC ĐUỔI THEO (ƯU TIÊN 5) ⭐
            ChaseTargetWithWaypointLimit();
        }
        else // Player nằm ngoài vùng chữ nhật
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

        yield return new WaitForSeconds(0.1f);
        if (attackCollider != null) attackCollider.enabled = true;
        canDealDamage = true;
        hitPlayers.Clear();

        yield return new WaitForSeconds(0.2f);
        if (attackCollider != null) attackCollider.enabled = false;
        canDealDamage = false;

        yield return new WaitForSeconds(attackCooldown - 0.3f);

        isAttacking = false;
        attackCoroutine = null;
    }

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
    public void HandleAnimation_DisableAttackCollider()
    {
        if (attackCollider == null) return;
        Debug.Log($"[{entityName}] --- TẮT Hitbox Tấn công.", this.gameObject);
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
    private void OnTriggerEnter2D(Collider2D collision) { TryDealDamage(collision); }
    private void OnTriggerStay2D(Collider2D collision) { TryDealDamage(collision); }

    public void HandleAnimation_TriggerTakeDamage(float damage) { TakeDamage(damage); }


    // --- LOGIC SKILL CỘT SÉT (SKILL 1) ---

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
        StartCoroutine(Skill1CooldownRoutine());
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
        Debug.Log($"[{entityName}] Skill 1 (Cột Sét) đã sẵn sàng!");
    }


    // --- LOGIC SKILL VÒNG XOÁY (SKILL 2) ---

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
        StartCoroutine(Skill2CooldownRoutine());
    }

    private IEnumerator Skill2Routine()
    {
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.7f);

        if (skill2SpawnPoint == null)
        {
            Debug.LogError("Skill 2 Spawn Point chưa được gán! Không thể tạo vòng xoáy.");
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
        Debug.Log($"[{entityName}] Skill 2 (Vòng Xoáy Năng Lượng) đã sẵn sàng!");
    }


    // --- LOGIC SKILL BẤT TỬ (SKILL 3) ---

    void UseSkill3()
    {
        if (!isSkill3Ready) return;

        isSkill3Ready = false;
        isCastingSkill3 = true;

        // ⭐ BẬT BẤT TỬ NGAY LẬP TỨC
        isInvulnerable = true;

        if (anim) anim.SetTrigger("Skill3");

        if (target != null)
            RotateToDirection(target.position.x - transform.position.x);

        // ⭐ TẠO HIỆU ỨNG BẤT TỬ NGAY LẬP TỨC
        if (invulnerabilityEffectPrefab != null && invulnEffectInstance == null)
        {
            invulnEffectInstance = Instantiate(invulnerabilityEffectPrefab, transform.position, Quaternion.identity);
            invulnEffectInstance.transform.SetParent(transform);
            invulnEffectInstance.transform.localPosition = Vector3.zero;
            Debug.Log($"[{entityName}] KÍCH HOẠT BẤT TỬ ngay khi bắt đầu cast!");
        }

        StartCoroutine(Skill3Routine());
        StartCoroutine(Skill3CooldownRoutine());
    }

    private IEnumerator Skill3Routine()
    {
        rb.velocity = Vector2.zero;
        float castDuration = 0.8f;

        // 1. TRIỆU HỒI HIỆU ỨNG CAST (THI TRIỂN)
        if (skill3CastPrefab != null)
        {
            castEffectInstance = Instantiate(skill3CastPrefab, transform.position, Quaternion.identity);
            castEffectInstance.transform.SetParent(transform);
            castEffectInstance.transform.localPosition = Vector3.zero;
        }

        // Chờ thời gian cast
        yield return new WaitForSeconds(castDuration);

        // 2. KẾT THÚC CAST VÀ DUY TRÌ BẤT TỬ (Bất tử đã bật ở UseSkill3)
        if (castEffectInstance != null)
        {
            Destroy(castEffectInstance);
            castEffectInstance = null;
        }

        isCastingSkill3 = false; // Boss có thể di chuyển/dùng skill khác

        // 3. Giữ Bất tử tồn tại (thời gian còn lại = tổng thời gian - thời gian cast)
        // Nếu skill3Duration > castDuration
        float activeDuration = skill3Duration - castDuration;
        if (activeDuration > 0)
        {
            yield return new WaitForSeconds(activeDuration);
        }

        // Nếu không, chỉ cần chờ 0.2s phục hồi
        else if (skill3Duration <= castDuration)
        {
            // Nếu thời gian Bất tử quá ngắn, ta hủy luôn hiệu ứng cast
            Debug.LogWarning($"[{entityName}] Skill3 Duration ({skill3Duration}s) ngắn hơn Cast Duration ({castDuration}s). Vui lòng đặt Duration lớn hơn Cast!");
        }


        // 4. Kết thúc Bất tử và hủy hiệu ứng
        isInvulnerable = false;
        Debug.Log($"[{entityName}] HỦY BẤT TỬ.");

        if (invulnEffectInstance != null)
        {
            Destroy(invulnEffectInstance);
            invulnEffectInstance = null;
        }

        // 5. Giai đoạn phục hồi sau chiêu (0.2s)
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator Skill3CooldownRoutine()
    {
        yield return new WaitForSeconds(skill3Cooldown);
        isSkill3Ready = true;
        Debug.Log($"[{entityName}] Skill 3 (Bất Tử) ĐÃ SẴN SÀNG LẠI!");
    }


    // ... (Logic Di Chuyển/Patrol giữ nguyên) ...
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

    public override void TakeDamage(float damage)
    {
        if (isDead) return;

        // ⭐ KIỂM TRA BẤT TỬ ⭐
        if (isInvulnerable)
        {
            Debug.Log($"[{entityName}] Đang trong trạng thái BẤT TỬ! Không mất máu.");
            if (anim) anim.SetTrigger("Hit");
            return;
        }

        base.TakeDamage(damage);
        if (anim) anim.SetTrigger("Hit");

        if (isAttacking || isUsingSkill || isCastingSkill3)
        {
            // Nếu Boss bị ngắt skill, hủy tất cả Coroutine và reset các cờ
            if (isUsingSkill || isCastingSkill3)
            {
                if (explosionInstance != null) { Destroy(explosionInstance); explosionInstance = null; }
                if (castEffectInstance != null) { Destroy(castEffectInstance); castEffectInstance = null; }
                // KHÔNG hủy invulnEffectInstance nếu đang Bất tử vì có thể bị ngắt trong lúc Bất tử
                // Tuy nhiên, logic hiện tại cho phép nó chạy đến hết cooldown. Để an toàn, chỉ hủy khi không phải trạng thái Invulnerable.
                if (invulnEffectInstance != null && !isInvulnerable) { Destroy(invulnEffectInstance); invulnEffectInstance = null; }
            }

            // Hủy tất cả Coroutine đang chạy, bao gồm Skill3Routine nếu nó đang chạy
            StopAllCoroutines();

            // Đảm bảo trạng thái Skill 3 được reset hoàn toàn
            isInvulnerable = false; // Hủy trạng thái bất tử
            isCastingSkill3 = false;
            if (invulnEffectInstance != null) { Destroy(invulnEffectInstance); invulnEffectInstance = null; } // Hủy hiệu ứng

            // Reset trạng thái các skill khác
            isAttacking = false;
            isUsingSkill = false;
            attackCoroutine = null;

            isSkill1Ready = true;
            isSkill2Ready = true;
            isSkill3Ready = true; // Cho phép dùng lại ngay nếu bị ngắt

            if (attackCollider != null) attackCollider.enabled = false;
            Debug.Log($"[{entityName}] Bị đánh trúng! Reset trạng thái tấn công/skill.");
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
        else if (managerNotified)
        {
            Debug.LogWarning($"⚠️ Hàm Die được gọi lần nữa, nhưng đã bảo vệ Manager!");
        }

        if (explosionInstance != null) { Destroy(explosionInstance); }
        if (invulnEffectInstance != null) { Destroy(invulnEffectInstance); }
        if (castEffectInstance != null) { Destroy(castEffectInstance); }

        if (anim) anim.SetTrigger("Dead");
        StopAllCoroutines();
        Destroy(gameObject, 2.5f);
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
            if (fallbackPatrolPoint != null && currentLimitWaypoint != fallbackPatrolPoint)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(fallbackPatrolPoint.Position, 0.4f);
            }
        }
    }
}