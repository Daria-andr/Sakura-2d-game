using UnityEngine;

namespace Sakura.Trial
{
    public sealed class TrialCrate : MonoBehaviour
    {
        TrialEnemy owner;
        TrialPlayer player;
        Vector2 origin, destination;
        float elapsed, duration, apex, radius, spin;
        int damage;

        public void Launch(TrialEnemy source, TrialPlayer target, Vector2 aim, float flightSeconds,
            float arcHeight, float hitRadius, float degreesPerSecond, int amount)
        {
            owner = source;
            player = target;
            origin = transform.position;
            destination = aim;
            duration = flightSeconds;
            apex = arcHeight;
            radius = hitRadius;
            spin = degreesPerSecond;
            damage = amount;
        }

        void Update()
        {
            if (Time.deltaTime <= 0f) return;
            if (!owner || !owner.CanAttack()) { Destroy(gameObject); return; }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector2 next = Vector2.Lerp(origin, destination, t) + Vector2.up * (4f * apex * t * (1f - t));
            Vector2 last = transform.position;
            if (!owner.InView(next) || owner.Blocked(last, next)) { Destroy(gameObject); return; }

            Bounds bounds = owner.PlayerBounds;
            bounds.Expand(new Vector3(radius * 2, radius * 2, 0));
            // Sweep through the player's 2D bounds. Render depth is independent of collisions.
            Vector3 start = new Vector3(last.x, last.y, bounds.center.z);
            Vector2 delta = next - last;
            if (bounds.Contains(start) || (delta.sqrMagnitude > 0f &&
                bounds.IntersectRay(new Ray(start, delta.normalized), out float distance) && distance <= delta.magnitude))
            {
                player.Hit(damage);
                Destroy(gameObject);
                return;
            }
            transform.position = next;
            transform.Rotate(0f, 0f, spin * Time.deltaTime);
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
