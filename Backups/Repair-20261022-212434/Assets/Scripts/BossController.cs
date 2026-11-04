
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 8;
    public Slider healthBar;
    public int contactDamage = 1;

    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float attackInterval = 2.5f;
    public float attackAnimationDelay = 0.3f;

    [Header("Death")]
    public float deathAnimationDuration = 0.8f;

    private int currentHealth;
    private bool isDead;
    private float nextAttackTime;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();

        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    void Start()
    {
        nextAttackTime = Time.time + attackInterval;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints =
                RigidbodyConstraints2D.FreezePositionX |
                RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Update()
    {
        if (isDead) return;
        if (projectilePrefab == null || shootPoint == null)
            return;

        if (Time.time >= nextAttackTime)
        {
            nextAttackTime =
                Time.time + Mathf.Max(0.2f, attackInterval);

            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(attackAnimationDelay);

        if (isDead) yield break;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.identity
        );

        BossProjectile projectileScript =
            projectile.GetComponent<BossProjectile>();

        if (projectileScript != null)
        {
            // Босс справа, стреляет влево
            projectileScript.SetDirection(-1);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= Mathf.Max(0, amount);
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        Debug.Log("Boss HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(
            deathAnimationDuration
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BossKilled();
        }

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        PlayerHealth health =
            collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(contactDamage);
        }
    }
}
