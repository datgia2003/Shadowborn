using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Floating damage indicator for professional visual feedback
/// Shows damage numbers with smooth animation and color coding
/// </summary>
public class DamageIndicator : MonoBehaviour
{
    [Header("Text Components")]
    public TextMeshProUGUI damageText;
    public CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    public float floatHeight = 100f;
    public float animationDuration = 1.5f;
    public AnimationCurve movementCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f, 0f, 0f));
    public AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.8f));
    public AnimationCurve alphaCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    [Header("Damage Type Colors")]
    public Color normalDamageColor = Color.white;
    public Color criticalDamageColor = Color.yellow;
    public Color healingColor = Color.green;
    public Color magicDamageColor = Color.cyan;
    public Color fireColor = Color.red;
    public Color iceColor = Color.blue;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 originalScale;
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (damageText == null)
            damageText = GetComponent<TextMeshProUGUI>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalScale = transform.localScale;
    }

    public void ShowDamage(int damage, DamageType type = DamageType.Normal, bool isCritical = false)
    {
        // Setup text
        if (isCritical)
        {
            damageText.text = $"CRIT! {damage}";
            damageText.color = criticalDamageColor;
            damageText.fontSize *= 1.3f;
        }
        else
        {
            damageText.text = damage.ToString();
            damageText.color = GetDamageColor(type);
        }

        // Setup positions
        startPosition = rectTransform.localPosition;
        targetPosition = startPosition + Vector3.up * floatHeight;

        // Start animation
        StartCoroutine(AnimateDamage());
    }

    public void ShowHealing(int healing)
    {
        damageText.text = $"+{healing}";
        damageText.color = healingColor;

        startPosition = rectTransform.localPosition;
        targetPosition = startPosition + Vector3.up * floatHeight;

        StartCoroutine(AnimateDamage());
    }

    public void ShowText(string text, Color color)
    {
        damageText.text = text;
        damageText.color = color;

        startPosition = rectTransform.localPosition;
        targetPosition = startPosition + Vector3.up * floatHeight;

        StartCoroutine(AnimateDamage());
    }

    private Color GetDamageColor(DamageType type)
    {
        return type switch
        {
            DamageType.Normal => normalDamageColor,
            DamageType.Magic => magicDamageColor,
            DamageType.Fire => fireColor,
            DamageType.Ice => iceColor,
            DamageType.Critical => criticalDamageColor,
            _ => normalDamageColor
        };
    }

    IEnumerator AnimateDamage()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationDuration;

            // Position animation
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, movementCurve.Evaluate(t));
            rectTransform.localPosition = currentPos;

            // Scale animation
            float scale = scaleCurve.Evaluate(t);
            transform.localScale = originalScale * scale;

            // Alpha animation
            if (canvasGroup != null)
            {
                canvasGroup.alpha = alphaCurve.Evaluate(t);
            }

            yield return null;
        }

        // Destroy when animation is complete
        Destroy(gameObject);
    }

    public enum DamageType
    {
        Normal,
        Magic,
        Fire,
        Ice,
        Critical
    }
}

/// <summary>
/// Static factory for creating damage indicators
/// Use this for easy damage indicator creation
/// </summary>
public static class DamageIndicatorFactory
{
    private static GameObject damageIndicatorPrefab;
    private static Transform uiParent;

    public static void Initialize(GameObject prefab, Transform parent)
    {
        damageIndicatorPrefab = prefab;
        uiParent = parent;
    }

    public static void ShowDamage(Vector3 worldPosition, int damage, DamageIndicator.DamageType type = DamageIndicator.DamageType.Normal, bool isCritical = false)
    {
        if (damageIndicatorPrefab == null || uiParent == null) return;

        // Convert world position to screen position
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // Create indicator
        GameObject indicator = Object.Instantiate(damageIndicatorPrefab, uiParent);
        indicator.transform.position = screenPosition;

        // Setup damage display
        DamageIndicator damageComponent = indicator.GetComponent<DamageIndicator>();
        if (damageComponent != null)
        {
            damageComponent.ShowDamage(damage, type, isCritical);
        }
    }

    public static void ShowHealing(Vector3 worldPosition, int healing)
    {
        if (damageIndicatorPrefab == null || uiParent == null) return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject indicator = Object.Instantiate(damageIndicatorPrefab, uiParent);
        indicator.transform.position = screenPosition;

        DamageIndicator damageComponent = indicator.GetComponent<DamageIndicator>();
        if (damageComponent != null)
        {
            damageComponent.ShowHealing(healing);
        }
    }

    public static void ShowText(Vector3 worldPosition, string text, Color color)
    {
        if (damageIndicatorPrefab == null || uiParent == null) return;

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        GameObject indicator = Object.Instantiate(damageIndicatorPrefab, uiParent);
        indicator.transform.position = screenPosition;

        DamageIndicator damageComponent = indicator.GetComponent<DamageIndicator>();
        if (damageComponent != null)
        {
            damageComponent.ShowText(text, color);
        }
    }
}
