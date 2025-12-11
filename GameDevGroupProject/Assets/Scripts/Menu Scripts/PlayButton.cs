using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public void LoadFirstScene()
    {
        SceneManager.LoadScene("Level1"); // Loads first level upon button press
    }
}