using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Floating damage number effect với TextMeshPro
/// </summary>
public class FloatingDamageNumber : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 2f;
    public float fadeSpeed = 1f;
    public float lifetime = 2f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1f);

    [Header("Visual Settings")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color healColor = Color.green;
    public float startScale = 1f;
    public float endScale = 0.5f;

    [Header("Movement")]
    public Vector3 floatDirection = Vector3.up;
    public float randomSpread = 0.5f;

    private TextMeshProUGUI textComponent;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0f;
    private Color startColor;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = gameObject.AddComponent<TextMeshProUGUI>();
            // Setup default TextMeshPro settings
            textComponent.fontSize = 24;
            textComponent.alignment = TextAlignmentOptions.Center;
        }

        InitializeAnimation();
        StartCoroutine(AnimateFloatingNumber());
    }

    void InitializeAnimation()
    {
        // Store starting values
        startPosition = transform.position;
        startColor = textComponent.color;

        // Add random spread
        Vector3 randomOffset = new Vector3(
            Random.Range(-randomSpread, randomSpread),
            Random.Range(-randomSpread * 0.5f, randomSpread * 0.5f),
            0f
        );

        targetPosition = startPosition + floatDirection * floatSpeed + randomOffset;

        // Set initial scale
        transform.localScale = Vector3.one * startScale;
    }

    IEnumerator AnimateFloatingNumber()
    {
        while (elapsedTime < lifetime)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / lifetime;

            // Movement animation
            float moveProgress = movementCurve.Evaluate(progress);
            transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);

            // Scale animation
            float scaleProgress = scaleCurve.Evaluate(progress);
            float currentScale = Mathf.Lerp(startScale, endScale, scaleProgress);
            transform.localScale = Vector3.one * currentScale;

            // Fade animation
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(1f, 0f, progress);
            textComponent.color = currentColor;

            yield return null;
        }

        // Destroy when animation is complete
        Destroy(gameObject);
    }

    // Public methods to setup damage number
    public void SetDamage(int damage, bool isCritical = false)
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        textComponent.text = damage.ToString();
        textComponent.color = isCritical ? criticalDamageColor : normalDamageColor;
        startColor = textComponent.color;

        // Critical damage has bigger scale
        if (isCritical)
        {
            startScale *= 1.5f;
            textComponent.fontStyle = FontStyles.Bold;
        }
    }

    public void SetHeal(int healAmount)
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        textComponent.text = "+" + healAmount.ToString();
        textComponent.color = healColor;
        startColor = textComponent.color;
    }

    public void SetCustomText(string text, Color color)
    {
        if (textComponent == null)
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        textComponent.text = text;
        textComponent.color = color;
        startColor = textComponent.color;
    }

    // Static factory methods
    public static GameObject CreateDamageNumber(int damage, Vector3 position, bool isCritical = false)
    {
        GameObject damageNumberObj = new GameObject("FloatingDamageNumber");
        damageNumberObj.transform.position = position;

        // Add Canvas component for proper rendering
        Canvas canvas = damageNumberObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        FloatingDamageNumber damageNumber = damageNumberObj.AddComponent<FloatingDamageNumber>();
        damageNumber.SetDamage(damage, isCritical);

        return damageNumberObj;
    }

    public static GameObject CreateHealNumber(int healAmount, Vector3 position)
    {
        GameObject healNumberObj = new GameObject("FloatingHealNumber");
        healNumberObj.transform.position = position;

        // Add Canvas component for proper rendering
        Canvas canvas = healNumberObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        FloatingDamageNumber healNumber = healNumberObj.AddComponent<FloatingDamageNumber>();
        healNumber.SetHeal(healAmount);

        return healNumberObj;
    }

    public static GameObject CreateCustomNumber(string text, Color color, Vector3 position)
    {
        GameObject customNumberObj = new GameObject("FloatingCustomNumber");
        customNumberObj.transform.position = position;

        // Add Canvas component for proper rendering
        Canvas canvas = customNumberObj.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;

        FloatingDamageNumber customNumber = customNumberObj.AddComponent<FloatingDamageNumber>();
        customNumber.SetCustomText(text, color);

        return customNumberObj;
    }
}