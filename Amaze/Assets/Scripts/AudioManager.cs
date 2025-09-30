using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("General Sounds")]
    public Sound[] sounds;
    public bool mute = false;

    [Header("Core Clips")]
    public AudioClip playerMoveSound;
    public AudioClip tilebreakSound;

    [Header("Reward Sounds")]
    public AudioClip[] rewardClips; // assign reward tracks in Inspector

    private AudioSource oneShotSource;   // ✅ pooled for short SFX only
    private AudioSource musicSource;     // optional if you add music later

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // One main pooled source
        oneShotSource = gameObject.AddComponent<AudioSource>();
        oneShotSource.spatialBlend = 0f; // force 2D
        oneShotSource.playOnAwake = false;

        // Setup sound bank
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // --- Generic ---
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null && !mute)
        {
            oneShotSource.pitch = 1f;   // ✅ reset pitch each play
            oneShotSource.volume = volume;
            oneShotSource.spatialBlend = 0f;
            oneShotSource.PlayOneShot(clip);
        }
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null)
        {
            Debug.LogWarning($"⚠️ Sound '{name}' not found!");
            return;
        }
        if (!mute) s.source.Play();
    }

    // --- Dedicated helpers ---
    public void PlayMoveSound()
    {
        PlayOneShot(playerMoveSound, 0.7f);
    }

    public void PlayTileBreakSound(Vector3 pos)
    {
        PlayOneShot(tilebreakSound, 0.2f);
    }

    public void PlayRandomRewardSound()
    {
        if (rewardClips == null || rewardClips.Length == 0 || mute)
        {
            Debug.LogWarning("⚠️ No reward sounds assigned!");
            return;
        }

        int index = UnityEngine.Random.Range(0, rewardClips.Length);
        AudioClip clip = rewardClips[index];
        PlayOneShot(clip, 1f);
        Debug.Log($"🎵 Playing reward sound: {clip.name}");
    }

    // --- Mute toggle ---
    public void ToggleMute()
    {
        mute = !mute;
        foreach (Sound sound in sounds)
        {
            if (sound.source != null)
                sound.source.mute = mute;
        }
        oneShotSource.mute = mute;
    }

    public void PlaySoundWithCallback(string name, Action onComplete)
    {
        StartCoroutine(PlayWithReadySource(name, onComplete));
    }

    private IEnumerator PlayWithReadySource(string name, Action onComplete)
    {
        yield return null;

        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null || s.clip == null)
        {
            Debug.LogWarning($"⚠️ Sound '{name}' missing or invalid!");
            onComplete?.Invoke();
            yield break;
        }

        s.source.PlayOneShot(s.clip);

        if (s.clip.length <= 0f)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(s.clip.length);
        onComplete?.Invoke();
    }
    public void ResetAllSounds()
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
            {
                s.source.volume = s.volume;
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
            }
        }
        oneShotSource.pitch = 1f;
        oneShotSource.volume = 1f;
    }
}
