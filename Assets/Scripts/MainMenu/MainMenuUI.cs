using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button shopButton;
    public Button upgradeButton;
    public Button settingsButton;
    public TMP_Text titleText;
    public Image backgroundImage;

    private Color normalColor = new Color(0.12f, 0.15f, 0.22f, 0.95f); // dark blue
    private Color hoverColor = new Color(0.22f, 0.35f, 0.55f, 1f); // lighter blue
    private Color normalTextColor = new Color(0.7f, 0.9f, 1f, 1f); // light blue
    private Color hoverTextColor = new Color(1f, 1f, 1f, 1f); // white

    void Start()
    {
        SetupButton(playButton, OnPlay);
        SetupButton(shopButton, OnShop);
        SetupButton(upgradeButton, OnUpgrade);
        SetupButton(settingsButton, OnSettings);
        SetupTitleEffect();
    }

    private void SetupButton(Button btn, UnityEngine.Events.UnityAction onClick)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClick);
        AddHoverEffect(btn);
        var txt = btn.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.color = normalTextColor;
        }
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            img.color = normalColor;
        }
    }

    private void AddHoverEffect(Button btn)
    {
        var trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => SetButtonHover(btn, true));
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => SetButtonHover(btn, false));
        trigger.triggers.Add(entryExit);
    }

    private void SetButtonHover(Button btn, bool isHover)
    {
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isHover ? hoverColor : normalColor;
        var txt = btn.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.color = isHover ? hoverTextColor : normalTextColor;
        btn.transform.localScale = isHover ? Vector3.one * 1.08f : Vector3.one;
    }

    private void SetupTitleEffect()
    {
        if (titleText != null)
        {
            // Outline + Glow (set in inspector for best result)
            titleText.fontSize = 120;
            titleText.outlineColor = new Color(0.3f, 0.7f, 1f, 1f);
            titleText.outlineWidth = 0.25f;
        }
    }

    private void OnPlay()
    {
        SceneManager.LoadScene("SampleScene"); // Đổi tên scene nếu cần
    }
    private void OnShop()
    {
        // Mở shop UI hoặc chuyển scene
    }
    private void OnUpgrade()
    {
        // Mở upgrade UI hoặc chuyển scene
    }
    private void OnSettings()
    {
        // Mở settings UI hoặc chuyển scene
    }
}
