using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    public static SimpleAudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip dungeonMusic;

    [Header("SFX Clips")]
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;
    public AudioClip coinPickupSound;
    public AudioClip itemPickupSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo AudioSource nếu chưa có
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
            }

            // Load saved volumes
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            UpdateVolumes();
        }
        else
        {
            Destroy(gameObject);
        }

        // Subscribe to scene change events
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // Auto-play music based on current scene
        CheckAndPlaySceneMusic();
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Auto-play music when scene changes
        CheckAndPlaySceneMusic();
    }

    void CheckAndPlaySceneMusic()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"[SimpleAudioManager] Current scene: {sceneName}");

        if (sceneName == "MainMenuScene")
        {
            Debug.Log("[SimpleAudioManager] Playing Main Menu Music");
            PlayMainMenuMusic();
        }
        else if (sceneName == "SampleScene")
        {
            Debug.Log("[SimpleAudioManager] Playing Dungeon Music");
            PlayDungeonMusic();
        }
        else
        {
            Debug.Log($"[SimpleAudioManager] Unknown scene: {sceneName}");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe when destroyed
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            Debug.Log($"[SimpleAudioManager] Switching to Main Menu Music: {mainMenuMusic.name}");
            musicSource.clip = mainMenuMusic;
            musicSource.loop = true; // Ensure loop
            musicSource.Play();
        }
        else
        {
            Debug.LogError("[SimpleAudioManager] Main Menu Music is null!");
        }
    }

    public void PlayDungeonMusic()
    {
        if (dungeonMusic != null)
        {
            Debug.Log($"[SimpleAudioManager] Switching to Dungeon Music: {dungeonMusic.name}");
            musicSource.clip = dungeonMusic;
            musicSource.loop = true; // Ensure loop
            musicSource.Play();
        }
        else
        {
            Debug.LogError("[SimpleAudioManager] Dungeon Music is null!");
        }
    }

    // SFX Methods
    public void PlayButtonHover()
    {
        PlaySFX(buttonHoverSound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayCoinPickup()
    {
        PlaySFX(coinPickupSound);
    }

    public void PlayItemPickup()
    {
        PlaySFX(itemPickupSound);
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Volume Controls
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    private void UpdateVolumes()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}