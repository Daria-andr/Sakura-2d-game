
using UnityEngine;

public class Bomb : Hazard
{
    [Header("Bomb Settings")]
    public float explosionRadius = 2f;
    public float fuseDuration = 2f;
    public float explosionDuration = 0.6f;

    private Animator animator;
    private Rigidbody2D rb;

    private bool isExploding = false;
    private bool hasLanded = false;

    protected override void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded || isExploding) return;

        if (collision.gameObject.CompareTag("Ground") ||
            collision.gameObject.CompareTag("Player"))
        {
            hasLanded = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            if (animator != null)
            {
                animator.SetTrigger("Fuse");
            }

            // Взрыв через 2 секунды
            Invoke(nameof(Explode), Mathf.Max(0, fuseDuration));
        }
    }

    void Explode()
    {
        if (isExploding) return;

        isExploding = true;

        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            explosionRadius
        );

        foreach (Collider2D hit in hits)
        {
            PlayerHealth player =
                hit.GetComponentInParent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
                break;
            }
        }

        // Удаляем бомбу после анимации взрыва
        Destroy(gameObject, explosionDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}
