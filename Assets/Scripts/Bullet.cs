
using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;

    [Header("Hit Animation")]
    public float hitAnimationDuration = 0.4f;

    private int direction = 1;
    private bool hasHit = false;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D bulletCollider;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        bulletCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
#if UNITY_EDITOR
        System.IO.Directory.CreateDirectory("Logs");
        System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"SPAWN pos={transform.position} direction={direction} speed={speed} damage={damage}\n");
#endif
    }

    public void SetDirection(int newDirection)
    {
        direction = newDirection >= 0 ? 1 : -1;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.flipX = direction < 0;
    }

    void FixedUpdate()
    {
        if (hasHit) return;
        foreach (var hit in Physics2D.CircleCastAll(transform.position, 0.06f, Vector2.right * direction, Mathf.Max(0,speed) * Time.fixedDeltaTime))
        {
            if (hit.collider == bulletCollider || hit.collider.transform.IsChildOf(transform)) continue;
            OnTriggerEnter2D(hit.collider);
            if (hasHit) return;
        }
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                direction * speed, 0f
            );
        }
        else
        {
            transform.position +=
                Vector3.right * direction * speed *
                Time.fixedDeltaTime;
        }
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        System.IO.Directory.CreateDirectory("Logs");
        System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"END pos={transform.position} hit={hasHit}\n");
#endif
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.GetComponentInParent<PlayerMovement>() != null)
            return;

        BossController boss =
            other.GetComponentInParent<BossController>();

        if (boss != null)
        {

#if UNITY_EDITOR
            System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"BOSS HIT {boss.name} bullet={transform.position} HP before={boss.CurrentHealth} damage={damage}\n");
#endif
            boss.TakeDamage(damage);
#if UNITY_EDITOR
            System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"HP after={boss.CurrentHealth} slider={(boss.healthBar!=null?boss.healthBar.value:-1)}\n");
#endif
            StartCoroutine(PlayHitAnimation());
            return;
        }

        Enemy enemy =
            other.GetComponentInParent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            StartCoroutine(PlayHitAnimation());
            return;
        }

        if (other.CompareTag("Ground"))
        {
#if UNITY_EDITOR
            System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"GROUND HIT {other.name} bullet={transform.position} bounds={other.bounds}\n");
#endif
            StartCoroutine(PlayHitAnimation());
        }
    }

    IEnumerator PlayHitAnimation()
    {
        hasHit = true;

        if (bulletCollider != null)
            bulletCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        yield return new WaitForSeconds(
            hitAnimationDuration
        );

        Destroy(gameObject);
    }
}
