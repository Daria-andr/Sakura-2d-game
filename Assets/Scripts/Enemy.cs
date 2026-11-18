
using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    void Awake() { LevelBalance.Apply(this); }
    public int contactDamage;
    public int maxHealth;
    public float moveSpeed;
    public float moveDistance;
    public float idleTime;
    public float deathDelay;

    private int currentHealth;
    private Vector3 startPosition;

    private bool movingRight = true;
    private bool isWaiting = false;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D enemyCollider;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
    }

    void FixedUpdate()
    {
        if (isDead || isWaiting)
        {
            rb.linearVelocity = new Vector2(
                0, rb.linearVelocity.y
            );

            if (animator != null)
                animator.SetBool("IsWalking", false);

            return;
        }

        float direction = movingRight ? 1f : -1f;

        rb.linearVelocity = new Vector2(
            direction * moveSpeed,
            rb.linearVelocity.y
        );

        if (animator != null)
            animator.SetBool("IsWalking", true);

        if (movingRight &&
            transform.position.x >= startPosition.x + moveDistance)
        {
            StartCoroutine(ChangeDirection());
        }
        else if (!movingRight &&
            transform.position.x <= startPosition.x - moveDistance)
        {
            StartCoroutine(ChangeDirection());
        }
    }

IEnumerator ChangeDirection()
{
    isWaiting = true;

    rb.linearVelocity = new Vector2(
        0, rb.linearVelocity.y
    );

    yield return new WaitForSeconds(idleTime);

    movingRight = !movingRight;

    if (spriteRenderer != null)
    {
        spriteRenderer.flipX = !movingRight;
    }

    isWaiting = false;
}

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

void Die()
{
    if (isDead) return;

    isDead = true;
    StopAllCoroutines();

    rb.linearVelocity = Vector2.zero;

    rb.simulated = false;

    if (animator != null)
    {
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Death");
    }

    if (GameManager.Instance != null)
    {
        GameManager.Instance.EnemyKilled();
    }

    StartCoroutine(DeathSequence());
}

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        PlayerHealth player =
            collision.gameObject.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(contactDamage);
        }
    }
}
