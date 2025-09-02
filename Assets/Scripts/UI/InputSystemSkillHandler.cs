using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System Skill Handler - Works with PlayerInput component
/// Add this to the same GameObject as PlayerInput to handle skill inputs properly
/// Ensures skills go through UI system with mana consumption and cooldowns
/// </summary>
public class InputSystemSkillHandler : MonoBehaviour
{
    [Header("Skill Input Settings")]
    public bool enableSkillInput = true;

    [Header("Component References")]
    public UIManager uiManager;
    public SkillCooldownManager cooldownManager;
    public PlayerResources playerResources;

    private void Start()
    {
        // Auto-find components if not assigned
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();

        if (cooldownManager == null)
            cooldownManager = FindObjectOfType<SkillCooldownManager>();

        if (playerResources == null)
            playerResources = FindObjectOfType<PlayerResources>();
    }

    // These methods are called by PlayerInput when the corresponding actions are triggered
    // Make sure your Input Actions are named exactly like these methods (without "On" prefix)

    public void OnSliceUp(InputValue value)
    {
        if (!enableSkillInput || !value.isPressed) return;
        HandleSkillInput("SliceUp");
    }

    public void OnSummonSkill(InputValue value)
    {
        if (!enableSkillInput || !value.isPressed) return;
        HandleSkillInput("SummonSkill");
    }

    public void OnUltimate(InputValue value)
    {
        if (!enableSkillInput || !value.isPressed) return;
        HandleSkillInput("Ultimate");
    }

    public void OnDodge(InputValue value)
    {
        if (!enableSkillInput || !value.isPressed) return;
        HandleSkillInput("Dodge");
    }

    private void HandleSkillInput(string skillName)
    {
        if (cooldownManager == null || uiManager == null)
            return;

        // Check if we have enough mana BEFORE trying to use skill
        var skillData = cooldownManager.GetSkillData(skillName);
        if (skillData != null && skillData.manaCost > 0)
        {
            if (playerResources != null && !playerResources.HasManaFor(skillData.manaCost))
                return;
        }

        // Check cooldown
        if (cooldownManager.IsOnCooldown(skillName))
            return;

        // Try to use skill through UI system
        bool success = cooldownManager.TryUseSkill(skillName);
        if (success)
        {
            // Trigger the actual skill effect
            uiManager.TriggerSkillAction(skillName);
        }
    }
}