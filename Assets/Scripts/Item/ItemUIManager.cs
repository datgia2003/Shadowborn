using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    [Header("Coin UI")]
    public Image coinIcon;
    public TMP_Text coinText;

    [Header("Potion UI")]
    public Image hpPotionIcon;
    public TMP_Text hpPotionText;
    public Image mpPotionIcon;
    public TMP_Text mpPotionText;

    // Call this to update UI
    public void UpdateItemUI(int coin, int hpPotion, int mpPotion)
    {
        coinText.text = coin.ToString();
        hpPotionText.text = hpPotion.ToString();
        mpPotionText.text = mpPotion.ToString();
    }

    // public void UpdateCoinCount(int count)
    // {
    //     coinText.text = count.ToString();
    // }

    // public void UpdateHealthPotionCount(int count)
    // {
    //     hpPotionText.text = count.ToString();
    // }

    // public void UpdateManaPotionCount(int count)
    // {
    //     mpPotionText.text = count.ToString();
    // }

    // // Optional: Set icons if needed
    // public void SetIcons(Sprite coinSprite, Sprite hpSprite, Sprite mpSprite)
    // {
    //     coinIcon.sprite = coinSprite;
    //     hpPotionIcon.sprite = hpSprite;
    //     mpPotionIcon.sprite = mpSprite;
    // }

}