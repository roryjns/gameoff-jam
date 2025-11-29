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
    [SerializeField] float moveSpeed, fallCheckRadius;
    [SerializeField] float chaseRange, strafeRange, attackRange, strafeJitter, knockbackForce;
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
        if (!idleAudioSource.isPlaying) idleAudioSource.Play();

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
        if (!idleAudioSource.isPlaying) idleAudioSource.Play();

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
        float moveDir = Mathf.Sign(rb.linearVelocityX);
        if (moveDir == 0) moveDir = facingRight ? 1 : -1;
        Vector2 checkPos = (Vector2)transform.position + Vector2.right * moveDir;
        bool groundAhead = Physics2D.OverlapCircle(checkPos, fallCheckRadius, LayerMask.GetMask("Ground"));
        return !groundAhead;
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