using UnityEngine;

namespace Sakura.Trial
{
    public sealed class BatPatroller : TrialEnemy
    {
        enum Mode { Patrol, Pursuit, Windup, Swing, Recovery }
        [SerializeField, Min(0.1f)] float patrolSpeed = 1.1f;
        [SerializeField, Min(0.1f)] float chaseSpeed = 2.45f;
        [SerializeField, Min(0.1f)] float batReach = 0.95f;
        [SerializeField, Min(0.05f)] float swingSeconds = 0.28f;
        [SerializeField, Min(0f)] float loseSightDelay = 0.65f;
        Mode mode;
        float lastSeenX, lostFor;

        protected override void BeginBehaviour() => Switch(Mode.Patrol, "Run");

        void Switch(Mode next, string clip)
        {
            mode = next;
            Enter(next.ToString(), clip, next == Mode.Windup);
        }

        protected override void Step(float dt)
        {
            switch (mode)
            {
                case Mode.Patrol:
                    if (SeesPlayer())
                    {
                        lastSeenX = PlayerBounds.center.x;
                        lostFor = 0;
                        Switch(Mode.Pursuit, "Run");
                    }
                    else Patrol(patrolSpeed, dt);
                    break;
                case Mode.Pursuit:
                    if (SeesPlayer())
                    {
                        lastSeenX = PlayerBounds.center.x;
                        lostFor = 0;
                    }
                    else lostFor += dt;
                    if (lostFor > loseSightDelay) Switch(Mode.Patrol, "Run");
                    else if (CanAttack() && InMeleeReach(batReach)) Switch(Mode.Windup, "Windup");
                    else MoveToward(lastSeenX, chaseSpeed * dt);
                    break;
                case Mode.Windup:
                    if (!CanAttack()) Switch(Mode.Recovery, "Recover");
                    else if (Elapsed >= windupSeconds)
                    {
                        // Resolve once, in the locked direction. Jumping or stepping back avoids it.
                        if (InMeleeReach(batReach)) Player.Hit(damage);
                        Switch(Mode.Swing, "Attack");
                    }
                    break;
                case Mode.Swing:
                    if (Elapsed >= swingSeconds) Switch(Mode.Recovery, "Recover");
                    break;
                case Mode.Recovery:
                    if (Elapsed >= cooldownSeconds) Switch(Mode.Patrol, "Run");
                    break;
            }
        }
    }
}
