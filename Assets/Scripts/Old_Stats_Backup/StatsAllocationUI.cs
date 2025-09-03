// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;
// using UnityEngine.InputSystem;

// /// <summary>
// /// Stats Allocation UI - Solo Leveling Style
// /// Displays player stats and allows point allocation
// /// </summary>
// public class StatsAllocationUI : MonoBehaviour
// {
//     [Header("UI Panels")]
//     [SerializeField] private GameObject statsPanel;
//     [SerializeField] private Button openStatsButton;
//     [SerializeField] private Button closeStatsButton;

//     [Header("Available Points")]
//     [SerializeField] private TextMeshProUGUI availablePointsText;
//     [SerializeField] private GameObject newPointsIndicator;

//     [Header("Stat Displays")]
//     [SerializeField] private StatSlotUI strSlot;
//     [SerializeField] private StatSlotUI intSlot;
//     [SerializeField] private StatSlotUI agiSlot;
//     [SerializeField] private StatSlotUI lukSlot;
//     [SerializeField] private StatSlotUI vitSlot;

//     [Header("Level Up Notification")]
//     [SerializeField] private GameObject levelUpNotification;
//     [SerializeField] private TextMeshProUGUI notificationText;

//     [Header("Settings")]
//     [SerializeField] private float notificationDisplayTime = 3f;

//     private bool isStatsOpen = false;
//     private int pendingPoints = 0;

//     private void Start()
//     {
//         Debug.Log("🚀 StatsAllocationUI: Starting initialization...");

//         InitializeUI();
//         SubscribeToEvents();

//         // Initially hide panels
//         if (statsPanel != null) 
//         {
//             statsPanel.SetActive(false);
//             Debug.Log("📊 Stats panel hidden on start");
//         }
//         else
//         {
//             Debug.LogWarning("❌ statsPanel is null!");
//         }

//         if (levelUpNotification != null) 
//         {
//             levelUpNotification.SetActive(false);
//             Debug.Log("⭐ Level up notification hidden on start");
//         }
//         else
//         {
//             Debug.LogWarning("❌ levelUpNotification is null!");
//         }

//         if (newPointsIndicator != null) 
//         {
//             newPointsIndicator.SetActive(false);
//             Debug.Log("💎 New points indicator hidden on start");
//         }

//         Debug.Log("✅ StatsAllocationUI: Initialization complete");
//     }

//     private void OnDestroy()
//     {
//         UnsubscribeFromEvents();
//     }

//     #region Event Management

//     private void SubscribeToEvents()
//     {
//         if (PlayerStats.Instance != null)
//         {
//             PlayerStats.OnStatChanged += OnStatChanged;
//             PlayerStats.OnAvailablePointsChanged += UpdateAvailablePoints;
//             PlayerStats.OnLevelUpPointsAwarded += OnLevelUpPointsAwarded;

//             Debug.Log("✅ StatsAllocationUI: Subscribed to PlayerStats events");
//         }
//         else
//         {
//             Debug.LogWarning("❌ StatsAllocationUI: PlayerStats.Instance is null, cannot subscribe to events");
//         }

//         // Note: ExperienceSystem integration can be added when available
//         // ExperienceSystem.OnLevelUp += OnPlayerLevelUp;
//     }

//     private void UnsubscribeFromEvents()
//     {
//         if (PlayerStats.Instance != null)
//         {
//             PlayerStats.OnStatChanged -= OnStatChanged;
//             PlayerStats.OnAvailablePointsChanged -= UpdateAvailablePoints;
//             PlayerStats.OnLevelUpPointsAwarded -= OnLevelUpPointsAwarded;
//         }

//         // Note: ExperienceSystem integration can be added when available
//         // ExperienceSystem.OnLevelUp -= OnPlayerLevelUp;
//     }

//     private void InitializeUI()
//     {
//         // Setup button listeners
//         if (openStatsButton != null)
//             openStatsButton.onClick.AddListener(OpenStatsPanel);

//         if (closeStatsButton != null)
//             closeStatsButton.onClick.AddListener(CloseStatsPanel);

//         // Initialize stat slots
//         if (strSlot != null) strSlot.Initialize(StatType.Strength, "STR", "Strength");
//         if (intSlot != null) intSlot.Initialize(StatType.Intelligence, "INT", "Intelligence");
//         if (agiSlot != null) agiSlot.Initialize(StatType.Agility, "AGI", "Agility");
//         if (lukSlot != null) lukSlot.Initialize(StatType.Luck, "LUK", "Luck");
//         if (vitSlot != null) vitSlot.Initialize(StatType.Vitality, "VIT", "Vitality");

//         // Update initial displays
//         if (PlayerStats.Instance != null)
//         {
//             UpdateStatDisplays(
//                 PlayerStats.Instance.Strength,
//                 PlayerStats.Instance.Intelligence,
//                 PlayerStats.Instance.Agility,
//                 PlayerStats.Instance.Luck,
//                 PlayerStats.Instance.Vitality
//             );
//             UpdateAvailablePoints(PlayerStats.Instance.AvailablePoints);
//         }
//     }

//     #endregion

//     #region Event Handlers

//     private void OnStatChanged(StatType statType, int newValue)
//     {
//         // Update individual stat display
//         switch (statType)
//         {
//             case StatType.Strength:
//                 if (strSlot != null) strSlot.UpdateValue(newValue);
//                 break;
//             case StatType.Intelligence:
//                 if (intSlot != null) intSlot.UpdateValue(newValue);
//                 break;
//             case StatType.Agility:
//                 if (agiSlot != null) agiSlot.UpdateValue(newValue);
//                 break;
//             case StatType.Luck:
//                 if (lukSlot != null) lukSlot.UpdateValue(newValue);
//                 break;
//             case StatType.Vitality:
//                 if (vitSlot != null) vitSlot.UpdateValue(newValue);
//                 break;
//         }
//     }

//     private void OnLevelUpPointsAwarded(int totalPoints)
//     {
//         Debug.Log($"🎉 StatsAllocationUI: OnLevelUpPointsAwarded called with {totalPoints} total points");
//         ShowLevelUpNotification(totalPoints);
//     }

//     private void OnPlayerLevelUp(int newLevel)
//     {
//         ShowLevelUpNotification(newLevel);
//     }

//     private void UpdateStatDisplays(int str, int intel, int agi, int luk, int vit)
//     {
//         if (strSlot != null) strSlot.UpdateValue(str);
//         if (intSlot != null) intSlot.UpdateValue(intel);
//         if (agiSlot != null) agiSlot.UpdateValue(agi);
//         if (lukSlot != null) lukSlot.UpdateValue(luk);
//         if (vitSlot != null) vitSlot.UpdateValue(vit);
//     }

//     private void UpdateAvailablePoints(int points)
//     {
//         Debug.Log($"🔔 StatsAllocationUI: UpdateAvailablePoints called with {points} points");

//         if (availablePointsText != null)
//         {
//             availablePointsText.text = $"Available Points: {points}";
//         }

//         // Show/hide new points indicator
//         if (newPointsIndicator != null)
//         {
//             newPointsIndicator.SetActive(points > 0);
//         }

//         // Update button interactability
//         bool canAllocate = points > 0;
//         if (strSlot != null) strSlot.SetButtonInteractable(canAllocate);
//         if (intSlot != null) intSlot.SetButtonInteractable(canAllocate);
//         if (agiSlot != null) agiSlot.SetButtonInteractable(canAllocate);
//         if (lukSlot != null) lukSlot.SetButtonInteractable(canAllocate);
//         if (vitSlot != null) vitSlot.SetButtonInteractable(canAllocate);
//     }

//     #endregion

//     #region Level Up Notification

//     private void ShowLevelUpNotification(int totalPointsOrLevel)
//     {
//         Debug.Log($"⭐ StatsAllocationUI: ShowLevelUpNotification called");

//         if (levelUpNotification == null) 
//         {
//             Debug.LogWarning("❌ levelUpNotification is null!");
//             return;
//         }

//         // Update notification text with 2 lines as requested
//         if (notificationText != null)
//         {
//             int totalPoints = PlayerStats.Instance?.AvailablePoints ?? 0;
//             string notificationMessage = $"LEVEL UP!\n+3 Points (Total: {totalPoints})";
//             notificationText.text = notificationMessage;
//             Debug.Log($"📝 Notification text set to: {notificationMessage}");
//         }
//         else
//         {
//             Debug.LogWarning("❌ notificationText is null!");
//         }

//         // Show notification at top of screen
//         levelUpNotification.SetActive(true);
//         Debug.Log("✅ Level up notification shown!");

//         // Auto-hide after time (no game pause)
//         StartCoroutine(HideNotificationAfterTime());
//     }

//     private IEnumerator HideNotificationAfterTime()
//     {
//         yield return new WaitForSeconds(notificationDisplayTime);
//         if (levelUpNotification != null)
//         {
//             levelUpNotification.SetActive(false);
//         }
//     }

//     #endregion

//     #region Panel Management

//     public void OpenStatsPanel()
//     {
//         if (statsPanel != null)
//         {
//             statsPanel.SetActive(true);
//             isStatsOpen = true;
//         }
//     }

//     public void CloseStatsPanel()
//     {
//         if (statsPanel != null)
//         {
//             statsPanel.SetActive(false);
//             isStatsOpen = false;
//         }
//     }

//     public void ToggleStatsPanel()
//     {
//         if (isStatsOpen)
//         {
//             CloseStatsPanel();
//         }
//         else
//         {
//             OpenStatsPanel();
//         }
//     }

//     #endregion

//     #region Input Handling

//     private void Update()
//     {
//         // Toggle stats panel with 'C' key (Character)
//         if (Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame)
//         {
//             ToggleStatsPanel();
//         }

//         // Close panel with Escape
//         if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && isStatsOpen)
//         {
//             CloseStatsPanel();
//         }
//     }

//     #endregion

//     #region Debug Methods

//     /// <summary>
//     /// Test method to trigger level up for debugging
//     /// </summary>
//     [ContextMenu("🧪 Test Level Up")]
//     public void TestLevelUp()
//     {
//         Debug.Log("🧪 StatsAllocationUI: TestLevelUp called");

//         if (PlayerStats.Instance != null)
//         {
//             PlayerStats.Instance.TriggerLevelUp(5);
//             Debug.Log("✅ Triggered level up on PlayerStats");
//         }
//         else
//         {
//             Debug.LogError("❌ PlayerStats.Instance is null!");
//         }
//     }

//     /// <summary>
//     /// Test method to show notification directly
//     /// </summary>
//     [ContextMenu("📢 Test Notification")]
//     public void TestNotification()
//     {
//         Debug.Log("🧪 StatsAllocationUI: TestNotification called");
//         ShowLevelUpNotification(1);
//     }

//     /// <summary>
//     /// Check if all UI components are properly assigned
//     /// </summary>
//     [ContextMenu("🔍 Check UI Components")]
//     public void CheckUIComponents()
//     {
//         Debug.Log("🔍 StatsAllocationUI: Checking UI components...");

//         Debug.Log($"📊 statsPanel: {(statsPanel != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"⭐ levelUpNotification: {(levelUpNotification != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"📝 notificationText: {(notificationText != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"💎 availablePointsText: {(availablePointsText != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"🔹 newPointsIndicator: {(newPointsIndicator != null ? "✅ Assigned" : "❌ NULL")}");

//         Debug.Log($"🔸 strSlot: {(strSlot != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"🔸 intSlot: {(intSlot != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"🔸 agiSlot: {(agiSlot != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"🔸 lukSlot: {(lukSlot != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"🔸 vitSlot: {(vitSlot != null ? "✅ Assigned" : "❌ NULL")}");

//         Debug.Log($"🔘 openStatsButton: {(openStatsButton != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"❌ closeStatsButton: {(closeStatsButton != null ? "✅ Assigned" : "❌ NULL")}");
//     }

//     #endregion
// }

// /// <summary>
// /// Individual stat slot UI component
// /// </summary>
// [System.Serializable]
// public class StatSlotUI
// {
//     [SerializeField] private TextMeshProUGUI statNameText;
//     [SerializeField] private TextMeshProUGUI statValueText;
//     [SerializeField] private Button allocateButton;
//     [SerializeField] private TextMeshProUGUI descriptionText;

//     private StatType statType;
//     private string statName;
//     private string description;

//     public void Initialize(StatType type, string name, string desc)
//     {
//         statType = type;
//         statName = name;
//         description = desc;

//         if (statNameText != null)
//             statNameText.text = statName;

//         if (descriptionText != null)
//             descriptionText.text = desc;

//         if (allocateButton != null)
//         {
//             allocateButton.onClick.RemoveAllListeners();
//             allocateButton.onClick.AddListener(() => AllocatePoint());
//         }
//     }

//     public void UpdateValue(int value)
//     {
//         if (statValueText != null)
//         {
//             statValueText.text = value.ToString();
//         }
//     }

//     public void SetButtonInteractable(bool interactable)
//     {
//         if (allocateButton != null)
//         {
//             allocateButton.interactable = interactable;
//         }
//     }

//     private void AllocatePoint()
//     {
//         if (PlayerStats.Instance != null)
//         {
//             PlayerStats.Instance.AllocatePoint(statType);
//         }
//     }
// }
