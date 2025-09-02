using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// ENHANCED Professional Player HUD with Modern Game Design
/// Features: Gradient bars, glow effects, animated fills, particle effects
/// Visual Style: Dark theme with neon accents and smooth animations
/// </summary>
public class EnhancedPlayerHUD : MonoBehaviour
{
    [Header("🎮 Player Avatar Section")]
    public Image playerAvatar;
    public Image avatarFrame;
    public Image avatarGlow; // Animated glow around avatar
    public Image avatarBorder; // Rotating border effect
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelText;
    public ParticleSystem levelUpParticles;

    [Header("🖼️ HUD Frame & Layout")]
    public Image mainHUDFrame; // Main HUD background frame
    public Image hudBorder; // Decorative border
    public GameObject hudCornerDecorations; // Corner ornaments

    [Header("❤️ Health Bar - Red to Green Gradient")]
    public Slider healthBar;
    public Image healthFill;
    public Image healthGhostTrail; // Ghost trail for lost health
    public Image healthGlow; // Background glow
    public Image healthShine; // Moving shine effect (only when full)
    public Image healthIcon; // Health icon (heart, etc.)
    public Image healthBarFrame; // Decorative frame
    public TextMeshProUGUI healthText;
    public ParticleSystem healthCriticalEffect; // When health < 30%

    [Header("🔮 Mana Bar - Blue Gradient")]
    public Slider manaBar;
    public Image manaFill;
    public Image manaGhostTrail; // Ghost trail for consumed mana
    public Image manaGlow;
    public Image manaShine; // Moving shine effect (only when full)
    public Image manaIcon; // Mana icon (crystal, etc.)
    public Image manaBarFrame; // Decorative frame
    public TextMeshProUGUI manaText;
    public ParticleSystem manaRegenerationEffect;

    [Header("⭐ Experience Bar - Gold Gradient")]
    public Slider expBar;
    public Image expFill;
    public Image expGhostTrail; // Ghost trail for exp changes
    public Image expGlow;
    public Image expShine; // Moving shine effect (only when full)
    public Image expIcon; // EXP icon (star, etc.)
    public Image expBarFrame; // Decorative frame
    public TextMeshProUGUI expText;
    public ParticleSystem expGainEffect;

    [Header("🎨 Color Schemes & Gradients")]
    public Gradient healthGradient = new Gradient();
    public Gradient manaGradient = new Gradient();
    public Gradient expGradient = new Gradient();
    public Color lowHealthColor = Color.red;
    public Color fullHealthColor = Color.green;
    public Color manaColor = new Color(0.2f, 0.6f, 1f, 1f);
    public Color expColor = new Color(1f, 0.8f, 0.2f, 1f);

    [Header("✨ Visual Effects")]
    public GameObject levelUpPrefab;
    public GameObject healthWarningUI;
    public Material glowMaterial; // For glow effects
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("🔊 Audio")]
    public AudioSource audioSource;
    public AudioClip levelUpSound;
    public AudioClip lowHealthWarning;
    public AudioClip healthRegenSound;
    public AudioClip manaRegenSound;

    [Header("⚙️ Animation Settings")]
    public float barAnimationSpeed = 0.8f;
    public float ghostTrailDelay = 0.3f; // Delay before ghost trail starts following
    public float ghostTrailSpeed = 2f; // Speed of ghost trail animation
    public float glowPulseSpeed = 2f;
    public float shineSpeed = 1.5f;
    public bool enableParticleEffects = true;
    public bool enableGlowEffects = true;
    public bool enableShineEffects = true;
    public bool enableGhostTrail = true;
    public bool onlyShineWhenFull = true; // Shine only appears when bar is full

    // Private fields
    private PlayerResources playerResources;
    private ExperienceSystem experienceSystem;
    private Coroutine healthShineCoroutine;
    private Coroutine manaShineCoroutine;
    private Coroutine expShineCoroutine;
    private Coroutine avatarGlowCoroutine;

    // Ghost trail system
    private Coroutine healthGhostTrailCoroutine;
    private Coroutine manaGhostTrailCoroutine;
    private Coroutine expGhostTrailCoroutine;

    private bool isLowHealth = false;
    private float lastHealthValue;
    private float lastManaValue;
    private float lastExpValue;

    private void Start()
    {
        InitializeHUD();
        SetupGradients();
        StartVisualEffects();
        SubscribeToEvents();
    }

    private void InitializeHUD()
    {
        // Find PlayerResources
        playerResources = FindObjectOfType<PlayerResources>();
        if (playerResources == null)
        {
            Debug.LogError("❌ PlayerResources not found! Enhanced HUD requires PlayerResources component.");
            return;
        }

        // Find ExperienceSystem
        experienceSystem = FindObjectOfType<ExperienceSystem>();
        if (experienceSystem == null)
        {
            Debug.LogWarning("⚠️ ExperienceSystem not found! EXP bar will not function properly.");
        }

        // Setup audio
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initial values
        UpdatePlayerInfo();
        // Other updates will come from events

        Debug.Log("✅ Enhanced PlayerHUD initialized successfully!");
    }

    private void SetupGradients()
    {
        // Setup Health Gradient (Red → Yellow → Green)
        GradientColorKey[] healthColors = new GradientColorKey[3];
        healthColors[0] = new GradientColorKey(Color.red, 0.0f);           // Low health
        healthColors[1] = new GradientColorKey(Color.yellow, 0.3f);       // Medium health
        healthColors[2] = new GradientColorKey(Color.green, 1.0f);        // Full health

        GradientAlphaKey[] healthAlphas = new GradientAlphaKey[2];
        healthAlphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        healthAlphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        healthGradient.SetKeys(healthColors, healthAlphas);

        // Setup Mana Gradient (Dark Blue → Light Blue)
        GradientColorKey[] manaColors = new GradientColorKey[2];
        manaColors[0] = new GradientColorKey(new Color(0.1f, 0.3f, 0.8f), 0.0f);  // Dark blue
        manaColors[1] = new GradientColorKey(new Color(0.3f, 0.7f, 1f), 1.0f);    // Light blue

        manaGradient.SetKeys(manaColors, healthAlphas);

        // Setup EXP Gradient (Orange → Gold)
        GradientColorKey[] expColors = new GradientColorKey[2];
        expColors[0] = new GradientColorKey(new Color(1f, 0.5f, 0f), 0.0f);       // Orange
        expColors[1] = new GradientColorKey(new Color(1f, 0.9f, 0.3f), 1.0f);    // Gold

        expGradient.SetKeys(expColors, healthAlphas);
    }

    private void StartVisualEffects()
    {
        // Shine effects only start when bars are full (controlled by onlyShineWhenFull)
        if (enableShineEffects && !onlyShineWhenFull)
        {
            healthShineCoroutine = StartCoroutine(AnimateShine(healthShine));
            manaShineCoroutine = StartCoroutine(AnimateShine(manaShine));
            expShineCoroutine = StartCoroutine(AnimateShine(expShine));
        }

        if (enableGlowEffects)
        {
            avatarGlowCoroutine = StartCoroutine(AnimateAvatarGlow());
            StartCoroutine(AnimateBarGlows());
        }

        // Initialize ghost trail positions
        InitializeGhostTrails();
    }

    private void SubscribeToEvents()
    {
        if (playerResources != null)
        {
            // Subscribe to resource change events
            PlayerResources.OnHealthChanged += UpdateHealthDisplay;
            PlayerResources.OnManaChanged += UpdateManaDisplay;

            Debug.Log("✅ Enhanced PlayerHUD subscribed to PlayerResources events");

            // Force initial update for health and mana
            UpdateHealthDisplay(playerResources.GetCurrentHealth(), playerResources.GetMaxHealth());
            UpdateManaDisplay(playerResources.GetCurrentMana(), playerResources.GetMaxMana());
        }
        else
        {
            Debug.LogError("❌ PlayerResources is null! Cannot subscribe to events.");
        }

        if (experienceSystem != null)
        {
            // Subscribe to experience system events
            ExperienceSystem.OnExpChanged += UpdateExpDisplay;
            ExperienceSystem.OnLevelUp += OnLevelUp;

            Debug.Log("✅ Enhanced PlayerHUD subscribed to ExperienceSystem events");

            // Force initial update for exp (should be 0/100)
            UpdateExpDisplay(0, experienceSystem.GetExpToNextLevel());
        }
        else
        {
            Debug.LogWarning("⚠️ ExperienceSystem is null! EXP bar will not update.");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from PlayerResources events
        if (playerResources != null)
        {
            PlayerResources.OnHealthChanged -= UpdateHealthDisplay;
            PlayerResources.OnManaChanged -= UpdateManaDisplay;
        }

        // Unsubscribe from ExperienceSystem events
        if (experienceSystem != null)
        {
            ExperienceSystem.OnExpChanged -= UpdateExpDisplay;
            ExperienceSystem.OnLevelUp -= OnLevelUp;
        }

        // Stop coroutines
        StopAllCoroutines();
    }

    private void InitializeGhostTrails()
    {
        // Initialize ghost trails to match current bar values
        if (healthGhostTrail != null && healthBar != null)
        {
            healthGhostTrail.fillAmount = healthBar.value;
        }

        if (manaGhostTrail != null && manaBar != null)
        {
            manaGhostTrail.fillAmount = manaBar.value;
        }

        if (expGhostTrail != null && expBar != null)
        {
            expGhostTrail.fillAmount = expBar.value;
        }
    }

    #region Health System with Enhanced Visuals
    #region Health System with Enhanced Visuals

    private void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
        {
            Debug.LogWarning("⚠️ Health bar is null!");
            return;
        }

        float healthPercent = (float)currentHealth / maxHealth;
        float previousPercent = lastHealthValue;

        Debug.Log($"🩸 UpdateHealthDisplay: {currentHealth}/{maxHealth} = {healthPercent:F2}");

        // Handle ghost trail for health loss
        if (enableGhostTrail && healthPercent < previousPercent)
        {
            StartGhostTrail(healthGhostTrail, previousPercent, healthPercent);
        }

        // Animate health bar
        StartCoroutine(AnimateBar(healthBar, healthPercent));

        // Update gradient color
        if (healthFill != null)
        {
            healthFill.color = healthGradient.Evaluate(healthPercent);
        }

        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Glow effect based on health percentage
        if (healthGlow != null)
        {
            Color glowColor = healthGradient.Evaluate(healthPercent);
            glowColor.a = 0.3f;
            healthGlow.color = glowColor;
        }

        // Shine effect only when full health
        HandleShineEffect(healthShine, healthPercent, ref healthShineCoroutine, "Health");

        // Critical health effects
        CheckCriticalHealth(healthPercent);

        lastHealthValue = healthPercent;
    }
    private void CheckCriticalHealth(float healthPercent)
    {
        if (healthPercent <= 0.3f && !isLowHealth)
        {
            isLowHealth = true;

            // Start critical health effects
            if (healthCriticalEffect != null && enableParticleEffects)
            {
                healthCriticalEffect.Play();
            }

            // Play warning sound
            if (audioSource != null && lowHealthWarning != null)
            {
                audioSource.PlayOneShot(lowHealthWarning);
            }

            // Start pulsing effect
            StartCoroutine(PulseHealthBar());
        }
        else if (healthPercent > 0.3f && isLowHealth)
        {
            isLowHealth = false;

            // Stop critical effects
            if (healthCriticalEffect != null)
            {
                healthCriticalEffect.Stop();
            }
        }
    }

    private IEnumerator PulseHealthBar()
    {
        while (isLowHealth)
        {
            float time = 0;
            while (time < 1f)
            {
                time += Time.deltaTime * glowPulseSpeed;
                float pulseValue = pulseCurve.Evaluate(time);

                if (healthGlow != null)
                {
                    Color color = healthGlow.color;
                    color.a = 0.2f + (pulseValue * 0.3f);
                    healthGlow.color = color;
                }

                yield return null;
            }
        }
    }

    #endregion

    #region Mana System with Enhanced Visuals

    private void UpdateManaDisplay(int currentMana, int maxMana)
    {
        if (manaBar == null)
        {
            Debug.LogWarning("⚠️ Mana bar is null!");
            return;
        }

        float manaPercent = (float)currentMana / maxMana;
        float previousPercent = lastManaValue;

        Debug.Log($"🔮 UpdateManaDisplay: {currentMana}/{maxMana} = {manaPercent:F2}");

        // Handle ghost trail for mana loss
        if (enableGhostTrail && manaPercent < previousPercent)
        {
            StartGhostTrail(manaGhostTrail, previousPercent, manaPercent);
        }

        // Animate mana bar
        StartCoroutine(AnimateBar(manaBar, manaPercent));

        // Update gradient color
        if (manaFill != null)
        {
            manaFill.color = manaGradient.Evaluate(manaPercent);
        }

        // Update text
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }

        // Glow effect
        if (manaGlow != null)
        {
            Color glowColor = manaGradient.Evaluate(manaPercent);
            glowColor.a = 0.2f + (manaPercent * 0.3f);
            manaGlow.color = glowColor;
        }

        // Shine effect only when full mana
        HandleShineEffect(manaShine, manaPercent, ref manaShineCoroutine, "Mana");

        // Mana regeneration effect
        if (manaPercent > lastManaValue && manaRegenerationEffect != null && enableParticleEffects)
        {
            manaRegenerationEffect.Play();

            if (audioSource != null && manaRegenSound != null)
            {
                audioSource.PlayOneShot(manaRegenSound, 0.5f);
            }
        }

        lastManaValue = manaPercent;
    }

    #endregion

    #region Experience System with Enhanced Visuals

    private void UpdateExpDisplay(int currentExp, int expToNextLevel)
    {
        if (expBar == null) return;

        float expPercent = (float)currentExp / expToNextLevel;
        float previousPercent = lastExpValue;

        // Handle ghost trail for exp changes (usually increases, but can reset on level up)
        if (enableGhostTrail && expPercent < previousPercent)
        {
            StartGhostTrail(expGhostTrail, previousPercent, expPercent);
        }

        // Animate exp bar
        StartCoroutine(AnimateBar(expBar, expPercent));

        // Update gradient color
        if (expFill != null)
        {
            expFill.color = expGradient.Evaluate(expPercent);
        }

        // Update text
        if (expText != null)
        {
            expText.text = $"{currentExp}/{expToNextLevel}";
        }

        // Glow effect
        if (expGlow != null)
        {
            Color glowColor = expGradient.Evaluate(expPercent);
            glowColor.a = 0.3f;
            expGlow.color = glowColor;
        }

        // Shine effect only when exp bar is full (about to level up)
        HandleShineEffect(expShine, expPercent, ref expShineCoroutine, "EXP");

        // EXP gain effect
        if (expPercent > lastExpValue && expGainEffect != null && enableParticleEffects)
        {
            expGainEffect.Play();
        }

        lastExpValue = expPercent;
    }

    #endregion

    #region Level System with Enhanced Visuals

    private void UpdatePlayerInfo()
    {
        if (playerNameText != null)
        {
            playerNameText.text = "Sung Cằm nhọn"; // Default name
        }

        if (levelText != null)
        {
            if (experienceSystem != null)
            {
                levelText.text = $"LV.{experienceSystem.GetCurrentLevel()}";
            }
            else if (playerResources != null)
            {
                levelText.text = $"LV.{playerResources.currentLevel}"; // Fallback
            }
            else
            {
                levelText.text = "LV.1"; // Default fallback
            }
        }
    }

    private void OnLevelUp(int newLevel)
    {
        // Level up particle effect
        if (levelUpParticles != null && enableParticleEffects)
        {
            levelUpParticles.Play();
        }

        // Level up sound
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }

        // Flash effect
        StartCoroutine(FlashLevelText());

        // Avatar glow boost
        StartCoroutine(LevelUpAvatarGlow());

        // Update level display
        UpdatePlayerInfo();
    }
    private IEnumerator FlashLevelText()
    {
        if (levelText == null) yield break;

        Color originalColor = levelText.color;

        for (int i = 0; i < 3; i++)
        {
            levelText.color = Color.yellow;
            yield return new WaitForSeconds(0.2f);
            levelText.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator LevelUpAvatarGlow()
    {
        if (avatarGlow == null) yield break;

        float time = 0;
        Color originalColor = avatarGlow.color;

        while (time < 2f)
        {
            time += Time.deltaTime;
            float intensity = Mathf.Sin(time * 5f) * 0.5f + 0.5f;
            Color glowColor = Color.yellow;
            glowColor.a = intensity * 0.8f;
            avatarGlow.color = glowColor;
            yield return null;
        }

        avatarGlow.color = originalColor;
    }

    #endregion

    #region Visual Effects & Animations

    private IEnumerator AnimateBar(Slider slider, float targetValue)
    {
        if (slider == null)
        {
            Debug.LogWarning("⚠️ AnimateBar: Slider is null!");
            yield break;
        }

        float startValue = slider.value;
        float time = 0;

        Debug.Log($"🎛️ AnimateBar: {startValue:F2} → {targetValue:F2}");

        while (time < barAnimationSpeed)
        {
            time += Time.deltaTime;
            float progress = time / barAnimationSpeed;
            float currentValue = Mathf.Lerp(startValue, targetValue, progress);
            slider.value = currentValue;
            yield return null;
        }

        slider.value = targetValue;
        Debug.Log($"✅ AnimateBar complete: Final value = {slider.value:F2}");
    }

    private void StartGhostTrail(Image ghostTrail, float fromValue, float toValue)
    {
        if (ghostTrail == null || !enableGhostTrail) return;

        // Set ghost trail to previous value and animate to current value with delay
        ghostTrail.fillAmount = fromValue;
        StartCoroutine(AnimateGhostTrail(ghostTrail, fromValue, toValue));
    }

    private IEnumerator AnimateGhostTrail(Image ghostTrail, float fromValue, float toValue)
    {
        // Wait for main bar animation to start
        yield return new WaitForSeconds(ghostTrailDelay);

        float time = 0;
        while (time < ghostTrailSpeed)
        {
            time += Time.deltaTime;
            float progress = time / ghostTrailSpeed;
            // Use smooth curve for more natural effect
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            ghostTrail.fillAmount = Mathf.Lerp(fromValue, toValue, smoothProgress);
            yield return null;
        }

        ghostTrail.fillAmount = toValue;
    }

    private void HandleShineEffect(Image shineImage, float barPercent, ref Coroutine shineCoroutine, string barName)
    {
        if (!enableShineEffects || !onlyShineWhenFull) return;

        bool isFull = barPercent >= 0.99f; // Consider 99%+ as full to avoid floating point issues

        if (isFull && shineCoroutine == null)
        {
            // Start shine effect when full
            shineCoroutine = StartCoroutine(AnimateShine(shineImage));
            Debug.Log($"✨ {barName} bar is full - shine effect started!");
        }
        else if (!isFull && shineCoroutine != null)
        {
            // Stop shine effect when not full
            StopCoroutine(shineCoroutine);
            shineCoroutine = null;

            // Reset shine alpha
            if (shineImage != null)
            {
                Color color = shineImage.color;
                color.a = 0f;
                shineImage.color = color;
            }

            Debug.Log($"🌑 {barName} bar not full - shine effect stopped.");
        }
    }

    private IEnumerator AnimateShine(Image shineImage)
    {
        if (shineImage == null) yield break;

        while (true)
        {
            float time = 0;
            while (time < shineSpeed)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Sin(time / shineSpeed * Mathf.PI) * 0.3f;
                Color color = shineImage.color;
                color.a = alpha;
                shineImage.color = color;
                yield return null;
            }

            yield return new WaitForSeconds(2f); // Pause between shines
        }
    }

    private IEnumerator AnimateAvatarGlow()
    {
        if (avatarGlow == null) yield break;

        while (true)
        {
            float time = 0;
            while (time < 2f)
            {
                time += Time.deltaTime;
                float pulse = Mathf.Sin(time * glowPulseSpeed) * 0.3f + 0.5f;
                Color color = avatarGlow.color;
                color.a = pulse * 0.4f;
                avatarGlow.color = color;
                yield return null;
            }
        }
    }

    private IEnumerator AnimateBarGlows()
    {
        while (true)
        {
            float time = 0;
            while (time < 3f)
            {
                time += Time.deltaTime;
                float glow = Mathf.Sin(time * 2f) * 0.2f + 0.3f;

                // Apply to all bar glows
                if (healthGlow != null && !isLowHealth)
                {
                    Color color = healthGlow.color;
                    color.a = glow;
                    healthGlow.color = color;
                }

                if (manaGlow != null)
                {
                    Color color = manaGlow.color;
                    color.a = glow;
                    manaGlow.color = color;
                }

                if (expGlow != null)
                {
                    Color color = expGlow.color;
                    color.a = glow;
                    expGlow.color = color;
                }

                yield return null;
            }
        }
    }

    #endregion
}
#endregion
