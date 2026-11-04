using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Все элементы оформлены в Canvas. Здесь только надписи и действия кнопок.
public class VictoryPanelView : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text primaryButtonText;
    public Button primaryButton;
    public Button menuButton;

    public void Prepare(GameManager manager, bool defeat = false)
    {
        primaryButton.onClick.RemoveAllListeners();
        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(manager.BackToMenu);

        if (defeat)
        {
            titleText.text = "GAME OVER";
            primaryButtonText.text = "RESTART";
            primaryButton.onClick.AddListener(manager.RestartGame);
        }
        else if (SceneManager.GetActiveScene().name == "BossFight")
        {
            titleText.text = "VICTORY!";
            primaryButtonText.text = "RESTART";
            primaryButton.onClick.AddListener(manager.RestartGame);
        }
        else
        {
            titleText.text = "LEVEL COMPLETE!";
            primaryButtonText.text = "CONTINUE";
            primaryButton.onClick.AddListener(manager.ContinueGame);
        }
    }
}
