using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffSelectionUI : MonoBehaviour
{
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
}