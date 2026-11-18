using UnityEngine;

namespace Sakura.Trial
{
    public sealed class CrateThrower : TrialEnemy
    {
        enum Mode { Watch, Aim, Release, Reload }
        [SerializeField] Sprite crateSprite;
        [SerializeField] Vector2 handOffset = new Vector2(0.30f, 0.95f);
        [SerializeField, Min(0.1f)] float flightSeconds = 0.90f;
        [SerializeField, Min(0.05f)] float arcHeight = 1.3f;
        [SerializeField, Min(0.05f)] float crateRadius = 0.18f;
        [SerializeField, Min(0.05f)] float crateScale = 0.64f;
        [SerializeField, Min(0.05f)] float releaseSeconds = 0.27f;
        [SerializeField] float rotationPerSecond = 250f;
        [SerializeField, Min(0.25f)] float lookAroundSeconds = 2.5f;
        Mode mode;
        Vector2 aim;

        public void SetCrateSprite(Sprite sprite) => crateSprite = sprite;
        protected override void BeginBehaviour() => Switch(Mode.Watch, "Idle");

        void Switch(Mode next, string clip)
        {
            mode = next;
            Enter(next.ToString(), clip, next == Mode.Aim);
        }

        protected override void Step(float dt)
        {
            switch (mode)
            {
                case Mode.Watch:
                    if (crateSprite && CanAttack() && SeesPlayer())
                    {
                        aim = PlayerBounds.center;
                        Face(aim.x - Eye.x);
                        Switch(Mode.Aim, "Windup");
                    }
                    else if (Elapsed >= lookAroundSeconds)
                    {
                        Face(-Facing);
                        Switch(Mode.Watch, "Idle");
                    }
                    break;
                case Mode.Aim:
                    if (!CanAttack()) Switch(Mode.Reload, "Recover");
                    else if (Elapsed >= windupSeconds)
                    {
                        var origin = (Vector2)transform.position + new Vector2(handOffset.x * Facing, handOffset.y);
                        if (!Blocked(Eye, origin) && InView(origin))
                        {
                            var crate = new GameObject("Thrown crate");
                            crate.transform.position = origin;
                            crate.transform.localScale = Vector3.one * crateScale;
                            var image = crate.AddComponent<SpriteRenderer>();
                            image.sprite = crateSprite;
                            image.sortingLayerID = Visual.sortingLayerID;
                            image.sortingOrder = Visual.sortingOrder + 1;
                            crate.AddComponent<TrialCrate>().Launch(this, Player, aim, flightSeconds, arcHeight,
                                crateRadius, -Facing * rotationPerSecond, damage);
                        }
                        Switch(Mode.Release, "Attack");
                    }
                    break;
                case Mode.Release:
                    if (Elapsed >= releaseSeconds) Switch(Mode.Reload, "Recover");
                    break;
                case Mode.Reload:
                    if (Elapsed >= cooldownSeconds) Switch(Mode.Watch, "Idle");
                    break;
            }
        }
    }
}
