using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    
    [Header("Audio Settings")]
    public bool playHoverSound = true;
    public bool playClickSound = true;
    
    [Header("Custom Audio Clips (Optional)")]
    public AudioClip customHoverSound;
    public AudioClip customClickSound;

    void Start()
    {
        button = GetComponent<Button>();
        
        // Add click sound to button
        if (playClickSound)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (playHoverSound && AudioManager.Instance != null)
        {
            if (customHoverSound != null)
                AudioManager.Instance.PlaySoundOneShot(customHoverSound);
            else
                AudioManager.Instance.PlayUIHover();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Optional: Could add exit sound here if needed
    }

    void PlayClickSound()
    {
        if (AudioManager.Instance != null)
        {
            if (customClickSound != null)
                AudioManager.Instance.PlaySoundOneShot(customClickSound);
            else
                AudioManager.Instance.PlayUIClick();
        }
    }
}