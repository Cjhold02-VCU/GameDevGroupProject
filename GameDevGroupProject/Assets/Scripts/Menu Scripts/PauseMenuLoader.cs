using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuLoader : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ClosePauseMenu();
            else OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        SceneManager.LoadScene("PauseMenuScene", LoadSceneMode.Additive);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ClosePauseMenu()
    {
        SceneManager.UnloadSceneAsync("PauseMenuScene");
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
