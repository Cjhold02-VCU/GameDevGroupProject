using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlay : MonoBehaviour
{
    public void LoadInstructions()
    {
        SceneManager.LoadScene("Instructions"); // Loads instuctions on buttonpress
    }
}
