using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleUIButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (SimpleAudioManager.Instance != null)
        {
            SimpleAudioManager.Instance.PlayButtonHover();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (SimpleAudioManager.Instance != null)
        {
            SimpleAudioManager.Instance.PlayButtonClick();
        }
    }
}