// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Reflection;

// /// <summary>
// /// Auto wires the Stats UI system - finds and assigns all UI references automatically
// /// Includes comprehensive null safety to prevent crashes during auto-discovery
// /// </summary>
// public class StatsUIAutoWirer : MonoBehaviour
// {
//     [Header("Debug Configuration")]
//     [SerializeField] private bool debugWiring = true;
//     [SerializeField] private bool wireOnStart = false;

//     [Header("Auto-Recovery Settings")]
//     [SerializeField] private bool autoRecoverOnNull = true;
//     [SerializeField] private float recoveryCheckInterval = 5f;

//     private StatsAllocationUI statsUI;
//     private float lastRecoveryCheck;

//     private void Start()
//     {
//         if (wireOnStart)
//         {
//             AutoWireStatsUI();
//         }
//     }

//     private void Update()
//     {
//         // Auto-recovery system - periodically check for null references
//         if (autoRecoverOnNull && Time.time - lastRecoveryCheck > recoveryCheckInterval)
//         {
//             lastRecoveryCheck = Time.time;
//             CheckAndRecoverNullReferences();
//         }
//     }

//     [ContextMenu("🔌 Auto Wire Stats UI")]
//     public void AutoWireStatsUI()
//     {
//         // Find StatsAllocationUI component
//         if (!FindStatsAllocationUI())
//         {
//             if (debugWiring) Debug.LogError("❌ Could not find StatsAllocationUI component to wire");
//             return;
//         }

//         // Wire main panels
//         WireMainPanels();

//         // Wire stat slot components
//         WireStatSlots();

//         // Wire notification system
//         WireNotificationSystem();

//         // Wire UI text components
//         WireUITexts();

//         if (debugWiring) Debug.Log("✅ Stats UI Auto Wiring Complete!");
//     }

//     private bool FindStatsAllocationUI()
//     {
//         statsUI = FindObjectOfType<StatsAllocationUI>();

//         if (statsUI == null)
//         {
//             // Try to find it on this GameObject
//             statsUI = GetComponent<StatsAllocationUI>();
//         }

//         if (statsUI == null)
//         {
//             // Try to find it on Canvas
//             Canvas canvas = FindObjectOfType<Canvas>();
//             if (canvas != null)
//             {
//                 statsUI = canvas.GetComponent<StatsAllocationUI>();
//             }
//         }

//         bool found = statsUI != null;
//         if (debugWiring)
//         {
//             Debug.Log(found ? "🎯 Found StatsAllocationUI component" : "❌ StatsAllocationUI component not found");
//         }

//         return found;
//     }

//     private void WireMainPanels()
//     {
//         // Find and assign statsPanel
//         GameObject statsPanel = FindGameObjectSafely("StatsPanel");
//         if (statsPanel != null && HasField("statsPanel"))
//         {
//             SetFieldValue("statsPanel", statsPanel);
//             if (debugWiring) Debug.Log("📊 Wired Stats Panel");
//         }

//         // Find and assign levelUpNotification
//         GameObject levelUpNotification = FindGameObjectSafely("LevelUpNotification");
//         if (levelUpNotification != null && HasField("levelUpNotification"))
//         {
//             SetFieldValue("levelUpNotification", levelUpNotification);
//             if (debugWiring) Debug.Log("⭐ Wired Level Up Notification");
//         }
//     }

//     private void WireStatSlots()
//     {
//         string[] statNames = { "STR", "INT", "AGI", "LUK", "VIT" };

//         for (int i = 0; i < statNames.Length; i++)
//         {
//             string statName = statNames[i];
//             WireStatSlot(statName, i);
//         }
//     }

//     private void WireStatSlot(string statName, int index)
//     {
//         // Create StatSlotUI if it doesn't exist
//         StatSlotUI slotUI = new StatSlotUI();

//         // Wire value text
//         TextMeshProUGUI valueText = FindComponentSafely<TextMeshProUGUI>($"{statName}ValueText");
//         if (valueText != null)
//         {
//             SetStatSlotField(slotUI, "valueText", valueText);
//             if (debugWiring) Debug.Log($"📊 Wired {statName} value text");
//         }

//         // Wire allocate button
//         Button allocateButton = FindComponentSafely<Button>($"{statName}Button");
//         if (allocateButton != null)
//         {
//             SetStatSlotField(slotUI, "allocateButton", allocateButton);
//             if (debugWiring) Debug.Log($"🔘 Wired {statName} allocate button");
//         }

//         // Assign the slot to the appropriate array position
//         if (HasField("statSlots"))
//         {
//             StatSlotUI[] statSlots = GetFieldValue("statSlots") as StatSlotUI[];
//             if (statSlots == null)
//             {
//                 statSlots = new StatSlotUI[5];
//                 SetFieldValue("statSlots", statSlots);
//             }

//             if (index < statSlots.Length)
//             {
//                 statSlots[index] = slotUI;
//                 if (debugWiring) Debug.Log($"✅ Assigned {statName} slot to index {index}");
//             }
//         }
//     }

//     private void WireNotificationSystem()
//     {
//         // Find notification text component
//         TextMeshProUGUI notificationText = FindComponentSafely<TextMeshProUGUI>("NotificationText");
//         if (notificationText != null && HasField("notificationText"))
//         {
//             SetFieldValue("notificationText", notificationText);
//             if (debugWiring) Debug.Log("⭐ Wired Notification Text");
//         }
//     }

//     private void WireUITexts()
//     {
//         // Wire available points text
//         TextMeshProUGUI availablePointsText = FindComponentSafely<TextMeshProUGUI>("AvailablePointsText");
//         if (availablePointsText != null && HasField("availablePointsText"))
//         {
//             SetFieldValue("availablePointsText", availablePointsText);
//             if (debugWiring) Debug.Log("💎 Wired Available Points Text");
//         }

//         // Wire close button
//         Button closeButton = FindComponentSafely<Button>("CloseButton");
//         if (closeButton != null && HasField("closeButton"))
//         {
//             SetFieldValue("closeButton", closeButton);
//             if (debugWiring) Debug.Log("❌ Wired Close Button");
//         }
//     }

//     private void CheckAndRecoverNullReferences()
//     {
//         if (statsUI == null) return;

//         bool needsRecovery = false;

//         // Check if main panels are null
//         GameObject statsPanel = GetFieldValue("statsPanel") as GameObject;
//         GameObject levelUpNotification = GetFieldValue("levelUpNotification") as GameObject;

//         if (statsPanel == null || levelUpNotification == null)
//         {
//             needsRecovery = true;
//         }

//         // Check if stat slots have null references
//         StatSlotUI[] statSlots = GetFieldValue("statSlots") as StatSlotUI[];
//         if (statSlots != null)
//         {
//             for (int i = 0; i < statSlots.Length; i++)
//             {
//                 if (statSlots[i] != null)
//                 {
//                     if (GetStatSlotField(statSlots[i], "valueText") == null ||
//                         GetStatSlotField(statSlots[i], "allocateButton") == null)
//                     {
//                         needsRecovery = true;
//                         break;
//                     }
//                 }
//             }
//         }

//         if (needsRecovery)
//         {
//             if (debugWiring) Debug.LogWarning("🔧 Null references detected, attempting auto-recovery...");
//             AutoWireStatsUI();
//         }
//     }

//     // SAFE UTILITY METHODS WITH NULL CHECKS

//     private GameObject FindGameObjectSafely(string name)
//     {
//         try
//         {
//             GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
//             foreach (GameObject obj in allObjects)
//             {
//                 if (obj != null && obj.name == name)
//                 {
//                     return obj;
//                 }
//             }
//             return null;
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error finding GameObject '{name}': {e.Message}");
//             return null;
//         }
//     }

//     private T FindComponentSafely<T>(string gameObjectName) where T : Component
//     {
//         try
//         {
//             GameObject targetGO = FindGameObjectSafely(gameObjectName);
//             if (targetGO != null)
//             {
//                 return targetGO.GetComponent<T>();
//             }
//             return null;
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error finding component {typeof(T).Name} on '{gameObjectName}': {e.Message}");
//             return null;
//         }
//     }

//     private bool HasField(string fieldName)
//     {
//         if (statsUI == null) return false;

//         try
//         {
//             FieldInfo field = statsUI.GetType().GetField(fieldName,
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//             return field != null;
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error checking field '{fieldName}': {e.Message}");
//             return false;
//         }
//     }

//     private object GetFieldValue(string fieldName)
//     {
//         if (statsUI == null) return null;

//         try
//         {
//             FieldInfo field = statsUI.GetType().GetField(fieldName,
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//             if (field != null)
//             {
//                 return field.GetValue(statsUI);
//             }
//             return null;
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error getting field value '{fieldName}': {e.Message}");
//             return null;
//         }
//     }

//     private void SetFieldValue(string fieldName, object value)
//     {
//         if (statsUI == null) return;

//         try
//         {
//             FieldInfo field = statsUI.GetType().GetField(fieldName,
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//             if (field != null)
//             {
//                 field.SetValue(statsUI, value);
//             }
//             else
//             {
//                 if (debugWiring) Debug.LogWarning($"⚠️ Field '{fieldName}' not found for assignment");
//             }
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error setting field value '{fieldName}': {e.Message}");
//         }
//     }

//     private object GetStatSlotField(StatSlotUI slotUI, string fieldName)
//     {
//         if (slotUI == null) return null;

//         try
//         {
//             FieldInfo field = typeof(StatSlotUI).GetField(fieldName,
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//             if (field != null)
//             {
//                 return field.GetValue(slotUI);
//             }
//             return null;
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error getting StatSlotUI field '{fieldName}': {e.Message}");
//             return null;
//         }
//     }

//     private void SetStatSlotField(StatSlotUI slotUI, string fieldName, object value)
//     {
//         if (slotUI == null) return;

//         try
//         {
//             FieldInfo field = typeof(StatSlotUI).GetField(fieldName,
//                 BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

//             if (field != null)
//             {
//                 field.SetValue(slotUI, value);
//             }
//             else
//             {
//                 if (debugWiring) Debug.LogWarning($"⚠️ StatSlotUI field '{fieldName}' not found for assignment");
//             }
//         }
//         catch (System.Exception e)
//         {
//             if (debugWiring) Debug.LogWarning($"⚠️ Error setting StatSlotUI field '{fieldName}': {e.Message}");
//         }
//     }

//     [ContextMenu("🧪 Test Field Detection")]
//     public void TestFieldDetection()
//     {
//         if (!FindStatsAllocationUI())
//         {
//             Debug.LogError("❌ No StatsAllocationUI found for testing");
//             return;
//         }

//         string[] testFields = { "statsPanel", "levelUpNotification", "statSlots", "availablePointsText", "notificationText", "closeButton" };

//         Debug.Log("🧪 Testing field detection:");
//         foreach (string field in testFields)
//         {
//             bool exists = HasField(field);
//             Debug.Log($"   {field}: {(exists ? "✅ Found" : "❌ Missing")}");
//         }
//     }

//     [ContextMenu("🔍 Debug Current Wiring State")]
//     public void DebugCurrentWiringState()
//     {
//         if (!FindStatsAllocationUI())
//         {
//             Debug.LogError("❌ No StatsAllocationUI found for debugging");
//             return;
//         }

//         Debug.Log("🔍 Current Stats UI Wiring State:");

//         // Check main panels
//         GameObject statsPanel = GetFieldValue("statsPanel") as GameObject;
//         GameObject levelUpNotification = GetFieldValue("levelUpNotification") as GameObject;

//         Debug.Log($"   📊 Stats Panel: {(statsPanel != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"   ⭐ Level Up Notification: {(levelUpNotification != null ? "✅ Assigned" : "❌ NULL")}");

//         // Check stat slots
//         StatSlotUI[] statSlots = GetFieldValue("statSlots") as StatSlotUI[];
//         if (statSlots != null)
//         {
//             Debug.Log($"   📊 Stat Slots Array: ✅ Size {statSlots.Length}");
//             string[] statNames = { "STR", "INT", "AGI", "LUK", "VIT" };
//             for (int i = 0; i < Mathf.Min(statSlots.Length, statNames.Length); i++)
//             {
//                 if (statSlots[i] != null)
//                 {
//                     bool hasValue = GetStatSlotField(statSlots[i], "valueText") != null;
//                     bool hasButton = GetStatSlotField(statSlots[i], "allocateButton") != null;
//                     Debug.Log($"      {statNames[i]}: Value={hasValue}, Button={hasButton}");
//                 }
//                 else
//                 {
//                     Debug.Log($"      {statNames[i]}: ❌ NULL slot");
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("   📊 Stat Slots Array: ❌ NULL");
//         }

//         // Check other components
//         TextMeshProUGUI availablePointsText = GetFieldValue("availablePointsText") as TextMeshProUGUI;
//         TextMeshProUGUI notificationText = GetFieldValue("notificationText") as TextMeshProUGUI;
//         Button closeButton = GetFieldValue("closeButton") as Button;

//         Debug.Log($"   💎 Available Points Text: {(availablePointsText != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"   ⭐ Notification Text: {(notificationText != null ? "✅ Assigned" : "❌ NULL")}");
//         Debug.Log($"   ❌ Close Button: {(closeButton != null ? "✅ Assigned" : "❌ NULL")}");
//     }
// }
