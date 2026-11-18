using UnityEngine;

namespace Sakura.Trial
{
    // The enemies talk to stock Sakura components without adding a script to the hero.
    public sealed class TrialPlayer : MonoBehaviour
    {
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerHealth health;
        [SerializeField] Camera gameCamera;
        static TrialPlayer instance;

        public static TrialPlayer Current
        {
            get
            {
                if (instance != null) return instance;
                var hero = Object.FindAnyObjectByType<PlayerMovement>();
                if (hero == null) return null;
                var adapter = new GameObject("Enemy player reference");
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(adapter, hero.gameObject.scene);
                var bridge = adapter.AddComponent<TrialPlayer>();
                bridge.Bind(hero, Camera.main);
                return bridge;
            }
        }

        public Transform Target => movement != null ? movement.transform : null;
        public bool Alive => movement != null && movement.isActiveAndEnabled && health != null;
        public Camera GameCamera => gameCamera;

        public void Bind(PlayerMovement hero, Camera camera)
        {
            movement = hero;
            health = hero.GetComponent<PlayerHealth>();
            gameCamera = camera;
        }

        void Awake() => instance = this;
        void OnDestroy() { if (instance == this) instance = null; }
        public void Hit(int damage) { if (Alive) health.TakeDamage(damage); }
    }
}
