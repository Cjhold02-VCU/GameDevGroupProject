using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("UI References")]
    public GameObject levelCompleteUI; // Assign your "You Win" Panel here
    public GameObject gameOverUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowLevelCompleteUI()
    {
        // 1. Unlock Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Pause Game (Optional, prevents player moving while in menu)
        Time.timeScale = 0f;

        // 3. Show UI
        if (levelCompleteUI != null) levelCompleteUI.SetActive(true);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f; // Unpause

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("No more levels! Loading Main Menu.");
            SceneManager.LoadScene(0); // Assuming 0 is Main Menu
        }
    }

    public void PlayerDied()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over Logic Started...");

        // 1. Disable Player Controls (Optional, but feels good)
        // 2. Show Game Over UI
        // 3. Restart Scene after delay

        Invoke(nameof(RestartGame), 3f);
    }

    public void RestartGame()
    {
        isGameOver = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}