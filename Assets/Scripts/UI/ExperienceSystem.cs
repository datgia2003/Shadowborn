using UnityEngine;
using System;

/// <summary>
/// Dedicated Experience and Leveling System
/// Handles all experience gain, level progression, and rewards
/// </summary>
public class ExperienceSystem : MonoBehaviour
{
    [Header("🌟 Experience Configuration")]
    [SerializeField] private int baseExpToNextLevel = 100;
    [SerializeField] private float expGrowthRate = 1.2f; // Exponential growth
    [SerializeField] private bool showDebugLogs = true;

    [Header("💰 Experience Rewards")]
    [SerializeField] private int batEnemyExp = 50;
    [SerializeField] private int bossEnemyExp = 200;
    [SerializeField] private int questCompleteExp = 100;
    [SerializeField] private int skillUseExp = 2;

    [Header("🎁 Level Up Rewards")]
    [SerializeField] private int healthBonusPerLevel = 10;
    [SerializeField] private int manaBonusPerLevel = 5;
    [SerializeField] private int energyBonusPerLevel = 3;
    [SerializeField] private int statPointsPerLevel = 3; // Solo Leveling style stat points

    // Current state
    [Header("📊 Current Status (Read Only)")]
    [SerializeField, ReadOnly] private int currentLevel = 1;
    [SerializeField, ReadOnly] private int currentExp = 0;
    [SerializeField, ReadOnly] private int expToNextLevel = 100;

    // System references
    private PlayerResources playerResources;

    // Events for UI updates
    public static event Action<int, int> OnExpChanged; // current exp, exp needed for next level
    public static event Action<int> OnLevelUp; // new level (single level up)
    public static event Action<int, int, int> OnMultiLevelUp; // startLevel, endLevel, totalPoints
    public static event Action<string, int> OnExpGained; // source, amount

    void Awake()
    {
        // Find player resources
        playerResources = FindObjectOfType<PlayerResources>();
        if (playerResources == null)
        {
            Debug.LogError("❌ ExperienceSystem: PlayerResources not found!");
        }

        // Calculate initial exp requirement
        CalculateExpToNextLevel();
    }

    void Start()
    {
        // Initialize with 0 exp and fire events
        currentExp = 0;
        currentLevel = 1;
        CalculateExpToNextLevel();

        // Fire initial events for UI setup
        OnExpChanged?.Invoke(currentExp, expToNextLevel);

        if (showDebugLogs)
        {
            Debug.Log($"⭐ ExperienceSystem initialized - Level {currentLevel}, 0/{expToNextLevel} EXP");
        }
    }

    private void CalculateExpToNextLevel()
    {
        expToNextLevel = Mathf.RoundToInt(baseExpToNextLevel * Mathf.Pow(expGrowthRate, currentLevel - 1));
    }

    /// <summary>
    /// Award experience from defeating enemies
    /// </summary>
    public void GainExpFromEnemy(string enemyType)
    {
        int expAmount = enemyType.ToLower() switch
        {
            "bat" => batEnemyExp,
            "boss" => bossEnemyExp,
            _ => 10 // Default fallback
        };

        GainExperience($"Enemy ({enemyType})", expAmount);
    }

    /// <summary>
    /// Award experience from completing quests
    /// </summary>
    public void GainExpFromQuest(string questName)
    {
        GainExperience($"Quest ({questName})", questCompleteExp);
    }

    /// <summary>
    /// Award experience from using skills
    /// </summary>
    public void GainExpFromSkillUse(string skillName)
    {
        GainExperience($"Skill ({skillName})", skillUseExp);
    }

    /// <summary>
    /// Generic experience gain method
    /// </summary>
    public void GainExperience(string source, int amount)
    {
        if (amount <= 0) return;

        currentExp += amount;

        if (showDebugLogs)
        {
            Debug.Log($"⭐ +{amount} EXP from {source} → {currentExp}/{expToNextLevel}");
        }

        // Fire exp gained event for UI effects
        OnExpGained?.Invoke(source, amount);

        // Check for level up
        CheckForLevelUp();

        // Update UI
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    private void CheckForLevelUp()
    {
        int levelsGained = 0;
        int startLevel = currentLevel;

        while (currentExp >= expToNextLevel)
        {
            LevelUp();
            levelsGained++;
        }

        // Fire appropriate events based on levels gained
        if (levelsGained > 0)
        {
            int totalPointsAwarded = levelsGained * statPointsPerLevel;
            Debug.Log($"🎉 LEVEL UP! Gained {levelsGained} levels ({startLevel}→{currentLevel}), Total points: {totalPointsAwarded}");

            // Award stat points to PlayerStats system (total for all levels gained)
            var playerStats = FindObjectOfType<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.AddAvailablePoints(totalPointsAwarded);
                Debug.Log($"🎯 Awarded {totalPointsAwarded} total stat points for {levelsGained} levels!");
            }
            else
            {
                Debug.LogWarning("⚠️ PlayerStats not found - stat points not awarded!");
            }

            if (levelsGained == 1)
            {
                // Single level up - fire both events for compatibility
                OnLevelUp?.Invoke(currentLevel);
                OnMultiLevelUp?.Invoke(startLevel, currentLevel, totalPointsAwarded);
            }
            else
            {
                // Multi level up - fire multi event only
                OnMultiLevelUp?.Invoke(startLevel, currentLevel, totalPointsAwarded);
            }
        }
    }

    private void LevelUp()
    {
        // Subtract exp used for level up
        currentExp -= expToNextLevel;
        currentLevel++;

        // Recalculate next level requirement
        CalculateExpToNextLevel();

        // Apply level up bonuses (but don't fire individual notifications here)
        ApplyLevelUpBonuses();

        if (showDebugLogs)
        {
            Debug.Log($"🎉 LEVEL UP! Now Level {currentLevel} → Next: {expToNextLevel} EXP");
        }
    }

    private void ApplyLevelUpBonuses()
    {
        if (playerResources != null)
        {
            // Apply stat bonuses directly to public fields
            int oldHealth = playerResources.maxHealth;
            int oldMana = playerResources.maxMana;
            int oldEnergy = playerResources.maxEnergy;

            // Health bonus from level up (separate from VIT bonus)
            playerResources.maxHealth += healthBonusPerLevel;
            playerResources.maxMana += manaBonusPerLevel;
            playerResources.maxEnergy += energyBonusPerLevel;

            // Also restore full health/mana/energy on level up
            playerResources.AddHealth(healthBonusPerLevel); // Add the bonus health
            playerResources.AddMana(manaBonusPerLevel);     // Add the bonus mana
            playerResources.AddEnergy(energyBonusPerLevel); // Add the bonus energy

            if (showDebugLogs)
            {
                Debug.Log($"💪 Level {currentLevel} bonuses applied:");
                Debug.Log($"   Health: {oldHealth} → {playerResources.maxHealth} (+{healthBonusPerLevel})");
                Debug.Log($"   Mana: {oldMana} → {playerResources.maxMana} (+{manaBonusPerLevel})");
                Debug.Log($"   Energy: {oldEnergy} → {playerResources.maxEnergy} (+{energyBonusPerLevel})");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerResources not found - level up bonuses not applied!");
        }

        // Note: Stat points now awarded in GainExperience for total levels gained
    }

    // Public getters for UI
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;
    public float GetExpProgress() => (float)currentExp / expToNextLevel;

    /// <summary>
    /// Test method to force level up for debugging
    /// </summary>
    [ContextMenu("Test Level Up")]
    public void TestLevelUp()
    {
        GainExperience("Test", expToNextLevel);
    }
}
