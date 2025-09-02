using UnityEngine;
using System;

/// <summary>
/// Dedicated Experience and Leveling System
/// Handles all experience gain, level progression, and rewards
/// </summary>
public class ExperienceSystem : MonoBehaviour
{
    // Singleton pattern
    public static ExperienceSystem Instance { get; private set; }

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

    // Current state
    [Header("📊 Current Status (Read Only)")]
    [SerializeField, ReadOnly] private int currentLevel = 1;
    [SerializeField, ReadOnly] private int currentExp = 0;
    [SerializeField, ReadOnly] private int expToNextLevel = 100;

    // System references
    private PlayerResources playerResources;

    // Events for UI updates
    public static event Action<int, int> OnExpChanged; // current exp, exp needed for next level
    public static event Action<int> OnLevelUp; // new level
    public static event Action<string, int> OnExpGained; // source, amount

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

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
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Subtract exp used for level up
        currentExp -= expToNextLevel;
        currentLevel++;

        // Recalculate next level requirement
        CalculateExpToNextLevel();

        // Apply level up bonuses
        ApplyLevelUpBonuses();

        // Fire level up event
        OnLevelUp?.Invoke(currentLevel);

        if (showDebugLogs)
        {
            Debug.Log($"🎉 LEVEL UP! Now Level {currentLevel} → Next: {expToNextLevel} EXP");
        }
    }

    private void ApplyLevelUpBonuses()
    {
        if (playerResources != null)
        {
            // Get current values for logging
            int oldHealth = playerResources.GetMaxHealth();
            int oldMana = playerResources.GetMaxMana();
            int oldEnergy = playerResources.maxEnergy;

            // Apply bonuses - the methods will add to base values
            playerResources.AddMaxHealth(healthBonusPerLevel);
            playerResources.AddMaxMana(manaBonusPerLevel);
            playerResources.maxEnergy += energyBonusPerLevel;

            // Also restore health/mana/energy on level up
            playerResources.AddHealth(healthBonusPerLevel);
            playerResources.AddMana(manaBonusPerLevel);
            playerResources.AddEnergy(energyBonusPerLevel);

            if (showDebugLogs)
            {
                Debug.Log($"💪 Level {currentLevel} bonuses applied:");
                Debug.Log($"   Health: {oldHealth} → {playerResources.GetMaxHealth()} (+{healthBonusPerLevel})");
                Debug.Log($"   Mana: {oldMana} → {playerResources.GetMaxMana()} (+{manaBonusPerLevel})");
                Debug.Log($"   Energy: {oldEnergy} → {playerResources.maxEnergy} (+{energyBonusPerLevel})");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ PlayerResources not found - level up bonuses not applied!");
        }
    }    // Public getters for UI
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;
    public float GetExpProgress() => (float)currentExp / expToNextLevel;
}
