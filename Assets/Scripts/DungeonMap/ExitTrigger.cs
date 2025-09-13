using UnityEngine;

/// <summary>
/// Trigger component đặt tại Exit của mỗi room để spawn room tiếp theo khi player chạm vào
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExitTrigger : MonoBehaviour
{
    [Header("🚪 Exit Trigger Settings")]
    [Tooltip("Có debug log khi player chạm trigger không")]
    [SerializeField] private bool enableDebugLog = true;

    [Tooltip("Tag của Player để kiểm tra va chạm")]
    [SerializeField] private string playerTag = "Player";

    [Header("🎮 Trigger State")]
    [Tooltip("Đã được trigger chưa (để tránh spawn nhiều lần)")]
    [SerializeField] private bool hasTriggered = false;

    [Tooltip("Có thể reset trigger sau khi đã kích hoạt không")]
    [SerializeField] private bool canReset = false;

    // Events (có thể dùng cho effects, sounds, etc.)
    public static System.Action OnPlayerEnterExit;
    public static System.Action OnRoomSpawnRequested;

    private void Awake()
    {
        // Đảm bảo Collider2D được cấu hình đúng
        ValidateColliderSetup();
    }

    private void Start()
    {
        // Kiểm tra xem RoomManager có tồn tại không
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ ExitTrigger: Không tìm thấy RoomManager! Hãy đảm bảo có RoomManager trong scene.");
        }
    }

    /// <summary>
    /// Kiểm tra và cấu hình Collider2D để đảm bảo hoạt động đúng
    /// </summary>
    private void ValidateColliderSetup()
    {
        Collider2D collider = GetComponent<Collider2D>();

        if (collider == null)
        {
            Debug.LogError($"❌ ExitTrigger: Object {gameObject.name} không có Collider2D! Hãy thêm Collider2D component.");
            return;
        }

        // Đảm bảo collider là trigger
        if (!collider.isTrigger)
        {
            Debug.LogWarning($"⚠️ ExitTrigger: Collider2D trên {gameObject.name} không phải trigger! Đang tự động set isTrigger = true.");
            collider.isTrigger = true;
        }

        if (enableDebugLog)
        {
            Debug.Log($"✅ ExitTrigger: Collider2D setup validated for {gameObject.name}");
        }
    }

    /// <summary>
    /// Được gọi khi object khác chạm vào trigger
    /// </summary>
    /// <param name="other">Collider của object chạm vào</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem có phải Player không
        if (!other.CompareTag(playerTag))
        {
            if (enableDebugLog)
            {
                Debug.Log($"🚫 ExitTrigger: Object {other.name} with tag '{other.tag}' entered, but not Player tag '{playerTag}'");
            }
            return;
        }

        // Kiểm tra xem đã trigger chưa
        if (hasTriggered)
        {
            if (enableDebugLog)
            {
                Debug.Log($"⏹️ ExitTrigger: Already triggered! Ignoring Player enter on {gameObject.name}");
            }
            return;
        }

        // Player đã chạm vào exit trigger
        if (enableDebugLog)
        {
            Debug.Log($"🎯 ExitTrigger: Player entered exit trigger on {gameObject.name}!");
        }

        // Đánh dấu đã triggered để tránh spam
        hasTriggered = true;

        // Trigger event cho các system khác
        OnPlayerEnterExit?.Invoke();

        // Yêu cầu RoomManager spawn room tiếp theo
        RequestNextRoom();
    }

    /// <summary>
    /// Yêu cầu RoomManager spawn room tiếp theo
    /// </summary>
    private void RequestNextRoom()
    {
        // Kiểm tra RoomManager có tồn tại không
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ ExitTrigger: RoomManager.Instance is null! Cannot spawn next room.");
            return;
        }

        if (enableDebugLog)
        {
            Debug.Log($"📞 ExitTrigger: Requesting next room from RoomManager...");
        }

        // Trigger event
        OnRoomSpawnRequested?.Invoke();

        // Gọi RoomManager để spawn room tiếp theo
        RoomManager.Instance.SpawnNextRoom();

        if (enableDebugLog)
        {
            Debug.Log($"✅ ExitTrigger: Next room request sent successfully!");
        }
    }

    /// <summary>
    /// Reset trigger state (có thể dùng cho testing hoặc special cases)
    /// </summary>
    [ContextMenu("Reset Trigger")]
    public void ResetTrigger()
    {
        if (!canReset)
        {
            Debug.LogWarning("⚠️ ExitTrigger: Reset not allowed. Set 'canReset' to true if needed.");
            return;
        }

        hasTriggered = false;

        if (enableDebugLog)
        {
            Debug.Log($"🔄 ExitTrigger: Trigger state reset for {gameObject.name}");
        }
    }

    /// <summary>
    /// Force trigger (cho testing hoặc special events)
    /// </summary>
    [ContextMenu("Force Trigger")]
    public void ForceTrigger()
    {
        if (enableDebugLog)
        {
            Debug.Log($"🔧 ExitTrigger: Force triggering {gameObject.name}");
        }

        hasTriggered = false; // Temporarily reset để có thể trigger
        RequestNextRoom();
    }

    /// <summary>
    /// Kiểm tra xem trigger đã được kích hoạt chưa
    /// </summary>
    /// <returns>True nếu đã triggered</returns>
    public bool HasBeenTriggered()
    {
        return hasTriggered;
    }

    /// <summary>
    /// Set trigger state manually (cho advanced usage)
    /// </summary>
    /// <param name="triggered">True để đánh dấu đã triggered</param>
    public void SetTriggerState(bool triggered)
    {
        hasTriggered = triggered;

        if (enableDebugLog)
        {
            Debug.Log($"🔧 ExitTrigger: Trigger state manually set to {triggered} for {gameObject.name}");
        }
    }

    // Visual debugging trong Scene view
    private void OnDrawGizmos()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null) return;

        // Đổi màu dựa trên trigger state
        Gizmos.color = hasTriggered ? Color.red : Color.green;

        // Vẽ bounds của collider
        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);

        // Vẽ arrow chỉ hướng exit
        Vector3 center = collider.bounds.center;
        Vector3 arrowEnd = center + Vector3.right * (collider.bounds.size.x * 0.5f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, arrowEnd);

        // Vẽ arrow head
        Vector3 arrowHead1 = arrowEnd + Vector3.left * 0.3f + Vector3.up * 0.2f;
        Vector3 arrowHead2 = arrowEnd + Vector3.left * 0.3f + Vector3.down * 0.2f;

        Gizmos.DrawLine(arrowEnd, arrowHead1);
        Gizmos.DrawLine(arrowEnd, arrowHead2);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (enableDebugLog)
        {
            Debug.Log($"👋 ExitTrigger: Player exited trigger on {gameObject.name}");
        }
    }
}