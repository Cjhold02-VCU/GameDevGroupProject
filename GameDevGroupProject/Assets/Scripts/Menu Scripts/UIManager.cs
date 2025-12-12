using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Manages the UI elements for a specific scene, including win/loss screens and the pause menu.
/// It listens to events from the GameManager and also handles direct player input for pausing.
/// This script should be placed on a Canvas or dedicated UI manager object within your scene.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Panel References")]
    [Tooltip("The 'You Win' panel to show when the level is complete.")]
    public GameObject levelCompleteUI;
    [Tooltip("The 'Game Over' panel to show when the player dies.")]
    public GameObject gameOverUI;
    [Tooltip("The 'Pause Menu' panel to show when the player pauses the game.")]
    public GameObject pauseMenuUI; // <-- New reference for your pause menu

    private bool isPaused = false;

    void Start()
    {
        // --- Subscribe to the GameManager's events ---
        // This makes sure our UI automatically shows up when the game state changes.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete.AddListener(ShowLevelCompleteUI);
            GameManager.Instance.OnPlayerDeath.AddListener(ShowGameOverUI);
        }
        else
        {
            Debug.LogError("UIManager could not find GameManager.Instance! Make sure a GameManager is present.", this);
        }

        // --- Initialize UI State ---
        // Make sure all panels are hidden when the level starts.
        if (levelCompleteUI != null) levelCompleteUI.SetActive(false);
        if (gameOverUI != null) gameOverUI.SetActive(false);
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);

        // Start with the game unpaused
        isPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        // --- Listen for Pause Input ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Toggle the paused state
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Main Menu");

    }

    // --- Public methods for managing game state ---

    public void PauseGame()
    {
        isPaused = true;

        // Freeze game time
        Time.timeScale = 0f;

        // Show the pause menu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(true);
        }

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Resume game time
        Time.timeScale = 1f;

        // Hide the pause menu
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // Lock and hide the cursor for FPS gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    // --- Event Handlers for GameManager ---
    // These methods are called automatically by the GameManager's events.

    private void OnDestroy()
    {
        // --- Unsubscribe from events when this object is destroyed ---
        // This is crucial to prevent errors when loading new scenes.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete.RemoveListener(ShowLevelCompleteUI);
            GameManager.Instance.OnPlayerDeath.RemoveListener(ShowGameOverUI);
        }
    }

    private void ShowLevelCompleteUI()
    {
        Debug.Log("UIManager: Received OnLevelComplete event. Showing UI.");
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }
    }

    private void ShowGameOverUI()
    {
        Debug.Log("UIManager: Received OnPlayerDeath event. Showing UI.");
        if (gameOverUI != null)
        {
            // Also unlock the cursor on the Game Over screen
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            gameOverUI.SetActive(true);
        }
    }
}