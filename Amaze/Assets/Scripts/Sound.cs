// using UnityEngine;

// [System.Serializable]
// public class Sound
// {
//     public string name;
//     public AudioClip clip;
//     public float volume = 1f;
//     public float pitch = 1f;
//     public bool loop = false;

//     [HideInInspector]
//     public AudioSource audioSource;
// }
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
    public bool loop;

    [HideInInspector]
    public AudioSource source; // ← This is what was missing!
}