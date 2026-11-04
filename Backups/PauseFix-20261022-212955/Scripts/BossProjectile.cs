using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 6f;
    public float lifetime = 4f;
    public int damage = 1;
    private int direction = -1;
    private bool used;

    void Start() { Destroy(gameObject, lifetime); }

    public void SetDirection(int value)
    {
        direction = value >= 0 ? 1 : -1;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.flipX = direction < 0;
    }

    void Update()
    {
        transform.position += Vector3.right * (direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (used || other.GetComponentInParent<BossController>() != null) return;
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            used = true;
            health.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            used = true;
            Destroy(gameObject);
        }
    }
}
