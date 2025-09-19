using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;


public class PauseUIManager : MonoBehaviour
{
    [SerializeField] private InputActionReference pauseActionReference;

    [Header("Pause UI References")]
    public GameObject pauseUI; // Gốc PauseUI Canvas
    public Button resumeButton;
    public Button buffButton;
    public Button mainMenuButton;
    public Button quitGameButton;
    public Image blurImage; // Image dùng để làm mờ background

    private bool isPaused = false;
    // Đã bỏ PlayerInput, chỉ dùng InputActionReference

    void Awake()
    {
        Debug.Log("[PauseUIManager] Awake");
        // Tắt UI khi khởi động
        if (pauseUI != null)
            pauseUI.SetActive(false);
        // Nếu đã gán blurImage sẵn thì không tạo lại
        if (blurImage == null && pauseUI != null)
        {
            GameObject blurObj = new GameObject("BlurImage");
            blurObj.transform.SetParent(pauseUI.transform.parent, false);
            blurImage = blurObj.AddComponent<Image>();
            blurImage.color = new Color(0, 0, 0, 0.55f); // màu đen mờ
            var rect = blurObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            blurObj.transform.SetSiblingIndex(0); // Đảm bảo nằm dưới PauseUI
            Debug.Log("[PauseUIManager] BlurImage created");
        }
        // Luôn tắt blurImage khi khởi động
        if (blurImage != null)
            blurImage.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Debug.Log("[PauseUIManager] OnEnable");
        if (pauseActionReference == null)
        {
            Debug.LogError("[PauseUIManager] pauseActionReference is NULL!");
        }
        else if (pauseActionReference.action == null)
        {
            Debug.LogError("[PauseUIManager] pauseActionReference.action is NULL!");
        }
        else
        {
            Debug.Log($"[PauseUIManager] pauseActionReference.action: {pauseActionReference.action.name}, enabled={pauseActionReference.action.enabled}, bindings={pauseActionReference.action.bindings.Count}");
            pauseActionReference.action.Enable();
            pauseActionReference.action.performed += OnPauseGame;
            Debug.Log("[PauseUIManager] Registered PauseGame action from InputActionReference");
        }
        // Gán sự kiện cho các nút
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (buffButton != null) buffButton.onClick.AddListener(ShowBuffs);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        if (quitGameButton != null) quitGameButton.onClick.AddListener(QuitGame);
        // Hover effect
        if (resumeButton != null) AddHoverEffect(resumeButton);
        if (buffButton != null) AddHoverEffect(buffButton);
        if (mainMenuButton != null) AddHoverEffect(mainMenuButton);
        if (quitGameButton != null) AddHoverEffect(quitGameButton);
    }

    void OnDisable()
    {
        if (pauseActionReference != null && pauseActionReference.action != null)
            pauseActionReference.action.performed -= OnPauseGame;
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (buffButton != null) buffButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        if (quitGameButton != null) quitGameButton.onClick.RemoveAllListeners();
    }

    private void OnPauseGame(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[PauseUIManager] OnPauseGame called. isPaused={isPaused}, phase={ctx.phase}, control={ctx.control?.name}, value={ctx.ReadValueAsObject()}, pauseUI.activeSelf={(pauseUI != null ? pauseUI.activeSelf.ToString() : "null")}");
        // Nếu UI đang tắt thì bật lên và pause game
        if (pauseUI != null && !pauseUI.activeSelf)
        {
            Debug.Log("[PauseUIManager] PauseUI is disabled, enabling and pausing game");
            PauseGame();
            return;
        }
        // Nếu UI đang bật thì resume game
        if (isPaused)
        {
            Debug.Log("[PauseUIManager] Resuming game");
            ResumeGame();
        }
        else
        {
            Debug.Log("[PauseUIManager] Pausing game");
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("[PauseUIManager] PauseGame()");
        isPaused = true;
        if (pauseUI != null)
            pauseUI.SetActive(true);
        if (blurImage != null)
            blurImage.gameObject.SetActive(true);
        // Dừng thời gian khi pause game
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Đảm bảo EventSystem tồn tại và bật
        var eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogWarning("[PauseUIManager] EventSystem missing! Creating new EventSystem...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[PauseUIManager] EventSystem created!");
        }
        else if (!eventSystem.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[PauseUIManager] EventSystem exists but is disabled! Enabling...");
            eventSystem.gameObject.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        Debug.Log("[PauseUIManager] ResumeGame()");
        isPaused = false;
        if (pauseUI != null)
            pauseUI.SetActive(false);
        if (blurImage != null)
            blurImage.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    void ShowBuffs()
    {
        Debug.Log("Show Buffs");
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void AddHoverEffect(Button btn)
    {
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

    void OnButtonHover(Button btn, bool isHover)
    {
        var txt = btn.GetComponentInChildren<TMP_Text>();
        if (txt != null)
            txt.color = isHover ? new Color(1f, 0.95f, 0.7f) : Color.white; // vàng sáng
        var img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isHover ? new Color(0.7f, 0.3f, 0.7f, 1f) : new Color(0.2f, 0.1f, 0.2f, 0.9f); // tím đậm sáng
        btn.transform.localScale = isHover ? Vector3.one * 1.08f : Vector3.one;
    }
}
