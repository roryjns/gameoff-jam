using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [SerializeField] Rigidbody2D rb;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] Animator animator;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce, jumpBufferTime, coyoteTime, acceleration, deceleration;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask tilemapLayer;
    [SerializeField] bool isGrounded;

    Vector2 moveInput;
    float jumpBufferCounter;
    float coyoteTimeCounter;
    bool facingRight = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        playerInput.actions["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerInput.actions["Move"].canceled += ctx => moveInput = Vector2.zero;
        playerInput.actions["Jump"].performed += ctx => jumpBufferCounter = jumpBufferTime;
    }

    void OnEnable() => playerInput.actions.Enable();
    void OnDisable() => playerInput.actions.Disable();

    private void FixedUpdate()
    {
        float targetVelocity = moveInput.x * moveSpeed;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, tilemapLayer);
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Moving", Mathf.Abs(targetVelocity) > 0);

        if (isGrounded && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Prevent unintended bouncing

        if (rb.linearVelocityY < -15)
            rb.linearVelocityY = -15; // Clamp falling speed to retain control during long falls

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
            targetVelocity *= 0.8f; // Slower horizontal movement while in the air
        }

        if (jumpBufferCounter > 0f)
            jumpBufferCounter -= Time.fixedDeltaTime;

        // Horizontal movement
        float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;  // Accelerate or decelerate
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocity, accelRate * Time.fixedDeltaTime);

        // Jumping
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocityY = jumpForce; // Jump
            jumpBufferCounter = coyoteTimeCounter = 0f;
        }

        if ((moveInput.x > 0 && !facingRight) || (moveInput.x < 0 && facingRight))
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}