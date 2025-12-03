using UnityEngine;

/// <summary>
/// A simple utility script that destroys the GameObject it is attached to after a set lifetime.
/// Perfect for temporary visual effects (VFX) like explosions, muzzle flashes, or impacts.
/// This prevents an endless number of effect objects from bogging down the game.
/// </summary>
public class AutoDestroyVFX : MonoBehaviour
{
    [Tooltip("The lifetime of this game object in seconds. It will be destroyed after this time.")]
    public float lifetime = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        // Schedule the destruction of this game object after 'lifetime' seconds.
        Destroy(gameObject, lifetime);
    }
}