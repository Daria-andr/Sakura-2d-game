
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public bool isOpen = false;
    [Header("Audio")]
    public AudioClip portalSound;
    [Range(0f, 1f)] public float portalVolume = 0.7f;
    private AudioSource portalAudio;
    private bool openingSoundPlayed;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool playerEntered = false;

    void Awake()
    {
        portalAudio = GetComponent<AudioSource>();
        if (portalAudio == null) portalAudio = gameObject.AddComponent<AudioSource>();
        portalAudio.playOnAwake = false;
        portalAudio.spatialBlend = 0f;
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
        if (!openingSoundPlayed && portalSound != null)
        {
            openingSoundPlayed = true;
            portalAudio.PlayOneShot(portalSound, portalVolume);
        }

        if (animator != null)
        {
            animator.SetTrigger("Open");

        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isOpen || playerEntered)
            return;

        playerEntered = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishLevel();
        }
    }
}
