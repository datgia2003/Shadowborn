using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Integration bridge between existing skill systems and new UI/resource management
/// Retrofits existing skills to work with the new UI system
/// </summary>
public class SkillUIIntegration : MonoBehaviour
{
    [Header("Integration Settings")]
    public bool interceptSkillInputs = true;
    public bool enforceResourceCosts = true;
    public bool enforceCooldowns = true;

    // System references
    private PlayerResources playerResources;
    private SkillCooldownManager cooldownManager;
    private UIManager uiManager;

    // Existing skill references
    private SliceUpSkill sliceUpSkill;
    private SummonSkill summonSkill;
    private TruthMultilateUltimate ultimateSkill;

    void Awake()
    {
        // Find system references
        playerResources = GetComponent<PlayerResources>();
        cooldownManager = GetComponent<SkillCooldownManager>();
        uiManager = FindObjectOfType<UIManager>();

        // Find existing skill components
        sliceUpSkill = GetComponent<SliceUpSkill>();
        summonSkill = GetComponent<SummonSkill>();
        ultimateSkill = GetComponent<TruthMultilateUltimate>();
    }

    void Start()
    {
        // Ensure systems are initialized
        if (playerResources == null)
        {
            Debug.LogError("PlayerResources component required on same GameObject!");
        }

        if (cooldownManager == null)
        {
            Debug.LogError("SkillCooldownManager component required on same GameObject!");
        }
    }

    // Integration methods for existing skill input handlers

    /// <summary>
    /// Enhanced SliceUp handler with resource/cooldown checking
    /// Call this instead of the original OnSliceUp
    /// </summary>
    public void OnSliceUpIntegrated(InputValue value)
    {
        if (!value.isPressed) return;

        if (interceptSkillInputs)
        {
            // Use new system
            if (cooldownManager != null && cooldownManager.TryUseSkill("SliceUp"))
            {
                ExecuteSliceUp();
            }
        }
        else
        {
            // Fallback to original system
            if (sliceUpSkill != null)
            {
                sliceUpSkill.OnSliceUp(value);
            }
        }
    }

    /// <summary>
    /// Enhanced Summon handler with resource/cooldown checking
    /// </summary>
    public void OnSummonSkillIntegrated(InputValue value)
    {
        if (!value.isPressed) return;

        if (interceptSkillInputs)
        {
            if (cooldownManager != null && cooldownManager.TryUseSkill("SummonSkill"))
            {
                ExecuteSummonSkill();
            }
        }
        else
        {
            if (summonSkill != null)
            {
                summonSkill.OnSummonSkill(value);
            }
        }
    }

    /// <summary>
    /// Enhanced Ultimate handler with resource/cooldown checking
    /// </summary>
    public void OnUltimateIntegrated(InputValue value)
    {
        if (!value.isPressed) return;

        if (interceptSkillInputs)
        {
            if (cooldownManager != null && cooldownManager.TryUseSkill("Ultimate"))
            {
                ExecuteUltimate();
            }
        }
        else
        {
            if (ultimateSkill != null)
            {
                ultimateSkill.StartUltimate();
            }
        }
    }

    // Skill execution methods (called after resource/cooldown checks pass)

    private void ExecuteSliceUp()
    {
        if (sliceUpSkill != null)
        {
            // Use the existing OnSliceUp method which has all the checks
            // Create a mock InputValue to trigger the skill
            var mockInput = System.Activator.CreateInstance<InputValue>();
            sliceUpSkill.OnSliceUp(mockInput);
            Debug.Log("SliceUp executed via UI system");
        }
    }

    private void ExecuteSummonSkill()
    {
        if (summonSkill != null)
        {
            // Create mock InputValue for existing system
            var mockInput = new UnityEngine.InputSystem.InputValue();
            summonSkill.OnSummonSkill(mockInput);
            Debug.Log("Summon executed via UI system");
        }
    }

    private void ExecuteUltimate()
    {
        if (ultimateSkill != null)
        {
            // Use the existing StartUltimate method which has all the checks
            ultimateSkill.StartUltimate();
            Debug.Log("Ultimate executed via UI system");
        }
    }

    // Public methods for manual skill triggering (useful for UI buttons)

    public bool TryTriggerSliceUp()
    {
        if (cooldownManager != null && cooldownManager.TryUseSkill("SliceUp"))
        {
            ExecuteSliceUp();
            return true;
        }
        return false;
    }

    public bool TryTriggerSummon()
    {
        if (cooldownManager != null && cooldownManager.TryUseSkill("SummonSkill"))
        {
            ExecuteSummonSkill();
            return true;
        }
        return false;
    }

    public bool TryTriggerUltimate()
    {
        if (cooldownManager != null && cooldownManager.TryUseSkill("Ultimate"))
        {
            ExecuteUltimate();
            return true;
        }
        return false;
    }

    // Utility methods for checking skill availability

    public bool CanUseSliceUp()
    {
        if (cooldownManager == null) return false;

        // Just check the cooldown/resource system - let the skill handle its own validation
        return cooldownManager.CanUseSkill("SliceUp");
    }

    public bool CanUseSummon()
    {
        if (cooldownManager == null) return false;
        return cooldownManager.CanUseSkill("SummonSkill");
    }

    public bool CanUseUltimate()
    {
        if (cooldownManager == null) return false;

        // Just check the cooldown/resource system - let the skill handle its own validation
        return cooldownManager.CanUseSkill("Ultimate");
    }

    // Event handlers for automatic cooldown management

    void OnEnable()
    {
        // Listen for skill usage to automatically start cooldowns
        // This is useful if skills are triggered outside the UI system
    }

    void OnDisable()
    {
        // Clean up event subscriptions
    }

    // Debug/Testing methods

    [ContextMenu("Test SliceUp")]
    public void TestSliceUp()
    {
        bool success = TryTriggerSliceUp();
        Debug.Log($"SliceUp test: {(success ? "Success" : "Failed")}");
    }

    [ContextMenu("Test Summon")]
    public void TestSummon()
    {
        bool success = TryTriggerSummon();
        Debug.Log($"Summon test: {(success ? "Success" : "Failed")}");
    }

    [ContextMenu("Test Ultimate")]
    public void TestUltimate()
    {
        bool success = TryTriggerUltimate();
        Debug.Log($"Ultimate test: {(success ? "Success" : "Failed")}");
    }
}
