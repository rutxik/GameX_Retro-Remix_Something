using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool wasGrounded;
    private float moveInput;
    private bool canDoubleJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Just landed
        if (isGrounded && !wasGrounded)
        {
            canDoubleJump = false;
            anim.SetBool("isJumping", false);
            anim.SetBool("isDoubleJumping", false);
        }

        // Just left ground
        if (!isGrounded && wasGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetBool("isJumping", true);
                anim.SetBool("isDoubleJumping", false);
            }
            else if (canDoubleJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                anim.SetBool("isDoubleJumping", true);
                anim.SetBool("isJumping", false);
                canDoubleJump = false;
            }
        }

        anim.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}