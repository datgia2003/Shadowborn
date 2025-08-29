using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual skill UI slot component
/// Displays skill icon, cooldown overlay, keybind, and visual feedback
/// </summary>
public class SkillUISlot : MonoBehaviour
{
    [Header("UI References")]
    public Image skillIcon;
    public Image cooldownOverlay;
    public Image keybindBackground;
    public TextMeshProUGUI keybindText;
    public TextMeshProUGUI cooldownText;
    public GameObject unavailableIndicator;

    [Header("Visual Settings")]
    public Color availableColor = Color.white;
    public Color cooldownColor = Color.gray;
    public Color insufficientManaColor = Color.red;
    public Color unavailableColor = Color.black;

    [Header("Animation")]
    public float flashDuration = 0.2f;
    public Color flashColor = Color.yellow;
    public bool enablePulseWhenReady = true;
    public float pulseSpeed = 2f;

    // Internal state
    private string assignedSkillName;
    private SkillCooldownManager.SkillCooldownData skillData;
    private SkillCooldownManager cooldownManager;
    private PlayerResources playerResources;

    // Animation state
    private bool isFlashing = false;
    private float flashTimer = 0f;
    private Color originalIconColor;
    private float pulseTimer = 0f;

    void Awake()
    {
        // Store original color
        if (skillIcon != null)
        {
            originalIconColor = skillIcon.color;
        }
    }

    void Start()
    {
        // Find managers
        cooldownManager = FindObjectOfType<SkillCooldownManager>();
        playerResources = FindObjectOfType<PlayerResources>();

        // Subscribe to events
        SkillCooldownManager.OnCooldownUpdated += OnCooldownUpdated;
        SkillCooldownManager.OnSkillReady += OnSkillReady;
        SkillCooldownManager.OnSkillUsed += OnSkillUsed;
        PlayerResources.OnManaChanged += OnManaChanged;
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        SkillCooldownManager.OnCooldownUpdated -= OnCooldownUpdated;
        SkillCooldownManager.OnSkillReady -= OnSkillReady;
        SkillCooldownManager.OnSkillUsed -= OnSkillUsed;
        PlayerResources.OnManaChanged -= OnManaChanged;
    }

    void Update()
    {
        // Handle flash animation
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer >= flashDuration)
            {
                EndFlash();
            }
            else
            {
                // Interpolate flash color
                float t = flashTimer / flashDuration;
                Color currentColor = Color.Lerp(flashColor, originalIconColor, t);
                if (skillIcon != null) skillIcon.color = currentColor;
            }
        }

        // Handle ready pulse animation
        if (enablePulseWhenReady && IsSkillAvailable())
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulseAlpha = (Mathf.Sin(pulseTimer) + 1f) * 0.1f + 0.9f; // 0.9-1.1 range
            if (skillIcon != null)
            {
                Color baseColor = availableColor;
                skillIcon.color = baseColor * pulseAlpha;
            }
        }
    }

    /// <summary>
    /// Initialize this slot with a skill
    /// </summary>
    public void SetupSkill(string skillName)
    {
        assignedSkillName = skillName;

        if (cooldownManager != null)
        {
            skillData = cooldownManager.GetSkillData(skillName);
            if (skillData != null)
            {
                // Set up visual elements
                if (skillIcon != null && skillData.skillIcon != null)
                {
                    skillIcon.sprite = skillData.skillIcon;
                }

                if (keybindText != null)
                {
                    keybindText.text = skillData.inputKey.ToString();
                }

                // Initial visual update
                UpdateVisuals();
            }
        }
    }

    private void OnCooldownUpdated(string skillName, float currentCooldown, float maxCooldown)
    {
        if (skillName != assignedSkillName) return;

        // Update cooldown overlay
        if (cooldownOverlay != null)
        {
            float progress = 1f - (currentCooldown / maxCooldown);
            cooldownOverlay.fillAmount = 1f - progress; // Reverse fill (starts full, empties as cooldown decreases)
        }

        // Update cooldown text
        if (cooldownText != null)
        {
            if (currentCooldown > 0f)
            {
                cooldownText.text = currentCooldown.ToString("F1") + "s";
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }

        UpdateVisuals();
    }

    private void OnSkillReady(string skillName)
    {
        if (skillName != assignedSkillName) return;

        // Flash to indicate skill is ready
        StartFlash();
    }

    private void OnSkillUsed(string skillName)
    {
        if (skillName != assignedSkillName) return;

        // Visual feedback for skill use
        StartFlash();
    }

    private void OnManaChanged(int currentMana, int maxMana)
    {
        // Update visuals when mana changes (affects skill availability)
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (cooldownManager == null || skillData == null) return;

        var availability = cooldownManager.GetSkillAvailability(assignedSkillName);

        // Update icon color based on availability
        if (skillIcon != null && !isFlashing)
        {
            switch (availability)
            {
                case SkillCooldownManager.SkillAvailability.Available:
                    skillIcon.color = availableColor;
                    break;
                case SkillCooldownManager.SkillAvailability.OnCooldown:
                    skillIcon.color = cooldownColor;
                    break;
                case SkillCooldownManager.SkillAvailability.InsufficientMana:
                    skillIcon.color = insufficientManaColor;
                    break;
                case SkillCooldownManager.SkillAvailability.Unavailable:
                    skillIcon.color = unavailableColor;
                    break;
            }
        }

        // Update unavailable indicator
        if (unavailableIndicator != null)
        {
            unavailableIndicator.SetActive(availability != SkillCooldownManager.SkillAvailability.Available);
        }

        // Update cooldown overlay visibility
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(availability == SkillCooldownManager.SkillAvailability.OnCooldown);
        }
    }

    private bool IsSkillAvailable()
    {
        if (cooldownManager == null) return false;
        return cooldownManager.GetSkillAvailability(assignedSkillName) == SkillCooldownManager.SkillAvailability.Available;
    }

    private void StartFlash()
    {
        if (skillIcon == null) return;

        isFlashing = true;
        flashTimer = 0f;
        skillIcon.color = flashColor;
    }

    private void EndFlash()
    {
        isFlashing = false;
        if (skillIcon != null)
        {
            skillIcon.color = originalIconColor;
        }

        // Immediately update visuals to correct state
        UpdateVisuals();
    }

    /// <summary>
    /// Manually trigger this skill (for click/touch input)
    /// </summary>
    public void TriggerSkill()
    {
        if (cooldownManager != null && !string.IsNullOrEmpty(assignedSkillName))
        {
            cooldownManager.TryUseSkill(assignedSkillName);
        }
    }

    // Public getters
    public string GetAssignedSkill() => assignedSkillName;
    public bool IsReady() => IsSkillAvailable();
}
