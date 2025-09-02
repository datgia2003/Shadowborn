using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple Input Manager using InputActionReferences
/// Không cần PlayerInputActions class, chỉ cần assign references trong Inspector
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [Header("Input Action References")]
    public InputActionReference moveActionRef;
    public InputActionReference jumpActionRef;
    public InputActionReference lightAttackActionRef;
    public InputActionReference heavyAttackActionRef;
    public InputActionReference dodgeActionRef;
    public InputActionReference sliceUpActionRef;
    public InputActionReference summonSkillActionRef;
    public InputActionReference ultimateActionRef;

    // Input Action properties for easy access
    public InputAction moveAction => moveActionRef?.action;
    public InputAction jumpAction => jumpActionRef?.action;
    public InputAction lightAttackAction => lightAttackActionRef?.action;
    public InputAction heavyAttackAction => heavyAttackActionRef?.action;
    public InputAction dodgeAction => dodgeActionRef?.action;
    public InputAction sliceUpAction => sliceUpActionRef?.action;
    public InputAction summonSkillAction => summonSkillActionRef?.action;
    public InputAction ultimateAction => ultimateActionRef?.action;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Enable all action references
        EnableActionReference(moveActionRef);
        EnableActionReference(jumpActionRef);
        EnableActionReference(lightAttackActionRef);
        EnableActionReference(heavyAttackActionRef);
        EnableActionReference(dodgeActionRef);
        EnableActionReference(sliceUpActionRef);
        EnableActionReference(summonSkillActionRef);
        EnableActionReference(ultimateActionRef);
    }

    private void OnDisable()
    {
        // Disable all action references
        DisableActionReference(moveActionRef);
        DisableActionReference(jumpActionRef);
        DisableActionReference(lightAttackActionRef);
        DisableActionReference(heavyAttackActionRef);
        DisableActionReference(dodgeActionRef);
        DisableActionReference(sliceUpActionRef);
        DisableActionReference(summonSkillActionRef);
        DisableActionReference(ultimateActionRef);
    }

    private void EnableActionReference(InputActionReference actionRef)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.Enable();
        }
    }

    private void DisableActionReference(InputActionReference actionRef)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.Disable();
        }
    }

    // Helper methods for easy access
    public bool IsSkillPressed(string skillName)
    {
        switch (skillName.ToLower())
        {
            case "sliceup":
                return sliceUpAction?.WasPressedThisFrame() ?? false;
            case "summonskill":
                return summonSkillAction?.WasPressedThisFrame() ?? false;
            case "ultimate":
                return ultimateAction?.WasPressedThisFrame() ?? false;
            default:
                return false;
        }
    }

    public bool IsActionPressed(string actionName)
    {
        switch (actionName.ToLower())
        {
            case "jump":
                return jumpAction?.WasPressedThisFrame() ?? false;
            case "light":
                return lightAttackAction?.WasPressedThisFrame() ?? false;
            case "heavy":
                return heavyAttackAction?.WasPressedThisFrame() ?? false;
            case "dodge":
                return dodgeAction?.WasPressedThisFrame() ?? false;
            default:
                return false;
        }
    }

    public Vector2 GetMoveInput()
    {
        return moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    }

    // Status check methods
    public bool IsInputSystemReady()
    {
        return sliceUpActionRef != null && summonSkillActionRef != null && ultimateActionRef != null;
    }

    public void LogInputStatus()
    {
        Debug.Log("=== INPUT MANAGER STATUS ===");
        Debug.Log($"SliceUp Action: {(sliceUpActionRef != null ? "✅" : "❌")}");
        Debug.Log($"SummonSkill Action: {(summonSkillActionRef != null ? "✅" : "❌")}");
        Debug.Log($"Ultimate Action: {(ultimateActionRef != null ? "✅" : "❌")}");
        Debug.Log($"Move Action: {(moveActionRef != null ? "✅" : "❌")}");
    }
}
