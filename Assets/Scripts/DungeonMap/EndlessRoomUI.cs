using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Manager cho Endless Room system - hiển thị thống kê và thông tin
/// </summary>
public class EndlessRoomUI : MonoBehaviour
{
    [Header("📱 UI References")]
    [Tooltip("Text hiển thị số room đã clear")]
    [SerializeField] private TextMeshProUGUI roomsClearedText;

    [Tooltip("Text hiển thị difficulty level")]
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Tooltip("Text hiển thị số room active")]
    [SerializeField] private TextMeshProUGUI activeRoomsText;

    [Tooltip("Text hiển thị current room type")]
    [SerializeField] private TextMeshProUGUI roomTypeText;

    [Tooltip("Text hiển thị boss countdown")]
    [SerializeField] private TextMeshProUGUI bossCountdownText;

    [Tooltip("Slider hoặc bar hiển thị progress")]
    [SerializeField] private Slider progressSlider;

    [Header("🎨 UI Styling")]
    [Tooltip("Màu text cho difficulty thấp")]
    [SerializeField] private Color lowDifficultyColor = Color.green;

    [Tooltip("Màu text cho difficulty cao")]
    [SerializeField] private Color highDifficultyColor = Color.red;

    [Tooltip("Difficulty threshold để đổi màu")]
    [SerializeField] private int colorChangeThreshold = 10;

    [Header("⚙️ Update Settings")]
    [Tooltip("Cập nhật UI mỗi bao nhiêu giây")]
    [SerializeField] private float updateInterval = 0.5f;

    [Tooltip("Có hiển thị debug info không")]
    [SerializeField] private bool showDebugInfo = true;

    // Private variables
    private float updateTimer = 0f;
    private int lastDifficulty = 0;
    private int lastRoomsCleared = 0;

    private void Start()
    {
        // Subscribe to events
        ExitTrigger.OnPlayerEnterExit += OnPlayerEnterExit;
        ExitTrigger.OnRoomSpawnRequested += OnRoomSpawnRequested;

        // Initial UI update
        UpdateUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        ExitTrigger.OnPlayerEnterExit -= OnPlayerEnterExit;
        ExitTrigger.OnRoomSpawnRequested -= OnRoomSpawnRequested;
    }

    private void Update()
    {
        // Update UI periodically
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateUI();
            updateTimer = 0f;
        }
    }

    /// <summary>
    /// Cập nhật tất cả UI elements
    /// </summary>
    private void UpdateUI()
    {
        if (RoomManager.Instance == null)
        {
            ShowNoManagerWarning();
            return;
        }

        // Get data from RoomManager
        int currentDifficulty = RoomManager.Instance.GetCurrentDifficulty();
        int totalRooms = RoomManager.Instance.GetTotalRoomsSpawned();

        // Update rooms cleared (total rooms - 1 vì room đầu tiên không count)
        int roomsCleared = Mathf.Max(0, totalRooms - 1);
        UpdateRoomsClearedText(roomsCleared);

        // Update difficulty
        UpdateDifficultyText(currentDifficulty);

        // Update room type and boss countdown
        UpdateRoomTypeAndBossInfo();

        // Update progress slider if available
        UpdateProgressSlider(currentDifficulty);

        // Check for milestone achievements
        CheckMilestones(currentDifficulty, roomsCleared);
    }

    private void UpdateRoomsClearedText(int roomsCleared)
    {
        if (roomsClearedText != null)
        {
            roomsClearedText.text = $"Rooms Cleared: {roomsCleared}";

            // Animate nếu số thay đổi
            if (roomsCleared != lastRoomsCleared)
            {
                AnimateTextChange(roomsClearedText);
                lastRoomsCleared = roomsCleared;
            }
        }
    }

    private void UpdateDifficultyText(int difficulty)
    {
        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {difficulty}";

            // Thay đổi màu dựa trên difficulty
            float t = Mathf.Clamp01((float)difficulty / colorChangeThreshold);
            difficultyText.color = Color.Lerp(lowDifficultyColor, highDifficultyColor, t);

            // Animate nếu difficulty thay đổi
            if (difficulty != lastDifficulty)
            {
                AnimateTextChange(difficultyText);
                lastDifficulty = difficulty;
            }
        }
    }

    private void UpdateProgressSlider(int difficulty)
    {
        if (progressSlider != null)
        {
            // Có thể customize logic này - ví dụ progress đến boss fight
            float progress = (difficulty % 5) / 5f; // Reset mỗi 5 levels
            progressSlider.value = progress;
        }
    }

    private void AnimateTextChange(TextMeshProUGUI text)
    {
        if (text == null) return;

        // Simple scale animation without external libs
        StartCoroutine(ScaleAnimation(text.transform));
    }

    private System.Collections.IEnumerator ScaleAnimation(Transform target)
    {
        float time = 0f;
        float duration = 0.3f;
        Vector3 startScale = Vector3.one * 1.2f;
        Vector3 endScale = Vector3.one;
        target.localScale = startScale;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            target.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        target.localScale = endScale;
    }

    /// <summary>
    /// Update room type and boss countdown info
    /// </summary>
    private void UpdateRoomTypeAndBossInfo()
    {
        if (RoomManager.Instance == null) return;

        // Update current room type
        string roomType = RoomManager.Instance.GetCurrentRoomType();
        if (roomTypeText != null)
        {
            roomTypeText.text = $"Room Type: {roomType}";

            // Color code based on room type
            if (roomType == "Boss")
            {
                roomTypeText.color = Color.red;
            }
            else
            {
                roomTypeText.color = Color.white;
            }
        }

        // Update boss countdown
        if (bossCountdownText != null)
        {
            int roomsUntilBoss = RoomManager.Instance.GetRoomsUntilNextBoss();
            bool isNextBoss = RoomManager.Instance.IsNextRoomBoss();

            if (roomType == "Boss")
            {
                bossCountdownText.text = "🔥 BOSS FIGHT! 🔥";
                bossCountdownText.color = Color.red;
            }
            else if (isNextBoss)
            {
                bossCountdownText.text = "⚠️ NEXT: BOSS ROOM ⚠️";
                bossCountdownText.color = Color.yellow;
            }
            else
            {
                bossCountdownText.text = $"Boss in: {roomsUntilBoss} rooms";
                bossCountdownText.color = Color.cyan;
            }
        }
    }

    private void CheckMilestones(int difficulty, int roomsCleared)
    {
        // Boss room milestones (mỗi 5 levels)
        if (difficulty > 1 && difficulty % 5 == 1 && lastDifficulty != difficulty)
        {
            ShowMilestone($"Boss Defeated! Entering Area {difficulty / 5 + 1}");
        }

        // Room clearing milestones
        if (roomsCleared > 0 && roomsCleared % 10 == 0 && roomsCleared != lastRoomsCleared)
        {
            ShowMilestone($"Survivor! {roomsCleared} Rooms Cleared!");
        }

        // Special achievements
        if (difficulty == 20)
        {
            ShowMilestone("Legendary Explorer! Difficulty 20 Reached!");
        }
        else if (difficulty == 50)
        {
            ShowMilestone("Dungeon Master! Difficulty 50 Reached!");
        }
        else if (difficulty == 100)
        {
            ShowMilestone("ENDLESS CHAMPION! Difficulty 100 Reached!");
        }
    }

    private void ShowMilestone(string message)
    {
        Debug.Log($"🏆 MILESTONE: {message}");
        // TODO: Implement popup/sound/reward
    }

    private void ShowNoManagerWarning()
    {
        if (roomsClearedText != null)
            roomsClearedText.text = "No RoomManager Found!";
        if (difficultyText != null)
            difficultyText.text = "Check Setup";
        if (activeRoomsText != null)
            activeRoomsText.text = "---";
    }

    private void OnPlayerEnterExit()
    {
        if (showDebugInfo)
        {
            Debug.Log("📱 UI: Player entered exit trigger");
        }
    }

    private void OnRoomSpawnRequested()
    {
        if (showDebugInfo)
        {
            Debug.Log("📱 UI: Room spawn requested");
        }
        UpdateUI();
    }

    [ContextMenu("Reset UI")]
    public void ResetUI()
    {
        lastDifficulty = 0;
        lastRoomsCleared = 0;
        updateTimer = 0f;
        UpdateUI();
        Debug.Log("📱 UI Reset complete");
    }

    [ContextMenu("Toggle Debug Info")]
    public void ToggleDebugInfo()
    {
        showDebugInfo = !showDebugInfo;
        Debug.Log($"📱 Debug info: {(showDebugInfo ? "ON" : "OFF")}");
    }
}