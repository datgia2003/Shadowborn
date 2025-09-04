using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Solo Leveling Style Stats Panel UI
/// Shows stats with allocation buttons, toggles with Tab key
/// </summary>
public class StatsPanel : MonoBehaviour
{
    [Header("🎯 Solo Leveling Stats Panel")]
    [Space(5)]

    [Header("📱 Panel References")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("📊 Stat Display")]
    [SerializeField] private TextMeshProUGUI vitalityText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI agilityText;
    [SerializeField] private TextMeshProUGUI criticalText;
    [SerializeField] private TextMeshProUGUI availablePointsText;

    [Header("⚡ Stat Effects Display")]
    [SerializeField] private TextMeshProUGUI vitalityEffectText;
    [SerializeField] private TextMeshProUGUI strengthEffectText;
    [SerializeField] private TextMeshProUGUI intelligenceEffectText;
    [SerializeField] private TextMeshProUGUI agilityEffectText;
    [SerializeField] private TextMeshProUGUI criticalEffectText;

    [Header("🔘 Allocation Buttons")]
    [SerializeField] private Button vitalityButton;
    [SerializeField] private Button strengthButton;
    [SerializeField] private Button intelligenceButton;
    [SerializeField] private Button agilityButton;
    [SerializeField] private Button criticalButton;

    [Header("🎨 Colors")]
    [SerializeField] private Color vitalityColor = Color.green;
    [SerializeField] private Color strengthColor = Color.red;
    [SerializeField] private Color intelligenceColor = Color.blue;
    [SerializeField] private Color agilityColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.magenta;

    private bool isPanelOpen = false;

    void Start()
    {
        // Auto-setup panel reference
        if (statsPanel == null)
        {
            statsPanel = gameObject;
        }

        // Auto-setup CanvasGroup
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Setup button listeners
        SetupButtons();

        // Subscribe to events
        PlayerStats.OnStatChanged += OnStatChanged;
        PlayerStats.OnPointsChanged += OnPointsChanged;

        // Start hidden
        ClosePanel();

        // Initial UI update
        UpdateAllStats();

        Debug.Log("🎯 StatsPanel initialized");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        PlayerStats.OnStatChanged -= OnStatChanged;
        PlayerStats.OnPointsChanged -= OnPointsChanged;
    }

    /// <summary>
    /// Setup allocation button listeners
    /// </summary>
    private void SetupButtons()
    {
        if (vitalityButton != null)
            vitalityButton.onClick.AddListener(() => AllocatePoint(PlayerStats.StatType.Vitality));
        if (strengthButton != null)
            strengthButton.onClick.AddListener(() => AllocatePoint(PlayerStats.StatType.Strength));
        if (intelligenceButton != null)
            intelligenceButton.onClick.AddListener(() => AllocatePoint(PlayerStats.StatType.Intelligence));
        if (agilityButton != null)
            agilityButton.onClick.AddListener(() => AllocatePoint(PlayerStats.StatType.Agility));
        if (criticalButton != null)
            criticalButton.onClick.AddListener(() => AllocatePoint(PlayerStats.StatType.Critical));
    }

    /// <summary>
    /// Toggle panel open/close
    /// </summary>
    public void TogglePanel()
    {
        if (isPanelOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    /// <summary>
    /// Open stats panel
    /// </summary>
    public void OpenPanel()
    {
        isPanelOpen = true;

        if (statsPanel != null)
        {
            statsPanel.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // Pause game
        Time.timeScale = 0f;

        // Update UI
        UpdateAllStats();

        Debug.Log("📊 Stats Panel Opened");
    }

    /// <summary>
    /// Close stats panel
    /// </summary>
    public void ClosePanel()
    {
        isPanelOpen = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        // Resume game
        Time.timeScale = 1f;

        Debug.Log("📊 Stats Panel Closed");
    }

    /// <summary>
    /// Allocate point to specific stat
    /// </summary>
    private void AllocatePoint(PlayerStats.StatType statType)
    {
        if (PlayerStats.Instance != null)
        {
            bool success = PlayerStats.Instance.AllocatePoint(statType);
            if (success)
            {
                Debug.Log($"✅ Point allocated to {statType}");
            }
            else
            {
                Debug.Log($"❌ No available points to allocate");
            }
        }
    }

    /// <summary>
    /// Called when a stat changes
    /// </summary>
    private void OnStatChanged(PlayerStats.StatType statType, int newValue)
    {
        UpdateStatDisplay(statType, newValue);
        UpdateStatEffect(statType);
    }

    /// <summary>
    /// Called when available points change
    /// </summary>
    private void OnPointsChanged(int availablePoints)
    {
        UpdateAvailablePointsDisplay(availablePoints);
        UpdateButtonStates(availablePoints > 0);
    }

    /// <summary>
    /// Update all stat displays
    /// </summary>
    private void UpdateAllStats()
    {
        if (PlayerStats.Instance == null) return;

        UpdateStatDisplay(PlayerStats.StatType.Vitality, PlayerStats.Instance.Vitality);
        UpdateStatDisplay(PlayerStats.StatType.Strength, PlayerStats.Instance.Strength);
        UpdateStatDisplay(PlayerStats.StatType.Intelligence, PlayerStats.Instance.Intelligence);
        UpdateStatDisplay(PlayerStats.StatType.Agility, PlayerStats.Instance.Agility);
        UpdateStatDisplay(PlayerStats.StatType.Critical, PlayerStats.Instance.Critical);

        UpdateAllStatEffects();
        UpdateAvailablePointsDisplay(PlayerStats.Instance.AvailablePoints);
        UpdateButtonStates(PlayerStats.Instance.AvailablePoints > 0);
    }

    /// <summary>
    /// Update individual stat display
    /// </summary>
    private void UpdateStatDisplay(PlayerStats.StatType statType, int value)
    {
        TextMeshProUGUI targetText = statType switch
        {
            PlayerStats.StatType.Vitality => vitalityText,
            PlayerStats.StatType.Strength => strengthText,
            PlayerStats.StatType.Intelligence => intelligenceText,
            PlayerStats.StatType.Agility => agilityText,
            PlayerStats.StatType.Critical => criticalText,
            _ => null
        };

        if (targetText != null)
        {
            targetText.text = value.ToString();

            // Set color
            Color statColor = statType switch
            {
                PlayerStats.StatType.Vitality => vitalityColor,
                PlayerStats.StatType.Strength => strengthColor,
                PlayerStats.StatType.Intelligence => intelligenceColor,
                PlayerStats.StatType.Agility => agilityColor,
                PlayerStats.StatType.Critical => criticalColor,
                _ => Color.white
            };
            targetText.color = statColor;
        }
    }

    /// <summary>
    /// Update all stat effects
    /// </summary>
    private void UpdateAllStatEffects()
    {
        UpdateStatEffect(PlayerStats.StatType.Vitality);
        UpdateStatEffect(PlayerStats.StatType.Strength);
        UpdateStatEffect(PlayerStats.StatType.Intelligence);
        UpdateStatEffect(PlayerStats.StatType.Agility);
        UpdateStatEffect(PlayerStats.StatType.Critical);
    }

    /// <summary>
    /// Update stat effect description
    /// </summary>
    private void UpdateStatEffect(PlayerStats.StatType statType)
    {
        if (PlayerStats.Instance == null) return;

        TextMeshProUGUI targetText = statType switch
        {
            PlayerStats.StatType.Vitality => vitalityEffectText,
            PlayerStats.StatType.Strength => strengthEffectText,
            PlayerStats.StatType.Intelligence => intelligenceEffectText,
            PlayerStats.StatType.Agility => agilityEffectText,
            PlayerStats.StatType.Critical => criticalEffectText,
            _ => null
        };

        if (targetText != null)
        {
            string effectText = statType switch
            {
                PlayerStats.StatType.Vitality => GetVitalityEffectText(),
                PlayerStats.StatType.Strength => $"ATK: {PlayerStats.Instance.AttackDamage}",
                PlayerStats.StatType.Intelligence => $"MP: {PlayerStats.Instance.MaxMana} | Skill DMG: +{PlayerStats.Instance.SkillDamageBonus:F1}",
                PlayerStats.StatType.Agility => $"SPD: {PlayerStats.Instance.MovementSpeed:F1} | ATK SPD: {PlayerStats.Instance.AttackSpeed:F2}x",
                PlayerStats.StatType.Critical => $"CRIT: {PlayerStats.Instance.CriticalChancePercent:F1}%",
                _ => ""
            };
            targetText.text = effectText;
        }
    }

    /// <summary>
    /// Update available points display
    /// </summary>
    private void UpdateAvailablePointsDisplay(int points)
    {
        if (availablePointsText != null)
        {
            availablePointsText.text = $"Available Points: {points}";
            availablePointsText.color = points > 0 ? Color.yellow : Color.gray;
        }
    }

    /// <summary>
    /// Update button interactable states
    /// </summary>
    private void UpdateButtonStates(bool hasPoints)
    {
        if (vitalityButton != null) vitalityButton.interactable = hasPoints;
        if (strengthButton != null) strengthButton.interactable = hasPoints;
        if (intelligenceButton != null) intelligenceButton.interactable = hasPoints;
        if (agilityButton != null) agilityButton.interactable = hasPoints;
        if (criticalButton != null) criticalButton.interactable = hasPoints;
    }

    /// <summary>
    /// Get vitality effect text from PlayerResources
    /// </summary>
    private string GetVitalityEffectText()
    {
        var playerResources = FindObjectOfType<PlayerResources>();
        if (playerResources != null && PlayerStats.Instance != null)
        {
            return $"HP: {playerResources.maxHealth} | DEF: +{PlayerStats.Instance.DefenseReduction:F1}";
        }
        else
        {
            return "HP: -- | DEF: --";
        }
    }

    /// <summary>
    /// Public method for external toggle (Input Manager)
    /// </summary>
    public bool IsOpen => isPanelOpen;
}
