using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main HUD controller for player resources (Health, Mana, Energy)
/// Manages health bar, mana bar, energy bar, and player avatar display
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [Header("Health UI")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public Image healthFill;
    public Gradient healthGradient;

    [Header("Mana UI")]
    public Slider manaBar;
    public TextMeshProUGUI manaText;
    public Image manaFill;
    public Color manaColor = Color.blue;

    [Header("Energy UI")]
    public Slider energyBar;
    public TextMeshProUGUI energyText;
    public Image energyFill;
    public Color energyColor = Color.yellow;

    [Header("Player Avatar")]
    public Image playerAvatar;
    public Sprite playerPortrait;
    public Image avatarFrame;
    public Color normalFrameColor = Color.white;
    public Color lowHealthFrameColor = Color.red;

    [Header("Animation Settings")]
    public float barAnimationSpeed = 2f;
    public float lowHealthThreshold = 0.3f; // 30% health
    public bool enableHealthPulse = true;
    public float pulseSpeed = 2f;

    // Internal animation tracking
    private float targetHealthValue;
    private float targetManaValue;
    private float targetEnergyValue;

    private bool isLowHealth = false;
    private float pulseTimer = 0f;

    void Start()
    {
        // Subscribe to resource events
        PlayerResources.OnHealthChanged += UpdateHealth;
        PlayerResources.OnManaChanged += UpdateMana;
        PlayerResources.OnEnergyChanged += UpdateEnergy;
        PlayerResources.OnPlayerDied += OnPlayerDied;

        // Initialize UI
        InitializeUI();
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        PlayerResources.OnHealthChanged -= UpdateHealth;
        PlayerResources.OnManaChanged -= UpdateMana;
        PlayerResources.OnEnergyChanged -= UpdateEnergy;
        PlayerResources.OnPlayerDied -= OnPlayerDied;
    }

    void Update()
    {
        // Animate bars to target values
        AnimateBars();

        // Handle low health effects
        HandleLowHealthEffects();
    }

    private void InitializeUI()
    {
        // Set up player avatar
        if (playerAvatar != null && playerPortrait != null)
        {
            playerAvatar.sprite = playerPortrait;
        }

        // Initialize bar colors
        if (manaFill != null) manaFill.color = manaColor;
        if (energyFill != null) energyFill.color = energyColor;

        // Set initial frame color
        if (avatarFrame != null) avatarFrame.color = normalFrameColor;
    }

    private void UpdateHealth(int currentHealth, int maxHealth)
    {
        targetHealthValue = (float)currentHealth / maxHealth;

        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Check for low health state
        isLowHealth = targetHealthValue <= lowHealthThreshold;
    }

    private void UpdateMana(int currentMana, int maxMana)
    {
        targetManaValue = (float)currentMana / maxMana;

        // Update text
        if (manaText != null)
        {
            manaText.text = $"{currentMana}/{maxMana}";
        }
    }

    private void UpdateEnergy(int currentEnergy, int maxEnergy)
    {
        targetEnergyValue = (float)currentEnergy / maxEnergy;

        // Update text
        if (energyText != null)
        {
            energyText.text = $"{currentEnergy}/{maxEnergy}";
        }
    }

    private void AnimateBars()
    {
        // Smoothly animate health bar
        if (healthBar != null)
        {
            healthBar.value = Mathf.Lerp(healthBar.value, targetHealthValue,
                barAnimationSpeed * Time.deltaTime);

            // Update health color gradient
            if (healthFill != null && healthGradient != null)
            {
                healthFill.color = healthGradient.Evaluate(healthBar.value);
            }
        }

        // Smoothly animate mana bar
        if (manaBar != null)
        {
            manaBar.value = Mathf.Lerp(manaBar.value, targetManaValue,
                barAnimationSpeed * Time.deltaTime);
        }

        // Smoothly animate energy bar
        if (energyBar != null)
        {
            energyBar.value = Mathf.Lerp(energyBar.value, targetEnergyValue,
                barAnimationSpeed * Time.deltaTime);
        }
    }

    private void HandleLowHealthEffects()
    {
        if (!enableHealthPulse) return;

        if (isLowHealth)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;

            // Pulse avatar frame color
            if (avatarFrame != null)
            {
                float pulseAlpha = (Mathf.Sin(pulseTimer) + 1f) * 0.5f; // 0-1 range
                Color currentColor = Color.Lerp(normalFrameColor, lowHealthFrameColor, pulseAlpha);
                avatarFrame.color = currentColor;
            }

            // Optional: Pulse health bar
            if (healthFill != null)
            {
                float pulseIntensity = (Mathf.Sin(pulseTimer * 1.5f) + 1f) * 0.1f + 0.9f; // 0.9-1.1 range
                Color baseColor = healthGradient != null ? healthGradient.Evaluate(healthBar.value) : Color.red;
                healthFill.color = baseColor * pulseIntensity;
            }
        }
        else
        {
            // Reset to normal colors
            if (avatarFrame != null)
            {
                avatarFrame.color = normalFrameColor;
            }

            if (healthFill != null && healthGradient != null)
            {
                healthFill.color = healthGradient.Evaluate(healthBar.value);
            }
        }
    }

    private void OnPlayerDied()
    {
        // Handle player death UI changes
        Debug.Log("Player died - updating UI");

        // You can add death screen transition here
        // For now, just ensure health bar shows 0
        targetHealthValue = 0f;

        // Optional: Show death overlay or transition to game over screen
        // GameManager.Instance?.ShowGameOverScreen();
    }

    // Public methods for manual updates (if needed)
    public void SetHealthBarValue(float value)
    {
        targetHealthValue = Mathf.Clamp01(value);
    }

    public void SetManaBarValue(float value)
    {
        targetManaValue = Mathf.Clamp01(value);
    }

    public void SetEnergyBarValue(float value)
    {
        targetEnergyValue = Mathf.Clamp01(value);
    }

    public void SetPlayerPortrait(Sprite newPortrait)
    {
        if (playerAvatar != null && newPortrait != null)
        {
            playerAvatar.sprite = newPortrait;
        }
    }
}
