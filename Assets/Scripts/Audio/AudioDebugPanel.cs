// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class AudioDebugPanel : MonoBehaviour
// {
//     [Header("Debug UI Elements")]
//     public TextMeshProUGUI statusText;
//     public Button playMenuMusicBtn;
//     public Button playDungeonMusicBtn;
//     public Button playBossMusicBtn;
//     public Button testButtonSoundBtn;
//     public Button testPickupSoundBtn;
//     public Slider musicVolumeSlider;
//     public Slider sfxVolumeSlider;

//     void Start()
//     {
//         SetupButtons();
//         SetupSliders();
//     }

//     void Update()
//     {
//         UpdateStatusText();
//     }

//     void SetupButtons()
//     {
//         if (playMenuMusicBtn != null)
//             playMenuMusicBtn.onClick.AddListener(() => AudioManager.Instance?.PlayMainMenuMusic());

//         if (playDungeonMusicBtn != null)
//             playDungeonMusicBtn.onClick.AddListener(() => AudioManager.Instance?.PlayRandomDungeonMusic());

//         if (playBossMusicBtn != null)
//             playBossMusicBtn.onClick.AddListener(() => AudioManager.Instance?.PlayBossMusic());

//         if (testButtonSoundBtn != null)
//             testButtonSoundBtn.onClick.AddListener(() => AudioManager.Instance?.PlaySound("button_click"));

//         if (testPickupSoundBtn != null)
//             testPickupSoundBtn.onClick.AddListener(() => AudioManager.Instance?.PlaySound("pickup_item"));
//     }

//     void SetupSliders()
//     {
//         if (musicVolumeSlider != null)
//         {
//             musicVolumeSlider.value = AudioManager.Instance?.musicVolume ?? 0.7f;
//             musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
//         }

//         if (sfxVolumeSlider != null)
//         {
//             sfxVolumeSlider.value = AudioManager.Instance?.sfxVolume ?? 0.8f;
//             sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
//         }
//     }

//     void SetMusicVolume(float value)
//     {
//         AudioManager.Instance?.SetMusicVolume(value);
//     }

//     void SetSFXVolume(float value)
//     {
//         AudioManager.Instance?.SetSFXVolume(value);
//     }

//     void UpdateStatusText()
//     {
//         if (statusText == null) return;

//         string status = "AudioManager Debug Panel\n\n";

//         if (AudioManager.Instance != null)
//         {
//             status += "✅ AudioManager: Active\n";
//             status += $"🎵 Music Volume: {AudioManager.Instance.musicVolume:F2}\n";
//             status += $"🔊 SFX Volume: {AudioManager.Instance.sfxVolume:F2}\n";
//             status += $"🎼 Current Music: {GetCurrentMusicName()}\n";
//         }
//         else
//         {
//             status += "❌ AudioManager: Missing\n";
//         }

//         SceneMusicManager sceneMusicManager = FindObjectOfType<SceneMusicManager>();
//         if (sceneMusicManager != null)
//         {
//             status += "✅ SceneMusicManager: Active\n";
//             status += $"🏠 Scene Type: {sceneMusicManager.GetCurrentSceneType()}\n";
//         }
//         else
//         {
//             status += "❌ SceneMusicManager: Missing\n";
//         }

//         BossMusicTrigger bossTrigger = FindObjectOfType<BossMusicTrigger>();
//         if (bossTrigger != null)
//         {
//             status += "✅ BossMusicTrigger: Found\n";
//         }
//         else
//         {
//             status += "⚠️ BossMusicTrigger: Not Found\n";
//         }

//         status += $"\n🎯 Current Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";

//         statusText.text = status;
//     }

//     string GetCurrentMusicName()
//     {
//         if (AudioManager.Instance?.musicSource?.clip != null)
//         {
//             return AudioManager.Instance.musicSource.clip.name;
//         }
//         return "None";
//     }

//     // Quick test methods for inspector buttons
//     [ContextMenu("Test All Audio")]
//     public void TestAllAudio()
//     {
//         StartCoroutine(TestAudioSequence());
//     }

//     System.Collections.IEnumerator TestAudioSequence()
//     {
//         Debug.Log("🎵 Testing Audio System...");

//         // Test button sound
//         AudioManager.Instance?.PlaySound("button_click");
//         yield return new WaitForSeconds(1f);

//         // Test pickup sound
//         AudioManager.Instance?.PlaySound("pickup_item");
//         yield return new WaitForSeconds(1f);

//         // Test music switching
//         AudioManager.Instance?.PlayMainMenuMusic();
//         yield return new WaitForSeconds(2f);

//         AudioManager.Instance?.PlayRandomDungeonMusic();
//         yield return new WaitForSeconds(2f);

//         AudioManager.Instance?.PlayBossMusic();
//         yield return new WaitForSeconds(2f);

//         Debug.Log("✅ Audio test completed!");
//     }
// }