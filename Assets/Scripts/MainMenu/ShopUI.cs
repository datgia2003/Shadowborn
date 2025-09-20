using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ShopUI : MonoBehaviour
{
    [Header("Warning Text")]
    public TMP_Text hpWarningText;
    public TMP_Text mpWarningText;

    [Header("References")]
    public GameObject shopPanel;
    public CanvasGroup shopCanvasGroup;
    public Button buyHpButton;
    public Button buyMpButton;
    public Button closeButton;
    public TMP_Text coinText;
    public TMP_Text hpCountText;
    public TMP_Text mpCountText;
    public TMP_Text hpPriceText;
    public TMP_Text mpPriceText;
    public TMP_Text hpDescriptionText;
    public TMP_Text mpDescriptionText;
    public Image hpIcon;
    public Image mpIcon;
    public Image hpBorderOut;
    public Image mpBorderOut;
    public Image hpButtonBorder;
    public Image mpButtonBorder;
    public Image closeButtonBorder;

    [Header("Config")]
    public int hpPotionPrice = 100;
    public int mpPotionPrice = 80;
    public int hpRestoreAmount = 50;
    public int mpRestoreAmount = 50;

    private int coin = 0;
    private int hpAmount = 0;
    private int mpAmount = 0;

    private Coroutine fadeCoroutine;
    private Coroutine hpGlowCoroutine;
    private Coroutine mpGlowCoroutine;

    void Start()
    {
        if (hpWarningText != null) hpWarningText.gameObject.SetActive(false);
        if (mpWarningText != null) mpWarningText.gameObject.SetActive(false);
        shopPanel.SetActive(false);
        shopCanvasGroup.alpha = 0f;
        buyHpButton.onClick.AddListener(BuyHpPotion);
        buyMpButton.onClick.AddListener(BuyMpPotion);
        closeButton.onClick.AddListener(HideShop);
        hpPriceText.text = $"{hpPotionPrice}";
        mpPriceText.text = $"{mpPotionPrice}";
        hpDescriptionText.text = $"Hồi {hpRestoreAmount} HP/ 1 lần sử dụng";
        mpDescriptionText.text = $"Hồi {mpRestoreAmount} MP/ 1 lần sử dụng";
        SetupButtonEffect(buyHpButton, hpButtonBorder);
        SetupButtonEffect(buyMpButton, mpButtonBorder);
        SetupButtonEffect(closeButton, closeButtonBorder);
        StartHpGlow();
        StartMpGlow();
        UpdateUI();
    }

    public void ShowShop(int currentCoin)
    {
        InventoryManager.Instance.LoadInventory();
        coin = InventoryManager.Instance.Coin;
        shopPanel.SetActive(true);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeShop(true));
        UpdateUI();
    }

    public void HideShop()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeShop(false));
    }

    IEnumerator FadeShop(bool fadeIn)
    {
        float duration = 0.22f;
        float t = 0f;
        float startAlpha = shopCanvasGroup.alpha;
        float endAlpha = fadeIn ? 1f : 0f;
        Vector3 startScale = shopPanel.transform.localScale;
        Vector3 endScale = fadeIn ? Vector3.one : Vector3.one * 0.8f;
        if (fadeIn) shopPanel.transform.localScale = Vector3.one * 0.8f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            shopCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t / duration);
            shopPanel.transform.localScale = Vector3.Lerp(startScale, endScale, t / duration);
            yield return null;
        }
        shopCanvasGroup.alpha = endAlpha;
        shopPanel.transform.localScale = endScale;
        if (!fadeIn) shopPanel.SetActive(false);
    }

    void BuyHpPotion()
    {
        if (InventoryManager.Instance.SpendCoin(hpPotionPrice))
        {
            InventoryManager.Instance.AddHpPotion(1);
            coin = InventoryManager.Instance.Coin;
            hpAmount = InventoryManager.Instance.HpPotion;
            UpdateUI();
        }
        else
        {
            if (hpWarningText != null) StartCoroutine(ShowWarning(hpWarningText));
        }
    }

    void BuyMpPotion()
    {
        if (InventoryManager.Instance.SpendCoin(mpPotionPrice))
        {
            InventoryManager.Instance.AddMpPotion(1);
            coin = InventoryManager.Instance.Coin;
            mpAmount = InventoryManager.Instance.MpPotion;
            UpdateUI();
        }
        else
        {
            if (mpWarningText != null) StartCoroutine(ShowWarning(mpWarningText));
        }
    }

    void UpdateUI()
    {
        coinText.text = $"{InventoryManager.Instance.Coin}";
        hpCountText.text = $"x{InventoryManager.Instance.HpPotion}";
        mpCountText.text = $"x{InventoryManager.Instance.MpPotion}";
    }

    IEnumerator ShowWarning(TMP_Text warningText)
    {
        warningText.text = "Không đủ vàng!";
        warningText.gameObject.SetActive(true);
        float duration = 1.1f;
        float t = 0f;
        Color startColor = new Color(1f, 0.7f, 0.2f, 0f);
        Color endColor = new Color(1f, 0.7f, 0.2f, 1f);
        Vector3 startPos = warningText.rectTransform.localPosition + new Vector3(0, -20, 0);
        Vector3 endPos = warningText.rectTransform.localPosition + new Vector3(0, 20, 0);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Sin(Mathf.PI * t / duration);
            warningText.color = Color.Lerp(startColor, endColor, alpha);
            warningText.rectTransform.localPosition = Vector3.Lerp(startPos, endPos, t / duration);
            yield return null;
        }
        warningText.gameObject.SetActive(false);
        warningText.rectTransform.localPosition = startPos - new Vector3(0, -20, 0); // reset position
    }

    void SetupButtonEffect(Button btn, Image border)
    {
        btn.onClick.AddListener(() => StartCoroutine(ButtonPressEffect(btn)));
        var trigger = btn.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        trigger.triggers.Clear();
        // Hover
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => OnButtonHover(btn, border, true));
        trigger.triggers.Add(entryEnter);
        // Exit
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => OnButtonHover(btn, border, false));
        trigger.triggers.Add(entryExit);
    }

    void OnButtonHover(Button btn, Image border, bool isHover)
    {
        var txt = btn.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.color = isHover ? new Color(1f, 0.95f, 0.7f) : Color.white;
            txt.outlineColor = isHover ? new Color(0.3f, 0.7f, 1f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            txt.outlineWidth = isHover ? 0.22f : 0.12f;
        }
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isHover ? new Color(0.22f, 0.35f, 0.55f, 1f) : new Color(0.12f, 0.15f, 0.22f, 0.95f);
        btn.transform.localScale = isHover ? Vector3.one * 1.08f : Vector3.one;
        if (border != null)
            border.color = isHover ? new Color(0.3f, 0.7f, 1f, 0.7f) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
    }

    IEnumerator ButtonPressEffect(Button btn)
    {
        Vector3 start = btn.transform.localScale;
        Vector3 target = Vector3.one * 0.95f;
        float t = 0f;
        float duration = 0.08f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            btn.transform.localScale = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            btn.transform.localScale = Vector3.Lerp(target, Vector3.one, t / duration);
            yield return null;
        }
        btn.transform.localScale = Vector3.one;
    }

    void StartHpGlow()
    {
        if (hpGlowCoroutine != null) StopCoroutine(hpGlowCoroutine);
        hpGlowCoroutine = StartCoroutine(BorderGlowLoop(hpBorderOut));
    }
    void StartMpGlow()
    {
        if (mpGlowCoroutine != null) StopCoroutine(mpGlowCoroutine);
        mpGlowCoroutine = StartCoroutine(BorderGlowLoop(mpBorderOut));
    }
    IEnumerator BorderGlowLoop(Image border)
    {
        float t = 0f;
        float duration = 1.2f;
        Color baseColor = new Color(0.3f, 0.7f, 1f, 0.7f);
        Color glowColor = new Color(1f, 0.95f, 0.7f, 1f);
        while (true)
        {
            t += Time.unscaledDeltaTime;
            float lerp = Mathf.PingPong(t, duration) / duration;
            border.color = Color.Lerp(baseColor, glowColor, lerp);
            yield return null;
        }
    }
}