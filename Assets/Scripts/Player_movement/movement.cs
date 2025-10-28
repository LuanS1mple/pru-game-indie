using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Cần thiết cho List/HashSet nếu dùng
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
public class movement : MonoBehaviour // <-- Quay lại MonoBehaviour
{
    AudioManager audioManager;

    // Movement parameters
    public float speed = 5f; // Gán giá trị mặc định
    public float jumpHeight = 17f; // Gán giá trị mặc định
    public LayerMask groundLayer;
    public Transform groundCheck;
    public bool facingRight = true; // Gán giá trị mặc định
    private bool isGrounded = true; // Gán giá trị mặc định
    // Roll parameters
    public float rollSpeed = 7f; // Gán giá trị mặc định
    public float rollDuration = 0.4f;
    public float rollCooldown = 1.5f;
    private bool canRoll = true; // Gán giá trị mặc định
    public bool isRolling { get; private set; } = false; // Gán giá trị mặc định
    // Components
    private Rigidbody2D rb;
    private Animator animator;
    // ⭐ THÊM LẠI: Tham chiếu BaseStats
    private BaseStats playerStats;

    private bool canFlip = true;
    [Header("Skill Settings")]
    public GameObject windBlastPrefab;
    public Transform windSpawnPoint;
    [Tooltip("Thời gian hồi chiêu của kỹ năng Gió Bụi")]
    public float windBlastCooldown = 10f; // Ví dụ: 3 giây
    private float nextWindBlastTime = 0f; // Thời điểm kỹ năng sẵn sàng lần tới
    [Tooltip("Prefab của chiêu CrossBlast")]
    public GameObject crossBlastPrefab;
    [Tooltip("Vị trí xuất phát của chiêu CrossBlast")]
    public Transform crossSpawnPoint;
    [Tooltip("Thời gian hồi chiêu của kỹ năng CrossBlast")]
    public float crossBlastCooldown = 10f; 
    private float nextCrossBlastTime = 0f;
    [Header("Trap Damage Settings")]
    public float trapDamageInterval = 1.5f;
    public int trapDamageAmount = 10;
    private Coroutine trapDamageCoroutine;

    [Header("Death Sequence Settings")]
    public float deathZoomSize = 2.5f; // Kích thước orthographic size khi zoom (giá trị nhỏ hơn = zoom gần hơn)
    public float zoomSpeed = 2f;       // Tốc độ zoom camera
    public float deathAnimDuration = 1.5f; // Thời gian đợi animation chết chạy xong (Thay đổi tùy theo animation của bạn)
    private Camera mainCamera;
    private Cinemachine.CinemachineBrain cinemachineBrain;
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio")?.GetComponent<AudioManager>(); // Thêm ? để tránh lỗi nếu không tìm thấy
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        // ⭐ THÊM LẠI: Lấy component BaseStats
        playerStats = GetComponent<BaseStats>();
        if (playerStats == null)
        {
            Debug.LogError("Player thiếu component BaseStats!", this.gameObject);
        }
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Không tìm thấy Main Camera!");
        }

        // ⭐ LẤY THAM CHIẾU CINEMACHINE BRAIN ⭐
        if (mainCamera != null)
        {
            cinemachineBrain = mainCamera.GetComponent<Cinemachine.CinemachineBrain>();
        }
    }

    void Start()
    {
        // Các giá trị mặc định đã được gán ở trên
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );
    }

    void Update()
    {
        // Kiểm tra isDead
        if (playerStats != null && playerStats.isDead) return;

        if (isRolling) return;
        float move = Input.GetAxis("Horizontal");
        HandleMovement(move);
        HandleFacingDirection(move);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleJump();
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            HandleRoll();
        }
        HandleSkillInput();
        HandleFalling();
    }
    void HandleSkillInput()
    {
        // Ví dụ: Kỹ năng kích hoạt bằng phím 'F'
        if (Input.GetKeyDown(KeyCode.E))
        {
            // ⭐ SỬ DỤNG StartCoroutine để gọi hàm mới
            StartCoroutine(CastWindBlastCoroutine());
        }
        if (Input.GetMouseButtonDown(1))
        {
            StartCoroutine(CastCrossBlastCoroutine());
        }
    }
    IEnumerator CastCrossBlastCoroutine()
    {
        // 1. KIỂM TRA COOLDOWN VÀ TRẠNG THÁI
        // ⭐ SỬ DỤNG nextCrossBlastTime ⭐
        if (Time.time < nextCrossBlastTime || isRolling || (playerStats != null && playerStats.isDead))
        {
            yield break;
        }

        if (windBlastPrefab == null || windSpawnPoint == null)
        {
            Debug.LogError("Thiếu Prefab Kỹ năng CrossBlast hoặc Spawn Point!");
            yield break;
        }

        // 2. CẬP NHẬT THỜI GIAN HỒI CHIÊU
        // ⭐ SỬ DỤNG crossBlastCooldown ⭐
        nextCrossBlastTime = Time.time + crossBlastCooldown;

        // 3. Kích hoạt Animation Cast của Player
        if (animator)
        {
            animator.SetTrigger("skill2");
        }

        // 4. TẠM DỪNG SCRIPT TẠI ĐÂY TRONG 0.2 GIÂY
        yield return new WaitForSeconds(0.2f);

        // 5. Xác định hướng quay (Scale X)
        float scaleX = facingRight ? 1f : -1f;

        // 6. Sinh ra Prefab tại vị trí đã thiết lập
        GameObject skillInstance = Instantiate(
            windBlastPrefab, // Giả định windBlastPrefab là Prefab CrossBlast
            windSpawnPoint.position,
            Quaternion.identity
        );

        // 7. Truy cập script CrossBlast và khởi tạo
        CrossBlast blastScript = skillInstance.GetComponent<CrossBlast>();
        if (blastScript != null)
        {
            blastScript.Initialize(scaleX);
        }
        else
        {
            Debug.LogError("Prefab không có script CrossBlast.cs!");
        }
    }
    IEnumerator CastWindBlastCoroutine()
    {
        // 1. KIỂM TRA COOLDOWN VÀ TRẠNG THÁI (Giữ nguyên)
        if (Time.time < nextWindBlastTime || isRolling || (playerStats != null && playerStats.isDead))
        {
            yield break; // Trả về false nếu không thể Cast
        }

        if (windBlastPrefab == null || windSpawnPoint == null)
        {
            Debug.LogError("Thiếu Prefab Kỹ năng Gió hoặc Spawn Point!");
            yield break; // Thoát Coroutine
        }

        // 2. CẬP NHẬT THỜI GIAN HỒI CHIÊU NGAY LẬP TỨC
        nextWindBlastTime = Time.time + windBlastCooldown;

        // 3. Kích hoạt Animation Cast của Player NGAY LẬP TỨC
        if (animator)
        {
            animator.SetTrigger("wind");
        }

        // ⭐ 4. TẠM DỪNG SCRIPT TẠI ĐÂY TRONG 0.5 GIÂY ⭐
        yield return new WaitForSeconds(0.25f);

        // ----------- LOGIC SINH PREFAB SAU KHI DỪNG -----------

        // 5. Xác định hướng quay (Scale X)
        float scaleX = facingRight ? 1f : -1f;

        // 6. Sinh ra Prefab tại vị trí đã thiết lập
        GameObject skillInstance = Instantiate(
            windBlastPrefab,
            windSpawnPoint.position,
            Quaternion.identity
        );

        // 7. Truy cập script WindBlast và khởi tạo
        WindBlast blastScript = skillInstance.GetComponent<WindBlast>();
        if (blastScript != null)
        {
            blastScript.Initialize(scaleX);
        }
    }

    // ⭐ HÀM MỚI: Sẽ được gọi bởi Animation Event
    // QUAN TRỌNG: Hàm phải là public để Animation Event truy cập được
    public void SpawnWindBlastEffect()
    {
        // 1. Xác định hướng quay (Scale X)
        float scaleX = facingRight ? 1f : -1f;

        // 2. Sinh ra Prefab tại vị trí đã thiết lập
        GameObject skillInstance = Instantiate(
            windBlastPrefab,
            windSpawnPoint.position,
            Quaternion.identity
        );

        // 3. Truy cập script WindBlast và khởi tạo
        WindBlast blastScript = skillInstance.GetComponent<WindBlast>();
        if (blastScript != null)
        {
            blastScript.Initialize(scaleX);
        }

        // Tùy chọn: Thêm logic để Player thoát khỏi trạng thái "CastWind"
    }
    void HandleMovement(float move)
    {
        rb.velocity = new Vector2(move * speed, rb.velocity.y);
        if (animator) animator.SetFloat("Speed", Mathf.Abs(move)); // Thêm kiểm tra null
        if (isGrounded && Mathf.Abs(move) > 0.1f)
        {
            if (audioManager != null && !audioManager.SFXSource.isPlaying)
            {
                audioManager.PlaySFX(audioManager.walk);
            }
        }
    }
    void HandleJump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            isGrounded = false;
            if (animator)
            {
                animator.SetBool("isJumping", true);
                animator.SetBool("isFalling", false);
            }
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.jump);
            }
        }
    }
    void FixedUpdate()
    {
        if (playerStats != null && playerStats.isDead)
        {
            isGrounded = false;
            return;
        }
        if (groundCheck != null) // Thêm kiểm tra null
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.25f, groundLayer);
        else
            isGrounded = false; // Mặc định là không chạm đất nếu thiếu groundCheck
    }
    void HandleFalling()
    {
        if (animator == null) return; // Thoát nếu không có Animator

        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        // Kiểm tra rb tồn tại trước khi truy cập velocity
        else if (rb != null && rb.velocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (rb != null && rb.velocity.y < -0.1f)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", true);
        }
    }
    void HandleRoll()
    {
        if (canRoll && isGrounded)
        {
            StartCoroutine(Roll());
        }
    }
    IEnumerator Roll()
    {
        canRoll = false;
        isRolling = true;
        if (animator) animator.SetBool("isRolling", true);
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.playerRoll);
        }
        float rollDirection = facingRight ? 1f : -1f;
        float elapsed = 0f;

        // Dùng playerStats đã lấy ở Awake
        if (playerStats != null)
            playerStats.isInvulnerable = true;

        while (elapsed < rollDuration)
        {
            if (rb != null) rb.velocity = new Vector2(rollDirection * rollSpeed, rb.velocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (animator) animator.SetBool("isRolling", false);
        isRolling = false;

        if (playerStats != null)
            playerStats.isInvulnerable = false;

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }
    void HandleFacingDirection(float move)
    {
        if (!canFlip) return;
        if (move > 0 && !facingRight)
        {
            Flip();
        }
        else if (move < 0 && facingRight)
        {
            Flip();
        }
    }
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }
    public void MoveLeft()
    {
        if (playerStats != null && playerStats.isDead) return;
        if (rb != null) rb.velocity = new Vector2(-speed, rb.velocity.y);
        if (facingRight) Flip();
        if (animator && rb != null) animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
    public void MoveRight()
    {
        if (playerStats != null && playerStats.isDead) return;
        if (rb != null) rb.velocity = new Vector2(speed, rb.velocity.y);
        if (!facingRight) Flip();
        if (animator && rb != null) animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
    public void SetCanFlip(bool value)
    {
        canFlip = value;
    }

    // ⭐⭐⭐ THÊM LẠI: HÀM ĐỂ BASESTATS GỌI ⭐⭐⭐

    public void PlayHitAnimation()
    {
        // Kiểm tra animator và isDead
        if (animator != null && (playerStats == null || !playerStats.isDead))
        {
            animator.SetTrigger("hit"); // Chữ thường theo code cũ của bạn
            Debug.Log("Player triggered Hit animation!");
        }
    }

    public void HandleDeath()
    {
        // 1. Kích hoạt animation chết
        if (animator != null)
        {
            animator.SetTrigger("dead");
            Debug.Log("Player triggered Dead animation!");
        }

        // 2. Tắt tất cả hành động di chuyển/vật lý ngay lập tức
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 3. Khóa các Collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. ⭐ BẮT ĐẦU COROUTINE ĐIỀU KHIỂN QUÁ TRÌNH CHẾT VÀ CHUYỂN SCENE ⭐
        StartCoroutine(HandleDeathSequence("Defeat_End"));
    }
    // ⭐ COROUTINE MỚI: Xử lý quá trình chết và chuyển cảnh
    IEnumerator HandleDeathSequence(string sceneName)
    {
        // 1. KHÓA INPUT BẰNG CÁCH TẮT SCRIPT NÀY
        this.enabled = false;

        // ⭐ TẮT CINEMACHINE BRAIN ĐỂ NÓ KHÔNG GHI ĐÈ CAMERA SIZE ⭐
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        // 2. BẮT ĐẦU ZOOM CAMERA (Bây giờ nó sẽ hoạt động)
        if (mainCamera != null && mainCamera.orthographic)
        {
            float startSize = mainCamera.orthographicSize;
            float targetSize = deathZoomSize;
            float elapsed = 0f;

            // Camera cần phải được đặt ở vị trí nhân vật để zoom tập trung đúng
            // Nếu bạn đang dùng Cinemachine, Virtual Camera sẽ tự đặt vị trí, 
            // nên khi tắt Brain, camera sẽ giữ nguyên vị trí cuối cùng đó.

            while (elapsed < zoomSpeed)
            {
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / zoomSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            mainCamera.orthographicSize = targetSize;
        }

        // 3. CHỜ ANIMATION CHẾT CHẠY XONG
        yield return new WaitForSeconds(deathAnimDuration);

        // 4. CHUYỂN SCENE CUỐI CÙNG
        SceneManager.LoadScene(sceneName);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            // 1. Nếu Player đang sống và Coroutine chưa chạy, bắt đầu Coroutine
            if (playerStats != null && !playerStats.isDead)
            {
                // Đảm bảo không có Coroutine nào đang chạy
                if (trapDamageCoroutine != null)
                {
                    StopCoroutine(trapDamageCoroutine);
                }

                // 2. Kích hoạt Coroutine gây sát thương liên tục
                trapDamageCoroutine = StartCoroutine(ApplyTrapDamagePeriodically());
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            // Dừng Coroutine khi Player rời khỏi bẫy
            if (trapDamageCoroutine != null)
            {
                StopCoroutine(trapDamageCoroutine);
                trapDamageCoroutine = null; // Đặt lại về null
            }
        }
    }

    // Hàm Coroutine mới để xử lý sát thương bẫy liên tục
    IEnumerator ApplyTrapDamagePeriodically()
    {
        // Lặp vô hạn cho đến khi bị dừng bằng StopCoroutine (từ OnTriggerExit2D)
        while (true)
        {
            // 1. Kiểm tra trạng thái: Nếu Player chết hoặc đang lăn, thoát
            if (playerStats == null || playerStats.isDead || isRolling)
            {
                trapDamageCoroutine = null; // Đảm bảo đặt lại nếu thoát
                yield break;
            }

            // 2. Áp dụng sát thương
            playerStats.TakeDamageTrap(trapDamageAmount);
            Debug.Log($"Player bị mất {trapDamageAmount} máu do bẫy (Coroutine). Máu hiện tại: {playerStats.currentHP}");
            PlayHitAnimation(); // Kích hoạt animation trúng đòn

            // 3. Chờ khoảng thời gian hồi chiêu sát thương
            yield return new WaitForSeconds(trapDamageInterval);
        }
    }   
}