using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Solo Leveling Style Level Up Notification System
/// Shows animated "Level UP!" and "+X Points" messages
/// </summary>
public class LevelUpNotification : MonoBehaviour
{
    [Header("🎉 Notification UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI levelUpText;
    [SerializeField] private TextMeshProUGUI pointsAwardedText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("✨ Visual Effects")]
    [SerializeField] private ParticleSystem levelUpParticles;
    [SerializeField] private ParticleSystem pointsParticles;

    [Header("🔊 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private AudioClip pointsSound;

    [Header("⚙️ Animation Settings")]
    [SerializeField] private float showDuration = 3f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.5f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float maxScale = 1.2f;

    [Header("🎨 Style Settings")]
    [SerializeField] private Color levelUpColor = Color.yellow;
    [SerializeField] private Color pointsColor = Color.cyan;

    void Start()
    {
        // Auto-setup notificationPanel if not assigned
        if (notificationPanel == null)
        {
            notificationPanel = gameObject;
            Debug.Log($"🎉 Auto-assigned notificationPanel to {gameObject.name}");
        }

        // Setup CanvasGroup for proper show/hide
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Initialize - start hidden using CanvasGroup instead of SetActive
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Debug.Log($"🎉 LevelUpNotification initialized with CanvasGroup hidden");
        }

        // Subscribe to events - chỉ listen OnMultiLevelUp để tránh duplicate
        ExperienceSystem.OnMultiLevelUp += OnMultiLevelUp;
        PlayerStats.OnPointsAwarded += OnPointsAwarded;

        Debug.Log("🎉 LevelUpNotification initialized and subscribed to events");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        ExperienceSystem.OnMultiLevelUp -= OnMultiLevelUp;
        PlayerStats.OnPointsAwarded -= OnPointsAwarded;
    }

    /// <summary>
    /// Called when player gains multiple levels at once
    /// </summary>
    private void OnMultiLevelUp(int startLevel, int endLevel, int totalPoints)
    {
        int levelsGained = endLevel - startLevel;
        Debug.Log($"LevelUpNotification: OnMultiLevelUp called - {startLevel}→{endLevel} ({levelsGained} levels), {totalPoints} points");

        // Show individual notifications for each level gained
        StartCoroutine(ShowMultipleLevelUpNotifications(startLevel, endLevel, totalPoints));
    }

    /// <summary>
    /// Called when stat points are awarded
    /// </summary>
    private void OnPointsAwarded(int points)
    {
        Debug.Log($"LevelUpNotification: OnPointsAwarded called with {points} points");
        // This will be shown together with level up notification
        // The points display is handled in ShowLevelUpNotification
    }

    /// <summary>
    /// Show level up notification with points
    /// </summary>
    public void ShowLevelUpNotification(int newLevel, int pointsAwarded = 3)
    {
        Debug.Log($"LevelUpNotification: ShowLevelUpNotification called for level {newLevel}, points {pointsAwarded}");

        string levelText = $"LEVEL {newLevel}!";
        string pointText = $"+{pointsAwarded} Points";

        Debug.Log($"LevelUpNotification: Starting notification coroutine: '{levelText}' '{pointText}'");
        StartCoroutine(ShowNotificationCoroutine(levelText, pointText));
    }

    /// <summary>
    /// Show multiple individual level up notifications
    /// </summary>
    private IEnumerator ShowMultipleLevelUpNotifications(int startLevel, int endLevel, int totalPoints)
    {
        int levelsGained = endLevel - startLevel;
        int pointsPerLevel = totalPoints / levelsGained;

        Debug.Log($"LevelUpNotification: Showing {levelsGained} individual notifications, {pointsPerLevel} points each (total: {totalPoints})");

        for (int i = 0; i < levelsGained; i++)
        {
            int currentLevel = startLevel + i + 1;

            // Show "LEVEL UP!" for each level as requested
            string levelText = "LEVEL UP!";
            
            // Option 1: Show points per level (current)
            // string pointText = $"+{pointsPerLevel} Points";
            
            // Option 2: Show total points gained (if you prefer this)
            string pointText = (levelsGained == 1) ? $"+{totalPoints} Points" : $"+{totalPoints} Points Total";

            Debug.Log($"LevelUpNotification: Showing notification {i + 1}/{levelsGained}: '{levelText}' '{pointText}' (reached level {currentLevel})");

            // Start the notification coroutine
            yield return StartCoroutine(ShowNotificationCoroutine(levelText, pointText));

            // Short delay between notifications if there are more
            if (i < levelsGained - 1)
            {
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }

        Debug.Log($"🎉 Completed showing {levelsGained} level up notifications!");
    }

    /// <summary>
    /// Main notification coroutine (accepts custom text)
    /// </summary>
    private IEnumerator ShowNotificationCoroutine(string levelText, string pointText)
    {
        Debug.Log($"LevelUpNotification: ShowNotificationCoroutine started - '{levelText}', '{pointText}'");

        // Check if CanvasGroup exists
        if (canvasGroup == null)
        {
            Debug.LogError("LevelUpNotification: canvasGroup is null! Cannot show notification!");
            yield break;
        }

        // Make sure panel is active (but invisible)
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }

        // Enable interaction
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Debug.Log($"LevelUpNotification: Preparing to show notification panel");

        // Set text content
        if (levelUpText != null)
        {
            levelUpText.text = levelText;
            levelUpText.color = levelUpColor;
            Debug.Log($"LevelUpNotification: Set levelUpText to '{levelText}'");
        }
        else
        {
            Debug.LogError("LevelUpNotification: levelUpText is null!");
        }

        if (pointsAwardedText != null)
        {
            pointsAwardedText.text = pointText;
            pointsAwardedText.color = pointsColor;
        }

        // Play sound
        PlayLevelUpSound();

        // Play particles
        PlayLevelUpParticles();

        // Animate in
        yield return StartCoroutine(FadeInAnimation());

        // Wait for display duration
        yield return new WaitForSecondsRealtime(showDuration);

        // Animate out
        yield return StartCoroutine(FadeOutAnimation());

        // Hide using CanvasGroup instead of deactivating
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        Debug.Log($"🎉 Level up notification completed: '{levelText}' '{pointText}'");
    }

    /// <summary>
    /// Fade in animation
    /// </summary>
    private IEnumerator FadeInAnimation()
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsedTime < fadeInTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / fadeInTime;

            // Fade alpha
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            // Scale animation with curve
            float scaleValue = scaleCurve.Evaluate(t);
            transform.localScale = originalScale * Mathf.Lerp(0.5f, maxScale, scaleValue);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = originalScale * maxScale;

        // Scale back to normal
        elapsedTime = 0f;
        float scaleBackTime = 0.2f;

        while (elapsedTime < scaleBackTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / scaleBackTime;

            transform.localScale = Vector3.Lerp(originalScale * maxScale, originalScale, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Fade out animation
    /// </summary>
    private IEnumerator FadeOutAnimation()
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / fadeOutTime;

            // Fade alpha
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            // Scale down slightly
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = originalScale;
    }

    /// <summary>
    /// Play level up sound effect
    /// </summary>
    private void PlayLevelUpSound()
    {
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
    }

    /// <summary>
    /// Play level up particle effects
    /// </summary>
    private void PlayLevelUpParticles()
    {
        if (levelUpParticles != null)
        {
            levelUpParticles.Play();
        }

        // Delay points particles slightly
        if (pointsParticles != null)
        {
            StartCoroutine(DelayedParticles());
        }
    }

    /// <summary>
    /// Play points particles with delay
    /// </summary>
    private IEnumerator DelayedParticles()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        if (pointsParticles != null)
        {
            pointsParticles.Play();
        }

        // Play points sound
        if (audioSource != null && pointsSound != null)
        {
            audioSource.PlayOneShot(pointsSound);
        }
    }

    /// <summary>
    /// Test method to trigger notification manually
    /// </summary>
    [ContextMenu("Test Level Up Notification")]
    public void TestNotification()
    {
        ShowLevelUpNotification(99, 3);
    }

    /// <summary>
    /// Test multi-level up notification
    /// </summary>
    [ContextMenu("Test Multi-Level Up")]
    public void TestMultiLevelNotification()
    {
        StartCoroutine(ShowMultipleLevelUpNotifications(5, 8, 9)); // 3 levels, 9 points
    }

    /// <summary>
    /// Set custom colors for different notification types
    /// </summary>
    public void SetNotificationColors(Color levelColor, Color pointColor)
    {
        levelUpColor = levelColor;
        pointsColor = pointColor;
    }
}