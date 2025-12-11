using UnityEngine.Audio;
using UnityEngine;

// The [System.Serializable] attribute lets us see and edit instances
// of this class in the Unity Inspector.
[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 0.75f;
    [Range(.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    // This field will be hidden in the inspector. It's used by the SoundManager
    // to hold the AudioSource component that will play this sound.
    [HideInInspector]
    public AudioSource source;
}