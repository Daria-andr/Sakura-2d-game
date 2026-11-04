
using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float invulnerabilityTime = 2f;
    public float deathDelay = 1.2f;

    public TextMeshProUGUI healthText;

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerMovement movement;
    private Animator animator;
    public HeartUI heartUI;
    void Start()
    {
        currentHealth = maxHealth;
        if (heartUI != null)
{
    heartUI.Initialize(maxHealth);
}

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || isDead)
            return;
            
        Debug.Log("BEFORE DAMAGE: " + currentHealth);
        Debug.Log("DAMAGE AMOUNT: " + damage);

        currentHealth = Mathf.Max(
            0, currentHealth - damage
            
        );Debug.Log("AFTER DAMAGE: " + currentHealth);

        

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
                animator.SetTrigger("Damage");

            StartCoroutine(Invulnerability());
        }
    }

    public void InstantKill()
{
    if (isDead) return;

    currentHealth = 0;
    UpdateHealthUI();
    Die();
}
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(
            maxHealth,
            currentHealth + amount
        );

        UpdateHealthUI();

        Debug.Log("Player healed! HP: " + currentHealth);
    }


    void UpdateHealthUI()
{
    Debug.Log("PlayerHealth updating UI");

    if (heartUI != null)
    {
        heartUI.UpdateHearts(currentHealth);
    }
    else
    {
        Debug.LogError("HeartUI is not assigned!");
    }
}

    
IEnumerator Invulnerability()
{
    isInvulnerable = true;

    float timer = 0f;

    while (timer < invulnerabilityTime)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled =
                !spriteRenderer.enabled;
        }

        yield return new WaitForSeconds(0.15f);
        timer += 0.15f;
    }

    if (spriteRenderer != null)
    {
        spriteRenderer.enabled = true;
    }

    isInvulnerable = false;
}


    void Die()
    {
        if (isDead) return;

        isDead = true;
        StopAllCoroutines();

        if (spriteRenderer != null)
    spriteRenderer.enabled = true;

if (movement != null)
    movement.enabled = false;

if (rb != null)
    rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.ResetTrigger("Damage");
            animator.SetTrigger("Death");
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
