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
    [SerializeField] float acceleration;
    [SerializeField] float chaseRange, strafeRange, attackRange;
    [SerializeField] float strafeMinTime, strafeMaxTime;
    float strafeDir, strafeChangeTimer;
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
        if ((rb.linearVelocityX > 0 && !facingRight) || (rb.linearVelocityX < 0 && facingRight)) Flip();
        anim.SetBool("Moving", rb.linearVelocityX * rb.linearVelocityX > 0f);

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
        if (Vector2.Distance(transform.position, player.position) < chaseRange) currentState = State.Chase;
    }

    private void HandleChase()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        transform.position = Vector2.MoveTowards(transform.position, player.position, Time.deltaTime * 2f);

        if (dist < attackRange) SelectAttack();
        else if (dist < strafeRange) currentState = State.Strafe;

        if (dist > chaseRange) currentState = State.Idle;
    }

    private void HandleStrafe()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (strafeChangeTimer <= 0f)
        {
            strafeDir = (Random.value > 0.5f) ? 1f : -1f;
            strafeChangeTimer = Random.Range(strafeMinTime, strafeMaxTime);
        }
        else strafeChangeTimer -= Time.deltaTime;

        rb.linearVelocityX = Mathf.MoveTowards(rb.linearVelocityX, strafeDir * moveSpeed, acceleration * Time.fixedDeltaTime);

        if (dist <= attackRange)
        {
            rb.linearVelocityX = 0;
            SelectAttack();
        }
        else if (dist > strafeRange * 1.3f)
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

        currentState = State.Recovery;
        currentAttack.ToggleHitbox();
        yield return new WaitForSeconds(currentAttack.recoveryTime);

        currentState = State.Chase;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        flash.DamageFlash();
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

        GameManager.Instance.EnemyDied(this);

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