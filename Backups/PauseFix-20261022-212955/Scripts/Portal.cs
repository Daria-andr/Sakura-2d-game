
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public bool isOpen = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool playerEntered = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        if (animator == null)
        {
            Debug.LogError("Portal: Animator not found!");
        }
    }

    void Start()
    {
        if (isOpen)
        {
            OpenPortal();
        }
    }

    public void OpenPortal()
    {
        if (playerEntered) return;

        isOpen = true;

        Debug.Log("PORTAL OPENED!");

        if (animator != null)
        {
            animator.SetTrigger("Open");

            Debug.Log("OPEN ANIMATION STARTED!");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isOpen || playerEntered)
            return;

        playerEntered = true;

        Debug.Log("PLAYER ENTERED PORTAL!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishLevel();
        }
    }
}
