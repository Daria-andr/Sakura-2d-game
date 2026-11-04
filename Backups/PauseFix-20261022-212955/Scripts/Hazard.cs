
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public int damage = 1;
    public float lifetime = 10f;

    protected virtual void Start()
    {
        Destroy(gameObject, lifetime);
    }

    protected void DamagePlayer(Collider2D target)
    {
        PlayerHealth player =
            target.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
