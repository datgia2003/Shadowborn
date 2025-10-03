using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility để bulk setup health bars cho enemies
/// Menu: Tools > Shadowborn > Enemy Health Bar Tools
/// </summary>
public class EnemyHealthBarTools : MonoBehaviour
{
    [MenuItem("Tools/Shadowborn/Add Health Bars to All Enemies")]
    static void AddHealthBarsToAllEnemies()
    {
        // Find all BatController and SkeletonController in scene
        BatController[] bats = FindObjectsOfType<BatController>();
        SkeletonController[] skeletons = FindObjectsOfType<SkeletonController>();
        
        int addedCount = 0;
        
        // Setup health bars for bats
        foreach (BatController bat in bats)
        {
            if (SetupHealthBarForEnemy(bat.gameObject, "Bat"))
                addedCount++;
        }
        
        // Setup health bars for skeletons
        foreach (SkeletonController skeleton in skeletons)
        {
            if (SetupHealthBarForEnemy(skeleton.gameObject, "Skeleton"))
                addedCount++;
        }
        
        Debug.Log($"[EnemyHealthBarTools] Added health bars to {addedCount} enemies");
        
        if (addedCount > 0)
        {
            EditorUtility.DisplayDialog("Health Bar Setup Complete", 
                $"Successfully added health bars to {addedCount} enemies!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Health Bar Setup", 
                "No enemies found or all enemies already have health bars.", "OK");
        }
    }
    
    [MenuItem("Tools/Shadowborn/Remove Health Bars from All Enemies")]
    static void RemoveHealthBarsFromAllEnemies()
    {
        EnemyHealthBar[] healthBars = FindObjectsOfType<EnemyHealthBar>();
        EnemyHealthBarSetup[] setupScripts = FindObjectsOfType<EnemyHealthBarSetup>();
        
        int removedCount = 0;
        
        // Remove health bar components
        foreach (EnemyHealthBar healthBar in healthBars)
        {
            DestroyImmediate(healthBar);
            removedCount++;
        }
        
        // Remove setup scripts
        foreach (EnemyHealthBarSetup setup in setupScripts)
        {
            DestroyImmediate(setup);
        }
        
        Debug.Log($"[EnemyHealthBarTools] Removed {removedCount} health bars");
        
        EditorUtility.DisplayDialog("Health Bar Removal Complete", 
            $"Removed {removedCount} health bars from enemies.", "OK");
    }
    
    [MenuItem("Tools/Shadowborn/Configure Health Bar Settings")]
    static void OpenHealthBarSettings()
    {
        // Create a simple settings window
        EnemyHealthBarSettingsWindow.ShowWindow();
    }
    
    static bool SetupHealthBarForEnemy(GameObject enemy, string enemyType)
    {
        // Check if already has health bar
        if (enemy.GetComponent<EnemyHealthBar>() != null)
            return false;
            
        // Add setup script
        EnemyHealthBarSetup setup = enemy.GetComponent<EnemyHealthBarSetup>();
        if (setup == null)
        {
            setup = enemy.AddComponent<EnemyHealthBarSetup>();
        }
        
        // Configure based on enemy type
        ConfigureForEnemyType(setup, enemyType);
        
        // Setup the health bar
        setup.ManualSetup();
        
        Debug.Log($"[EnemyHealthBarTools] Added health bar to {enemy.name} ({enemyType})");
        return true;
    }
    
    static void ConfigureForEnemyType(EnemyHealthBarSetup setup, string enemyType)
    {
        switch (enemyType.ToLower())
        {
            case "bat":
                setup.healthBarOffset = new Vector3(0, 1.2f, 0);
                setup.healthBarScale = Vector3.one * 0.8f;
                setup.hideDelay = 2f;
                break;
                
            case "skeleton":
                setup.healthBarOffset = new Vector3(0, 1.8f, 0);
                setup.healthBarScale = Vector3.one;
                setup.hideDelay = 3f;
                break;
                
            default:
                // Default settings already applied
                break;
        }
    }
}

/// <summary>
/// Simple settings window for health bar configuration
/// </summary>
public class EnemyHealthBarSettingsWindow : EditorWindow
{
    private static bool showOnlyWhenDamaged = true;
    private static bool alwaysVisible = false;
    private static float hideDelay = 3f;
    private static Vector3 offset = new Vector3(0, 1.5f, 0);
    private static Vector3 scale = Vector3.one;
    private static Color fullHealthColor = Color.green;
    private static Color midHealthColor = Color.yellow;
    private static Color lowHealthColor = Color.red;
    
    public static void ShowWindow()
    {
        GetWindow<EnemyHealthBarSettingsWindow>("Health Bar Settings");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Enemy Health Bar Settings", EditorStyles.boldLabel);
        
        showOnlyWhenDamaged = EditorGUILayout.Toggle("Show Only When Damaged", showOnlyWhenDamaged);
        alwaysVisible = EditorGUILayout.Toggle("Always Visible", alwaysVisible);
        hideDelay = EditorGUILayout.FloatField("Hide Delay", hideDelay);
        
        EditorGUILayout.Space();
        
        offset = EditorGUILayout.Vector3Field("Offset", offset);
        scale = EditorGUILayout.Vector3Field("Scale", scale);
        
        EditorGUILayout.Space();
        
        fullHealthColor = EditorGUILayout.ColorField("Full Health Color", fullHealthColor);
        midHealthColor = EditorGUILayout.ColorField("Mid Health Color", midHealthColor);
        lowHealthColor = EditorGUILayout.ColorField("Low Health Color", lowHealthColor);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply to All Enemies"))
        {
            ApplySettingsToAllEnemies();
        }
        
        if (GUILayout.Button("Apply to Selected Enemies"))
        {
            ApplySettingsToSelectedEnemies();
        }
    }
    
    void ApplySettingsToAllEnemies()
    {
        EnemyHealthBarSetup[] setups = FindObjectsOfType<EnemyHealthBarSetup>();
        
        foreach (EnemyHealthBarSetup setup in setups)
        {
            ApplySettings(setup);
        }
        
        Debug.Log($"[EnemyHealthBarSettings] Applied settings to {setups.Length} enemies");
    }
    
    void ApplySettingsToSelectedEnemies()
    {
        int appliedCount = 0;
        
        foreach (GameObject obj in Selection.gameObjects)
        {
            EnemyHealthBarSetup setup = obj.GetComponent<EnemyHealthBarSetup>();
            if (setup != null)
            {
                ApplySettings(setup);
                appliedCount++;
            }
        }
        
        Debug.Log($"[EnemyHealthBarSettings] Applied settings to {appliedCount} selected enemies");
    }
    
    void ApplySettings(EnemyHealthBarSetup setup)
    {
        setup.showOnlyWhenDamaged = showOnlyWhenDamaged;
        setup.alwaysVisible = alwaysVisible;
        setup.hideDelay = hideDelay;
        setup.healthBarOffset = offset;
        setup.healthBarScale = scale;
        setup.fullHealthColor = fullHealthColor;
        setup.midHealthColor = midHealthColor;
        setup.lowHealthColor = lowHealthColor;
        
        // Re-setup the health bar with new settings
        setup.ManualSetup();
    }
}