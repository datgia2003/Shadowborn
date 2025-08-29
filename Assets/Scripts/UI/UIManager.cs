using UnityEngine;

/// <summary>
/// Central UI manager that coordinates all UI systems
/// Handles skill bar setup, HUD initialization, and cross-system communication
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public PlayerHUD playerHUD;
    public SkillUISlot[] skillSlots;
    public Canvas mainCanvas;

    [Header("UI Configuration")]
    public bool showHUDAtStart = true;
    public bool enableSkillBarInput = true;

    // System references
    private PlayerResources playerResources;
    private SkillCooldownManager cooldownManager;

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
                skillSlots[i].SetupSkill(allSkills[i].skillName);
                Debug.Log($"Assigned skill '{allSkills[i].skillName}' to slot {i}");
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
    }

    private void HandleSkillBarInput()
    {
        if (cooldownManager == null) return;

        // Check for skill inputs
        var allSkills = cooldownManager.GetAllSkills();
        foreach (var skill in allSkills)
        {
            if (Input.GetKeyDown(skill.inputKey))
            {
                bool success = cooldownManager.TryUseSkill(skill.skillName);

                // Provide feedback for failed attempts
                if (!success)
                {
                    var availability = cooldownManager.GetSkillAvailability(skill.skillName);
                    switch (availability)
                    {
                        case SkillCooldownManager.SkillAvailability.OnCooldown:
                            ShowMessage($"{skill.displayName} on cooldown ({cooldownManager.GetRemainingCooldown(skill.skillName):F1}s)");
                            break;
                        case SkillCooldownManager.SkillAvailability.InsufficientMana:
                            ShowMessage($"Not enough mana for {skill.displayName}");
                            break;
                        case SkillCooldownManager.SkillAvailability.Unavailable:
                            ShowMessage($"{skill.displayName} unavailable");
                            break;
                    }
                }
                else
                {
                    // Success - trigger the actual skill
                    TriggerSkillAction(skill.skillName);
                }
            }
        }
    }

    private void TriggerSkillAction(string skillName)
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

    private void TriggerSliceUp()
    {
        var sliceUpSkill = FindObjectOfType<SliceUpSkill>();
        if (sliceUpSkill != null)
        {
            sliceUpSkill.PlaySkill();
            Debug.Log("Triggered SliceUp from UI");
        }
    }

    private void TriggerSummonSkill()
    {
        var summonSkill = FindObjectOfType<SummonSkill>();
        if (summonSkill != null)
        {
            summonSkill.OnSummonSkill(new UnityEngine.InputSystem.InputValue()); // Mock input
            Debug.Log("Triggered Summon from UI");
        }
    }

    private void TriggerUltimate()
    {
        var ultimateSkill = FindObjectOfType<TruthMultilateUltimate>();
        if (ultimateSkill != null)
        {
            ultimateSkill.StartUltimate();
            Debug.Log("Triggered Ultimate from UI");
        }
    }

    private void TriggerDodge()
    {
        // Implement dodge logic or find dodge component
        Debug.Log("Triggered Dodge from UI");
        // You may need to implement this based on your dodge system
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
            if (slot != null && slot.GetAssignedSkill() == skillName)
            {
                return slot;
            }
        }
        return null;
    }

    public PlayerResources GetPlayerResources() => playerResources;
    public SkillCooldownManager GetCooldownManager() => cooldownManager;
}
