using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        ShowBuffSelection();
    }

    public void ShowBuffSelection()
    {
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

    void SelectBuff(Buff buff)
    {
        buff.Apply(player);
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