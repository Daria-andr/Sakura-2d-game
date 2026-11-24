using UnityEngine;

namespace Sakura.Trial
{
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class TrialEnemy : MonoBehaviour
    {
        [Header("Route and vision")]
        [SerializeField] Transform[] waypoints;
        [SerializeField] LayerMask solidTerrain = Physics2D.DefaultRaycastLayers;
        [SerializeField] int startingDirection = -1;
        [SerializeField, Min(0.1f)] float fallbackRouteHalfWidth = 2f;
        [SerializeField, Min(0.1f)] float visionRange = 6f;
        [SerializeField, Range(5f, 360f)] float visionAngle = 145f;
        [SerializeField, Min(0.05f)] float eyeHeight = 0.62f;
        [SerializeField, Min(0f)] float bodyHalfWidth = 0.36f;
        [SerializeField, Min(0.01f)] float footProbeUp = 0.35f;
        [SerializeField, Min(0.01f)] float footProbeDown = 0.65f;
        [SerializeField, Min(0f)] float meleeHeightAboveEye = 0.18f;

        [Header("Combat")]
        [SerializeField, Min(1)] int health = 2;
        [SerializeField, Min(1)] protected int damage = 1;
        [SerializeField, Range(0.3f, 0.8f)] protected float windupSeconds = 0.6f;
        [SerializeField, Min(0.1f)] protected float cooldownSeconds = 1.25f;
        [SerializeField, Min(0.05f)] float defeatSeconds = 0.4f;
        [SerializeField] Color warningColor = new Color(1f, 0.48f, 0.23f);
        [SerializeField, Min(0f)] float warningHeight = 1.35f;
        [SerializeField, Min(0.01f)] float hitFlashSeconds = 0.12f;

        protected int Facing { get; private set; }
        protected float Elapsed { get; private set; }
        protected TrialPlayer Player { get; private set; }
        protected Vector2 Eye => (Vector2)transform.position + Vector2.up * eyeHeight;
        protected SpriteRenderer Visual { get; private set; }
        public bool IsDefeated { get; private set; }
        public string CurrentState { get; private set; }

        Animator animator;
        SpriteRenderer warning;
        Color defaultColor;
        Vector2[] route;
        float leftLimit, rightLimit, flashEnd;
        int nextPoint;
        int remainingHealth;
        bool winding;
        Collider2D playerBody;
        Transform cachedTarget;
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        static Sprite warningSprite;

        public void Configure(Transform[] points, LayerMask walls, int initialFacing = -1)
        {
            waypoints = points;
            solidTerrain = walls;
            startingDirection = initialFacing;
            var body = GetComponent<BoxCollider2D>();
            body.isTrigger = true;
            body.size = new Vector2(0.95f, 0.95f);
            body.offset = new Vector2(0f, 0.5f);
        }

        protected virtual void Start()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            remainingHealth = health;
            Visual = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            defaultColor = Visual ? Visual.color : Color.white;
            Face(startingDirection);
            CaptureRoute();
            CreateWarning();
            BeginBehaviour();
        }

        protected abstract void BeginBehaviour();
        protected abstract void Step(float dt);

        void Update()
        {
            if (IsDefeated || Time.deltaTime <= 0f) return;
            Player = TrialPlayer.Current;
            if (!Player || !Player.Alive || !Player.Target) return;
            if (Player.Target != cachedTarget || !playerBody || !playerBody.enabled)
            {
                cachedTarget = Player.Target;
                playerBody = null;
                foreach (var col in cachedTarget.GetComponentsInChildren<Collider2D>())
                {
                    if (col.enabled && !col.isTrigger) { playerBody = col; break; }
                }
            }
            Elapsed += Time.deltaTime;
            Step(Time.deltaTime);
            if (Visual)
                Visual.color = winding ? Color.Lerp(defaultColor, warningColor, Mathf.PingPong(Elapsed * 5f, 1f)) :
                    Time.time < flashEnd ? Color.Lerp(defaultColor, Color.red, 0.65f) : defaultColor;
        }

        void CaptureRoute()
        {
            var points = new System.Collections.Generic.List<Vector2>();
            if (waypoints != null)
                foreach (var point in waypoints) if (point) points.Add(point.position);
            if (points.Count < 2)
            {
                points.Clear();
                points.Add((Vector2)transform.position + Vector2.left * fallbackRouteHalfWidth);
                points.Add((Vector2)transform.position + Vector2.right * fallbackRouteHalfWidth);
            }
            route = points.ToArray();
            leftLimit = rightLimit = transform.position.x;
            for (int i = 0; i < route.Length; i++)
            {
                leftLimit = Mathf.Min(leftLimit, route[i].x);
                rightLimit = Mathf.Max(rightLimit, route[i].x);
            }
            nextPoint = Facing < 0 ? 0 : route.Length - 1;
        }

        protected void Patrol(float speed, float dt)
        {
            if (MoveToward(route[nextPoint].x, speed * dt))
                nextPoint = (nextPoint + 1) % route.Length;
        }

        // Routes are horizontal platforms. Clamp pursuit and charges to their authored limits.
        protected bool MoveToward(float x, float maxStep)
        {
            var pos = transform.position;
            float dest = Mathf.Clamp(x, leftLimit, rightLimit);
            float next = Mathf.MoveTowards(pos.x, dest, maxStep);
            if (Mathf.Approximately(next, pos.x)) return true;
            int direction = next > pos.x ? 1 : -1;
            Face(direction);
            var end = Eye + Vector2.right * (next - pos.x + direction * bodyHalfWidth);
            if (Blocked(Eye, end)) return true;
            // Stop before the front foot goes beyond a ledge (also works on relocated prefabs).
            Vector2 foot = new Vector2(next + direction * bodyHalfWidth, pos.y);
            if (!Blocked(foot + Vector2.up * footProbeUp, foot + Vector2.down * footProbeDown)) return true;
            pos.x = next;
            transform.position = pos;
            return Mathf.Approximately(dest, next);
        }

        protected void Face(float direction)
        {
            if (Mathf.Approximately(direction, 0f)) return;
            Facing = direction > 0 ? 1 : -1;
            if (Visual) Visual.flipX = Facing > 0;
        }

        internal Bounds PlayerBounds
        {
            get
            {
                if (playerBody && playerBody.enabled) return playerBody.bounds;
                return new Bounds(Player.Target.position, new Vector3(0.5f, 0.9f, 1f));
            }
        }

        protected bool SeesPlayer()
        {
            Vector2 delta = (Vector2)PlayerBounds.center - Eye;
            return delta.sqrMagnitude <= visionRange * visionRange &&
                Vector2.Angle(Vector2.right * Facing, delta) <= visionAngle * 0.5f &&
                !Blocked(Eye, PlayerBounds.center);
        }

        internal bool Blocked(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(solidTerrain);
            int count = Physics2D.Raycast(from, delta.normalized, filter, hits, delta.magnitude);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i].collider;
                if (hit.transform.IsChildOf(transform)) continue;
                if (cachedTarget && hit.transform.IsChildOf(cachedTarget)) continue;
                return true;
            }
            return false;
        }

        internal bool InView(Vector2 point)
        {
            if (!Player || !Player.GameCamera) return false;
            Vector3 v = Player.GameCamera.WorldToViewportPoint(new Vector3(point.x, point.y, transform.position.z));
            return v.z > 0f && v.x >= 0 && v.x <= 1 && v.y >= 0 && v.y <= 1;
        }

        internal bool CanAttack()
        {
            return !IsDefeated && Player && Player.Alive && Player.Target && InView(Eye) && InView(PlayerBounds.center);
        }

        protected bool InMeleeReach(float reach) => InMeleeReach(Eye, reach);

        protected bool InMeleeReach(Vector2 from, float reach)
        {
            var box = PlayerBounds;
            float nearest = box.ClosestPoint(from).x;
            return (nearest - from.x) * Facing >= -bodyHalfWidth &&
                Mathf.Abs(nearest - from.x) <= reach && box.min.y <= from.y + meleeHeightAboveEye &&
                box.max.y >= transform.position.y && !Blocked(from, box.center);
        }

        protected void Enter(string state, string animationState, bool telegraph = false)
        {
            if (telegraph) SakuraAudio.Play(SakuraAudio.Cue.Windup);
            if (animationState == "Attack") SakuraAudio.Play(SakuraAudio.Cue.Attack);
            CurrentState = state;
            Elapsed = 0;
            winding = telegraph;
            if (warning) warning.enabled = telegraph;
            if (animator && animator.runtimeAnimatorController)
                animator.Play(animationState, 0, 0f);
        }

        void CreateWarning()
        {
            if (!Visual) return;
            if (!warningSprite)
            {
                string[] rows = { ".###.", "#yyy#", "#yyy#", "#yyy#", ".###.", ".....", ".###.", "#yyy#", ".###." };
                var texture = new Texture2D(5, 9, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                {
                    char px = rows[y][x];
                    texture.SetPixel(x, rows.Length - 1 - y, px == 'y' ? new Color32(255, 215, 91, 255) :
                        px == '#' ? new Color32(77, 32, 46, 255) : new Color32(0, 0, 0, 0));
                }
                texture.Apply(false, true);
                warningSprite = Sprite.Create(texture, new Rect(0, 0, 5, 9), Vector2.one * 0.5f, 16f, 0, SpriteMeshType.FullRect);
            }
            var obj = new GameObject("Attack warning");
            obj.transform.SetParent(transform, false);
            obj.transform.localPosition = Vector3.up * warningHeight;
            warning = obj.AddComponent<SpriteRenderer>();
            warning.sprite = warningSprite;
            warning.sortingLayerID = Visual.sortingLayerID;
            warning.sortingOrder = Visual.sortingOrder + 2;
            warning.enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsDefeated || !other.enabled || !other.gameObject.activeInHierarchy) return;
            Bullet bullet = other.GetComponentInParent<Bullet>();
            if (!bullet || !bullet.gameObject.activeInHierarchy) return;
            int amount = bullet.damage;
            bullet.gameObject.SetActive(false);
            Destroy(bullet.gameObject);
            SakuraAudio.Play(SakuraAudio.Cue.Hit);
            remainingHealth -= Mathf.Max(0, amount);
            flashEnd = Time.time + hitFlashSeconds;
            if (remainingHealth > 0) return;
            IsDefeated = true;
            SakuraAudio.Play(SakuraAudio.Cue.Death);
            GetComponent<BoxCollider2D>().enabled = false;
            Enter("Defeated", animator && animator.HasState(0, Animator.StringToHash("Base Layer.Defeat")) ? "Defeat" : "Recover");
            if (Visual) Visual.color = defaultColor;
            Destroy(gameObject, defeatSeconds);
        }
    }
}
