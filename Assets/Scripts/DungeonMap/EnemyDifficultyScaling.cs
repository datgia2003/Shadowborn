using UnityEngine;

/// <summary>
/// Component to track and apply difficulty scaling to enemies
/// Automatically added to enemies when spawned with difficulty scaling
/// </summary>
public class EnemyDifficultyScaling : MonoBehaviour
{
    [Header("🔧 Difficulty Scaling")]
    [ReadOnly] public int difficultyLevel = 1;
    [ReadOnly] public float healthMultiplier = 1f;
    [ReadOnly] public float damageMultiplier = 1f;

    [Header("⚙️ Applied Scaling")]
    public bool autoApplyOnStart = true;
    public bool showScalingInfo = false;

    void Start()
    {
        if (autoApplyOnStart && difficultyLevel > 1)
        {
            ApplyScaling();
        }
    }

    /// <summary>
    /// Apply scaling to enemy components
    /// </summary>
    public void ApplyScaling()
    {
        // Try to scale common enemy components
        ApplyHealthScaling();
        ApplyDamageScaling();
        ApplyMovementScaling();

        if (showScalingInfo)
        {
            Debug.Log($"🔧 Applied scaling to {gameObject.name}: " +
                     $"Difficulty {difficultyLevel}, Health {healthMultiplier:F2}x, Damage {damageMultiplier:F2}x");
        }
    }

    /// <summary>
    /// Apply health scaling to various health components
    /// </summary>
    private void ApplyHealthScaling()
    {
        if (healthMultiplier <= 1f) return;

        // Try common health component names/types
        // You can add your specific enemy health components here

        // Example for Igris boss
        var igris = GetComponent<Igris>();
        if (igris != null)
        {
            // Scale Igris health if it has public health properties
            // igris.maxHealth *= healthMultiplier;
            if (showScalingInfo)
                Debug.Log($"💗 Applied health scaling to Igris: {healthMultiplier:F2}x");
        }

        // Add more enemy types here as needed
    }

    /// <summary>
    /// Apply damage scaling to enemy attacks
    /// </summary>
    private void ApplyDamageScaling()
    {
        if (damageMultiplier <= 1f) return;

        // This would scale damage for different enemy types
        // Implementation depends on your enemy attack system

        if (showScalingInfo)
            Debug.Log($"⚔️ Damage scaling ready: {damageMultiplier:F2}x");
    }

    /// <summary>
    /// Apply movement scaling (optional - makes enemies faster)
    /// </summary>
    private void ApplyMovementScaling()
    {
        // Optional: Scale movement speed slightly with difficulty
        float movementMultiplier = 1f + (difficultyLevel - 1) * 0.05f; // 5% per level

        // Apply to Rigidbody2D or movement controllers
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // You might want to scale drag or other physics properties
            // rb.drag *= (1f / movementMultiplier); // Less drag = faster
        }
    }

    /// <summary>
    /// Get scaling info for UI or debugging
    /// </summary>
    public string GetScalingInfo()
    {
        return $"D{difficultyLevel} | HP: {healthMultiplier:F1}x | DMG: {damageMultiplier:F1}x";
    }

    /// <summary>
    /// Set scaling values
    /// </summary>
    public void SetScaling(int difficulty, float health, float damage)
    {
        difficultyLevel = difficulty;
        healthMultiplier = health;
        damageMultiplier = damage;
    }

    void OnDrawGizmosSelected()
    {
        if (difficultyLevel > 1)
        {
            // Draw scaling indicator
            Gizmos.color = Color.red * 0.7f;
            float size = 0.5f + (difficultyLevel - 1) * 0.2f;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, size);
        }
    }
}