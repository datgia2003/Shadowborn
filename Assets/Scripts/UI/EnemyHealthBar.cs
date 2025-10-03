using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Health bar system cho enemy thường (bat, skeleton) 
/// Floating health bar xuất hiện phía trên enemy
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider healthBar;
    public Image healthFill;
    public Canvas healthBarCanvas;
    public GameObject healthBarPanel;

    [Header("Health Bar Settings")]
    public Vector3 offset = new Vector3(0, 1.5f, 0); // Offset từ enemy
    public float hideDelay = 3f; // Thời gian ẩn health bar sau khi không combat
    public bool alwaysVisible = false; // Luôn hiển thị health bar
    public bool showOnlyWhenDamaged = true; // Chỉ hiện khi bị damage

    [Header("Visual Settings")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float colorTransitionSpeed = 3f;
    public float animationSpeed = 5f;

    [Header("Scale Settings")]
    public Vector3 healthBarScale = Vector3.one;
    public bool scaleWithDistance = false;
    public float minScale = 0.5f;
    public float maxScale = 1.5f;
    public float scaleDistance = 10f;

    // Private variables
    private int currentHealth;
    private int maxHealth;
    private float targetHealthPercentage;
    private float displayedHealthPercentage;
    private bool isVisible = false;
    private float lastDamageTime;
    private Transform player;
    private Camera playerCamera;
    private Coroutine hideCoroutine;
    private Coroutine animationCoroutine;

    // Component references
    private Transform enemyTransform;
    private BatController batController;
    private SkeletonController skeletonController;

    void Awake()
    {
        enemyTransform = transform;
        
        // Try to find controllers
        batController = GetComponent<BatController>();
        skeletonController = GetComponent<SkeletonController>();
        
        // Create health bar if not assigned
        if (healthBarCanvas == null)
        {
            CreateHealthBarUI();
        }

        // Find player and camera
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerCamera = Camera.main;
            if (playerCamera == null)
                playerCamera = FindObjectOfType<Camera>();
        }
    }

    void Start()
    {
        // Initialize health values
        UpdateMaxHealth();
        UpdateCurrentHealth();
        
        // Setup initial state
        targetHealthPercentage = GetHealthPercentage();
        displayedHealthPercentage = targetHealthPercentage;
        
        if (healthBar != null)
            healthBar.value = displayedHealthPercentage;
        
        UpdateHealthColor();
        
        // Hide by default unless always visible
        if (!alwaysVisible)
            HideHealthBar();
        else
            ShowHealthBar();
    }

    void Update()
    {
        if (healthBarCanvas == null) return;

        // Update position
        UpdateHealthBarPosition();
        
        // Update scale if enabled
        if (scaleWithDistance && player != null)
            UpdateHealthBarScale();
        
        // Update health values
        UpdateHealthValues();
        
        // Auto-hide logic
        if (!alwaysVisible && isVisible && showOnlyWhenDamaged)
        {
            if (Time.time - lastDamageTime > hideDelay)
            {
                HideHealthBar();
            }
        }

        // Always face camera
        if (playerCamera != null && healthBarCanvas != null)
        {
            healthBarCanvas.transform.LookAt(playerCamera.transform);
            healthBarCanvas.transform.Rotate(0, 180, 0); // Flip to face camera properly
        }
    }

    void CreateHealthBarUI()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("EnemyHealthBar_Canvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;
        
        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.worldCamera = Camera.main;
        
        // Scale canvas
        canvasObj.transform.localScale = healthBarScale;
        
        // Add CanvasScaler for consistent sizing
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1f;
        
        // Create health bar panel
        GameObject panelObj = new GameObject("HealthBar_Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        healthBarPanel = panelObj;
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(100, 10);
        panelRect.anchoredPosition = Vector2.zero;
        
        // Create background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(panelObj.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create health bar slider
        GameObject sliderObj = new GameObject("HealthBar_Slider");
        sliderObj.transform.SetParent(panelObj.transform, false);
        
        healthBar = sliderObj.AddComponent<Slider>();
        healthBar.minValue = 0f;
        healthBar.maxValue = 1f;
        healthBar.value = 1f;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        // Create fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        
        healthFill = fillObj.AddComponent<Image>();
        healthFill.color = fullHealthColor;
        healthFill.type = Image.Type.Filled;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // Assign fill to slider
        healthBar.fillRect = fillRect;
        healthBar.targetGraphic = healthFill;
        
        Debug.Log($"[EnemyHealthBar] Created health bar UI for {gameObject.name}");
    }

    void UpdateHealthBarPosition()
    {
        if (healthBarCanvas != null)
        {
            healthBarCanvas.transform.position = enemyTransform.position + offset;
        }
    }

    void UpdateHealthBarScale()
    {
        if (player == null || healthBarCanvas == null) return;
        
        float distance = Vector3.Distance(player.position, enemyTransform.position);
        float scale = Mathf.Lerp(maxScale, minScale, distance / scaleDistance);
        scale = Mathf.Clamp(scale, minScale, maxScale);
        
        healthBarCanvas.transform.localScale = healthBarScale * scale;
    }

    void UpdateHealthValues()
    {
        int newCurrentHealth = GetCurrentHealth();
        int newMaxHealth = GetMaxHealth();
        
        bool healthChanged = newCurrentHealth != currentHealth || newMaxHealth != maxHealth;
        
        if (healthChanged)
        {
            // Check if took damage
            if (newCurrentHealth < currentHealth)
            {
                OnTakeDamage(currentHealth - newCurrentHealth);
            }
            
            currentHealth = newCurrentHealth;
            maxHealth = newMaxHealth;
            
            float newTargetPercentage = GetHealthPercentage();
            if (Mathf.Abs(targetHealthPercentage - newTargetPercentage) > 0.01f)
            {
                targetHealthPercentage = newTargetPercentage;
                AnimateHealthBar();
                UpdateHealthColor();
            }
        }
        
        // Update displayed health bar
        if (Mathf.Abs(displayedHealthPercentage - targetHealthPercentage) > 0.01f)
        {
            displayedHealthPercentage = Mathf.Lerp(displayedHealthPercentage, targetHealthPercentage, Time.deltaTime * animationSpeed);
            if (healthBar != null)
                healthBar.value = displayedHealthPercentage;
        }
    }

    void OnTakeDamage(int damage)
    {
        lastDamageTime = Time.time;
        
        if (showOnlyWhenDamaged && !isVisible)
        {
            ShowHealthBar();
        }
        
        // Stop hide coroutine if running
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }

    void AnimateHealthBar()
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
        
        animationCoroutine = StartCoroutine(AnimateHealthBarCoroutine());
    }

    IEnumerator AnimateHealthBarCoroutine()
    {
        float startValue = displayedHealthPercentage;
        float endValue = targetHealthPercentage;
        float duration = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            displayedHealthPercentage = Mathf.Lerp(startValue, endValue, progress);
            
            if (healthBar != null)
                healthBar.value = displayedHealthPercentage;
            
            yield return null;
        }
        
        displayedHealthPercentage = endValue;
        if (healthBar != null)
            healthBar.value = displayedHealthPercentage;
    }

    void UpdateHealthColor()
    {
        if (healthFill == null) return;
        
        Color targetColor;
        
        if (targetHealthPercentage > 0.6f)
            targetColor = fullHealthColor;
        else if (targetHealthPercentage > 0.3f)
            targetColor = Color.Lerp(midHealthColor, fullHealthColor, (targetHealthPercentage - 0.3f) / 0.3f);
        else
            targetColor = Color.Lerp(lowHealthColor, midHealthColor, targetHealthPercentage / 0.3f);
        
        healthFill.color = Color.Lerp(healthFill.color, targetColor, Time.deltaTime * colorTransitionSpeed);
    }

    void UpdateMaxHealth()
    {
        if (batController != null)
            maxHealth = batController.maxHealth;
        else if (skeletonController != null)
            maxHealth = skeletonController.maxHealth;
        else
            maxHealth = 100; // Fallback
    }

    void UpdateCurrentHealth()
    {
        currentHealth = GetCurrentHealth();
    }

    int GetCurrentHealth()
    {
        // Get current health from enemy controllers
        if (batController != null)
        {
            return batController.GetCurrentHealth();
        }
        else if (skeletonController != null)
        {
            return skeletonController.GetCurrentHealth();
        }
        
        return maxHealth;
    }

    int GetMaxHealth()
    {
        if (batController != null)
            return batController.GetMaxHealth();
        else if (skeletonController != null)
            return skeletonController.GetMaxHealth();
        else
            return 100;
    }

    float GetHealthPercentage()
    {
        if (maxHealth <= 0) return 0f;
        return (float)currentHealth / maxHealth;
    }

    public void ShowHealthBar()
    {
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(true);
            isVisible = true;
        }
    }

    public void HideHealthBar()
    {
        if (healthBarPanel != null)
        {
            healthBarPanel.SetActive(false);
            isVisible = false;
        }
    }

    public void SetHealth(int current, int max)
    {
        currentHealth = current;
        maxHealth = max;
        targetHealthPercentage = GetHealthPercentage();
        
        if (healthBar != null)
            healthBar.value = targetHealthPercentage;
        
        UpdateHealthColor();
    }

    public void TakeDamage(int damage)
    {
        int newHealth = Mathf.Max(0, currentHealth - damage);
        SetHealth(newHealth, maxHealth);
        OnTakeDamage(damage);
    }

    void OnDestroy()
    {
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);
    }

    void OnDrawGizmosSelected()
    {
        // Draw health bar position in editor
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(1f, 0.1f, 0.1f));
    }
}