
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        foreach (var canvas in FindObjectsByType<Canvas>())
        {
            if (!canvas.isRootCanvas || canvas.gameObject.scene != gameObject.scene) continue;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            foreach (var image in canvas.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                if (image.name != "Background") continue;
                var rect = image.rectTransform;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                image.preserveAspect = false;
            }
        }
    }
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {

        Application.Quit();
    }
}
