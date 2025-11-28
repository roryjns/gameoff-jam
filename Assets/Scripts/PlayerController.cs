using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    [SerializeField] PauseMenu pauseMenu;
    [HideInInspector] public PlayerInput playerInput;
    Rigidbody2D rb;
    Animator anim;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [HideInInspector] public float controllerDeadzone;
    [SerializeField] float jumpForce, jumpBufferTime, coyoteTime, acceleration, deceleration, groundCheckRadius;
    [SerializeField] Transform groundCheck;
    Vector2 moveInput;
    bool facingRight = true, isGrounded, isLunging;
    float jumpBufferCounter, coyoteTimeCounter;

    [Header("Dashing")]
    [SerializeField] float dashPower;
    [SerializeField] float dashDuration, dashCooldown;
    bool canDash = true, isDashing = false, hasAirDashed = false;

    [Header("Audio")]
    [SerializeField] float footstepInterval = 0.4f;
    private float footstepTimer;
    private bool wasGrounded;

    [Header("Health")]
    [SerializeField] HealthBar healthBar;
    [SerializeField] int currentHealth, maxHealth;

    [Header("Attacking")]
    [SerializeField] Weapon weapon;
    [SerializeField] float maxHeavyChargeTime, lungeForce;
    [HideInInspector] public int currentComboStep;
    float heavyChargeTime;
    bool isChargingHeavy;
    bool comboQueued;

    [System.Serializable]
    public struct HitboxSettings
    {
        public Vector2 offset;
        public Vector2 size;
        public int damage;
    }

    [Header("Attack Hitboxes")]
    [SerializeField] HitboxSettings light1;
    [SerializeField] HitboxSettings light2, light3, heavy;

    [Header("Underwater")]
    [SerializeField] float underwaterGravityScale;
    [SerializeField] float moveMultiplier, jumpForceMultiplier;
    [HideInInspector] public bool underwater;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            playerInput = gameObject.GetComponent<PlayerInput>();
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        playerInput.actions["Jump"].performed += ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed += OnDash;
        playerInput.actions["Light Attack"].started += OnLightAttack;
        playerInput.actions["Heavy Attack"].started += OnHeavyAttackBegin;
        playerInput.actions["Heavy Attack"].canceled += OnHeavyAttackRelease;
        playerInput.actions["Pause"].performed += pauseMenu.OnTogglePause;
        playerInput.actions["Cancel"].performed += pauseMenu.OnMenuClose;
    }

    private void OnDisable() 
    {
        playerInput.actions["Jump"].performed -= ctx => jumpBufferCounter = jumpBufferTime;
        playerInput.actions["Dash"].performed -= OnDash;
        playerInput.actions["Light Attack"].started -= OnLightAttack;
        playerInput.actions["Heavy Attack"].started -= OnHeavyAttackBegin;
        playerInput.actions["Heavy Attack"].canceled -= OnHeavyAttackRelease;
        playerInput.actions["Pause"].performed -= pauseMenu.OnTogglePause;
        playerInput.actions["Cancel"].performed -= pauseMenu.OnMenuClose;
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        anim = gameObject.GetComponent<Animator>();
        healthBar.Initialise(PlayerPrefs.GetInt("MaxHealth", 5));
        healthBar.UpdateHealth(currentHealth);
        GameManager.Instance.runData.currentHealth = currentHealth;
    }

    private void FixedUpdate()
    {
        if (isDashing || isLunging) return;

        moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        if ((moveInput.x > 0 && !facingRight) || (moveInput.x < 0 && facingRight)) Flip();

        if (isChargingHeavy)
        {
            heavyChargeTime += Time.deltaTime;
            if (heavyChargeTime >= maxHeavyChargeTime) HeavyAttack();
            return;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("LightAttack"))
        {
            rb.linearVelocityX = 0;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("Ground"));
        anim.SetBool("Grounded", isGrounded);

        // Play landing sound
        if (isGrounded && !wasGrounded && rb.linearVelocityY <= 0f)
        {
            AudioManager.PlaySound(AudioManager.SoundType.LAND);
        }
        wasGrounded = isGrounded;

        float targetVelocity;
        bool isUsingGamepad = playerInput.currentControlScheme == "Gamepad";
        if ((isUsingGamepad && moveInput.sqrMagnitude > controllerDeadzone * controllerDeadzone) || (!isUsingGamepad && moveInput.sqrMagnitude > 0.001f))
        {
            targetVelocity = moveInput.x * moveSpeed;
            anim.SetBool("Moving", true);
        }
        else
        {
            targetVelocity = 0;
            anim.SetBool("Moving", false);
        }

        if (underwater) targetVelocity *= moveMultiplier;

        // Footstep system
        if (isGrounded && Mathf.Abs(rb.linearVelocityX) > 0.1f)
        {
            footstepTimer -= Time.fixedDeltaTime;
            if (footstepTimer <= 0f)
            {
                AudioManager.PlaySound(AudioManager.SoundType.WALK);
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // Reset timer to interval when not walking so first step is delayed
            footstepTimer = footstepInterval;
        }

        rb.gravityScale = underwater ? underwaterGravityScale : 1f;

        if (isGrounded && rb.linearVelocity.y > 0f) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Prevent unintended bouncing

        if (rb.linearVelocityY < -15) rb.linearVelocityY = -15; // Clamp falling speed to retain control during long falls

        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            hasAirDashed = false;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
            targetVelocity *= 0.8f; // Slower horizontal movement while in the air
            currentComboStep = 0;
        }

        if (jumpBufferCounter > 0f) jumpBufferCounter -= Time.fixedDeltaTime;

        // Horizontal movement
        float accelRate = (Mathf.Abs(targetVelocity) > 0.01f) ? acceleration : deceleration;
        if (underwater) accelRate *= 0.5f;
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocity, accelRate * Time.fixedDeltaTime);

        // Jumping
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocityY = jumpForce;
            if (underwater) rb.linearVelocityY = jumpForce * jumpForceMultiplier;
            jumpBufferCounter = coyoteTimeCounter = 0f;
            AudioManager.PlaySound(AudioManager.SoundType.JUMP);
        }

        anim.SetFloat("VerticalSpeed", rb.linearVelocityY);
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
        if (!canDash || isDashing || currentComboStep != 0) return;

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
        anim.SetBool("Dashing", true);
        anim.SetInteger("ComboStep", currentComboStep);
        isChargingHeavy = false;
        heavyChargeTime = 0f;
        float originalGravity = rb.gravityScale; 
        rb.gravityScale = 0; 

        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = new Vector2(transform.localScale.x * dashPower, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        AudioManager.PlaySound(AudioManager.SoundType.DASH);
        yield return new WaitForSeconds(dashDuration); 
        rb.gravityScale = originalGravity; 
        isDashing = false;
        anim.SetBool("Dashing", false);
        yield return new WaitForSeconds(dashCooldown); 
        canDash = true;
    }

    private void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!isGrounded || isDashing) return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsTag("LightAttack")) 
        {
            comboQueued = true;
            return; 
        }

        currentComboStep = 1;
        anim.SetInteger("ComboStep", currentComboStep);
        anim.SetTrigger("LightAttack");
        AudioManager.PlaySound(AudioManager.SoundType.LIGHTATTACK1);
        StartCoroutine(LungeCoroutine());
    }

    private IEnumerator LungeCoroutine()
    {
        float dir = facingRight ? 1f : -1f;
        isLunging = true;
        rb.linearVelocityX = dir * lungeForce;
        yield return new WaitForSeconds(0.3f);
        isLunging = false;
    }

    public void CheckComboContinue()
    {
        if (comboQueued)
        {   
            comboQueued = false;
            currentComboStep++;
            if (currentComboStep > 3) currentComboStep = 1;
            anim.SetInteger("ComboStep", currentComboStep);
            anim.SetTrigger("LightAttack");

            // Play appropriate attack sound
            if (currentComboStep == 1)
            {
                StartCoroutine(LungeCoroutine());
                AudioManager.PlaySound(AudioManager.SoundType.LIGHTATTACK1);
            }
            else if (currentComboStep == 2)
                AudioManager.PlaySound(AudioManager.SoundType.LIGHTATTACK2);
            else if (currentComboStep == 3)
                AudioManager.PlaySound(AudioManager.SoundType.LIGHTATTACK3);
        }
        else
        {
            currentComboStep = 0;
            anim.SetInteger("ComboStep", currentComboStep);
        }
    }

    private void OnHeavyAttackBegin(InputAction.CallbackContext context)
    {
        rb.linearVelocityX = 0;
        heavyChargeTime = 0;
        isChargingHeavy = true;
        anim.SetTrigger("HeavyBegin");
        Debug.Log("Charging heavy attack...");
    }

    private void OnHeavyAttackRelease(InputAction.CallbackContext context)
    {
        HeavyAttack();
    }

    private void HeavyAttack()
    {
        if (heavyChargeTime <= 0) return;
        heavyChargeTime = 0;
        isChargingHeavy = false;
        anim.SetTrigger("HeavyRelease");
        StartCoroutine(LungeCoroutine());
        Debug.Log("Heavy attack!");
    }

    public void EnableHitbox()
    {
        switch (currentComboStep)
        {
            case 0:
                weapon.SetHitboxSettings(heavy);
                break;
            case 1:
                weapon.SetHitboxSettings(light1);
                break;
            case 2:
                weapon.SetHitboxSettings(light2);
                break;
            case 3:
                weapon.SetHitboxSettings(light3);
                break;
        }
        weapon.gameObject.SetActive(true);
        weapon.DetectHits();
    }

    public void DisableHitbox()
    {
        weapon.gameObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            GameManager.Instance.runData.currentHealth = currentHealth;
            anim.SetTrigger("Death");
            StartCoroutine(GameManager.Instance.OnPlayerDeath());
        }
        healthBar.UpdateHealth(currentHealth);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}