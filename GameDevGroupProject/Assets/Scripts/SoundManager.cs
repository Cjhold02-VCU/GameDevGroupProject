using UnityEngine;
using System;
using UnityEngine.SceneManagement; // <-- Add this

/// <summary>
/// A robust, centralized audio manager for the game.
/// It uses two dedicated AudioSources: one for one-shot sound effects (SFX)
/// and another for looping ambient tracks or music. This is a scalable design
/// that prevents sounds from cutting each other off.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [Tooltip("The AudioSource for playing short, non-looping sound effects.")]
    public AudioSource sfxSource;
    [Tooltip("The AudioSource for playing looping ambient sounds or music.")]
    public AudioSource musicSource;

    [Header("Sound Library")]
    public Sound[] sounds;

    void Awake()
    {
        // --- Singleton Pattern ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- Setup Audio Sources ---
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
    }

    // --- NEW: Subscribe to the sceneLoaded event ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // --- NEW: Unsubscribe to prevent memory leaks ---
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- NEW: This method is called every time a new scene finishes loading ---
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop any music that was playing from the previous scene.
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
        }

        // --- LOOK FOR SCENE-SPECIFIC AUDIO SETTINGS ---
        // We can add a simple script to our scenes to define what music should play.
        // For now, let's just hard-code it for your first level.
        // Replace "Level1" with the actual name of your first gameplay scene.
        if (scene.name == "Level1")
        {
            Play("AmbientCity");
        }
    }

    /// <summary>
    /// Finds a sound by name in the library.
    /// </summary>
    private Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning($"SoundManager: Sound '{name}' not found!");
            return null;
        }
        return s;
    }

    public void Play(string name)
    {
        Sound s = FindSound(name);
        if (s == null) return;

        if (s.loop)
        {
            musicSource.clip = s.clip;
            musicSource.volume = s.volume;
            musicSource.pitch = s.pitch;
            musicSource.Play();
        }
        else
        {
            sfxSource.pitch = s.pitch;
            sfxSource.PlayOneShot(s.clip, s.volume);
        }
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }
}