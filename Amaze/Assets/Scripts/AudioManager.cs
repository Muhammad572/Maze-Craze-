using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;
    public bool mute = false;

    public AudioClip playerMoveSound;
    public AudioClip tilebreakSound;
    private AudioSource oneShotSource;

    [Header("Reward Sounds")]
    public AudioClip[] rewardClips; // assign your 2 (or more) reward tracks here

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

        oneShotSource = gameObject.AddComponent<AudioSource>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            oneShotSource.PlayOneShot(clip, volume);
        }
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null || s.source == null)
        {
            Debug.LogWarning($"Sound '{name}' not found or AudioSource missing!");
            return;
        }
        s.source.Play();
    }
    
    // // ✅ Add this new public method to play the tile break sound
    // public void PlayTileBreakSound()
    // {
    //     PlayOneShot(tilebreakSound);
    // }


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
            Debug.LogWarning($"Sound '{name}' missing or invalid!");
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


    private IEnumerator WaitAndRun(float delay, Action onComplete)
    {
        yield return new WaitForSeconds(delay);
        onComplete?.Invoke();
    }

    public void ToggleMute()
    {
        mute = !mute;
        foreach (Sound sound in sounds)
        {
            if (sound.source != null)
                sound.source.mute = mute;
        }
    }
    public void PlayRandomRewardSound()
    {
        if (rewardClips == null || rewardClips.Length == 0)
        {
            Debug.LogWarning("⚠️ No reward sounds assigned!");
            return;
        }

        int index = UnityEngine.Random.Range(0, rewardClips.Length);
        AudioClip clip = rewardClips[index];

        if (clip != null)
        {
            oneShotSource.PlayOneShot(clip);
            Debug.Log($"🎵 Playing reward sound: {clip.name}");
        }
    }

}