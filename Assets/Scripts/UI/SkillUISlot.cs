using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced circular skill UI slot with professional game-like design
/// Features: Circular cooldown, skill icon, keybind display, visual effects
/// Bottom-left corner skill bar layout
/// </summary>
public class SkillUISlot : MonoBehaviour
{
    [Header("Core UI Elements")]
    public Image skillIcon;
    public Image skillFrame;
    public Image skillGlow;
    public TextMeshProUGUI keybindText;
    public TextMeshProUGUI cooldownTimerText;

    [Header("Circular Cooldown System")]
    public Image cooldownOverlay; // Circular fill image
    public Image cooldownBorder; // Animated border
    public Image readyIndicator; // Glows when skill is ready
    public ParticleSystem skillReadyEffect;

    [Header("Visual States")]
    public Color availableColor = Color.white;
    public Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    public Color unavailableColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    public Color readyGlowColor = new Color(1f, 1f, 0.5f, 1f);
    public Color manaCostColor = new Color(0.2f, 0.6f, 1f, 1f);

    [Header("Animation Settings")]
    public float pulseSpeed = 2f;
    public float glowIntensity = 0.8f;
    public float borderRotationSpeed = 90f; // degrees per second
    public bool enablePulseEffect = true;
    public bool enableBorderRotation = true;
    public bool enableParticleEffects = true;

    [Header("Skill Data")]
    public string skillName;
    public Sprite skillSprite;
    public KeyCode assignedKey;
    public int skillIndex = 0; // Index for mapping to Input Actions
    public int manaCost = 0;

    // Internal state tracking
    private float currentCooldownTime;
    private float maxCooldownTime;
    private bool isOnCooldown;
    private bool isAvailable;
    private bool wasJustUsed;
    private float pulseTimer;

    // Public getters for debugging
    public float GetCurrentCooldownTime() => currentCooldownTime;
    public float GetMaxCooldownTime() => maxCooldownTime;
    public bool IsOnCooldown() => isOnCooldown;

    // Animation coroutines
    private Coroutine flashAnimation;
    private Coroutine shakeAnimation;
    private Coroutine pulseAnimation;

    // Components
    private AudioSource audioSource;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        // Subscribe to skill cooldown events
        SkillCooldownManager.OnSkillCooldownChanged += OnSkillCooldownChanged;
        SkillCooldownManager.OnSkillReady += OnSkillReady;
        SkillCooldownManager.OnSkillUsed += OnSkillUsed;

        // Subscribe to resource events for mana cost validation
        PlayerResources.OnManaChanged += OnManaChanged;

        InitializeUI();
    }

    void InitializeUI()
    {
        // Setup skill icon
        if (skillIcon != null && skillSprite != null)
        {
            skillIcon.sprite = skillSprite;
        }

        // Setup keybind text
        if (keybindText != null)
        {
            keybindText.text = assignedKey.ToString();
        }

        // Initialize cooldown overlay
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.type = Image.Type.Filled;
            cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            cooldownOverlay.fillOrigin = 2; // Top
            cooldownOverlay.fillClockwise = false;
        }

        // Initialize visual state
        UpdateVisualState();

        // Start animations
        if (enableBorderRotation && cooldownBorder != null)
        {
            StartCoroutine(RotateBorder());
        }
    }

    void OnSkillCooldownChanged(string skillName, float currentTime, float maxTime)
    {
        if (this.skillName != skillName) return;

        currentCooldownTime = currentTime;
        maxCooldownTime = maxTime;
        isOnCooldown = currentTime > 0f;

        UpdateVisualState();
        UpdateCooldownDisplay();
    }

    void OnSkillReady(string skillName)
    {
        if (this.skillName != skillName) return;

        isOnCooldown = false;
        currentCooldownTime = 0f;

        UpdateVisualState();

        // Play ready effects
        if (enableParticleEffects && skillReadyEffect != null)
        {
            skillReadyEffect.Play();
        }

        // Flash effect when skill becomes ready
        if (flashAnimation != null) StopCoroutine(flashAnimation);
        flashAnimation = StartCoroutine(FlashReady());

        // Pulse effect
        if (enablePulseEffect)
        {
            if (pulseAnimation != null) StopCoroutine(pulseAnimation);
            pulseAnimation = StartCoroutine(PulseReady());
        }
    }

    void OnSkillUsed(string skillName)
    {
        if (this.skillName != skillName) return;

        wasJustUsed = true;

        // Play use effects
        if (shakeAnimation != null) StopCoroutine(shakeAnimation);
        shakeAnimation = StartCoroutine(ShakeOnUse());

        // Stop ready animations
        if (pulseAnimation != null) StopCoroutine(pulseAnimation);
        if (flashAnimation != null) StopCoroutine(flashAnimation);
    }

    void OnManaChanged(int currentMana, int maxMana)
    {
        // Check if player has enough mana for this skill
        bool hadEnoughMana = isAvailable;
        isAvailable = currentMana >= manaCost;

        // Visual feedback for mana availability change
        if (hadEnoughMana != isAvailable && !isOnCooldown)
        {
            UpdateVisualState();
        }
    }

    void UpdateVisualState()
    {
        if (isOnCooldown)
        {
            // Cooldown state
            SetIconColor(cooldownColor);
            if (skillFrame != null) skillFrame.color = cooldownColor;
            if (skillGlow != null) skillGlow.color = Color.clear;
            if (readyIndicator != null) readyIndicator.color = Color.clear;
        }
        else if (isAvailable)
        {
            // Available state
            SetIconColor(availableColor);
            if (skillFrame != null) skillFrame.color = availableColor;
            if (readyIndicator != null) readyIndicator.color = readyGlowColor;
        }
        else
        {
            // Unavailable state (not enough mana)
            SetIconColor(unavailableColor);
            if (skillFrame != null) skillFrame.color = unavailableColor;
            if (skillGlow != null) skillGlow.color = Color.clear;
            if (readyIndicator != null) readyIndicator.color = Color.clear;
        }
    }

    void UpdateCooldownDisplay()
    {
        // Update circular cooldown overlay
        if (cooldownOverlay != null)
        {
            if (isOnCooldown && maxCooldownTime > 0f)
            {
                float fillAmount = currentCooldownTime / maxCooldownTime;
                cooldownOverlay.fillAmount = fillAmount;
                cooldownOverlay.color = cooldownColor;
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
            }
        }

        // Update cooldown timer text
        if (cooldownTimerText != null)
        {
            if (isOnCooldown && currentCooldownTime > 0.1f)
            {
                cooldownTimerText.text = Mathf.Ceil(currentCooldownTime).ToString();
                cooldownTimerText.gameObject.SetActive(true);
            }
            else
            {
                cooldownTimerText.gameObject.SetActive(false);
            }
        }
    }

    void SetIconColor(Color color)
    {
        if (skillIcon != null)
        {
            skillIcon.color = color;
        }
    }

    IEnumerator FlashReady()
    {
        Color originalColor = availableColor;
        Color flashColor = readyGlowColor;

        // Flash 3 times
        for (int i = 0; i < 3; i++)
        {
            // Flash to bright
            float elapsedTime = 0f;
            while (elapsedTime < 0.2f)
            {
                elapsedTime += Time.deltaTime;
                Color lerpedColor = Color.Lerp(originalColor, flashColor, elapsedTime / 0.2f);
                SetIconColor(lerpedColor);
                if (skillFrame != null) skillFrame.color = lerpedColor;
                yield return null;
            }

            // Flash back to normal
            elapsedTime = 0f;
            while (elapsedTime < 0.2f)
            {
                elapsedTime += Time.deltaTime;
                Color lerpedColor = Color.Lerp(flashColor, originalColor, elapsedTime / 0.2f);
                SetIconColor(lerpedColor);
                if (skillFrame != null) skillFrame.color = lerpedColor;
                yield return null;
            }
        }

        UpdateVisualState();
    }

    IEnumerator PulseReady()
    {
        while (!isOnCooldown && isAvailable)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0 to 1

            if (skillGlow != null)
            {
                Color glowColor = readyGlowColor;
                glowColor.a = pulse * glowIntensity;
                skillGlow.color = glowColor;
            }

            if (readyIndicator != null)
            {
                Color indicatorColor = readyGlowColor;
                indicatorColor.a = 0.5f + (pulse * 0.5f);
                readyIndicator.color = indicatorColor;
            }

            yield return null;
        }

        // Reset glow when stopped
        if (skillGlow != null) skillGlow.color = Color.clear;
    }

    IEnumerator ShakeOnUse()
    {
        Vector3 originalPosition = rectTransform.localPosition;
        float shakeDuration = 0.3f;
        float shakeIntensity = 5f;

        float elapsedTime = 0f;
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            Vector3 randomOffset = Random.insideUnitCircle * shakeIntensity;
            randomOffset.z = 0f;

            float damping = 1f - (elapsedTime / shakeDuration);
            rectTransform.localPosition = originalPosition + (randomOffset * damping);

            yield return null;
        }

        rectTransform.localPosition = originalPosition;
        wasJustUsed = false;
    }

    IEnumerator RotateBorder()
    {
        while (enableBorderRotation && cooldownBorder != null)
        {
            cooldownBorder.transform.Rotate(0f, 0f, borderRotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void Update()
    {
        // Input handling is now managed by UIManager to avoid Input System conflicts
        // This prevents the InvalidOperationException with Input.GetKeyDown

        // Update visual feedback based on hover
        UpdateHoverEffects();
    }

    void UpdateHoverEffects()
    {
        // You can add mouse hover effects here
        // This would require implementing IPointerEnterHandler, IPointerExitHandler
    }

    public void TriggerSkill()
    {
        if (isOnCooldown || !isAvailable) return;

        // Trigger the skill through UIManager
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.TriggerSkillAction(assignedKey);
        }
    }

    public void SetSkillData(string name, Sprite icon, KeyCode key, int cost)
    {
        skillName = name;
        skillSprite = icon;
        assignedKey = key;
        manaCost = cost;

        // Update UI immediately
        if (skillIcon != null && skillSprite != null)
            skillIcon.sprite = skillSprite;

        if (keybindText != null)
            keybindText.text = assignedKey.ToString();
    }

    public void SetCooldown(float current, float max)
    {
        currentCooldownTime = current;
        maxCooldownTime = max;
        isOnCooldown = current > 0f;

        UpdateVisualState();
        UpdateCooldownDisplay();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        SkillCooldownManager.OnSkillCooldownChanged -= OnSkillCooldownChanged;
        SkillCooldownManager.OnSkillReady -= OnSkillReady;
        SkillCooldownManager.OnSkillUsed -= OnSkillUsed;
        PlayerResources.OnManaChanged -= OnManaChanged;
    }
}
