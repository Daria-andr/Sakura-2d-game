using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class BossController : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHealth;
    public Slider healthBar;
    [Min(0)] public int contactDamage;
    [Header("Attack")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    [Min(0.2f)] public float attackInterval;
    [Min(0)] public float attackAnimationDelay;
    [Header("Phase 2")]
    [Range(0.1f, 0.9f)] public float phaseTwoHealthRatio;
    [Min(0.2f)] public float phaseTwoAttackInterval;
    [Min(1f)] public float phaseTwoProjectileSpeedMultiplier;
    public Color phaseTwoTint = new Color(1f, 0.65f, 0.65f);
    public bool IsPhaseTwo { get; private set; }
    private Transform target;
    private SpriteRenderer sprite;
    private float AttackInterval => IsPhaseTwo ? phaseTwoAttackInterval : attackInterval;
    [Header("Death")]
    [Min(0)] public float deathAnimationDuration;

    public int CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }
    private float nextAttackTime;
    private bool attacking;
    private bool contactAttacking;
    private float contactCooldown;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D[] colliders;

    void Awake() {
        LevelBalance.Apply(this);
        CurrentHealth = Mathf.Max(1, maxHealth);
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        var player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) target = player.transform;
        rb = GetComponent<Rigidbody2D>();
        var box = GetComponent<BoxCollider2D>();
        if (box != null && sprite != null && sprite.sprite != null)
        {
            var size = box.size;
            size.y = Mathf.Max(size.y, sprite.sprite.bounds.size.y * 0.9f);
            box.size = size;
        }
        colliders = GetComponentsInChildren<Collider2D>();
#if UNITY_EDITOR
        System.IO.Directory.CreateDirectory("Logs");
        System.IO.File.WriteAllText("Logs/BossDamageTrace.txt", $"BOSS {name} pos={transform.position} HP={CurrentHealth} active={isActiveAndEnabled}\n");
        foreach(var col in colliders) System.IO.File.AppendAllText("Logs/BossDamageTrace.txt", $"COLLIDER enabled={col.enabled} bounds={col.bounds}\n");
#endif
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
        SakuraAudio.Play(SakuraAudio.Cue.Windup);
        Trigger("Attack");
        yield return new WaitForSeconds(Mathf.Max(.45f, attackAnimationDelay));
        if (!IsDead && projectilePrefab != null && shootPoint != null)
        {
            SakuraAudio.Play(SakuraAudio.Cue.Attack);
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
        SakuraAudio.Play(SakuraAudio.Cue.Hit);
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
        SakuraAudio.Play(SakuraAudio.Cue.Death);
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

    IEnumerator ContactAttack(PlayerHealth health) {
        contactAttacking = true;
        SakuraAudio.Play(SakuraAudio.Cue.Windup);
        yield return new WaitForSeconds(.45f);
        if(!IsDead && health && health.TryGetComponent<Collider2D>(out var body))
            foreach(var col in colliders) if(col && col.IsTouching(body)) {
                SakuraAudio.Play(SakuraAudio.Cue.Attack); health.TakeDamage(contactDamage); break;
            }
        contactAttacking = false; contactCooldown = Time.time + .8f;
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return;
        var health = collision.gameObject.GetComponentInParent<PlayerHealth>();
        if (health != null && !contactAttacking && Time.time >= contactCooldown) StartCoroutine(ContactAttack(health));
    }
}