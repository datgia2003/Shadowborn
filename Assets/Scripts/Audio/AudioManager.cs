using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public bool playOnAwake = false;
    [HideInInspector] public AudioSource source;
}

[System.Serializable]
public class MusicTrack
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool loop = true;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;

    [Header("Background Music")]
    public MusicTrack[] musicTracks;
    private AudioSource musicSource;
    private Coroutine fadeCoroutine;

    [Header("Sound Effects")]
    public Sound[] sounds;

    [Header("UI Sounds")]
    public AudioClip uiButtonHover;
    public AudioClip uiButtonClick;
    public AudioClip uiMenuOpen;
    public AudioClip uiMenuClose;

    [Header("Gameplay Sounds")]
    public AudioClip coinPickup;
    public AudioClip chestOpen;
    public AudioClip itemPickup;
    public AudioClip doorOpen;
    public AudioClip dungeonEnter;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<string, MusicTrack> musicDict;
    private Dictionary<string, Sound> soundDict;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudio()
    {
        // Setup music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixerGroup;
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // Setup sound effects
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = sound.playOnAwake;
            sound.source.outputAudioMixerGroup = sfxMixerGroup;
        }

        // Create dictionaries for fast lookup
        musicDict = new Dictionary<string, MusicTrack>();
        foreach (MusicTrack track in musicTracks)
        {
            if (!musicDict.ContainsKey(track.name))
                musicDict.Add(track.name, track);
        }

        soundDict = new Dictionary<string, Sound>();
        foreach (Sound sound in sounds)
        {
            if (!soundDict.ContainsKey(sound.name))
                soundDict.Add(sound.name, sound);
        }

        // Apply volume settings
        UpdateVolumeSettings();
    }

    #region Music Control
    public void PlayMusic(string trackName, float fadeInTime = 1f)
    {
        if (musicDict.ContainsKey(trackName))
        {
            MusicTrack track = musicDict[trackName];
            PlayMusic(track, fadeInTime);
        }
        else
        {
            Debug.LogWarning($"Music track '{trackName}' not found!");
        }
    }

    public void PlayMusic(MusicTrack track, float fadeInTime = 1f)
    {
        if (track == null || track.clip == null) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeToMusic(track, fadeInTime));
    }

    public void PlayRandomDungeonMusic()
    {
        List<MusicTrack> dungeonTracks = new List<MusicTrack>();
        foreach (MusicTrack track in musicTracks)
        {
            if (track.name.ToLower().Contains("dungeon") || track.name.ToLower().Contains("battle"))
            {
                dungeonTracks.Add(track);
            }
        }

        if (dungeonTracks.Count > 0)
        {
            MusicTrack randomTrack = dungeonTracks[Random.Range(0, dungeonTracks.Count)];
            PlayMusic(randomTrack, 2f);
            Debug.Log($"Playing random dungeon music: {randomTrack.name}");
        }
    }

    public void StopMusic(float fadeOutTime = 1f)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutMusic(fadeOutTime));
    }

    IEnumerator FadeToMusic(MusicTrack newTrack, float fadeTime)
    {
        // Fade out current music
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
                yield return null;
            }
            musicSource.Stop();
        }

        // Switch to new track
        musicSource.clip = newTrack.clip;
        musicSource.volume = 0f;
        musicSource.loop = newTrack.loop;
        musicSource.Play();

        // Fade in new music
        float targetVolume = newTrack.volume * musicVolume * masterVolume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeTime);
            yield return null;
        }
        musicSource.volume = targetVolume;
    }

    IEnumerator FadeOutMusic(float fadeTime)
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = startVolume;
    }
    #endregion

    #region Sound Effects
    public void PlaySound(string soundName)
    {
        if (soundDict.ContainsKey(soundName))
        {
            soundDict[soundName].source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
        }
    }

    public void PlaySoundOneShot(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null)
        {
            // Create temporary audio source for one-shot sounds
            GameObject tempGO = new GameObject("TempAudio");
            AudioSource tempSource = tempGO.AddComponent<AudioSource>();
            tempSource.clip = clip;
            tempSource.volume = volumeScale * sfxVolume * masterVolume;
            tempSource.outputAudioMixerGroup = sfxMixerGroup;
            tempSource.Play();

            // Destroy after clip finishes
            Destroy(tempGO, clip.length);
        }
    }

    // Quick access methods for common sounds
    public void PlayUIHover() => PlaySoundOneShot(uiButtonHover);
    public void PlayUIClick() => PlaySoundOneShot(uiButtonClick);
    public void PlayUIMenuOpen() => PlaySoundOneShot(uiMenuOpen);
    public void PlayUIMenuClose() => PlaySoundOneShot(uiMenuClose);
    
    public void PlayCoinPickup() => PlaySoundOneShot(coinPickup);
    public void PlayChestOpen() => PlaySoundOneShot(chestOpen);
    public void PlayItemPickup() => PlaySoundOneShot(itemPickup);
    public void PlayDoorOpen() => PlaySoundOneShot(doorOpen);
    public void PlayDungeonEnter() => PlaySoundOneShot(dungeonEnter);
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }

    void UpdateVolumeSettings()
    {
        // Update music volume
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }

        // Update SFX volumes
        foreach (Sound sound in sounds)
        {
            if (sound.source != null)
            {
                sound.source.volume = sound.volume * sfxVolume * masterVolume;
            }
        }
    }
    #endregion

    #region Scene-Specific Music
    public void PlayMainMenuMusic()
    {
        PlayMusic("MainMenu", 2f);
    }

    public void PlayGameplayMusic()
    {
        PlayRandomDungeonMusic();
    }

    public void PlayBossMusic()
    {
        PlayMusic("Boss", 1f);
    }
    #endregion
}