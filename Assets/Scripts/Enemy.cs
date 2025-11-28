using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyAttack
{
    public float windupTime;
    public float attackTime;
    public float recoveryTime;
    [SerializeField] Collider2D hitbox;

    public void ToggleHitbox()
    {
        if (hitbox) hitbox.enabled = !hitbox.enabled;
    }
}

public class Enemy : MonoBehaviour
{
    public enum State { Idle, Chase, Strafe, Windup, Attack, Recovery, Stagger, Dead }
    public State currentState;
    [HideInInspector] public bool underwater;
    Rigidbody2D rb;
    Animator anim;
    Transform player;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float chaseRange, strafeRange, attackRange, strafeJitter;
    [SerializeField] Transform groundCheck;
    float idleFlipTimer = 2f, strafeTargetX, strafeChangeTimer;
    bool facingRight = true;

    [Header("Health")]
    [SerializeField] int currentHealth;
    [SerializeField] int maxHealth, baseOrbsDropped;
    Flash flash;

    [Header("Attacking")]
    [SerializeField] EnemyAttack[] attacks;
    EnemyAttack currentAttack;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        flash = GetComponent<Flash>();
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Strafe:
                HandleStrafe();
                break;
            case State.Windup:
                break;
            case State.Attack:
                break;
            case State.Recovery:
                break;
        }

        anim.SetBool("Moving", rb.linearVelocityX * rb.linearVelocityX > 0f);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    private void HandleIdle()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        idleFlipTimer -= Time.deltaTime;
        if (idleFlipTimer <= 0f)
        {
            Flip();
            idleFlipTimer = 2f;
        }

        if (dist < chaseRange)
        {
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;

            RaycastHit2D hit = Physics2D.Raycast(
                (Vector2) transform.position + Vector2.up,
                direction,
                dist,
                LayerMask.GetMask("Default", "Ground")
            );

            if (hit.collider && hit.collider.CompareTag("Player")) currentState = State.Chase;
        }
    }

    private void HandleChase()
    {
        float targetVelocity = Mathf.Sign(player.position.x - transform.position.x) * moveSpeed;
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocity, Time.fixedDeltaTime);

        if (IsAboutToFall()) rb.linearVelocityX = 0;

        if ((rb.linearVelocityX > 0 && !facingRight) || (rb.linearVelocityX < 0 && facingRight)) Flip();

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist < attackRange) SelectAttack();
        else if (dist < strafeRange) currentState = State.Strafe;
        else if (dist > chaseRange)
        {
            rb.linearVelocityX = 0;
            currentState = State.Idle;
        }
    }

    private void HandleStrafe()
    {
        strafeChangeTimer -= Time.deltaTime;
        if (strafeChangeTimer <= 0f)
        {
            float dir = (Random.value > 0.5f) ? 1f : -1f;
            strafeTargetX = player.position.x + dir * strafeJitter;
            strafeChangeTimer = 0.7f;
        }

        float targetVelocity = Mathf.Sign(strafeTargetX - transform.position.x) * moveSpeed;
        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, targetVelocity, Time.fixedDeltaTime);

        if (IsAboutToFall()) rb.linearVelocityX = 0;

        float playerDir = Mathf.Sign(player.position.x - transform.position.x);
        if ((playerDir > 0 && !facingRight) || playerDir < 0 && facingRight) Flip();

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            rb.linearVelocityX = 0;
            SelectAttack();
        }
        else if (dist > strafeRange * 1.4f)
        {
            rb.linearVelocityX = 0;
            currentState = State.Chase;
        }
        else if (dist > chaseRange)
        {
            rb.linearVelocityX = 0;
            currentState = State.Idle;
        }
    }

    private bool IsAboutToFall()
    {
        RaycastHit2D front = Physics2D.Raycast(groundCheck.position, Vector2.down, 1, LayerMask.GetMask("Ground"));
        RaycastHit2D behind = Physics2D.Raycast(-groundCheck.position, Vector2.down, 1, LayerMask.GetMask("Ground"));
        return (front.collider == null && behind.collider == null);
    }

    private void SelectAttack()
    {
        currentAttack = attacks[Random.Range(0, attacks.Length)];
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        anim.SetTrigger("Attack");

        currentState = State.Windup;
        yield return new WaitForSeconds(currentAttack.windupTime);

        currentState = State.Attack;
        currentAttack.ToggleHitbox();
        yield return new WaitForSeconds(currentAttack.attackTime);
        currentAttack.ToggleHitbox();

        currentState = State.Recovery;
        yield return new WaitForSeconds(currentAttack.recoveryTime);

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange) SelectAttack();
        else currentState = State.Chase;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        flash.DamageFlash();
        currentState = State.Strafe;
        if (currentHealth <= 0) StartCoroutine(Die());
    }

    public void Heal()
    {
        currentHealth = maxHealth;
        flash.HealFlash();
    }

    IEnumerator Die()
    {
        anim.SetTrigger("Death");
        yield return new WaitForSeconds(0.667f);

        var orbObject = ObjectPooler.Instance.GetFromPool("Orbs", transform.position + Vector3.up, Quaternion.identity);
        Orbs orbs = orbObject.GetComponent<Orbs>();

        if (underwater) orbs.SetOrbCount(baseOrbsDropped * 2);
        else orbs.SetOrbCount(baseOrbsDropped);

        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strafeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}