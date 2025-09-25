using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float maxSpeed; // bien di chuyen
    public float jumpHeigh;//bien nhay
    public bool facingRight;// bien huong
    public bool isGrounded;// bien dat(kiem tra nhan vat co cham co hay khong)
    Rigidbody2D rb;
    Animator animator;

    void Start()
    {
        // khoi tao 2 doi tung rb va animator voi 2 component trong unity
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        facingRight = true; // khoi tao huong ban dau la phai
        isGrounded = true; // khoi tao dat ban dau la true
        rb.isKinematic = true; // dat trang thai vat ly la true

    }

    // Update is called once per frame
    void Update()
    {
        maxSpeed = 5f; // dat toc do di chuyen la 5
        jumpHeigh = 7f; // dat chieu cao nhay la 7
        float move = Input.GetAxis("Horizontal"); // lay gia tri di chuyen tren truc x
        rb.velocity = new Vector2(move * maxSpeed, rb.velocity.y); // dat van toc cua nhan vat
        animator.SetFloat("SpeedMain", Mathf.Abs(move)); // dat bien Speed trong animator bang gia tri tuyet doi cua move
        if (move > 0 && !facingRight) // neu di chuyen sang phai va huong hien tai khong phai la phai
        {
            flip(); // goi ham flip de dao huong
        }
        else if (move < 0 && facingRight) // neu di chuyen sang trai va huong hien tai la phai
        {
            flip(); // goi ham flip de dao huong
        }
        else
        {
            // khong lam gi ca
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // neu nhan phim space va isGrounded la true
        {
            jump(); // goi ham jump de nhay
        }
        animator.SetFloat("SpeedMain", Mathf.Abs(move));
        animator.SetBool("isJumping", !isGrounded);
        animator.SetFloat("verticalVelocity", rb.velocity.y);


    }
    void OnCollisionEnter2D(Collision2D collision) // ham kiem tra va cham
    {
        if (collision.gameObject.tag == "ground") // neu va cham voi doi tuong co tag la ground
        {
            isGrounded = true; // dat isGrounded la true
            animator.SetBool("isJumping", false); // dat bien isJumping trong animator la false
        }
    }
    void OnCollisionExit2D(Collision2D collision) // ham kiem tra roi khoi va cham
    {
        if (collision.gameObject.tag == "ground")
        {
            isGrounded = false; // dat isGrounded la false
            animator.SetBool("isJumping", true); // dat bien isJumping trong animator la true
        }
    }
    void flip() // ham dao huong
    {
        facingRight = !facingRight; // dao huong
        Vector3 theScale = transform.localScale; // lay kich thuoc hien tai
        theScale.x *= -1; // dao kich thuoc tren truc x
        transform.localScale = theScale; // dat lai kich thuoc
    }
    public void jump() // ham nhay
    {
        if (isGrounded) // neu isGrounded la true
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeigh); // dat van toc cua nhan vat
            isGrounded = false; // dat isGrounded la false
            animator.SetBool("isJumping", true); // dat bien isJumping trong animator la true
        }
    }
    public void moveLeft() // ham di chuyen sang trai
    {
        rb.velocity = new Vector2(-maxSpeed, rb.velocity.y); // dat van toc cua nhan vat
        animator.SetFloat("SpeedMain", Mathf.Abs(-1)); // dat bien Speed trong animator bang 1
        if (facingRight) // neu huong hien tai la phai
        {
            flip(); // goi ham flip de dao huong
        }
    }
    public void moveRight() // ham di chuyen sang phai
    {
        rb.velocity = new Vector2(maxSpeed, rb.velocity.y); // dat van toc cua nhan vat
        animator.SetFloat("SpeedMain", Mathf.Abs(1)); // dat bien Speed trong animator bang 1
        if (!facingRight) // neu huong hien tai khong phai la phai
        {
            flip(); // goi ham flip de dao huong
        }
    }
    public void stopMoving() // ham dung di chuyen
    {
        rb.velocity = new Vector2(0, rb.velocity.y); // dat van toc cua nhan vat
        animator.SetFloat("SpeedMain", Mathf.Abs(0)); // dat bien Speed trong animator bang 0
    }
}
