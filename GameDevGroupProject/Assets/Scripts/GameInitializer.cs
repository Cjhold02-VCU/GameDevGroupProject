using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script ensures that the core game managers (like GameManager, SoundManager) are loaded
/// before any gameplay scene starts. It should be placed in every gameplay scene.
/// If it detects that the managers aren't present, it re-routes to the Initialization scene first.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    // Assign your prefab that contains GameManager, SoundManager, etc.
    [Tooltip("A prefab containing all persistent game managers (GameManager, SoundManager, etc.).")]
    public GameObject persistentManagersPrefab;

    void Awake()
    {
        // Check if the GameManager singleton has been initialized.
        // We use GameManager as the check because it's central to the game.
        if (GameManager.Instance == null)
        {
            // If the managers are not found, it means we started the game from a gameplay
            // scene directly in the editor. In this case, we instantiate the managers.
            Debug.Log("No managers found, instantiating from prefab.");
            Instantiate(persistentManagersPrefab);
        }
    }
}