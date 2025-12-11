using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public void LoadFirstScene()
    {
        SceneManager.LoadScene("Level1"); // Loads first level upon button press
    }

    public void LoadInstructions()
    {
        SceneManager.LoadScene("Instructions"); // Loads instuctions on buttonpress
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu"); // Loads instuctions on buttonpress
    }
}