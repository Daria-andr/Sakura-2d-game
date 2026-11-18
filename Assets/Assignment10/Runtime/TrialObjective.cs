using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Sakura.Trial
{
    [DefaultExecutionOrder(100)]
    public sealed class TrialObjective : MonoBehaviour
    {
        [SerializeField] TrialPlayer player;
        [SerializeField] TrialEnemy[] guards;
        [SerializeField] TrialGate gate;
        [SerializeField] Vector2 exit;
        [SerializeField] Vector2 doorHalfSize = new Vector2(.8f, 1.25f);
        [SerializeField] GameManager manager;
        [SerializeField] TMP_Text objectiveText;
        [SerializeField] TMP_Text counterText;
        [SerializeField] float fallLimit = -5f;
        int previousCount = -1;
        bool finished;

        public void Configure(TrialPlayer hero, TrialEnemy[] enemies, TrialGate door, Vector2 finish,
                              GameManager game, TMP_Text objective, TMP_Text counter)
        {
            player = hero;
            guards = enemies;
            gate = door;
            exit = finish;
            manager = game;
            objectiveText = objective;
            counterText = counter;
        }

        void Start()
        {
            PrepareResult(manager.winPanel, "COURTYARD CLEARED", "PLAY AGAIN");
            PrepareResult(manager.gameOverPanel, "TRY AGAIN", "RETRY");
            UpdateObjective();
        }

        void PrepareResult(GameObject panel, string title, string action)
        {
            var view = panel.GetComponent<VictoryPanelView>();
            view.titleText.text = title;
            view.primaryButtonText.text = action;
            view.primaryButton.onClick.RemoveAllListeners();
            view.primaryButton.onClick.AddListener(Retry);
            view.menuButton.gameObject.SetActive(Application.CanStreamedLevelBeLoaded("MainMenu"));
        }

        void Update()
        {
            if (finished || !player.Alive || Time.timeScale <= 0f) return;
            UpdateObjective();
            Vector2 pos = player.Target.position;
            if (pos.y < fallLimit)
                player.Target.GetComponent<PlayerHealth>().InstantKill();
            else if (previousCount == 0 && Mathf.Abs(pos.x - exit.x) <= doorHalfSize.x &&
                     Mathf.Abs(pos.y - exit.y) <= doorHalfSize.y)
            {
                finished = true;
                manager.FinishLevel();
            }
        }

        void UpdateObjective()
        {
            int count = 0;
            foreach (var guard in guards)
                if (guard != null && !guard.IsDefeated) count++;
            if (count == previousCount) return;
            previousCount = count;
            objectiveText.text = count == 0 ? "Reach the open gate  →" : "Defeat the guards";
            counterText.text = count == 0 ? "GATE OPEN" : $"GUARDS   {guards.Length - count} / {guards.Length}";
            if (count == 0) gate.Open();
        }

        public void Retry()
        {
            Time.timeScale = 1f;
            string path = gameObject.scene.path;
#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(path))
            {
                UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
                return;
            }
#endif
            SceneManager.LoadScene(path);
        }
    }
}
