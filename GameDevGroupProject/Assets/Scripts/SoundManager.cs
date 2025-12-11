using UnityEngine;
using System; // Needed for Array.Find

public class SoundManager : MonoBehaviour
{
    // The static instance for easy access from other scripts
    public static SoundManager Instance;

    // An array to hold all the sounds we want to use.
    // You'll drag your audio clips into this in the Inspector.
    public Sound[] sounds;

    void Awake()
    {
        // --- Singleton Pattern ---
        // This ensures there is only ever one SoundManager instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        // This prevents the SoundManager from being destroyed when a new scene is loaded.
        DontDestroyOnLoad(gameObject);
        // --- End Singleton Pattern ---


        // --- Create AudioSources ---
        // Loop through each of our 'Sound' objects...
        foreach (Sound s in sounds)
        {
            // ...create an AudioSource component on the SoundManager's GameObject...
            s.source = gameObject.AddComponent<AudioSource>();
            // ...and copy the properties from our Sound object to the AudioSource.
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // --- Public Methods for Playing Sounds ---

    /// <summary>
    /// Plays a sound from the sounds array by its name.
    /// </summary>
    /// <param name="name">The name of the sound to play.</param>
    public void Play(string name)
    {
        // Use Array.Find to search the 'sounds' array for a Sound object
        // where the sound's 'name' matches the name passed to the function.
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Play();
    }

    /// <summary>
    /// Stops a sound from the sounds array by its name.
    /// </summary>
    /// <param name="name">The name of the sound to stop.</param>
    public void Stop(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Stop();
    }
}