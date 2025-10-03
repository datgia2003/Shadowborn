using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicManager : MonoBehaviour
{
    [Header("Scene Music Settings")]
    public string musicTrackName;
    public float fadeInTime = 2f;
    public bool playOnStart = true;

    [Header("Scene Type")]
    public SceneType sceneType = SceneType.Auto;
    public bool randomDungeonMusic = true;

    public enum SceneType
    {
        Auto,           // Tự động detect: MainMenuScene = MainMenu, SimpleScene = Dungeon
        MainMenu,       // Main menu music
        Dungeon,        // Dungeon music (random hoặc specific)  
        Boss,           // Boss music (sẽ được trigger từ script khác)
        Custom          // Dùng musicTrackName
    }

    void Start()
    {
        if (playOnStart)
        {
            // Đợi một frame để AudioManager được initialize
            Invoke(nameof(PlaySceneMusic), 0.1f);
        }
    }

    void PlaySceneMusic()
    {
        if (SimpleAudioManager.Instance == null)
        {
            Debug.LogWarning("SimpleAudioManager not found! Retrying...");
            Invoke(nameof(PlaySceneMusic), 0.5f);
            return;
        }

        SceneType currentSceneType = GetSceneType();

        switch (currentSceneType)
        {
            case SceneType.MainMenu:
                SimpleAudioManager.Instance.PlayMainMenuMusic();
                Debug.Log("Playing Main Menu music");
                break;

            case SceneType.Dungeon:
                SimpleAudioManager.Instance.PlayDungeonMusic();
                Debug.Log("Playing Dungeon music");
                break;

            case SceneType.Boss:
                AudioManager.Instance.PlayBossMusic();
                Debug.Log("Playing Boss music");
                break;

            case SceneType.Custom:
                SimpleAudioManager.Instance.PlayDungeonMusic(); // Fallback to dungeon music
                break;
        }
    }

    SceneType GetSceneType()
    {
        if (sceneType != SceneType.Auto)
            return sceneType;

        // Auto-detect cho 2 scenes cụ thể
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "MainMenuScene")
        {
            return SceneType.MainMenu;
        }
        else if (currentSceneName == "SampleScene")
        {
            return SceneType.Dungeon; // Default dungeon music cho SimpleScene
        }

        // Fallback
        return SceneType.Dungeon;
    }

    // Public methods để call từ scripts khác (đặc biệt cho boss fights trong SimpleScene)
    public void SwitchToMainMenuMusic()
    {
        if (SimpleAudioManager.Instance != null)
            SimpleAudioManager.Instance.PlayMainMenuMusic();
    }

    public void SwitchToDungeonMusic()
    {
        if (SimpleAudioManager.Instance != null)
            SimpleAudioManager.Instance.PlayDungeonMusic();
    }

    public void SwitchToBossMusic()
    {
        // SimpleAudioManager không có boss music - fallback to dungeon
        if (SimpleAudioManager.Instance != null)
            SimpleAudioManager.Instance.PlayDungeonMusic();
    }

    public void StopMusic()
    {
        if (SimpleAudioManager.Instance != null && SimpleAudioManager.Instance.musicSource != null)
            SimpleAudioManager.Instance.musicSource.Stop();
    }
}