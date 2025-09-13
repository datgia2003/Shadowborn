using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Boss Health UI system với animated health bar, name plate, và phase indicators
/// Compatible với Igris boss system
/// </summary>
public class BossHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthBar;
    public Image healthFill;
    public TextMeshProUGUI bossNameText;
    public TextMeshProUGUI healthText; // "250 / 300"
    public GameObject bossUIPanel; // Toàn bộ boss UI để show/hide

    [Header("Health Bar Animation")]
    public float healthBarAnimationSpeed = 2f;
    public AnimationCurve healthBarCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visual Effects")]
    public Color normalHealthColor = Color.green;
    public Color enragedHealthColor = Color.red;
    public Color lowHealthColor = Color.yellow;
    public float colorTransitionSpeed = 1f;

    [Header("Phase Indicators")]
    public GameObject enragedIndicator; // "ENRAGED" text hoặc icon
    public float enragedIndicatorPulseSpeed = 2f;

    [Header("Boss Reference")]
    public Igris bossScript; // Reference to boss script

    // Private variables
    private float targetHealthPercentage;
    private float currentDisplayedHealth;
    private Color targetHealthColor;
    private bool isVisible = false;
    private bool isEnraged = false;

    // Animation coroutines
    private Coroutine healthBarCoroutine;
    private Coroutine colorTransitionCoroutine;
    private Coroutine enragedPulseCoroutine;

    void Start()
    {
        // Initialize UI - Hidden by default until boss becomes active
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false); // Hide UI by default
            isVisible = false;
        }

        if (enragedIndicator != null)
        {
            enragedIndicator.SetActive(false);
        }

        // Auto-find boss if not assigned
        if (bossScript == null)
        {
            bossScript = FindObjectOfType<Igris>();
        }

        // Setup initial values
        if (bossScript != null)
        {
            targetHealthColor = normalHealthColor;
            targetHealthPercentage = 1f; // Start at full health
            currentDisplayedHealth = 1f;

            if (healthFill != null)
            {
                healthFill.color = targetHealthColor;
            }

            if (healthBar != null)
            {
                healthBar.value = 1f;
            }

            if (bossNameText != null)
            {
                bossNameText.text = "Shadow of Igris"; // Default boss name
            }

            if (healthText != null)
            {
                healthText.text = $"{bossScript.maxHP} / {bossScript.maxHP}";
            }
        }
        else
        {
            Debug.LogWarning("BossHealthUI: No Igris boss script found!");
        }
    }

    void Update()
    {
        if (bossScript == null) return;

        // Update health values
        UpdateHealthDisplay();

        // Check enrage state
        CheckEnrageState();

        // Auto show/hide based on boss state
        UpdateVisibility();
    }

    void UpdateHealthDisplay()
    {
        int currentHP = bossScript.GetCurrentHP();
        int maxHP = bossScript.maxHP;

        // Calculate target percentage
        float newTargetPercentage = (float)currentHP / maxHP;

        // Only animate if health changed
        if (Mathf.Abs(targetHealthPercentage - newTargetPercentage) > 0.001f)
        {
            targetHealthPercentage = newTargetPercentage;

            // Start health bar animation
            if (healthBarCoroutine != null)
            {
                StopCoroutine(healthBarCoroutine);
            }
            healthBarCoroutine = StartCoroutine(AnimateHealthBar());

            // Update health color based on percentage
            UpdateHealthColor(targetHealthPercentage);
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{currentHP} / {maxHP}";
        }
    }

    void UpdateHealthColor(float healthPercentage)
    {
        Color newTargetColor;

        if (isEnraged)
        {
            newTargetColor = enragedHealthColor;
        }
        else if (healthPercentage <= 0.25f) // Low health (25%)
        {
            newTargetColor = lowHealthColor;
        }
        else
        {
            newTargetColor = normalHealthColor;
        }

        // Only change if different
        if (targetHealthColor != newTargetColor)
        {
            targetHealthColor = newTargetColor;

            // Start color transition
            if (colorTransitionCoroutine != null)
            {
                StopCoroutine(colorTransitionCoroutine);
            }
            colorTransitionCoroutine = StartCoroutine(AnimateHealthColor());
        }
    }

    void CheckEnrageState()
    {
        // Check if boss is enraged using the IsEnraged() method
        bool bossEnraged = bossScript.IsEnraged();

        if (isEnraged != bossEnraged)
        {
            isEnraged = bossEnraged;

            if (enragedIndicator != null)
            {
                enragedIndicator.SetActive(isEnraged);

                if (isEnraged)
                {
                    // Start pulsing animation
                    if (enragedPulseCoroutine != null)
                    {
                        StopCoroutine(enragedPulseCoroutine);
                    }
                    enragedPulseCoroutine = StartCoroutine(PulseEnragedIndicator());
                }
                else
                {
                    // Stop pulsing
                    if (enragedPulseCoroutine != null)
                    {
                        StopCoroutine(enragedPulseCoroutine);
                    }
                }
            }

            // Update health color
            UpdateHealthColor(targetHealthPercentage);
        }
    }

    void UpdateVisibility()
    {
        // Show UI when boss is active and alive
        bool shouldBeVisible = bossScript.GetCurrentState() != Igris.BossState.Dead &&
                              bossScript.GetCurrentHP() > 0;

        if (isVisible != shouldBeVisible)
        {
            isVisible = shouldBeVisible;

            if (bossUIPanel != null)
            {
                bossUIPanel.SetActive(isVisible);
            }
        }
    }

    // Animation coroutines
    IEnumerator AnimateHealthBar()
    {
        float startValue = currentDisplayedHealth;
        float endValue = targetHealthPercentage;
        float elapsedTime = 0f;
        float duration = 1f / healthBarAnimationSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            float curveProgress = healthBarCurve.Evaluate(progress);

            currentDisplayedHealth = Mathf.Lerp(startValue, endValue, curveProgress);

            if (healthBar != null)
            {
                healthBar.value = currentDisplayedHealth;
            }

            yield return null;
        }

        // Ensure final value
        currentDisplayedHealth = endValue;
        if (healthBar != null)
        {
            healthBar.value = currentDisplayedHealth;
        }
    }

    IEnumerator AnimateHealthColor()
    {
        Color startColor = healthFill != null ? healthFill.color : normalHealthColor;
        Color endColor = targetHealthColor;
        float elapsedTime = 0f;
        float duration = 1f / colorTransitionSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            Color currentColor = Color.Lerp(startColor, endColor, progress);

            if (healthFill != null)
            {
                healthFill.color = currentColor;
            }

            yield return null;
        }

        // Ensure final color
        if (healthFill != null)
        {
            healthFill.color = endColor;
        }
    }

    IEnumerator PulseEnragedIndicator()
    {
        while (isEnraged && enragedIndicator != null)
        {
            float pulse = (Mathf.Sin(Time.time * enragedIndicatorPulseSpeed) + 1f) / 2f; // 0 to 1
            float scale = Mathf.Lerp(0.9f, 1.1f, pulse);

            enragedIndicator.transform.localScale = Vector3.one * scale;

            // Optional: pulse alpha/color too
            if (enragedIndicator.GetComponent<TextMeshProUGUI>() != null)
            {
                var text = enragedIndicator.GetComponent<TextMeshProUGUI>();
                Color color = text.color;
                color.a = Mathf.Lerp(0.7f, 1f, pulse);
                text.color = color;
            }

            yield return null;
        }

        // Reset scale when done
        if (enragedIndicator != null)
        {
            enragedIndicator.transform.localScale = Vector3.one;
        }
    }

    // Public methods for external control
    public void ShowBossUI()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(true);
            isVisible = true;
        }
    }

    public void HideBossUI()
    {
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(false);
            isVisible = false;
        }
    }

    public void SetBossName(string name)
    {
        if (bossNameText != null)
        {
            bossNameText.text = name;
        }
    }

    // Manual health update if needed
    public void ForceUpdateHealth()
    {
        UpdateHealthDisplay();
    }

    void OnDestroy()
    {
        // Stop all coroutines
        if (healthBarCoroutine != null)
        {
            StopCoroutine(healthBarCoroutine);
        }
        if (colorTransitionCoroutine != null)
        {
            StopCoroutine(colorTransitionCoroutine);
        }
        if (enragedPulseCoroutine != null)
        {
            StopCoroutine(enragedPulseCoroutine);
        }
    }
}