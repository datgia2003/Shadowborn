using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Setup script để tạo Boss Health UI trong scene một cách tự động
/// Attach script này vào một GameObject trong scene để tạo complete boss health UI
/// </summary>
public class BossHealthUISetup : MonoBehaviour
{
    [Header("Setup Configuration")]
    public bool createUIOnStart = true;
    public Canvas targetCanvas; // Canvas để đặt UI, null = tìm tự động
    public Igris bossReference; // Boss reference, null = tìm tự động

    [Header("UI Styling")]
    public Vector2 healthBarSize = new Vector2(400f, 20f);
    public Vector2 uiPosition = new Vector2(0f, 250f); // Offset từ center
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    public Color healthBarBackground = new Color(0.2f, 0.2f, 0.2f, 0.8f);

    private BossHealthUI createdUI;

    void Start()
    {
        if (createUIOnStart)
        {
            CreateBossHealthUI();
        }
    }

    [ContextMenu("Create Boss Health UI")]
    public void CreateBossHealthUI()
    {
        // Find canvas if not assigned
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
            if (targetCanvas == null)
            {
                Debug.LogError("BossHealthUISetup: No Canvas found in scene!");
                return;
            }
        }

        // Find boss if not assigned
        if (bossReference == null)
        {
            bossReference = FindObjectOfType<Igris>();
            if (bossReference == null)
            {
                Debug.LogError("BossHealthUISetup: No Igris boss found in scene!");
                return;
            }
        }

        // Create main UI panel
        GameObject bossUIPanel = new GameObject("Boss Health UI");
        bossUIPanel.transform.SetParent(targetCanvas.transform, false);

        RectTransform panelRect = bossUIPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = uiPosition;
        panelRect.sizeDelta = new Vector2(healthBarSize.x + 40f, 100f);

        // Add background image
        Image panelBg = bossUIPanel.AddComponent<Image>();
        panelBg.color = backgroundColor;

        // Create boss name text
        GameObject nameObject = new GameObject("Boss Name");
        nameObject.transform.SetParent(bossUIPanel.transform, false);

        TextMeshProUGUI nameText = nameObject.AddComponent<TextMeshProUGUI>();
        nameText.text = "Shadow of Igris";
        nameText.fontSize = 18;
        nameText.color = Color.white;
        nameText.alignment = TextAlignmentOptions.Center;

        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.7f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        // Create health bar background
        GameObject healthBarBg = new GameObject("Health Bar Background");
        healthBarBg.transform.SetParent(bossUIPanel.transform, false);

        Image healthBgImage = healthBarBg.AddComponent<Image>();
        healthBgImage.color = healthBarBackground;

        RectTransform healthBgRect = healthBarBg.GetComponent<RectTransform>();
        healthBgRect.anchorMin = new Vector2(0f, 0.4f);
        healthBgRect.anchorMax = new Vector2(1f, 0.7f);
        healthBgRect.offsetMin = new Vector2(10f, 5f);
        healthBgRect.offsetMax = new Vector2(-10f, -5f);

        // Create health bar slider
        GameObject healthBarObj = new GameObject("Health Bar");
        healthBarObj.transform.SetParent(bossUIPanel.transform, false);

        Slider healthBar = healthBarObj.AddComponent<Slider>();
        healthBar.minValue = 0f;
        healthBar.maxValue = 1f;
        healthBar.value = 1f;

        RectTransform sliderRect = healthBarObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.4f);
        sliderRect.anchorMax = new Vector2(1f, 0.7f);
        sliderRect.offsetMin = new Vector2(10f, 5f);
        sliderRect.offsetMax = new Vector2(-10f, -5f);

        // Create fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(healthBarObj.transform, false);

        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        fillImage.type = Image.Type.Filled;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // Assign fill to slider
        healthBar.fillRect = fillRect;
        healthBar.targetGraphic = fillImage;

        // Create health text
        GameObject healthTextObj = new GameObject("Health Text");
        healthTextObj.transform.SetParent(bossUIPanel.transform, false);

        TextMeshProUGUI healthText = healthTextObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "300 / 300";
        healthText.fontSize = 14;
        healthText.color = Color.white;
        healthText.alignment = TextAlignmentOptions.Center;

        RectTransform healthTextRect = healthTextObj.GetComponent<RectTransform>();
        healthTextRect.anchorMin = new Vector2(0f, 0.1f);
        healthTextRect.anchorMax = new Vector2(1f, 0.4f);
        healthTextRect.offsetMin = Vector2.zero;
        healthTextRect.offsetMax = Vector2.zero;

        // Create enraged indicator
        GameObject enragedIndicator = new GameObject("Enraged Indicator");
        enragedIndicator.transform.SetParent(bossUIPanel.transform, false);
        enragedIndicator.SetActive(false); // Hidden by default

        TextMeshProUGUI enragedText = enragedIndicator.AddComponent<TextMeshProUGUI>();
        enragedText.text = "ENRAGED!";
        enragedText.fontSize = 16;
        enragedText.color = Color.red;
        enragedText.alignment = TextAlignmentOptions.Center;
        enragedText.fontStyle = FontStyles.Bold;

        RectTransform enragedRect = enragedIndicator.GetComponent<RectTransform>();
        enragedRect.anchorMin = new Vector2(0f, 0f);
        enragedRect.anchorMax = new Vector2(1f, 0.3f);
        enragedRect.offsetMin = Vector2.zero;
        enragedRect.offsetMax = Vector2.zero;

        // Add BossHealthUI component
        BossHealthUI healthUIComponent = bossUIPanel.AddComponent<BossHealthUI>();

        // Assign references
        healthUIComponent.healthBar = healthBar;
        healthUIComponent.healthFill = fillImage;
        healthUIComponent.bossNameText = nameText;
        healthUIComponent.healthText = healthText;
        healthUIComponent.bossUIPanel = bossUIPanel;
        healthUIComponent.enragedIndicator = enragedIndicator;
        healthUIComponent.bossScript = bossReference;

        createdUI = healthUIComponent;

        // UI will be shown by ShowBossUI() call from boss script
        bossUIPanel.SetActive(false); // Hidden by default

        Debug.Log("Boss Health UI created successfully!");
    }

    [ContextMenu("Remove Boss Health UI")]
    public void RemoveBossHealthUI()
    {
        if (createdUI != null && createdUI.gameObject != null)
        {
            if (Application.isPlaying)
            {
                Destroy(createdUI.gameObject);
            }
            else
            {
                DestroyImmediate(createdUI.gameObject);
            }
            createdUI = null;
            Debug.Log("Boss Health UI removed.");
        }
    }

    // Public getters
    public BossHealthUI GetCreatedUI()
    {
        return createdUI;
    }

    public bool HasCreatedUI()
    {
        return createdUI != null;
    }
}