using UnityEngine;

namespace Sakura.Trial
{
    public sealed class CrownCharger : TrialEnemy
    {
        enum Mode { Guard, Brace, Charge, Rest }
        [SerializeField, Min(0.1f)] float guardSpeed = 0.72f;
        [SerializeField, Min(0.1f)] float chargeSpeed = 6.2f;
        [SerializeField, Min(0.1f)] float chargeDistance = 4.1f;
        [SerializeField, Min(0.1f)] float chargeReach = 0.60f;
        [SerializeField, Min(0.1f)] float laneHeight = 1.05f;
        Mode mode;
        float destination;

        protected override void BeginBehaviour() => Switch(Mode.Guard, "Run");

        void Switch(Mode next, string clip)
        {
            mode = next;
            Enter(next.ToString(), clip, next == Mode.Brace);
        }

        protected override void Step(float dt)
        {
            switch (mode)
            {
                case Mode.Guard:
                    if (CanAttack() && SeesPlayer() && Mathf.Abs(PlayerBounds.center.y - Eye.y) <= laneHeight)
                    {
                        Face(PlayerBounds.center.x - Eye.x);
                        destination = transform.position.x + Facing * chargeDistance;
                        Switch(Mode.Brace, "Windup");
                    }
                    else Patrol(guardSpeed, dt);
                    break;
                case Mode.Brace:
                    if (!CanAttack()) Switch(Mode.Rest, "Recover");
                    else if (Elapsed >= windupSeconds) Switch(Mode.Charge, "Attack");
                    break;
                case Mode.Charge:
                    if (!CanAttack()) { Switch(Mode.Rest, "Recover"); break; }
                    // Check the whole step so a fast dash cannot tunnel past a player or through cover.
                    Vector2 previousEye = Eye;
                    float step = Mathf.Min(chargeSpeed * dt, Mathf.Abs(destination - transform.position.x));
                    bool reachedEnd = MoveToward(destination, step);
                    float travelled = Mathf.Abs(Eye.x - previousEye.x);
                    if (CanAttack() && InMeleeReach(previousEye, chargeReach + travelled))
                    {
                        Player.Hit(damage);
                        Switch(Mode.Rest, "Recover");
                    }
                    else if (reachedEnd) Switch(Mode.Rest, "Recover");
                    break;
                case Mode.Rest:
                    if (Elapsed >= cooldownSeconds) Switch(Mode.Guard, "Run");
                    break;
            }
        }
    }
}
