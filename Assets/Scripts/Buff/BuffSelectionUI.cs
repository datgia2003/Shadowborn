using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuffSelectionUI : MonoBehaviour
{
    public GameObject panel;
    public BuffManager buffManager;
    public PlayerController player;
    public Button[] buffButtons; // 3 buttons for buffs
    public Button statPointButton; // Button for stat point conversion
    private Buff[] currentBuffs;

    public void ShowBuffSelection()
    {
        panel.SetActive(true);
        currentBuffs = buffManager.GetRandomBuffs(3);
        for (int i = 0; i < buffButtons.Length; i++)
        {
            buffButtons[i].gameObject.SetActive(true);
            buffButtons[i].GetComponentInChildren<Text>().text = currentBuffs[i].buffName;
            int idx = i;
            buffButtons[i].onClick.RemoveAllListeners();
            buffButtons[i].onClick.AddListener(() => SelectBuff(idx));
        }
        statPointButton.gameObject.SetActive(true);
        // statPointButton.GetComponentInChildren<Text>().text = $"Quy đổi thành {buffManager.statPointBuff.statPoints} điểm cộng";
        statPointButton.onClick.RemoveAllListeners();
        statPointButton.onClick.AddListener(SelectStatPoint);
    }

    public void SelectBuff(int idx)
    {
        currentBuffs[idx].Apply(player);
        HidePanel();
    }

    public void SelectStatPoint()
    {
        buffManager.statPointBuff.Apply(null);
        HidePanel();
    }

    private void HidePanel()
    {
        panel.SetActive(false);
    }
}
