using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input Manager for Stats Panel using Unity's New Input System
/// Handles Tab key toggle for stats panel
/// </summary>
public class StatsInputManager : MonoBehaviour
{
    [Header("🎮 Input Settings")]
    [SerializeField] private InputActionReference statsToggleAction;

    private StatsPanel statsPanel;

    void Start()
    {
        // Find StatsPanel in the scene
        FindStatsPanel();

        // Enable input action
        if (statsToggleAction != null)
        {
            statsToggleAction.action.Enable();
            statsToggleAction.action.performed += OnStatsToggle;
            Debug.Log("🎮 StatsInputManager: Input action enabled");
        }
        else
        {
            Debug.LogError("🎮 StatsInputManager: statsToggleAction is not assigned!");
        }
    }

    void OnDestroy()
    {
        // Disable input action
        if (statsToggleAction != null)
        {
            statsToggleAction.action.performed -= OnStatsToggle;
            statsToggleAction.action.Disable();
        }
    }

    /// <summary>
    /// Find StatsPanel in the scene (avoid affecting PlayerHUD)
    /// </summary>
    private void FindStatsPanel()
    {
        // Method 1: Try to find by exact GameObject name
        GameObject statsPanelObject = GameObject.Find("StatsPanel");
        if (statsPanelObject != null)
        {
            statsPanel = statsPanelObject.GetComponent<StatsPanel>();
            if (statsPanel != null)
            {
                Debug.Log($"🎮 StatsInputManager: Found StatsPanel by name on {statsPanel.gameObject.name}");
                return;
            }
        }

        // Method 2: Find first StatsPanel component in scene
        statsPanel = FindObjectOfType<StatsPanel>();
        if (statsPanel != null)
        {
            Debug.Log($"🎮 StatsInputManager: Found StatsPanel component on {statsPanel.gameObject.name}");
            return;
        }

        // Method 3: Search all GameObjects with "Stats" in name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Stats") && obj.GetComponent<StatsPanel>() != null)
            {
                statsPanel = obj.GetComponent<StatsPanel>();
                Debug.Log($"🎮 StatsInputManager: Found StatsPanel on object containing 'Stats': {statsPanel.gameObject.name}");
                return;
            }
        }

        Debug.LogError("🎮 StatsInputManager: Could not find StatsPanel component! Make sure there's a GameObject with StatsPanel script in the scene.");
    }    /// <summary>
         /// Called when stats toggle input is performed (Tab key)
         /// </summary>
    private void OnStatsToggle(InputAction.CallbackContext context)
    {
        if (statsPanel != null)
        {
            statsPanel.TogglePanel();
            Debug.Log($"🎮 Stats Panel Toggled - Now {(statsPanel.IsOpen ? "Open" : "Closed")}");
        }
        else
        {
            Debug.LogWarning("🎮 StatsInputManager: StatsPanel reference is null! Attempting to find it...");
            // Try to find it again
            FindStatsPanel();

            // If found, try to toggle
            if (statsPanel != null)
            {
                statsPanel.TogglePanel();
                Debug.Log($"🎮 Stats Panel Found and Toggled - Now {(statsPanel.IsOpen ? "Open" : "Closed")}");
            }
            else
            {
                Debug.LogError("🎮 StatsInputManager: Still cannot find StatsPanel! Please check scene setup.");
            }
        }
    }

    /// <summary>
    /// Manual setup method for InputActionReference
    /// </summary>
    public void SetStatsToggleAction(InputActionReference actionRef)
    {
        if (statsToggleAction != null)
        {
            statsToggleAction.action.performed -= OnStatsToggle;
            statsToggleAction.action.Disable();
        }

        statsToggleAction = actionRef;

        if (statsToggleAction != null)
        {
            statsToggleAction.action.Enable();
            statsToggleAction.action.performed += OnStatsToggle;
            Debug.Log("🎮 StatsInputManager: New input action assigned");
        }
    }

    /// <summary>
    /// Test method to verify setup
    /// </summary>
    [ContextMenu("Test Find StatsPanel")]
    public void TestFindStatsPanel()
    {
        Debug.Log("🎮 Testing StatsPanel search...");
        FindStatsPanel();

        if (statsPanel != null)
        {
            Debug.Log($"✅ SUCCESS: Found StatsPanel on {statsPanel.gameObject.name}");
        }
        else
        {
            Debug.LogError("❌ FAILED: Could not find StatsPanel");

            // Debug info
            StatsPanel[] allStatsPanels = FindObjectsOfType<StatsPanel>();
            Debug.Log($"🔍 Total StatsPanel components in scene: {allStatsPanels.Length}");

            for (int i = 0; i < allStatsPanels.Length; i++)
            {
                Debug.Log($"  {i}: {allStatsPanels[i].gameObject.name} (Active: {allStatsPanels[i].gameObject.activeInHierarchy})");
            }
        }
    }

    /// <summary>
    /// Manual test toggle
    /// </summary>
    [ContextMenu("Test Toggle Panel")]
    public void TestTogglePanel()
    {
        if (statsPanel == null)
        {
            FindStatsPanel();
        }

        if (statsPanel != null)
        {
            statsPanel.TogglePanel();
            Debug.Log($"🎮 Manual Test Toggle - Panel is now {(statsPanel.IsOpen ? "Open" : "Closed")}");
        }
        else
        {
            Debug.LogError("🎮 Cannot test toggle - StatsPanel not found!");
        }
    }
}
