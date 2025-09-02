using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Centralized skill cooldown management system
/// Tracks cooldowns for all player skills with event notifications for UI
/// </summary>
public class SkillCooldownManager : MonoBehaviour
{
    [System.Serializable]
    public class SkillCooldownData
    {
        public string skillName;
        public float cooldownDuration;
        public KeyCode inputKey;
        [Tooltip("Resource cost for this skill")]
        public int manaCost;

        [Header("UI Display")]
        public Sprite skillIcon;
        public string displayName;
        [TextArea(2, 3)]
        public string description;
    }

    [Header("Skill Cooldown Configuration")]
    public SkillCooldownData[] skills = new SkillCooldownData[]
    {
        new SkillCooldownData { skillName = "SliceUp", cooldownDuration = 5f, inputKey = KeyCode.U, manaCost = 20, displayName = "Slice Up" },
        new SkillCooldownData { skillName = "SummonSkill", cooldownDuration = 8f, inputKey = KeyCode.I, manaCost = 30, displayName = "Summon" },
        new SkillCooldownData { skillName = "Ultimate", cooldownDuration = 15f, inputKey = KeyCode.O, manaCost = 50, displayName = "Ultimate" },
        new SkillCooldownData { skillName = "Dodge", cooldownDuration = 2f, inputKey = KeyCode.L, manaCost = 0, displayName = "Dodge" }
    };

    // Internal cooldown tracking
    private Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
    private Dictionary<string, float> skillLastUsed = new Dictionary<string, float>();

    // Event system for UI updates
    public static event Action<string, float, float> OnSkillCooldownChanged; // skillName, currentCooldown, maxCooldown
    public static event Action<string, float, float> OnCooldownUpdated; // skillName, currentCooldown, maxCooldown
    public static event Action<string> OnSkillReady; // skillName
    public static event Action<string> OnSkillUsed;  // skillName

    // References
    private PlayerResources playerResources;

    void Awake()
    {
        // Find PlayerResources - try same GameObject first, then search globally
        playerResources = GetComponent<PlayerResources>();
        if (playerResources == null)
        {
            playerResources = FindObjectOfType<PlayerResources>();
        }

        if (playerResources == null)
        {
            Debug.LogError("SkillCooldownManager: PlayerResources not found! Mana consumption will not work.");
        }

        // Initialize cooldown tracking
        foreach (var skill in skills)
        {
            skillCooldowns[skill.skillName] = 0f;
            skillLastUsed[skill.skillName] = -skill.cooldownDuration; // Allow immediate use
        }
    }

    void Update()
    {
        UpdateCooldowns();
    }

    private void UpdateCooldowns()
    {
        foreach (var skill in skills)
        {
            string skillName = skill.skillName;
            float timeSinceLastUse = Time.time - skillLastUsed[skillName];
            float remainingCooldown = Mathf.Max(0f, skill.cooldownDuration - timeSinceLastUse);

            bool wasOnCooldown = skillCooldowns[skillName] > 0f;
            skillCooldowns[skillName] = remainingCooldown;

            // Fire events for UI updates
            OnCooldownUpdated?.Invoke(skillName, remainingCooldown, skill.cooldownDuration);
            OnSkillCooldownChanged?.Invoke(skillName, remainingCooldown, skill.cooldownDuration);

            // Fire ready event when cooldown finishes
            if (wasOnCooldown && remainingCooldown <= 0f)
            {
                OnSkillReady?.Invoke(skillName);
            }
        }
    }

    /// <summary>
    /// Attempt to use a skill - checks cooldown and resource requirements
    /// </summary>
    public bool TryUseSkill(string skillName)
    {
        var skillData = GetSkillData(skillName);
        if (skillData == null) return false;

        // Check if skill is on cooldown
        if (IsOnCooldown(skillName))
        {
            return false;
        }

        // Check skill-specific requirements BEFORE consuming mana
        if (!CheckSkillSpecificRequirements(skillName))
        {
            return false;
        }

        // Check resource requirements
        if (skillData.manaCost > 0 && playerResources != null)
        {
            if (!playerResources.HasManaFor(skillData.manaCost))
            {
                return false;
            }

            // Consume mana
            bool manaConsumed = playerResources.TryConsumeMana(skillData.manaCost);
            if (!manaConsumed)
            {
                Debug.LogError($"Failed to consume mana for {skillName}! This should not happen after the check.");
                return false;
            }
        }

        // Use skill
        UseSkill(skillName);
        return true;
    }

    /// <summary>
    /// Check skill-specific requirements (enemy presence, etc.) before consuming resources
    /// </summary>
    private bool CheckSkillSpecificRequirements(string skillName)
    {
        switch (skillName)
        {
            case "Ultimate":
                var ultimateSkill = FindObjectOfType<TruthMultilateUltimate>();
                if (ultimateSkill != null)
                {
                    return ultimateSkill.CanUseSkill();
                }
                break;

            // Other skills don't have special requirements yet
            case "SliceUp":
            case "SummonSkill":
            case "Dodge":
            default:
                return true;
        }

        return true;
    }

    /// <summary>
    /// Mark a skill as used (starts cooldown)
    /// </summary>
    public void UseSkill(string skillName)
    {
        if (skillLastUsed.ContainsKey(skillName))
        {
            skillLastUsed[skillName] = Time.time;
            OnSkillUsed?.Invoke(skillName);
        }
    }

    /// <summary>
    /// Force set cooldown for a skill (useful for external systems)
    /// </summary>
    public void SetSkillCooldown(string skillName, float cooldownTime)
    {
        if (skillLastUsed.ContainsKey(skillName))
        {
            skillLastUsed[skillName] = Time.time - (GetSkillData(skillName)?.cooldownDuration ?? 0f) + cooldownTime;
        }
    }

    // Utility methods
    public bool IsOnCooldown(string skillName)
    {
        return skillCooldowns.ContainsKey(skillName) && skillCooldowns[skillName] > 0f;
    }

    public float GetRemainingCooldown(string skillName)
    {
        return skillCooldowns.ContainsKey(skillName) ? skillCooldowns[skillName] : 0f;
    }

    public float GetCooldownProgress(string skillName)
    {
        var skillData = GetSkillData(skillName);
        if (skillData == null) return 1f;

        float remaining = GetRemainingCooldown(skillName);
        return 1f - (remaining / skillData.cooldownDuration);
    }

    public SkillCooldownData GetSkillData(string skillName)
    {
        foreach (var skill in skills)
        {
            if (skill.skillName == skillName) return skill;
        }
        return null;
    }

    public SkillCooldownData[] GetAllSkills() => skills;

    /// <summary>
    /// Check if skill can be used (combines cooldown + resource checks)
    /// </summary>
    public bool CanUseSkill(string skillName)
    {
        var skillData = GetSkillData(skillName);
        if (skillData == null) return false;

        // Check cooldown
        if (IsOnCooldown(skillName)) return false;

        // Check resources
        if (skillData.manaCost > 0 && playerResources != null)
        {
            if (!playerResources.HasManaFor(skillData.manaCost)) return false;
        }

        return true;
    }

    /// <summary>
    /// Get skill availability status for UI
    /// </summary>
    public enum SkillAvailability
    {
        Available,      // Can use right now
        OnCooldown,     // Waiting for cooldown
        InsufficientMana, // Not enough mana
        Unavailable     // Other reasons (stunned, etc)
    }

    public SkillAvailability GetSkillAvailability(string skillName)
    {
        var skillData = GetSkillData(skillName);
        if (skillData == null) return SkillAvailability.Unavailable;

        if (IsOnCooldown(skillName)) return SkillAvailability.OnCooldown;

        if (skillData.manaCost > 0 && playerResources != null)
        {
            if (!playerResources.HasManaFor(skillData.manaCost))
                return SkillAvailability.InsufficientMana;
        }

        return SkillAvailability.Available;
    }
}
