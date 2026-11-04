using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossController : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHealth = 8;
    public Slider healthBar;
    [Min(0)] public int contactDamage = 1;
    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    [Min(0.2f)] public float attackInterval = 2.5f;
    [Min(0)] public float attackAnimationDelay = 0.3f;
    [Header("Phase 2")]
    [Range(0.1f, 0.9f)] public float phaseTwoHealthRatio = 0.5f;
    [Min(0.2f)] public float phaseTwoAttackInterval = 1.25f;
    [Min(1f)] public float phaseTwoProjectileSpeedMultiplier = 1.3f;
    public Color phaseTwoTint = new Color(1f, 0.65f, 0.65f);
    public bool IsPhaseTwo { get; private set; }
    private Transform target;
    private SpriteRenderer sprite;
    private float AttackInterval => IsPhaseTwo ? phaseTwoAttackInterval : attackInterval;
    [Header("Death")]
    [Min(0)] public float deathAnimationDuration = 0.8f;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    private float nextAttackTime;
    private bool attacking;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    void Awake()
    {
        CurrentHealth = Mathf.Max(1, maxHealth);
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        var player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) target = player.transform;
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
        if (healthBar != null)
        {
            healthBar.minValue = 0;
            healthBar.maxValue = CurrentHealth;
            healthBar.value = CurrentHealth;
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.constraints |= RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void OnEnable() => nextAttackTime = Time.time + Mathf.Max(0.2f, AttackInterval);

    void Update()
    {
        if (IsDead || attacking || Time.timeScale == 0 || Time.time < nextAttackTime)
            return;
        if (projectilePrefab == null || shootPoint == null) return;
        nextAttackTime = Time.time + Mathf.Max(0.2f, AttackInterval);
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        attacking = true;
        Trigger("Attack");
        yield return new WaitForSeconds(Mathf.Max(0, attackAnimationDelay));
        if (!IsDead && projectilePrefab != null && shootPoint != null)
        {
            var projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);
            if (projectile.TryGetComponent<BossProjectile>(out var shot))
            {
                if (target != null)
                {
                    var hitbox=target.GetComponent<Collider2D>();
                    shot.AimAt(hitbox!=null ? (Vector2)hitbox.bounds.center : (Vector2)target.position);
                }
                else shot.SetDirection(1);
                if (IsPhaseTwo) shot.speed *= Mathf.Max(1f, phaseTwoProjectileSpeedMultiplier);
            }
        }
        attacking = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        if (healthBar != null) healthBar.value = CurrentHealth;
        if (CurrentHealth > 0)
        {
            if (!IsPhaseTwo && CurrentHealth <= Mathf.Max(1, maxHealth) * phaseTwoHealthRatio)
            {
                IsPhaseTwo = true;
                if (animator != null && animator.runtimeAnimatorController != null)
                    foreach (var parameter in animator.parameters)
                        if (parameter.name == "Phase2" && parameter.type == AnimatorControllerParameterType.Bool)
                            animator.SetBool("Phase2", true);
                nextAttackTime = Mathf.Min(nextAttackTime, Time.time + Mathf.Max(0.2f, AttackInterval));
            }
            if (!attacking) Trigger("Hit");
            return;
        }

        IsDead = true;
        StopAllCoroutines();
        attacking = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
        foreach (var col in colliders)
            if (col != null) col.enabled = false;
        Trigger("Death");
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(Mathf.Max(0, deathAnimationDuration));
        if (GameManager.Instance != null) GameManager.Instance.BossKilled();
        Destroy(gameObject);
    }

    // Animation is optional; combat also works before a controller is assigned.
    void Trigger(string parameter)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var item in animator.parameters)
            if (item.name == parameter && item.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(parameter);
                return;
            }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        attacking = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return;
        var health = collision.gameObject.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(contactDamage);
    }
}