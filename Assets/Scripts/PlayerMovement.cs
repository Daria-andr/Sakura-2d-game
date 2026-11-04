
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform bulletPoint;
    public float shootCooldown = 0.4f;
    public float bulletSpawnDelay = 0.25f;

    [Header("Sound Settings")]
    public AudioClip shootSound;

    [Range(0f, 1f)]
    public float shootVolume = 0.7f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private float moveInput;
    private float nextShootTime;

    private bool isGrounded;
    private bool facingRight = true;

    // Для мобильного контроллера позже
    private float mobileMove;
    private bool mobileJump;
    private bool mobileShoot;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    void Update()
    {
        // Update работает даже при остановленном времени. Не принимаем команды на паузе.
        if (Time.timeScale == 0f)
        {
            ClearInput();
            return;
        }

        // Клавиатура через новый Input System
        float keyboardMove = 0f;
        bool keyboardJump = false;
        bool keyboardShoot = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                keyboardMove -= 1f;
            }

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                keyboardMove += 1f;
            }

            keyboardJump =
                Keyboard.current.spaceKey.wasPressedThisFrame;

            keyboardShoot =
                Keyboard.current.fKey.wasPressedThisFrame;
        }

        // Объединяем клавиатуру и мобильное управление
        moveInput = Mathf.Clamp(
            keyboardMove + mobileMove, -1f, 1f
        );

        // Проверка земли
        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundRadius,
                groundLayer
            );

        // Поворот персонажа
        if (moveInput > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (moveInput < -0.1f && facingRight)
        {
            Flip();
        }

        // Анимации
        if (animator != null)
        {
            animator.SetFloat(
                "Speed", Mathf.Abs(moveInput)
            );

            animator.SetBool(
                "IsGrounded", isGrounded
            );
        }

        // Прыжок
        if ((keyboardJump || mobileJump) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        mobileJump = false;

        // Стрельба
        if (keyboardShoot || mobileShoot)
        {
            Shoot();
        }

        mobileShoot = false;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    void Flip()
    {
        facingRight = !facingRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !facingRight;
        }

        // BulletPoint поворачивается вместе с игроком
        if (bulletPoint != null)
        {
            Vector3 pos = bulletPoint.localPosition;
            pos.x = -pos.x;
            bulletPoint.localPosition = pos;
        }
    }

    void Shoot()
    {
        if (Time.time < nextShootTime)
            return;

        if (bulletPrefab == null || bulletPoint == null)
        {
            Debug.LogWarning(
                "Assign Bullet Prefab and Bullet Point!"
            );
            return;
        }

        nextShootTime = Time.time + shootCooldown;

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        int direction = facingRight ? 1 : -1;

        StartCoroutine(ShootWithDelay(direction));
    }

    IEnumerator ShootWithDelay(int direction)
    {
        yield return new WaitForSeconds(
            bulletSpawnDelay
        );

        if (!isActiveAndEnabled)
            yield break;

        GameObject bullet = Instantiate(
            bulletPrefab,
            bulletPoint.position,
            Quaternion.identity
        );

        Bullet bulletScript =
            bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }

        if (shootSound != null)
        {
            audioSource.PlayOneShot(
                shootSound, shootVolume
            );
        }
    }


    public void SetMobileMove(float value)
    {
        mobileMove = Mathf.Clamp(value, -1f, 1f);
    }

    public void MobileJump()
    {
        mobileJump = true;
    }

    public void MobileShoot()
    {
        mobileShoot = true;
    }

    void OnDisable()
    {
        ClearInput();
    }

    public void ClearInput()
    {
        mobileMove = 0f;
        mobileJump = false;
        mobileShoot = false;
        moveInput = 0f;
        StopAllCoroutines();
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundRadius
        );
    }
}

