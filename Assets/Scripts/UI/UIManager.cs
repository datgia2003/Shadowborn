using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central UI manager that coordinates all UI systems
/// Handles skill bar setup, HUD initialization, and cross-system communication
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public EnhancedPlayerHUD playerHUD;
    public SkillUISlot[] skillSlots;
    public Canvas mainCanvas;

    [Header("UI Configuration")]
    public bool showHUDAtStart = true;
    public bool enableSkillBarInput = true;

    [Header("Input Actions")]
    public InputActionReference sliceUpAction;
    public InputActionReference summonSkillAction;
    public InputActionReference ultimateAction;

    [Header("Alternative Input Manager")]
    public bool useInputManager = false; // Option to use InputManager instead of direct references

    // System references
    private PlayerResources playerResources;
    private SkillCooldownManager cooldownManager;
    private InputManager inputManager;

    void Awake()
    {
        // Ensure UI persists across scene loads (optional)
        // DontDestroyOnLoad(gameObject);

        // Initialize systems order
        InitializeSystems();
    }

    void Start()
    {
        // Setup UI after all systems are initialized
        SetupUI();
    }

    private void InitializeSystems()
    {
        // Find or create required systems
        playerResources = FindObjectOfType<PlayerResources>();
        if (playerResources == null)
        {
            Debug.LogWarning("PlayerResources not found! UI may not function properly.");
        }

        cooldownManager = FindObjectOfType<SkillCooldownManager>();
        if (cooldownManager == null)
        {
            Debug.LogWarning("SkillCooldownManager not found! Skill UI may not function properly.");
        }

        // Find InputManager if using InputManager approach
        if (useInputManager)
        {
            inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                Debug.LogWarning("InputManager not found! Creating one...");
                var inputManagerObj = new GameObject("InputManager");
                inputManager = inputManagerObj.AddComponent<InputManager>();
            }
        }
    }

    private void SetupUI()
    {
        // Setup skill bar
        SetupSkillBar();

        // Initialize HUD visibility
        if (playerHUD != null)
        {
            playerHUD.gameObject.SetActive(showHUDAtStart);
        }

        // Setup main canvas settings
        if (mainCanvas != null)
        {
            mainCanvas.sortingOrder = 100; // Ensure UI is always on top
        }
    }

    private void SetupSkillBar()
    {
        if (cooldownManager == null || skillSlots == null) return;

        var allSkills = cooldownManager.GetAllSkills();

        // Assign skills to slots
        for (int i = 0; i < skillSlots.Length && i < allSkills.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                var skill = allSkills[i];
                skillSlots[i].SetSkillData(skill.skillName, skill.skillIcon, skill.inputKey, skill.manaCost);
            }
        }
    }

    void Update()
    {
        // Handle UI input (optional - skills can also be handled by existing input system)
        if (enableSkillBarInput)
        {
            HandleSkillBarInput();
        }

        // INPUT SYSTEM: Use proper Input System approach instead of old Input class
        // Skills will be handled through Input Actions or PlayerInput callbacks
        // This prevents the InvalidOperationException when using Input System package
    }

    private void HandleDirectKeyboardInput()
    {
        // REMOVED: This method used old Input.GetKeyDown() which conflicts with Input System
        // Skills are now handled through Input Actions or PlayerInput callbacks
        // See HandleInputWithActionReferences() for proper Input System implementation
    }

    private void HandleSkillBarInput()
    {
        if (cooldownManager == null) return;

        // Use InputManager if available and enabled
        if (useInputManager && inputManager != null)
        {
            HandleInputWithInputManager();
        }
        else
        {
            HandleInputWithActionReferences();
        }
    }

    private void HandleInputWithInputManager()
    {
        // SliceUp
        if (inputManager.IsSkillPressed("sliceup"))
        {
            bool success = cooldownManager.TryUseSkill("SliceUp");
            if (success)
            {
                TriggerSkillAction("SliceUp");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("SliceUp");
                HandleSkillFailure("SliceUp", availability);
            }
        }

        // SummonSkill
        if (inputManager.IsSkillPressed("summonskill"))
        {
            bool success = cooldownManager.TryUseSkill("SummonSkill");
            if (success)
            {
                TriggerSkillAction("SummonSkill");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("SummonSkill");
                HandleSkillFailure("SummonSkill", availability);
            }
        }

        // Ultimate
        if (inputManager.IsSkillPressed("ultimate"))
        {
            bool success = cooldownManager.TryUseSkill("Ultimate");
            if (success)
            {
                TriggerSkillAction("Ultimate");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("Ultimate");
                HandleSkillFailure("Ultimate", availability);
            }
        }
    }

    private void HandleInputWithActionReferences()
    {
        // Check for skill inputs using Input Actions
        // SliceUp (U key)
        if (sliceUpAction != null && sliceUpAction.action.WasPressedThisFrame())
        {
            bool success = cooldownManager.TryUseSkill("SliceUp");
            if (success)
            {
                TriggerSkillAction("SliceUp");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("SliceUp");
                HandleSkillFailure("SliceUp", availability);
            }
        }

        // SummonSkill (I key)
        if (summonSkillAction != null && summonSkillAction.action.WasPressedThisFrame())
        {
            bool success = cooldownManager.TryUseSkill("SummonSkill");
            if (success)
            {
                TriggerSkillAction("SummonSkill");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("SummonSkill");
                HandleSkillFailure("SummonSkill", availability);
            }
        }

        // Ultimate (O key)
        if (ultimateAction != null && ultimateAction.action.WasPressedThisFrame())
        {
            bool success = cooldownManager.TryUseSkill("Ultimate");
            if (success)
            {
                TriggerSkillAction("Ultimate");
            }
            else
            {
                var availability = cooldownManager.GetSkillAvailability("Ultimate");
                HandleSkillFailure("Ultimate", availability);
            }
        }
    }
    private void HandleSkillFailure(string skillName, SkillCooldownManager.SkillAvailability availability)
    {
        switch (availability)
        {
            case SkillCooldownManager.SkillAvailability.OnCooldown:
                ShowMessage($"{skillName} on cooldown ({cooldownManager.GetRemainingCooldown(skillName):F1}s)");
                break;
            case SkillCooldownManager.SkillAvailability.InsufficientMana:
                ShowMessage($"Not enough mana for {skillName}");
                break;
            case SkillCooldownManager.SkillAvailability.Unavailable:
                ShowMessage($"{skillName} unavailable");
                break;
        }
    }

    public void TriggerSkillAction(string skillName)
    {
        // This bridges the UI system to the actual skill systems
        switch (skillName)
        {
            case "SliceUp":
                TriggerSliceUp();
                break;
            case "SummonSkill":
                TriggerSummonSkill();
                break;
            case "Ultimate":
                TriggerUltimate();
                break;
            case "Dodge":
                TriggerDodge();
                break;
            default:
                Debug.LogWarning($"Unknown skill action: {skillName}");
                break;
        }
    }

    public void TriggerSkillAction(KeyCode key)
    {
        // Find skill by key and trigger it
        if (cooldownManager != null)
        {
            var allSkills = cooldownManager.GetAllSkills();
            foreach (var skill in allSkills)
            {
                if (skill.inputKey == key)
                {
                    TriggerSkillAction(skill.skillName);
                    return;
                }
            }
        }
    }

    private void TriggerSliceUp()
    {
        // Mana already consumed in TryUseSkill, just trigger skill effect
        var sliceUpSkill = FindObjectOfType<SliceUpSkill>();
        if (sliceUpSkill != null)
        {
            // Call PlaySkill which is the public API method
            sliceUpSkill.PlaySkill();
        }
        else
        {
            Debug.LogWarning("SliceUpSkill component not found!");
        }
    }

    private void TriggerSummonSkill()
    {
        // Mana already consumed in TryUseSkill, just trigger skill effect
        var summonSkill = FindObjectOfType<SummonSkill>();
        if (summonSkill != null)
        {
            // Call the new PlaySkill method instead of the disabled OnSummonSkill
            summonSkill.PlaySkill();
        }
        else
        {
            Debug.LogWarning("SummonSkill component not found!");
        }
    }

    private void TriggerUltimate()
    {
        // Mana already consumed in TryUseSkill, just trigger skill effect
        var ultimateSkill = FindObjectOfType<TruthMultilateUltimate>();
        if (ultimateSkill != null)
        {
            // Call the new PlaySkill method for consistency
            ultimateSkill.PlaySkill();
        }
        else
        {
            Debug.LogWarning("TruthMultilateUltimate component not found!");
        }
    }

    private void TriggerDodge()
    {
        // No mana cost for dodge, just trigger it
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.PlayDodge();
        }
        else
        {
            Debug.LogWarning("PlayerController component not found!");
        }
    }

    private void ShowMessage(string message)
    {
        // Simple debug log - you can replace with proper UI message system
        Debug.Log($"UI Message: {message}");

        // TODO: Implement proper UI message/notification system
        // For example: messagePanel.ShowMessage(message, 2f);
    }

    // Public methods for external control
    public void ShowHUD()
    {
        if (playerHUD != null)
        {
            playerHUD.gameObject.SetActive(true);
        }
    }

    public void HideHUD()
    {
        if (playerHUD != null)
        {
            playerHUD.gameObject.SetActive(false);
        }
    }

    public void ToggleHUD()
    {
        if (playerHUD != null)
        {
            playerHUD.gameObject.SetActive(!playerHUD.gameObject.activeSelf);
        }
    }

    public void SetHUDVisibility(bool visible)
    {
        if (playerHUD != null)
        {
            playerHUD.gameObject.SetActive(visible);
        }
    }

    // Utility methods
    public SkillUISlot GetSkillSlot(string skillName)
    {
        if (skillSlots == null) return null;

        foreach (var slot in skillSlots)
        {
            if (slot != null && slot.skillName == skillName)
            {
                return slot;
            }
        }
        return null;
    }

    public PlayerResources GetPlayerResources() => playerResources;
    public SkillCooldownManager GetCooldownManager() => cooldownManager;
}
