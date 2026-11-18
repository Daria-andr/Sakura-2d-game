
using UnityEngine;

public class HeartPickup : Hazard
{
    protected override void Awake() { base.Awake(); LevelBalance.Apply(this); }
    public int healAmount;
    private bool collected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        PlayerHealth player =
            other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            collected = true;
            player.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}
