using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UpgradeNodeButton : MonoBehaviour
{
    public System.Action<RectTransform> onHover;
    public System.Action onExit;
    public Button button;
    private Vector3 targetScale = Vector3.one;
    private float scaleSpeed = 8f;
    public TMP_Text labelText;
    public Image iconImage;
    public Image glowImage; // assign a child Image for glow effect
    private UpgradeNode node;
    private UpgradeUI ui;
    private bool canActivate;
    private float glowTimer;

    public void Setup(UpgradeNode node, UpgradeUI ui)
    {
        this.node = node;
        this.ui = ui;
        labelText.text = node.name;
        canActivate = !node.unlocked && (string.IsNullOrEmpty(node.prerequisiteId) || UpgradeManager.Instance.nodes.Find(n => n.id == node.prerequisiteId)?.unlocked == true);
        button.interactable = canActivate;
        // Visual states
        if (node.unlocked)
        {
            iconImage.color = Color.yellow;
            if (glowImage != null) glowImage.enabled = false;
        }
        else if (canActivate)
        {
            iconImage.color = Color.white;
            if (glowImage != null) glowImage.enabled = true;
        }
        else
        {
            iconImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // dark
            if (glowImage != null) glowImage.enabled = false;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => ui.TryUpgrade(node));
        var trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.Clear();
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) =>
        {
            if (onHover != null) onHover(GetComponent<RectTransform>());
            targetScale = Vector3.one * 1.12f;
        });
        trigger.triggers.Add(entryEnter);
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) =>
        {
            if (onExit != null) onExit();
            targetScale = Vector3.one;
        });
        trigger.triggers.Add(entryExit);
    }

    void Update()
    {
        // Glow effect for activatable nodes
        if (glowImage != null && canActivate && !node.unlocked)
        {
            glowTimer += Time.deltaTime * 2f;
            float glow = 0.5f + 0.5f * Mathf.Sin(glowTimer);
            glowImage.color = new Color(1f, 1f, 0.3f, glow * 0.5f + 0.3f); // soft yellow glow
        }
        // Smooth scale for hover effect
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
    }

}
