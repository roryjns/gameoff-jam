using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public static PlayerInput playerInput;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Animator animator;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce, jumpBufferTime, coyoteTime, acceleration, deceleration;
    Vector2 moveInput;
    bool facingRight = true;

    [Header("Jumping")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius;
    [SerializeField] LayerMask tilemapLayer;
    [SerializeField] bool isGrounded;
    float jumpBufferCounter;
    float coyoteTimeCounter;

    [Header("Dashing")]
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] float dashPower, dashTime, dashCooldown;
    bool canDash = true, isDashing = false, hasAirDashed = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerInput = gameObject.GetComponent<PlayerInput>();
        }
        else
            Destroy(this);
    }

    void OnEnable()
    {
        playerInput.actions["Jump"].performed += ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed += OnDash;
    }

    void OnDisable() 
    {
        playerInput.actions["Jump"].performed -= ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed -= OnDash;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash || isDashing) return;

        if (isGrounded || !hasAirDashed)
        {
            StartCoroutine(Dash());
            if (!isGrounded)
                hasAirDashed = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
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
            hasAirDashed = false;
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

    private IEnumerator Dash() 
    { 
        canDash = false; 
        isDashing = true; 
        float originalGravity = rb.gravityScale; 
        rb.gravityScale = 0; 
        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f); 
        trailRenderer.emitting = true; 
        yield return new WaitForSeconds(dashTime); 
        trailRenderer.emitting = false; 
        rb.gravityScale = originalGravity; 
        isDashing = false; 
        yield return new WaitForSeconds(dashCooldown); 
        canDash = true; 
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