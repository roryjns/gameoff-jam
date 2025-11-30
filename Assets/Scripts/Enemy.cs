using UnityEngine;
using System.Collections;

[System.Serializable]
public class EnemyAttack
{
    public float windupTime, attackTime, recoveryTime;
    [SerializeField] int damage;
    [SerializeField] Collider2D hitbox;

    public void ToggleHitbox()
    {
        if (hitbox) hitbox.enabled = !hitbox.enabled;
        if (hitbox.enabled) DetectHits();
    }

    private void DetectHits()
    {
        ContactFilter2D filter = new()
        {
            useTriggers = true
        };

        Collider2D[] results = new Collider2D[10];
        int count = hitbox.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = results[i];
            if (!other) continue;
            if (!other.CompareTag("Player")) continue;
            if (other.TryGetComponent<PlayerController>(out var player)) player.TakeDamage(damage);
        }
    }
}

public class Enemy : MonoBehaviour
{
    public enum State { Idle, Chase, Strafe, Windup, Attack, Recovery }
    public State currentState;
    [HideInInspector] public bool underwater;
    Rigidbody2D rb;
    Animator anim;
    Transform player;
    AudioSource idleAudioSource;

    [Header("Movement")]
    [SerializeField] float moveSpeed, ledgeCheckDistance;
    [SerializeField] float chaseRange, strafeRange, attackRange, knockbackForce;
    float idleFlipTimer = 2f, strafeTargetX, strafeChangeTimer, strafeFixCooldown;
    int lastStrafeDir = 1;
    bool facingRight = true;
    Vector2 targetPos;

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
        idleAudioSource = GetComponent<AudioSource>();
        
        // Start playing idle sound on loop
        if (idleAudioSource != null && AudioManager.Instance != null)
        {
            var sound = AudioManager.Instance.GetSound(AudioManager.SoundType.ENEMYIDLE);
            if (sound.clip != null || (sound.clips != null && sound.clips.Length > 0))
            {
                AudioClip clipToPlay = sound.clip;
                if (sound.clips != null && sound.clips.Length > 0)
                {
                    clipToPlay = sound.clips[Random.Range(0, sound.clips.Length)];
                }
                
                idleAudioSource.clip = clipToPlay;
                idleAudioSource.volume = sound.defaultVolume;
                idleAudioSource.loop = true;
                idleAudioSource.Play();
            }
        }
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
        if (!idleAudioSource.isPlaying) idleAudioSource.Play();

        float dist = Vector2.Distance(transform.position, player.position);

        idleFlipTimer -= Time.deltaTime;
        if (idleFlipTimer <= 0f)
        {
            Flip();
            idleFlipTimer = 2f;
        }

        if (dist < chaseRange && CanSeePlayer(dist)) currentState = State.Chase;
    }

    private bool MoveTowardsX(float targetX)
    {
        if (!idleAudioSource.isPlaying) idleAudioSource.Play();
        float direction = Mathf.Sign(targetX - transform.position.x);
        if (direction == 0f) return true;
        targetPos = new (transform.position.x + direction * moveSpeed * Time.fixedDeltaTime, rb.position.y);
        if (direction > 0 && !facingRight) Flip();
        else if (direction < 0 && facingRight) Flip();
        if (IsAboutToFall(targetPos)) return false;
        rb.MovePosition(targetPos);
        return true;
    }

    private void HandleChase()
    {
        MoveTowardsX(player.transform.position.x);
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < attackRange) SelectAttack();
        else if (dist < strafeRange) currentState = State.Strafe;
        else if (dist > chaseRange) currentState = State.Idle;
    }

    private void HandleStrafe()
    {
        strafeChangeTimer -= Time.fixedDeltaTime;
        strafeFixCooldown -= Time.fixedDeltaTime;

        if (strafeChangeTimer <= 0f || Mathf.Abs(transform.position.x - strafeTargetX) < 0.1f)
        {
            lastStrafeDir *= -1;
            strafeTargetX = player.position.x + (lastStrafeDir * 2.5f);
            strafeChangeTimer = 1f;
        }

        bool moved = MoveTowardsX(strafeTargetX);

        if (!moved && strafeFixCooldown <= 0f)
        {
            lastStrafeDir *= -1;
            strafeTargetX = transform.position.x + (lastStrafeDir * 2.5f);
            strafeFixCooldown = strafeChangeTimer = 0.6f;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange) SelectAttack();
        else if (dist > strafeRange + 3f) currentState = State.Chase;
        else if (dist > chaseRange) currentState = State.Idle;
    }

    private bool IsAboutToFall(Vector2 targetPos)
    {
        return Physics2D.OverlapCircle(targetPos + Vector2.down * 0.4f, ledgeCheckDistance);
    }

    private bool CanSeePlayer(float dist)
    {
        Vector2 direction = facingRight ? Vector2.right : Vector2.left;

        RaycastHit2D hit = Physics2D.Raycast(
            (Vector2)transform.position + Vector2.up,
            direction,
            dist,
            LayerMask.GetMask("Default", "Ground")
        );

        return hit.collider && hit.collider.CompareTag("Player");
    }

    private void SelectAttack()
    {
        idleAudioSource.Stop();
        currentAttack = attacks[Random.Range(0, attacks.Length)];
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        anim.SetTrigger("Attack");
        currentState = State.Windup;
        AudioManager.PlaySound(AudioManager.SoundType.ENEMYWINDUP);
        yield return new WaitForSeconds(currentAttack.windupTime);

        currentState = State.Attack;
        AudioManager.PlaySound(AudioManager.SoundType.ENEMYATTACK);
        yield return new WaitForSeconds(currentAttack.attackTime);

        currentState = State.Recovery;
        yield return new WaitForSeconds(currentAttack.recoveryTime);

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange) SelectAttack();
        else currentState = State.Chase;
    }

    public void ToggleHitbox()
    {
        currentAttack.ToggleHitbox();
    }

    public void Heal()
    {
        currentHealth = maxHealth;
        flash.HealFlash();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        flash.DamageFlash();
        AudioManager.PlaySound(AudioManager.SoundType.PLAYERHIT);
        currentState = State.Strafe;
        float playerDir = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocityX += -playerDir * knockbackForce;
        if (currentHealth <= 0) StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        anim.SetTrigger("Death");
        idleAudioSource.Stop();
        AudioManager.PlaySound(AudioManager.SoundType.ENEMYDEATH);
        yield return null; // Wait a frame for animator to update
        yield return new WaitForSeconds(anim.GetCurrentAnimatorClipInfo(0).Length);

        var orbObject = ObjectPooler.Instance.GetFromPool("Orbs", transform.position + Vector3.up, Quaternion.identity);
        Orbs orbs = orbObject.GetComponent<Orbs>();

        if (underwater) orbs.SetOrbCount(baseOrbsDropped * 2);
        else orbs.SetOrbCount(baseOrbsDropped);

        GameManager.Instance.EnemyDied(this);

        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(targetPos + Vector2.down * 0.4f, ledgeCheckDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, strafeRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}