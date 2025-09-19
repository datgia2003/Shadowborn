using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MainMenuButtonSpawner : MonoBehaviour
{
    [Header("Button Settings")]
    public string[] buttonLabels = { "Play", "Shop", "Upgrade", "Settings" };
    public Color normalColor = new Color(0.12f, 0.15f, 0.22f, 0.95f); // dark blue
    public Color hoverColor = new Color(0.22f, 0.35f, 0.55f, 1f); // lighter blue
    public Color normalTextColor = new Color(0.7f, 0.9f, 1f, 1f); // light blue
    public Color hoverTextColor = new Color(1f, 1f, 1f, 1f); // white
    public Vector2 buttonSize = new Vector2(420, 80);
    public float buttonSpacing = 32f;
    public Transform buttonParent; // Assign a vertical layout panel or empty object in canvas

    public Font tmpFont;

    void Start()
    {
        SpawnButtons();
    }

    void SpawnButtons()
    {
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            var btnGO = new GameObject($"MainMenuButton_{buttonLabels[i]}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGO.transform.SetParent(buttonParent, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = buttonSize;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, -i * (buttonSize.y + buttonSpacing));

            var img = btnGO.GetComponent<Image>();
            img.color = normalColor;
            img.raycastTarget = true;
            img.type = Image.Type.Sliced;

            var btn = btnGO.GetComponent<Button>();
            btn.transition = Selectable.Transition.None;

            // TMP Text
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMP_Text));
            txtGO.transform.SetParent(btnGO.transform, false);
            var txtRect = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            var tmp = txtGO.GetComponent<TMP_Text>();
            tmp.text = buttonLabels[i];
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 48;
            tmp.color = normalTextColor;
            if (tmpFont != null) tmp.font = TMP_FontAsset.CreateFontAsset(tmpFont);
            tmp.enableWordWrapping = false;
            tmp.outlineColor = new Color(0.3f, 0.7f, 1f, 1f);
            tmp.outlineWidth = 0.15f;

            AddHoverEffect(btn, img, tmp);

            // Add button logic
            int idx = i;
            btn.onClick.AddListener(() => OnButtonClicked(idx));
        }
    }

    void AddHoverEffect(Button btn, Image img, TMP_Text txt)
    {
        var trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) =>
        {
            img.color = hoverColor;
            txt.color = hoverTextColor;
            btn.transform.localScale = Vector3.one * 1.08f;
        });
        trigger.triggers.Add(entryEnter);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) =>
        {
            img.color = normalColor;
            txt.color = normalTextColor;
            btn.transform.localScale = Vector3.one;
        });
        trigger.triggers.Add(entryExit);
    }

    void OnButtonClicked(int idx)
    {
        switch (buttonLabels[idx])
        {
            case "Play":
                // SceneManager.LoadScene("GamePlay");
                break;
            case "Shop":
                // Open shop UI
                break;
            case "Upgrade":
                // Open upgrade UI
                break;
            case "Settings":
                // Open settings UI
                break;
        }
    }
}
