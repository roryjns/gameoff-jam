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
    float jumpBufferCounter, coyoteTimeCounter;

    [Header("Dashing")]
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] float dashPower, dashDuration, dashCooldown;
    bool canDash = true, isDashing = false, hasAirDashed = false;

    [Header("Attacking")]
    [SerializeField] float lightComboResetTime;
    [HideInInspector] public int currentComboStep;
    public bool canQueueAttack;
    bool comboQueued;

    [SerializeField] float maxHeavyChargeTime;
    float heavyChargeTime;

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

    private void OnEnable()
    {
        playerInput.actions["Jump"].performed += ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed += OnDash;
        playerInput.actions["Light Attack"].started += OnLightAttack;
        playerInput.actions["Heavy Attack"].started += OnHeavyAttackBegin;
        playerInput.actions["Heavy Attack"].canceled += OnHeavyAttackRelease;
    }

    private void OnDisable() 
    {
        playerInput.actions["Jump"].performed -= ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed -= OnDash;
        playerInput.actions["Light Attack"].started -= OnLightAttack;
        playerInput.actions["Heavy Attack"].started -= OnHeavyAttackBegin;
        playerInput.actions["Heavy Attack"].canceled -= OnHeavyAttackRelease;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        float targetVelocity = moveInput.x * moveSpeed;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, tilemapLayer);
        animator.SetBool("Grounded", isGrounded);
        animator.SetBool("Moving", Mathf.Abs(targetVelocity) > 0.1);

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

        animator.SetFloat("VerticalSpeed", rb.linearVelocityY);

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

    private IEnumerator Dash() 
    { 
        canDash = false; 
        isDashing = true;
        animator.SetBool("Dashing", true);
        float originalGravity = rb.gravityScale; 
        rb.gravityScale = 0; 
        rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f); 
        trailRenderer.emitting = true;
        SoundManager.PlaySound(SoundManager.SoundType.DASH);
        yield return new WaitForSeconds(dashDuration); 
        trailRenderer.emitting = false; 
        rb.gravityScale = originalGravity; 
        isDashing = false;
        animator.SetBool("Dashing", false);
        yield return new WaitForSeconds(dashCooldown); 
        canDash = true;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!isGrounded) return;

        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("LightAttack")) 
        {
            comboQueued = true;
            return; 
        }

        currentComboStep = 1;
        animator.SetInteger("ComboStep", currentComboStep);
        animator.SetTrigger("LightAttack");
    }

    public void CheckComboContinue()
    {
        if (comboQueued)
        {
            comboQueued = false;
            currentComboStep++;
            if (currentComboStep > 3) currentComboStep = 1;
            animator.SetInteger("ComboStep", currentComboStep);
            animator.SetTrigger("LightAttack");
        }
        else
        {
            currentComboStep = 0;
            animator.SetInteger("ComboStep", currentComboStep);
        }
    }

    private void OnHeavyAttackBegin(InputAction.CallbackContext context)
    {
        heavyChargeTime = 0f;
        animator.SetTrigger("HeavyCharge");
        Debug.Log("Charging heavy attack...");
    }

    private void OnHeavyAttackRelease(InputAction.CallbackContext context)
    {
        if (heavyChargeTime <= 0) return;
        heavyChargeTime = 0f;
        animator.SetTrigger("HeavyRelease");
        Debug.Log("Heavy attack!");
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}