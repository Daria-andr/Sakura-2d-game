
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading || !other.CompareTag("Player"))
            return;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("Next Scene Name is empty!");
            return;
        }

        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }
}
