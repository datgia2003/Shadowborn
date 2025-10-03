using UnityEngine;

/// <summary>
/// Auto-setup script for Enemy Health Bars
/// Attach this to enemy prefabs to automatically configure health bar settings
/// </summary>
public class EnemyHealthBarSetup : MonoBehaviour
{
    [Header("Health Bar Configuration")]
    public bool enableHealthBar = true;
    public bool showOnlyWhenDamaged = true;
    public bool alwaysVisible = false;
    public float hideDelay = 3f;
    
    [Header("Visual Settings")]
    public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    public Vector3 healthBarScale = Vector3.one;
    public bool scaleWithDistance = false;
    
    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    
    [Header("Animation")]
    public float animationSpeed = 5f;
    public float colorTransitionSpeed = 3f;

    void Awake()
    {
        if (!enableHealthBar) return;
        
        SetupHealthBar();
    }

    void SetupHealthBar()
    {
        // Check if health bar already exists
        EnemyHealthBar existingHealthBar = GetComponent<EnemyHealthBar>();
        if (existingHealthBar != null)
        {
            // Configure existing health bar
            ConfigureHealthBar(existingHealthBar);
            return;
        }
        
        // Add health bar component
        EnemyHealthBar healthBar = gameObject.AddComponent<EnemyHealthBar>();
        ConfigureHealthBar(healthBar);
        
        Debug.Log($"[EnemyHealthBarSetup] Added and configured health bar for {gameObject.name}");
    }
    
    void ConfigureHealthBar(EnemyHealthBar healthBar)
    {
        // Configure settings
        healthBar.showOnlyWhenDamaged = showOnlyWhenDamaged;
        healthBar.alwaysVisible = alwaysVisible;
        healthBar.hideDelay = hideDelay;
        
        // Configure visual settings
        healthBar.offset = healthBarOffset;
        healthBar.healthBarScale = healthBarScale;
        healthBar.scaleWithDistance = scaleWithDistance;
        
        // Configure colors
        healthBar.fullHealthColor = fullHealthColor;
        healthBar.midHealthColor = midHealthColor;
        healthBar.lowHealthColor = lowHealthColor;
        
        // Configure animation
        healthBar.animationSpeed = animationSpeed;
        healthBar.colorTransitionSpeed = colorTransitionSpeed;
    }

    [ContextMenu("Setup Health Bar")]
    public void ManualSetup()
    {
        if (enableHealthBar)
            SetupHealthBar();
        else
            RemoveHealthBar();
    }
    
    void RemoveHealthBar()
    {
        EnemyHealthBar healthBar = GetComponent<EnemyHealthBar>();
        if (healthBar != null)
        {
            if (Application.isPlaying)
                Destroy(healthBar);
            else
                DestroyImmediate(healthBar);
            
            Debug.Log($"[EnemyHealthBarSetup] Removed health bar from {gameObject.name}");
        }
    }
}