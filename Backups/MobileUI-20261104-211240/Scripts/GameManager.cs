
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Portal portal;
    public BossController boss;

    public TextMeshProUGUI enemyText;

    public GameObject gameOverPanel;
    public GameObject winPanel;

    private int enemiesLeft;
    private bool levelFinished = false;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (winPanel != null)
        {
            var view = winPanel.GetComponent<VictoryPanelView>();
            if (view == null) view = winPanel.AddComponent<VictoryPanelView>();
            view.Prepare(this);
        }
        if (gameOverPanel != null)
        {
            var view = gameOverPanel.GetComponent<VictoryPanelView>();
            if (view == null) view = gameOverPanel.AddComponent<VictoryPanelView>();
            view.Prepare(this, true);
        }
        enemiesLeft = FindObjectsByType<Enemy>().Length;

        UpdateEnemyUI();

        if (boss == null)
        {
            boss = FindAnyObjectByType<BossController>();
        }
        
        if (boss != null && boss.healthBar != null)
        {
            var hud = boss.healthBar.GetComponent<BossHealthBarView>();
            if (hud == null) hud = boss.healthBar.gameObject.AddComponent<BossHealthBarView>();
            hud.Prepare(boss);
        }
        if (enemiesLeft == 0 && boss == null)
        {
            OpenPortal();
        }
    }

    public void EnemyKilled()
    {
        if (levelFinished) return;

        enemiesLeft = Mathf.Max(0, enemiesLeft - 1);

        UpdateEnemyUI();

        if (enemiesLeft == 0 && boss == null)
            OpenPortal();
    }

    public void BossKilled()
{
    if (levelFinished) return;

    FinishLevel();
}

    void UpdateEnemyUI()
    {
        if (enemyText != null)
            enemyText.text = "Monsters: " + enemiesLeft;
    }

    void OpenPortal()
{
    if (portal != null)
    {
        portal.OpenPortal();
    }
}
    public void GameOver()
    {
        if (levelFinished) return;

        levelFinished = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void FinishLevel()
    {
        if (levelFinished) return;

        levelFinished = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

public void ContinueGame()
{
    Time.timeScale = 1f;

    string sceneName =
        SceneManager.GetActiveScene().name;

    if (sceneName == "Level1")
    {
        SceneManager.LoadScene("Level2");
    }
    else if (sceneName == "Level2")
    {
        SceneManager.LoadScene("BossFight");
    }
    else
    {
        Debug.LogWarning(
            "No next level for: " + sceneName
        );
    }
}

public void BackToMenu()
{
    Time.timeScale = 1f;
    SceneManager.LoadScene("MainMenu");
}

}
