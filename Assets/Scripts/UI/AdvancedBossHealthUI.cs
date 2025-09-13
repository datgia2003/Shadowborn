using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Advanced Boss Health UI với particle effects, screen shake, và damage number
/// </summary>
public class AdvancedBossHealthUI : MonoBehaviour
{
    [Header("Core UI References")]
    public Slider healthBar;
    public Image healthFill;
    public Image healthBarFrame; // Frame/border around health bar
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI phaseText; // "Phase 1", "Phase 2" etc.
    public GameObject bossUIPanel;

    [Header("Advanced Visual Effects")]
    public ParticleSystem healthLossEffect; // Particles when taking damage
    public ParticleSystem enrageEffect; // Particles when entering enrage
    public GameObject damageNumberPrefab; // Floating damage numbers
    public Transform damageNumberSpawn; // Where to spawn damage numbers

    [Header("Health Bar Segments")]
    public bool useSegmentedHealthBar = true;
    public int healthSegments = 4; // Divide health bar into segments
    public Color segmentBorderColor = Color.black;
    public float segmentBorderWidth = 2f;

    [Header("Animation Settings")]
    public AnimationCurve healthBarCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float healthBarAnimSpeed = 3f;
    public float colorTransitionSpeed = 2f;
    public float shakeIntensity = 5f;
    public float shakeDuration = 0.3f;

    [Header("Color Schemes")]
    public Gradient healthGradient; // Health color based on percentage
    public Color enragedHealthColor = Color.red;
    public Color criticalHealthColor = new Color(1f, 0.5f, 0f); // Orange
    public Color frameNormalColor = Color.white;
    public Color frameEnragedColor = Color.red;

    [Header("Phase System")]
    public bool showPhaseIndicator = true;
    public string[] phaseNames = { "Shadow Form", "Enraged Form" };

    [Header("Screen Effects")]
    public bool enableScreenShake = true;
    public bool enableScreenFlash = true;
    public Image screenFlashImage; // Full screen overlay for damage flash

    [Header("Boss Reference")]
    public Igris bossScript;

    // Private variables
    private float targetHealthPercentage = 1f;
    private float displayedHealthPercentage = 1f;
    private int lastKnownHP;
    private bool isEnraged = false;
    private int currentPhase = 0;
    private Vector3 originalPosition;

    // Animation coroutines
    private Coroutine healthAnimCoroutine;
    private Coroutine colorAnimCoroutine;
    private Coroutine shakeCoroutine;
    private Coroutine enrageAnimCoroutine;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        // Auto-find boss if not assigned
        if (bossScript == null)
        {
            bossScript = FindObjectOfType<Igris>();
        }

        if (bossScript != null)
        {
            lastKnownHP = bossScript.GetCurrentHP();
            targetHealthPercentage = (float)lastKnownHP / bossScript.maxHP;
            displayedHealthPercentage = targetHealthPercentage;

            // Initialize health bar
            if (healthBar != null)
            {
                healthBar.value = targetHealthPercentage;
            }

            // Setup health gradient if not configured
            if (healthGradient == null || healthGradient.colorKeys.Length == 0)
            {
                SetupDefaultHealthGradient();
            }

            // Create health bar segments
            if (useSegmentedHealthBar)
            {
                CreateHealthBarSegments();
            }

            // Store original position for shake effect
            originalPosition = transform.position;
        }

        // Hide UI initially
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
        }

        // Setup screen flash
        if (screenFlashImage != null)
        {
            screenFlashImage.color = new Color(1f, 0f, 0f, 0f); // Transparent red
        }
    }

    void Update()
    {
        if (bossScript == null) return;

        // Update health display
        UpdateHealthSystem();

        // Update enrage state
        UpdateEnrageState();

        // Update phase system
        UpdatePhaseSystem();

        // Auto show/hide UI
        UpdateUIVisibility();
    }

    void UpdateHealthSystem()
    {
        int currentHP = bossScript.GetCurrentHP();

        // Check if health changed
        if (currentHP != lastKnownHP)
        {
            int damageTaken = lastKnownHP - currentHP;

            if (damageTaken > 0)
            {
                // Boss took damage
                OnBossDamaged(damageTaken);
            }
            else if (damageTaken < 0)
            {
                // Boss healed (if applicable)
                OnBossHealed(-damageTaken);
            }

            lastKnownHP = currentHP;
            targetHealthPercentage = (float)currentHP / bossScript.maxHP;

            // Start health bar animation
            AnimateHealthBar();
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{currentHP} / {bossScript.maxHP}";
        }
    }

    void OnBossDamaged(int damage)
    {
        // Spawn damage number
        SpawnDamageNumber(damage);

        // Play particle effect
        if (healthLossEffect != null)
        {
            healthLossEffect.Play();
        }

        // Screen shake
        if (enableScreenShake)
        {
            StartScreenShake();
        }

        // Screen flash
        if (enableScreenFlash)
        {
            StartScreenFlash();
        }

        Debug.Log($"Boss took {damage} damage!");
    }

    void OnBossHealed(int healAmount)
    {
        Debug.Log($"Boss healed for {healAmount}!");
    }

    void UpdateEnrageState()
    {
        bool bossEnraged = bossScript.IsEnraged();

        if (isEnraged != bossEnraged)
        {
            isEnraged = bossEnraged;

            if (isEnraged)
            {
                OnEnrageActivated();
            }
            else
            {
                OnEnrageDeactivated();
            }
        }
    }

    void OnEnrageActivated()
    {
        Debug.Log("Boss entered enrage state!");

        // Play enrage effect
        if (enrageEffect != null)
        {
            enrageEffect.Play();
        }

        // Start enrage animation
        if (enrageAnimCoroutine != null)
        {
            StopCoroutine(enrageAnimCoroutine);
        }
        enrageAnimCoroutine = StartCoroutine(EnrageAnimation());

        // Change phase
        currentPhase = 1;
    }

    void OnEnrageDeactivated()
    {
        Debug.Log("Boss left enrage state!");
        currentPhase = 0;
    }

    void UpdatePhaseSystem()
    {
        if (!showPhaseIndicator || phaseText == null) return;

        string phaseName = currentPhase < phaseNames.Length ?
                          phaseNames[currentPhase] :
                          $"Phase {currentPhase + 1}";

        if (phaseText.text != phaseName)
        {
            phaseText.text = phaseName;
        }
    }

    void UpdateUIVisibility()
    {
        bool shouldShow = bossScript.GetCurrentState() != Igris.BossState.Dead &&
                         bossScript.GetCurrentHP() > 0;

        if (bossUIPanel != null && bossUIPanel.activeSelf != shouldShow)
        {
            bossUIPanel.SetActive(shouldShow);
        }
    }

    // Animation methods
    void AnimateHealthBar()
    {
        if (healthAnimCoroutine != null)
        {
            StopCoroutine(healthAnimCoroutine);
        }
        healthAnimCoroutine = StartCoroutine(AnimateHealthBarCoroutine());

        // Animate color
        if (colorAnimCoroutine != null)
        {
            StopCoroutine(colorAnimCoroutine);
        }
        colorAnimCoroutine = StartCoroutine(AnimateHealthColorCoroutine());
    }

    IEnumerator AnimateHealthBarCoroutine()
    {
        float startValue = displayedHealthPercentage;
        float endValue = targetHealthPercentage;
        float elapsedTime = 0f;
        float duration = 1f / healthBarAnimSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = healthBarCurve.Evaluate(elapsedTime / duration);

            displayedHealthPercentage = Mathf.Lerp(startValue, endValue, progress);

            if (healthBar != null)
            {
                healthBar.value = displayedHealthPercentage;
            }

            yield return null;
        }

        displayedHealthPercentage = endValue;
        if (healthBar != null)
        {
            healthBar.value = displayedHealthPercentage;
        }
    }

    IEnumerator AnimateHealthColorCoroutine()
    {
        if (healthFill == null) yield break;

        Color startColor = healthFill.color;
        Color endColor = GetHealthColor(targetHealthPercentage);

        float elapsedTime = 0f;
        float duration = 1f / colorTransitionSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            healthFill.color = Color.Lerp(startColor, endColor, progress);

            // Also update frame color if available
            if (healthBarFrame != null)
            {
                Color frameColor = isEnraged ? frameEnragedColor : frameNormalColor;
                healthBarFrame.color = Color.Lerp(healthBarFrame.color, frameColor, progress);
            }

            yield return null;
        }

        healthFill.color = endColor;
    }

    IEnumerator EnrageAnimation()
    {
        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // Pulsing effect
            float pulse = Mathf.Sin(progress * Mathf.PI * 8f) * 0.1f + 1f;

            if (healthBarFrame != null)
            {
                healthBarFrame.transform.localScale = Vector3.one * pulse;
            }

            yield return null;
        }

        // Reset scale
        if (healthBarFrame != null)
        {
            healthBarFrame.transform.localScale = Vector3.one;
        }
    }

    void StartScreenShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ScreenShakeCoroutine());
    }

    IEnumerator ScreenShakeCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / shakeDuration;
            float intensity = shakeIntensity * (1f - progress); // Fade out

            Vector3 randomOffset = new Vector3(
                Random.Range(-intensity, intensity),
                Random.Range(-intensity, intensity),
                0f
            );

            transform.position = originalPosition + randomOffset;

            yield return null;
        }

        // Reset position
        transform.position = originalPosition;
    }

    void StartScreenFlash()
    {
        if (screenFlashImage != null)
        {
            StartCoroutine(ScreenFlashCoroutine());
        }
    }

    IEnumerator ScreenFlashCoroutine()
    {
        // Flash in
        float duration = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 0.3f, elapsedTime / duration);

            Color color = screenFlashImage.color;
            color.a = alpha;
            screenFlashImage.color = color;

            yield return null;
        }

        // Flash out
        elapsedTime = 0f;
        while (elapsedTime < duration * 2f)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0.3f, 0f, elapsedTime / (duration * 2f));

            Color color = screenFlashImage.color;
            color.a = alpha;
            screenFlashImage.color = color;

            yield return null;
        }

        // Ensure fully transparent
        Color finalColor = screenFlashImage.color;
        finalColor.a = 0f;
        screenFlashImage.color = finalColor;
    }

    // Helper methods
    Color GetHealthColor(float healthPercent)
    {
        if (isEnraged)
        {
            return enragedHealthColor;
        }
        else if (healthPercent <= 0.15f)
        {
            return criticalHealthColor;
        }
        else if (healthGradient != null)
        {
            return healthGradient.Evaluate(healthPercent);
        }
        else
        {
            return Color.green;
        }
    }

    void SetupDefaultHealthGradient()
    {
        healthGradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0] = new GradientColorKey(Color.red, 0f);    // 0% health
        colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f); // 50% health
        colorKeys[2] = new GradientColorKey(Color.green, 1f);  // 100% health

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0] = new GradientAlphaKey(1f, 0f);
        alphaKeys[1] = new GradientAlphaKey(1f, 1f);

        healthGradient.SetKeys(colorKeys, alphaKeys);
    }

    void CreateHealthBarSegments()
    {
        if (healthBar == null || healthSegments <= 1) return;

        // Create segment dividers
        for (int i = 1; i < healthSegments; i++)
        {
            float position = (float)i / healthSegments;
            CreateSegmentDivider(position);
        }
    }

    void CreateSegmentDivider(float position)
    {
        GameObject divider = new GameObject($"HealthSegment_{position:F1}");
        divider.transform.SetParent(healthBar.transform, false);

        Image dividerImage = divider.AddComponent<Image>();
        dividerImage.color = segmentBorderColor;

        RectTransform dividerRect = divider.GetComponent<RectTransform>();
        dividerRect.anchorMin = new Vector2(position, 0f);
        dividerRect.anchorMax = new Vector2(position, 1f);
        dividerRect.offsetMin = new Vector2(-segmentBorderWidth / 2, 0f);
        dividerRect.offsetMax = new Vector2(segmentBorderWidth / 2, 0f);
    }

    void SpawnDamageNumber(int damage)
    {
        if (damageNumberPrefab == null || damageNumberSpawn == null) return;

        GameObject damageNumber = Instantiate(damageNumberPrefab, damageNumberSpawn.position, Quaternion.identity);

        // Set damage text if it has a TextMeshProUGUI component
        TextMeshProUGUI damageText = damageNumber.GetComponent<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.text = damage.ToString();
        }
    }

    // Public methods
    public void SetBossName(string name)
    {
        if (bossNameText != null)
        {
            bossNameText.text = name;
        }
    }

    public void ForceUpdateUI()
    {
        if (bossScript != null)
        {
            lastKnownHP = bossScript.GetCurrentHP();
            targetHealthPercentage = (float)lastKnownHP / bossScript.maxHP;
            AnimateHealthBar();
        }
    }

    void OnDestroy()
    {
        // Stop all coroutines
        StopAllCoroutines();
    }
}