using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameOver = false;
    private bool isPaused = false;

    [Header("UI References")]
    public GameObject levelCompleteUI; // Assign your "You Win" Panel here
    public GameObject pauseMenuUI; // Assign pause menu Panel here
    public GameObject gameOverUI;


    private void Awake() // On Awake
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

    public void Update()
    {
        // Check for KeyCode P
        if (Input.GetKeyDown(KeyCode.P))
        {
            // Pause game if not already paused
            if (isPaused) ResumeGame();
            else ShowPauseMenuUI();
        }
    }

    private void Start()
    {
        // Start the ambient sound when the game begins
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("AmbientCity");
        }
    }

    public void ShowPauseMenuUI()
    {
        // Unlock Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause Game
        Time.timeScale = 0f;

        // Show UI
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true);

        // Stop Ambient Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Stop("AmbientCity");
        }
    }
    public void ResumeGame()
    {
        // Set is paused to false
        isPaused = false;

        // Lock Cursor back to game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Resume Game
        Time.timeScale = 1f;

        // Hide UI
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        // Resume Ambient Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play("AmbientCity");
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

        // 4. Stop Ambient Sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Stop("AmbientCity");
        }
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Loads main menu on buttonpress
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

        // Stop the ambient sound on death
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Stop("AmbientCity");
        }

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