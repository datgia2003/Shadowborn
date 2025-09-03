// using UnityEngine;
// using System.Reflection;
// using UnityEngine.InputSystem;

// /// <summary>
// /// Simple tester for the complete stats system
// /// </summary>
// public class StatsSystemTester : MonoBehaviour
// {
//     [Header("Test Configuration")]
//     [SerializeField] private bool setupOnStart = true;
//     [SerializeField] private bool testLevelUpOnStart = false;
//     [SerializeField] private bool showHotkeys = true;

//     private void Start()
//     {
//         if (setupOnStart)
//         {
//             SetupCompleteSystem();
//         }

//         if (testLevelUpOnStart)
//         {
//             // Wait a frame then test level up
//             Invoke(nameof(TestLevelUp), 0.1f);
//         }

//         if (showHotkeys)
//         {
//             Debug.Log("🎮 StatsSystemTester Hotkeys:\n" +
//                      "  L - Test Level Up\n" +
//                      "  S - Check System Status\n" +
//                      "  T - Setup Complete System\n" +
//                      "  C - Toggle Stats Panel");
//         }
//     }

//     [ContextMenu("🎯 Setup Complete System")]
//     public void SetupCompleteSystem()
//     {
//         Debug.Log("🧪 StatsSystemTester: Setting up complete system...");

//         // FIRST: Ensure PlayerStats exists and is initialized
//         EnsurePlayerStatsExists();

//         // SECOND: Find or create StatsUIAutoSetup
//         StatsUIAutoSetup autoSetup = FindObjectOfType<StatsUIAutoSetup>();
//         if (autoSetup == null)
//         {
//             GameObject setupGO = new GameObject("StatsUIAutoSetup");
//             autoSetup = setupGO.AddComponent<StatsUIAutoSetup>();
//             Debug.Log("🎨 Created StatsUIAutoSetup GameObject");
//         }

//         // THIRD: Setup complete system
//         autoSetup.SetupCompleteStatsSystem();

//         Debug.Log("✅ Complete system setup finished!");
//     }

//     private void EnsurePlayerStatsExists()
//     {
//         PlayerStats.EnsureInstance();
//         Debug.Log($"🎯 EnsurePlayerStatsExists() - Instance status: {(PlayerStats.Instance != null ? "✅ Success" : "❌ Failed")}");
//     }

//     [ContextMenu("⭐ Test Level Up")]
//     public void TestLevelUp()
//     {
//         Debug.Log("🧪 StatsSystemTester: Testing level up...");

//         // Ensure PlayerStats exists first
//         EnsurePlayerStatsExists();

//         if (PlayerStats.Instance != null)
//         {
//             PlayerStats.Instance.TriggerLevelUp(5);
//             Debug.Log("✅ Level up triggered!");
//         }
//         else
//         {
//             Debug.LogError("❌ PlayerStats.Instance still not found after creation attempt!");
//         }
//     }

//     [ContextMenu("🔍 Check System Status")]
//     public void CheckSystemStatus()
//     {
//         Debug.Log("🔍 StatsSystemTester: Checking system status...");

//         // Check PlayerStats
//         if (PlayerStats.Instance != null)
//         {
//             Debug.Log($"✅ PlayerStats found - Available Points: {PlayerStats.Instance.AvailablePoints}");
//         }
//         else
//         {
//             Debug.LogError("❌ PlayerStats.Instance not found!");
//         }

//         // Check StatsAllocationUI
//         StatsAllocationUI statsUI = FindObjectOfType<StatsAllocationUI>();
//         if (statsUI != null)
//         {
//             Debug.Log("✅ StatsAllocationUI found");
//         }
//         else
//         {
//             Debug.LogError("❌ StatsAllocationUI not found!");
//         }

//         // Check Canvas
//         Canvas canvas = FindObjectOfType<Canvas>();
//         if (canvas != null)
//         {
//             Debug.Log("✅ Canvas found");

//             // Check for UI components
//             GameObject statsPanel = GameObject.Find("StatsPanel");
//             GameObject levelUpNotification = GameObject.Find("LevelUpNotification");

//             Debug.Log($"📊 Stats Panel: {(statsPanel != null ? "Found" : "Not Found")}");
//             Debug.Log($"⭐ Level Up Notification: {(levelUpNotification != null ? "Found" : "Not Found")}");
//         }
//         else
//         {
//             Debug.LogError("❌ Canvas not found!");
//         }
//     }

//     [ContextMenu("🎮 Test Stats Panel Toggle")]
//     public void TestStatsPanelToggle()
//     {
//         Debug.Log("🎮 StatsSystemTester: Testing stats panel toggle...");

//         StatsAllocationUI statsUI = FindObjectOfType<StatsAllocationUI>();
//         if (statsUI != null)
//         {
//             statsUI.ToggleStatsPanel();
//             Debug.Log("✅ Stats panel toggled!");
//         }
//         else
//         {
//             Debug.LogError("❌ StatsAllocationUI not found!");
//         }
//     }

//     private void Update()
//     {
//         // Quick test hotkeys using new Input System
//         if (Keyboard.current != null)
//         {
//             if (Keyboard.current.lKey.wasPressedThisFrame)
//             {
//                 TestLevelUp();
//             }

//             if (Keyboard.current.sKey.wasPressedThisFrame)
//             {
//                 CheckSystemStatus();
//             }

//             if (Keyboard.current.tKey.wasPressedThisFrame)
//             {
//                 SetupCompleteSystem();
//             }

//             if (Keyboard.current.cKey.wasPressedThisFrame)
//             {
//                 TestStatsPanelToggle();
//             }
//         }
//     }
// }
