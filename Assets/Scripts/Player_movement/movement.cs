using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movement : MonoBehaviour
{
    //Khai báo để sử dụng âm thanh 
    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    // Movement parameters
    public float speed;
    public float jumpHeight;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public bool facingRight;
    private bool isGrounded;
    // Roll parameters
    public float  rollSpeed;
    public float rollDuration;  // thời gian lăn
    public float rollCooldown;    // thời gian hồi
    private bool canRoll;
    public bool isRolling { get; private set; } //
    // Components
    private Rigidbody2D rb;
    private Animator animator;

    private bool canFlip = true; // 👈 có được phép xoay hay không


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        facingRight = true;
        isGrounded = true;
        speed = 5f;
        jumpHeight = 17f;
        rollSpeed = 7f;
        rollDuration = 0.4f;
        rollCooldown = 1.5f;
        canRoll = true;
        isRolling = false;

        // 👇 Bỏ va chạm vật lý giữa Player và Enemy vĩnh viễn
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Enemy"),
            true
        );
    }


    void Update()
    {

        if (isRolling) return;// when rolling, ignore other inputs
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

        HandleFalling();
    }

    void HandleMovement(float move)
    {
        rb.velocity = new Vector2(move * speed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(move));

        //Âm thanh player di chuyển
        if (isGrounded && Mathf.Abs(move) > 0.1f)
        {
            if (!audioManager.SFXSource.isPlaying) 
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
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);

            //Âm thanh player nhảy
            if (audioManager != null)
            {
                audioManager.PlaySFX(audioManager.jump);
            }
            Debug.Log("Jump!");
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.25f, groundLayer);

    }
    void HandleFalling()
    {
        if (isGrounded)
        {
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
        }
        else if (rb.velocity.y > 0.1f)
        {
            animator.SetBool("isJumping", true);
            animator.SetBool("isFalling", false);
        }
        else if (rb.velocity.y < -0.1f)
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
        animator.SetBool("isRolling", true);

        //Âm thanh Player lộn
        if (audioManager != null)
        {
            audioManager.PlaySFX(audioManager.playerRoll);
        }

        float rollDirection = facingRight ? 1f : -1f;
        float elapsed = 0f;

        // 👇 Lấy BaseStats để bật invulnerable
        BaseStats stats = GetComponent<BaseStats>();
        if (stats != null)
            stats.isInvulnerable = true;

        // Di chuyển khi roll
        while (elapsed < rollDuration)
        {
            rb.velocity = new Vector2(rollDirection * rollSpeed, rb.velocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Kết thúc roll
        animator.SetBool("isRolling", false);
        isRolling = false;

        // 👇 Tắt invulnerability
        if (stats != null)
            stats.isInvulnerable = false;

        // Hồi chiêu
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }




    // movement handling
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
        rb.velocity = new Vector2(-speed, rb.velocity.y);
        if (facingRight) Flip();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }

    public void MoveRight()
    {
        rb.velocity = new Vector2(speed, rb.velocity.y);
        if (!facingRight) Flip();
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
    public void SetCanFlip(bool value)
    {
        canFlip = value;
    }



}