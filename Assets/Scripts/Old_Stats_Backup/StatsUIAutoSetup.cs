// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Reflection;

// /// <summary>
// /// Auto setup for Stats UI system - creates all UI components programmatically
// /// </summary>
// public class StatsUIAutoSetup : MonoBehaviour
// {
//     [Header("Setup Configuration")]
//     [SerializeField] private bool debugSetup = true;
//     [SerializeField] private bool setupOnStart = true; // Changed to true

//     // References to created components
//     private Canvas mainCanvas;
//     private GameObject statsPanel;
//     private GameObject levelUpPanel;
//     private StatsAllocationUI statsUI;

//     private void Start()
//     {
//         if (setupOnStart)
//         {
//             SetupStatsUI();
//         }
//     }

//     [ContextMenu("🎨 Auto Create Stats UI")]
//     public void SetupStatsUI()
//     {
//         if (debugSetup) Debug.Log("🚀 Starting Stats UI Auto Setup...");

//         // Find or create main canvas
//         SetupMainCanvas();

//         // Create Stats Panel
//         CreateStatsPanel();

//         // Create Level Up Notification
//         CreateLevelUpNotification();

//         // Setup StatsAllocationUI component
//         SetupStatsAllocationComponent();

//         if (debugSetup)
//         {
//             Debug.Log("✅ Stats UI Auto Setup Complete!");
//         }
//     }

//     /// <summary>
//     /// Complete setup that includes creating PlayerStats if needed
//     /// </summary>
//     [ContextMenu("🎯 Setup Complete Stats System")]
//     public void SetupCompleteStatsSystem()
//     {
//         if (debugSetup) Debug.Log("🚀 Setting up complete stats system...");

//         // Ensure PlayerStats exists FIRST
//         EnsurePlayerStatsExists();

//         // Setup UI
//         SetupStatsUI();

//         if (debugSetup) Debug.Log("✅ Complete stats system setup finished!");
//     }

//     private void EnsurePlayerStatsExists()
//     {
//         PlayerStats.EnsureInstance();
//         if (debugSetup) Debug.Log($"🎯 EnsurePlayerStatsExists() - Instance: {(PlayerStats.Instance != null ? "✅" : "❌")}");
//     }

//     private void SetupMainCanvas()
//     {
//         // Find existing canvas or create new one
//         mainCanvas = FindObjectOfType<Canvas>();

//         if (mainCanvas == null)
//         {
//             GameObject canvasGO = new GameObject("Stats UI Canvas");
//             mainCanvas = canvasGO.AddComponent<Canvas>();
//             mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//             mainCanvas.sortingOrder = 100; // High priority

//             // IMPORTANT: Allow canvas to work when game is paused
//             mainCanvas.worldCamera = null; // Use overlay mode

//             // Add Canvas Scaler
//             CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
//             scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
//             scaler.referenceResolution = new Vector2(1920, 1080);
//             scaler.matchWidthOrHeight = 0.5f;

//             // Add Graphic Raycaster
//             GraphicRaycaster raycaster = canvasGO.AddComponent<GraphicRaycaster>();
//             raycaster.ignoreReversedGraphics = true;
//             raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

//             if (debugSetup) Debug.Log("📱 Created new Canvas for Stats UI");
//         }
//         else
//         {
//             if (debugSetup) Debug.Log("📱 Using existing Canvas for Stats UI");
//         }
//     }

//     private void CreateStatsPanel()
//     {
//         if (statsPanel != null)
//         {
//             if (debugSetup) Debug.Log("📊 Stats Panel already exists, skipping creation");
//             return;
//         }

//         // Main Stats Panel
//         GameObject statsPanelGO = new GameObject("StatsPanel");
//         statsPanelGO.transform.SetParent(mainCanvas.transform, false);
//         statsPanel = statsPanelGO;

//         // Panel Background
//         Image panelBG = statsPanelGO.AddComponent<Image>();
//         panelBG.color = new Color(0.1f, 0.1f, 0.18f, 0.95f); // Dark blue background

//         RectTransform panelRect = statsPanelGO.GetComponent<RectTransform>();
//         panelRect.anchorMin = new Vector2(0.5f, 0.5f);
//         panelRect.anchorMax = new Vector2(0.5f, 0.5f);
//         panelRect.pivot = new Vector2(0.5f, 0.5f);
//         panelRect.sizeDelta = new Vector2(500, 600);

//         // Vertical Layout for content
//         VerticalLayoutGroup layout = statsPanelGO.AddComponent<VerticalLayoutGroup>();
//         layout.spacing = 20;
//         layout.padding = new RectOffset(30, 30, 30, 30);
//         layout.childAlignment = TextAnchor.UpperCenter;
//         layout.childControlHeight = false;
//         layout.childControlWidth = true;
//         layout.childForceExpandHeight = false;
//         layout.childForceExpandWidth = true;

//         // Create panel content
//         CreateStatsHeader(statsPanelGO);
//         CreateStatsSlots(statsPanelGO);
//         CreateCloseButton(statsPanelGO);

//         // Initially hide panel
//         statsPanel.SetActive(false);

//         if (debugSetup) Debug.Log("📊 Stats Panel created successfully");
//     }

//     private void CreateStatsHeader(GameObject parent)
//     {
//         // Header Container
//         GameObject headerGO = new GameObject("Header");
//         headerGO.transform.SetParent(parent.transform, false);

//         RectTransform headerRect = headerGO.AddComponent<RectTransform>();
//         headerRect.sizeDelta = new Vector2(0, 80);

//         // Title
//         GameObject titleGO = new GameObject("Title");
//         titleGO.transform.SetParent(headerGO.transform, false);

//         RectTransform titleRect = titleGO.GetComponent<RectTransform>();
//         titleRect.anchorMin = Vector2.zero;
//         titleRect.anchorMax = Vector2.one;
//         titleRect.offsetMin = Vector2.zero;
//         titleRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
//         titleText.text = "CHARACTER STATS";
//         titleText.fontSize = 24;
//         titleText.fontStyle = FontStyles.Bold;
//         titleText.color = Color.white;
//         titleText.alignment = TextAlignmentOptions.Center;

//         // Available Points
//         GameObject pointsGO = new GameObject("AvailablePointsText");
//         pointsGO.transform.SetParent(headerGO.transform, false);

//         RectTransform pointsRect = pointsGO.GetComponent<RectTransform>();
//         pointsRect.anchorMin = new Vector2(0, 0);
//         pointsRect.anchorMax = new Vector2(1, 0.4f);
//         pointsRect.offsetMin = Vector2.zero;
//         pointsRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI pointsText = pointsGO.AddComponent<TextMeshProUGUI>();
//         pointsText.text = "Available Points: 0";
//         pointsText.fontSize = 18;
//         pointsText.color = Color.yellow;
//         pointsText.alignment = TextAlignmentOptions.Center;
//     }

//     private void CreateStatsSlots(GameObject parent)
//     {
//         string[] statNames = { "STR", "INT", "AGI", "LUK", "VIT" };
//         string[] statDescriptions = {
//             "Strength - Increases damage",
//             "Intelligence - Increases mana",
//             "Agility - Increases speed",
//             "Luck - Increases critical hits",
//             "Vitality - Increases health"
//         };

//         for (int i = 0; i < statNames.Length; i++)
//         {
//             CreateStatSlot(parent, statNames[i], statDescriptions[i]);
//         }
//     }

//     private void CreateStatSlot(GameObject parent, string statName, string description)
//     {
//         // Stat Container
//         GameObject slotGO = new GameObject($"{statName}Slot");
//         slotGO.transform.SetParent(parent.transform, false);

//         RectTransform slotRect = slotGO.AddComponent<RectTransform>();
//         slotRect.sizeDelta = new Vector2(0, 60);

//         // Slot Background
//         Image slotBG = slotGO.AddComponent<Image>();
//         slotBG.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

//         // Horizontal Layout
//         HorizontalLayoutGroup slotLayout = slotGO.AddComponent<HorizontalLayoutGroup>();
//         slotLayout.spacing = 10;
//         slotLayout.padding = new RectOffset(10, 10, 10, 10);
//         slotLayout.childAlignment = TextAnchor.MiddleCenter;
//         slotLayout.childControlHeight = true;
//         slotLayout.childControlWidth = false;
//         slotLayout.childForceExpandHeight = true;
//         slotLayout.childForceExpandWidth = false;

//         // Stat Name
//         CreateStatText(slotGO, $"{statName}NameText", statName, 60);

//         // Stat Value
//         CreateStatText(slotGO, $"{statName}ValueText", "10", 50);

//         // Allocate Button
//         CreateAllocateButton(slotGO, statName);

//         // Description (smaller text)
//         CreateStatText(slotGO, $"{statName}DescText", description, 150);
//     }

//     private void CreateStatText(GameObject parent, string name, string text, float width)
//     {
//         GameObject textGO = new GameObject(name);
//         textGO.transform.SetParent(parent.transform, false);

//         RectTransform textRect = textGO.GetComponent<RectTransform>();
//         textRect.sizeDelta = new Vector2(width, 0);

//         TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
//         textComponent.text = text;
//         textComponent.fontSize = 16;
//         textComponent.color = Color.white;
//         textComponent.alignment = TextAlignmentOptions.Center;
//     }

//     private void CreateAllocateButton(GameObject parent, string statName)
//     {
//         GameObject buttonGO = new GameObject($"{statName}Button");
//         buttonGO.transform.SetParent(parent.transform, false);

//         RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
//         buttonRect.sizeDelta = new Vector2(40, 30);

//         Button button = buttonGO.AddComponent<Button>();
//         Image buttonImage = buttonGO.AddComponent<Image>();
//         buttonImage.color = new Color(0f, 0.7f, 0f, 0.8f);
//         buttonImage.raycastTarget = true;

//         // Button transition
//         button.transition = Selectable.Transition.ColorTint;
//         ColorBlock colors = button.colors;
//         colors.normalColor = new Color(0f, 0.7f, 0f, 0.8f);
//         colors.highlightedColor = new Color(0f, 0.9f, 0f, 1f);
//         colors.pressedColor = new Color(0f, 0.5f, 0f, 0.8f);
//         colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
//         button.colors = colors;

//         // Button Text
//         GameObject buttonTextGO = new GameObject("ButtonText");
//         buttonTextGO.transform.SetParent(buttonGO.transform, false);

//         RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
//         buttonTextRect.anchorMin = Vector2.zero;
//         buttonTextRect.anchorMax = Vector2.one;
//         buttonTextRect.offsetMin = Vector2.zero;
//         buttonTextRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI buttonText = buttonTextGO.AddComponent<TextMeshProUGUI>();
//         buttonText.text = "+";
//         buttonText.fontSize = 18;
//         buttonText.fontStyle = FontStyles.Bold;
//         buttonText.color = Color.white;
//         buttonText.alignment = TextAlignmentOptions.Center;
//         buttonText.raycastTarget = false;
//     }

//     private void CreateCloseButton(GameObject parent)
//     {
//         GameObject closeButtonGO = new GameObject("CloseButton");
//         closeButtonGO.transform.SetParent(parent.transform, false);

//         RectTransform closeRect = closeButtonGO.GetComponent<RectTransform>();
//         closeRect.sizeDelta = new Vector2(100, 40);

//         Button closeButton = closeButtonGO.AddComponent<Button>();
//         Image closeImage = closeButtonGO.AddComponent<Image>();
//         closeImage.color = new Color(0.7f, 0f, 0f, 0.8f);
//         closeImage.raycastTarget = true;

//         // Button transition
//         closeButton.transition = Selectable.Transition.ColorTint;
//         ColorBlock colors = closeButton.colors;
//         colors.normalColor = new Color(0.7f, 0f, 0f, 0.8f);
//         colors.highlightedColor = new Color(0.9f, 0f, 0f, 1f);
//         colors.pressedColor = new Color(0.5f, 0f, 0f, 0.8f);
//         closeButton.colors = colors;

//         // Close Button Text
//         GameObject closeTextGO = new GameObject("CloseText");
//         closeTextGO.transform.SetParent(closeButtonGO.transform, false);

//         RectTransform closeTextRect = closeTextGO.GetComponent<RectTransform>();
//         closeTextRect.anchorMin = Vector2.zero;
//         closeTextRect.anchorMax = Vector2.one;
//         closeTextRect.offsetMin = Vector2.zero;
//         closeTextRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI closeText = closeTextGO.AddComponent<TextMeshProUGUI>();
//         closeText.text = "Close";
//         closeText.fontSize = 16;
//         closeText.fontStyle = FontStyles.Bold;
//         closeText.color = Color.white;
//         closeText.alignment = TextAlignmentOptions.Center;
//         closeText.raycastTarget = false;
//     }

//     private void CreateLevelUpNotification()
//     {
//         if (levelUpPanel != null)
//         {
//             if (debugSetup) Debug.Log("⭐ Level Up Notification already exists, skipping creation");
//             return;
//         }

//         // Create Level Up Notification (small banner at top)
//         GameObject notificationGO = new GameObject("LevelUpNotification");
//         notificationGO.transform.SetParent(mainCanvas.transform, false);
//         levelUpPanel = notificationGO; // Keep same reference for compatibility

//         // Background with semi-transparent design
//         Image notificationBG = notificationGO.AddComponent<Image>();
//         notificationBG.color = new Color(1f, 0.84f, 0f, 0.8f); // Gold background

//         RectTransform notificationRect = notificationGO.GetComponent<RectTransform>();
//         notificationRect.anchorMin = new Vector2(0.5f, 1f); // Top center
//         notificationRect.anchorMax = new Vector2(0.5f, 1f);
//         notificationRect.pivot = new Vector2(0.5f, 1f);
//         notificationRect.sizeDelta = new Vector2(400, 80);
//         notificationRect.anchoredPosition = new Vector2(0, -20); // Slightly below top

//         // Add a subtle shadow/outline
//         Outline outline = notificationGO.AddComponent<Outline>();
//         outline.effectColor = new Color(0, 0, 0, 0.5f);
//         outline.effectDistance = new Vector2(2, -2);

//         // Notification Text
//         GameObject textGO = new GameObject("NotificationText");
//         textGO.transform.SetParent(notificationGO.transform, false);

//         RectTransform textRect = textGO.GetComponent<RectTransform>();
//         textRect.anchorMin = Vector2.zero;
//         textRect.anchorMax = Vector2.one;
//         textRect.offsetMin = Vector2.zero;
//         textRect.offsetMax = Vector2.zero;

//         TextMeshProUGUI notificationText = textGO.AddComponent<TextMeshProUGUI>();
//         notificationText.text = "LEVEL UP!\n+3 Points (Total: 3)";
//         notificationText.fontSize = 16;
//         notificationText.fontStyle = FontStyles.Bold;
//         notificationText.color = Color.white;
//         notificationText.alignment = TextAlignmentOptions.Center;

//         // Add subtle glow effect
//         notificationText.fontMaterial = Resources.Load<Material>("UI/Fonts/LiberationSans SDF - Outline");

//         // Initially hide notification
//         notificationGO.SetActive(false);

//         if (debugSetup) Debug.Log("⭐ Level Up Notification created successfully");
//     }

//     private void SetupStatsAllocationComponent()
//     {
//         // Add StatsAllocationUI component to canvas if not exists
//         if (statsUI == null)
//         {
//             statsUI = mainCanvas.gameObject.GetComponent<StatsAllocationUI>();
//             if (statsUI == null)
//             {
//                 statsUI = mainCanvas.gameObject.AddComponent<StatsAllocationUI>();
//             }

//             if (debugSetup) Debug.Log("🎛️ StatsAllocationUI component added");
//         }

//         // Auto-wire UI references directly
//         AutoWireUIReferences();
//     }

//     private void AutoWireUIReferences()
//     {
//         if (statsUI == null) return;

//         try
//         {
//             // Use reflection to set private fields
//             var statsUIType = typeof(StatsAllocationUI);

//             // Wire main panels
//             if (statsPanel != null)
//             {
//                 var statsPanelField = statsUIType.GetField("statsPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                 statsPanelField?.SetValue(statsUI, statsPanel);
//                 if (debugSetup) Debug.Log("📊 Wired stats panel");
//             }

//             if (levelUpPanel != null)
//             {
//                 var levelUpField = statsUIType.GetField("levelUpNotification", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                 levelUpField?.SetValue(statsUI, levelUpPanel);
//                 if (debugSetup) Debug.Log("⭐ Wired level up notification");

//                 // Wire notification text
//                 var notificationText = levelUpPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
//                 if (notificationText != null)
//                 {
//                     var notificationTextField = statsUIType.GetField("notificationText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                     notificationTextField?.SetValue(statsUI, notificationText);
//                     if (debugSetup) Debug.Log("� Wired notification text");
//                 }
//             }

//             // Wire available points text
//             var availablePointsText = FindTextByName("AvailablePointsText");
//             if (availablePointsText != null)
//             {
//                 var availablePointsField = statsUIType.GetField("availablePointsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//                 availablePointsField?.SetValue(statsUI, availablePointsText);
//                 if (debugSetup) Debug.Log("💎 Wired available points text");
//             }

//             if (debugSetup) Debug.Log("✅ UI Auto-wiring complete!");
//         }
//         catch (System.Exception e)
//         {
//             if (debugSetup) Debug.LogWarning($"⚠️ Auto-wiring failed: {e.Message}");
//         }
//     }

//     private TMPro.TextMeshProUGUI FindTextByName(string name)
//     {
//         GameObject foundObject = GameObject.Find(name);
//         if (foundObject == null)
//         {
//             // Search in children
//             var allTexts = mainCanvas.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
//             foreach (var text in allTexts)
//             {
//                 if (text.gameObject.name == name)
//                 {
//                     return text;
//                 }
//             }
//         }
//         return foundObject?.GetComponent<TMPro.TextMeshProUGUI>();
//     }

//     [ContextMenu("🧹 Clean Up Stats UI")]
//     public void CleanUpStatsUI()
//     {
//         if (statsPanel != null)
//         {
//             DestroyImmediate(statsPanel);
//             statsPanel = null;
//         }

//         if (levelUpPanel != null)
//         {
//             DestroyImmediate(levelUpPanel);
//             levelUpPanel = null;
//         }

//         if (debugSetup) Debug.Log("🧹 Stats UI cleaned up");
//     }
// }