
using UnityEngine;

public class HeartPickup : Hazard
{
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            collected = true;
            player.Heal(1);
            Destroy(gameObject);
        }
    }
}
