
using UnityEngine;
using System.Collections;

public class Rock : Hazard
{
    [Header("Rock Settings")]
    public float breakAnimationDuration = 0.5f;

    private Rigidbody2D rb;
    private Animator animator;


    private bool isBreaking = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBreaking) return;

        PlayerHealth player =
            collision.gameObject.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
            StartCoroutine(BreakRock());
        }
        else if (collision.gameObject.CompareTag("Ground"))
        {
            StartCoroutine(BreakRock());
        }
    }

    IEnumerator BreakRock()
    {
        isBreaking = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Break");
        }

        yield return new WaitForSeconds(
            breakAnimationDuration
        );

        Destroy(gameObject);
    }
}
