using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Portal portal;
    public BossController boss;
    public TextMeshProUGUI enemyText;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject pausePanel;

    private int enemiesLeft;
    private int score;
    private bool levelFinished;
    private PlayerMovement player;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        if (pausePanel != null) pausePanel.SetActive(false);
        PreparePanel(winPanel, false);
        PreparePanel(gameOverPanel, true);

        enemiesLeft = FindObjectsByType<Enemy>().Length;
        if (boss == null) boss = FindAnyObjectByType<BossController>();
        UpdateEnemyUI();

        if (enemiesLeft == 0 && boss == null) OpenPortal();
    }

    void PreparePanel(GameObject panel, bool defeat)
    {
        if (panel == null) return;
        panel.SetActive(false);
        VictoryPanelView view = panel.GetComponent<VictoryPanelView>();
        if (view != null) view.Prepare(this, defeat);
    }

    // Основа из лекции 8–9: показываем панель и останавливаем игровое время.
    public void SetPaused(bool paused)
    {
        if (levelFinished) return;
        if (paused) SaveSystem.Save();
        if (pausePanel != null) pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;

        // После паузы не должны оставаться удержанные команды движения.
        if (player != null) player.ClearInput();
        foreach (MobileController control in FindObjectsByType<MobileController>())
        {
            control.ResetControl();
        }
    }

    public void EnemyKilled()
    {
        if (levelFinished) return;
        enemiesLeft = Mathf.Max(0, enemiesLeft - 1);
        score += LevelContext.Current != null ? LevelContext.Current.enemyScore : 0;
        UpdateEnemyUI();
        if (enemiesLeft == 0 && boss == null) OpenPortal();
    }

    public void BossKilled()
    {
        if (levelFinished) return;
        score += LevelContext.Current != null ? LevelContext.Current.bossScore : 0;
        UpdateEnemyUI();
        FinishLevel();
    }

    void UpdateEnemyUI()
    {
        if (enemyText != null) enemyText.text = "Monsters: " + enemiesLeft;
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    void OpenPortal()
    {
        if (portal != null) portal.OpenPortal();
    }

    public void GameOver()
    {
        if (levelFinished) return;
        EndLevel(gameOverPanel);
    }

    public void FinishLevel()
    {
        if (levelFinished) return;
        if (LevelContext.Current != null) SaveSystem.Complete(LevelContext.Current.levelIndex, score);
        EndLevel(winPanel);
    }

    void EndLevel(GameObject panel)
    {
        levelFinished = true;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (panel != null) panel.SetActive(true);
        if (player != null) player.ClearInput();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueGame()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Level1")
        {
            Time.timeScale = 1f;
            SaveSystem.PlayLevel(1);
        }
        else if (sceneName == "Level2")
        {
            Time.timeScale = 1f;
            SaveSystem.PlayLevel(2);
        }
    }

    // Как в лекции: перед переходом в меню возвращаем обычную скорость времени.
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
