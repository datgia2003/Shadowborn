using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class DieUIManager : MonoBehaviour
{
    [Header("Die UI References")]
    public GameObject dieUI;
    public TMPro.TMP_Text finishText;
    public TMPro.TMP_Text clearedCountText;
    public TMPro.TMP_Text coinCountText;
    public Button tryAgainButton;
    public Button mainMenuButton;
    public CanvasGroup dieUICanvasGroup;

    public void ShowDieUI(int coinAmount, int stageCount)
    {
        if (dieUI == null) return;
        // Cập nhật dữ liệu
        if (finishText != null) finishText.text = "KẾT THÚC";
        if (clearedCountText != null) clearedCountText.text = stageCount.ToString();
        if (coinCountText != null) coinCountText.text = coinAmount.ToString();

        dieUI.SetActive(true);
        if (dieUICanvasGroup != null)
        {
            dieUICanvasGroup.alpha = 0f;
            dieUICanvasGroup.interactable = true;
            dieUICanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeInDieUIAndPause());
        }
        else
        {
            // Nếu không có CanvasGroup thì hiện luôn
            dieUI.SetActive(true);
            Time.timeScale = 0f;
        }
        SetupButtonEffects();
    }

    private System.Collections.IEnumerator FadeInDieUIAndPause()
    {
        float duration = 0.4f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            dieUICanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        dieUICanvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(0.5f); // Thêm delay cho anim death
        Time.timeScale = 0f;
    }

    private void SetupButtonEffects()
    {
        SetupHoverEffect(tryAgainButton);
        SetupHoverEffect(mainMenuButton);
        if (tryAgainButton != null) tryAgainButton.onClick.AddListener(TryAgain);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void SetupHoverEffect(Button btn)
    {
        if (btn == null) return;
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        // Pointer Enter
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHover(btn, true); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnButtonHover(btn, false); });
        trigger.triggers.Add(entryExit);
    }

    private void OnButtonHover(Button btn, bool isHover)
    {
        var txt = btn.GetComponentInChildren<TMPro.TMP_Text>();
        if (txt != null)
            txt.color = isHover ? new Color(1f, 0.85f, 0.3f) : new Color(0.85f, 0.75f, 0.45f); // vàng sáng
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isHover ? new Color(1f, 0.85f, 0.3f, 1f) : new Color(0.85f, 0.75f, 0.45f, 1f); // vàng sáng
        btn.transform.localScale = isHover ? Vector3.one * 1.08f : Vector3.one;
    }

    private void TryAgain()
    {
        Time.timeScale = 1f;

        // Explicitly destroy PlayerStats singleton
        if (PlayerStats.Instance != null)
        {
            Destroy(PlayerStats.Instance.gameObject);
        }

        // Explicitly destroy RoomManager singleton
        if (RoomManager.Instance != null)
        {
            Destroy(RoomManager.Instance.gameObject);
        }

        // Destroy objects in DontDestroyOnLoad before reload (legacy, fallback)
        DestroyDontDestroyOnLoadObjects();

        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); // Reload current gameplay scene
    }

    public void DestroyDontDestroyOnLoadObjects()
    {
        // Use recommended API to loop all loaded scenes
        int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;
            var roots = scene.GetRootGameObjects();
            foreach (var go in roots)
            {
                if (go.name == "DontDestroyOnLoad")
                {
                    foreach (Transform child in go.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
    }

    // Removed unused coroutine

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        // Destroy PlayerStats singleton
        if (PlayerStats.Instance != null)
            Destroy(PlayerStats.Instance.gameObject);

        // Destroy RoomManager singleton
        if (RoomManager.Instance != null)
            Destroy(RoomManager.Instance.gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }

    // ...existing code...
    // ...existing code...

    // ...existing code...
    // ...existing code...
}
