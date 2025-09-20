using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("StartSelect Panel")]
    public GameObject startSelectPanel;
    public Button continueButton;
    public Button newGameButton;
    public Button exitButton;

    [Header("MainMenu Buttons")]
    public GameObject mainMenuPanel;
    public Button startGameButton;
    public Button shopButton;
    public Button upgradeButton;
    public Button settingButton;
    public Button backButton;

    [Header("Shop UI")]
    public ShopUI shopUI;
    public int startCoin = 8888;

    [Header("UI References")]
    public TMP_Text titleText;
    public Image backgroundImage;

    private Color normalColor = new Color(0.12f, 0.15f, 0.22f, 0.95f); // dark blue
    private Color hoverColor = new Color(0.22f, 0.35f, 0.55f, 1f); // lighter blue
    private Color normalTextColor = new Color(0.7f, 0.9f, 1f, 1f); // light blue
    private Color hoverTextColor = new Color(1f, 1f, 1f, 1f); // white

    private Button[] mainMenuButtons;

    void Start()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startSelectPanel != null) startSelectPanel.SetActive(true);
        if (exitButton != null) SetupButton(exitButton, OnExit);
        if (backButton != null) SetupButton(backButton, OnBackToMenu);
        if (continueButton != null) SetupButton(continueButton, OnContinue);
        if (newGameButton != null) SetupButton(newGameButton, OnNewGame);
        if (startGameButton != null) SetupButton(startGameButton, OnStartGame);
        SetupButton(shopButton, OnShop);
        SetupButton(upgradeButton, OnUpgrade);
        SetupButton(settingButton, OnSettings);
        SetupTitleEffect();
        UpdateContinueButtonState();
    }
    private void OnExit()
    {
        Application.Quit();
    }

    private void OnStartButton()
    {
        StartCoroutine(FadeOutMainMenuAndShowStartSelect());
    }

    IEnumerator FadeOutMainMenuAndShowStartSelect()
    {
        float duration = 0.35f;
        float t = 0f;
        Vector3 offset = new Vector3(400, 0, 0);
        foreach (var btn in mainMenuButtons)
        {
            if (btn != null) btn.interactable = false;
        }
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = 1f - t / duration;
            float slide = Mathf.Lerp(0, offset.x, t / duration);
            foreach (var btn in mainMenuButtons)
            {
                if (btn != null)
                {
                    var cg = btn.GetComponent<CanvasGroup>();
                    if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = alpha;
                    btn.transform.localPosition = btn.transform.localPosition + new Vector3(slide, 0, 0);
                }
            }
            yield return null;
        }
        foreach (var btn in mainMenuButtons)
        {
            if (btn != null) btn.gameObject.SetActive(false);
        }
        if (startSelectPanel != null)
        {
            startSelectPanel.SetActive(true);
            yield return StartCoroutine(FadeInStartSelectPanel());
        }
        UpdateContinueButtonState();
    }

    IEnumerator FadeInStartSelectPanel()
    {
        float duration = 0.35f;
        float t = 0f;
        CanvasGroup cg = startSelectPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = startSelectPanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        Vector3 startPos = startSelectPanel.transform.localPosition + new Vector3(400, 0, 0);
        Vector3 endPos = startSelectPanel.transform.localPosition;
        startSelectPanel.transform.localPosition = startPos;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0, 1, t / duration);
            startSelectPanel.transform.localPosition = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        cg.alpha = 1f;
        startSelectPanel.transform.localPosition = endPos;
    }

    private void UpdateContinueButtonState()
    {
        bool hasSave = InventoryManager.Instance.Coin != 0 || InventoryManager.Instance.HpPotion > 0 || InventoryManager.Instance.MpPotion > 0;
        if (continueButton != null)
        {
            continueButton.interactable = hasSave;
            var cg = continueButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = continueButton.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = hasSave ? 1f : 0.5f;
        }
    }

    private void OnBackToMenu()
    {
        // Quay lại StartSelect, ẩn MainMenu
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (startSelectPanel != null) startSelectPanel.SetActive(true);
        UpdateContinueButtonState();
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

    private void OnStartGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnNewGame()
    {
        InventoryManager.Instance.ResetInventory();
        // Ẩn StartSelect, hiện MainMenu
        if (startSelectPanel != null) startSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        UpdateContinueButtonState();
    }

    private void OnContinue()
    {
        InventoryManager.Instance.LoadInventory();
        // Ẩn StartSelect, hiện MainMenu
        if (startSelectPanel != null) startSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    private void OnShop()
    {
        if (shopUI != null)
        {
            shopUI.ShowShop(startCoin);
        }
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
