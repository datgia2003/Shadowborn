using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BuffSelectionUI : MonoBehaviour
{
    private Color normalColor = new Color(1f, 1f, 1f, 1f);
    private Color hoverColor = new Color(1f, 0.85f, 0.3f, 1f); // vàng sáng
    public BuffManager buffManager;
    public PlayerController player;


    public GameObject panel;
    public GameObject[] buffButtonObjs = new GameObject[3];
    public Button[] buffButtons = new Button[3];
    public Image[] buffIcons = new Image[3];
    public TMP_Text[] buffNames = new TMP_Text[3];
    public TMP_Text[] buffDescs = new TMP_Text[3];
    public Button statPointButton;
    public TMP_Text statPointButtonText;

    void Start()
    {
        panel.SetActive(false);
        // Không tự động gọi ShowBuffSelection ở Start nữa
    }

    public void ShowBuffSelection()
    {
        Debug.Log("[BuffSelectionUI] ShowBuffSelection called");
        if (panel == null)
        {
            Debug.LogError("[BuffSelectionUI] panel is not assigned!");
            return;
        }
        panel.SetActive(true);
        var buffs = buffManager.GetRandomBuffs(3);
        Debug.Log($"[BuffSelectionUI] ShowBuffSelection: buffs count = {buffs.Length}");
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"[BuffSelectionUI] Buff {i}: {(buffs[i] != null ? buffs[i].buffName : "null")}");
            if (buffButtonObjs[i] != null) buffButtonObjs[i].SetActive(true);
            if (buffIcons[i] != null)
            {
                buffIcons[i].enabled = true;
                buffIcons[i].sprite = buffs[i].buffIcon;
                Debug.Log($"[BuffSelectionUI] Icon {i}: {(buffs[i].buffIcon != null ? buffs[i].buffIcon.name : "null")}");
            }
            if (buffNames[i] != null)
            {
                buffNames[i].enabled = true;
                buffNames[i].text = "";
                buffNames[i].text = buffs[i].buffName ?? "Buff";
                Debug.Log($"[BuffSelectionUI] Name {i}: {buffNames[i].text}");
            }
            if (buffDescs[i] != null)
            {
                buffDescs[i].enabled = true;
                buffDescs[i].text = "";
                buffDescs[i].text = buffs[i].buffDescription ?? "";
                Debug.Log($"[BuffSelectionUI] Desc {i}: {buffDescs[i].text}");
            }
            int idx = i;
            if (buffButtons[i] != null)
            {
                buffButtons[i].onClick.RemoveAllListeners();
                buffButtons[i].onClick.AddListener(() => SelectBuff(buffs[idx]));
                // Add hover effect
                AddBuffButtonHoverEvents(buffButtons[i], idx);
            }
        }

        // Hiệu ứng lật lần lượt từ trái sang phải
        StartCoroutine(FlipBuffsCoroutine(buffs));

        int pointAmount = 0;
        if (buffManager.statPointBuff != null)
        {
            if (buffManager.statPointBuff.GetType().GetMethod("GetPointAmount") != null)
                pointAmount = (int)buffManager.statPointBuff.GetType().GetMethod("GetPointAmount").Invoke(buffManager.statPointBuff, null);
            else if (buffManager.statPointBuff.GetType().GetField("statPoints") != null)
                pointAmount = (int)buffManager.statPointBuff.GetType().GetField("statPoints").GetValue(buffManager.statPointBuff);
        }
        statPointButtonText.text = $"BỎ QUA (+{pointAmount} điểm cộng)";
        statPointButton.onClick.RemoveAllListeners();
        statPointButton.onClick.AddListener(SelectStatPoint);
    }

    // Hiệu ứng lật và xử lý mờ/disable cho buff đã chọn
    private IEnumerator FlipBuffsCoroutine(Buff[] buffs)
    {
        float fadeDuration = 0.35f;
        float delayBetween = 0.13f;
        for (int i = 0; i < buffs.Length; i++)
        {
            var obj = buffButtonObjs[i];
            if (obj != null)
            {
                // Bắt đầu ở vị trí thấp hơn và alpha = 0
                Vector3 startPos = obj.transform.localPosition;
                Vector3 fromPos = startPos + new Vector3(0f, -60f, 0f);
                obj.transform.localPosition = fromPos;
                CanvasGroup cg = obj.GetComponent<CanvasGroup>();
                if (cg == null) cg = obj.AddComponent<CanvasGroup>();
                cg.alpha = 0f;

                // Nếu buff đã chọn rồi thì mờ và disable
                bool isChosen = buffs[i] != null && buffManager.IsBuffChosen(buffs[i]);
                if (isChosen)
                {
                    if (buffIcons[i] != null)
                    {
                        var c = buffIcons[i].color;
                        c.a = 0.4f;
                        buffIcons[i].color = c;
                    }
                    if (buffButtons[i] != null)
                    {
                        buffButtons[i].interactable = false;
                    }
                }
                else
                {
                    if (buffIcons[i] != null)
                    {
                        var c = buffIcons[i].color;
                        c.a = 1f;
                        buffIcons[i].color = c;
                    }
                    if (buffButtons[i] != null)
                    {
                        buffButtons[i].interactable = true;
                    }
                }

                // Hiệu ứng fade-in từ dưới lên
                float t = 0f;
                while (t < fadeDuration)
                {
                    float progress = t / fadeDuration;
                    obj.transform.localPosition = Vector3.Lerp(fromPos, startPos, progress);
                    cg.alpha = Mathf.Lerp(0f, 1f, progress);
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                obj.transform.localPosition = startPos;
                cg.alpha = 1f;
                yield return new WaitForSecondsRealtime(delayBetween);
            }
        }
    }

    void SelectBuff(Buff buff)
    {
        if (buff != null)
        {
            buff.Apply(player);
            // Thêm buff vào danh sách đã chọn
            if (buffManager != null && !buffManager.chosenBuffs.Contains(buff))
            {
                buffManager.chosenBuffs.Add(buff);
            }
        }
        HidePanel();
    }

    void SelectStatPoint()
    {
        buffManager.statPointBuff.Apply(null);
        HidePanel();
    }

    void HidePanel()
    {
        panel.SetActive(false);
    }

    private void AddBuffButtonHoverEvents(Button btn, int idx)
    {
        var trigger = btn.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
        {
            trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
        }
        trigger.triggers.Clear();

        // Pointer Enter
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { SetBuffButtonHover(idx, true); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { SetBuffButtonHover(idx, false); });
        trigger.triggers.Add(entryExit);
    }

    private void SetBuffButtonHover(int idx, bool isHover)
    {
        if (buffButtonObjs[idx] != null)
        {
            buffButtonObjs[idx].transform.localScale = isHover ? Vector3.one * 1.08f : Vector3.one;
        }
        if (buffIcons[idx] != null)
        {
            buffIcons[idx].color = isHover ? hoverColor : normalColor;
        }
    }
}