using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; // <-- Add this namespace

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;

    // --- REPLACED UI REFERENCES WITH EVENTS ---
    [Header("Game Events")]
    [Tooltip("This event is fired when the level is successfully completed.")]
    public UnityEvent OnLevelComplete;
    [Tooltip("This event is fired when the player has died.")]
    public UnityEvent OnPlayerDeath;


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

    // This is now the CENTRAL place to trigger the "Level Complete" logic.
    public void TriggerLevelComplete()
    {
        // Unlock Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 2. Pause Game
        Time.timeScale = 0f;

        // 3. Announce that the level is complete. Any listener (like our UIManager) will hear this.
        Debug.Log("GameManager: Firing OnLevelComplete event.");
        OnLevelComplete?.Invoke();
    }

    // This is now the CENTRAL place to trigger the "Game Over" logic.
    public void TriggerPlayerDied()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Announce that the player has died.
        Debug.Log("GameManager: Firing OnPlayerDeath event.");
        OnPlayerDeath?.Invoke();

        // Stop sounds, etc. here if needed.
        // SoundManager.Instance.StopMusic();

        // 3. Restart Scene after delay
        Invoke(nameof(RestartGame), 3f);
    }

    #region Scene Management
    // These functions can be called by UI buttons

    public void LoadNextLevel()
    {
        isGameOver = false;
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

    public void RestartGame()
    {
        isGameOver = false;
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
}