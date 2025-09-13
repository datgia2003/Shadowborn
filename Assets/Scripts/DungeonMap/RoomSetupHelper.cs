using UnityEngine;

/// <summary>
/// Helper script để tạo room prefab nhanh chóng và setup Endless Room system
/// </summary>
public class RoomSetupHelper : MonoBehaviour
{
    [Header("🛠️ Quick Room Setup")]
    [Tooltip("Tự động tạo Entry và Exit points cho room này")]
    [SerializeField] private bool autoCreateEntryExit = true;

    [Tooltip("Khoảng cách từ center đến Entry point")]
    [SerializeField] private float entryOffset = -10f;

    [Tooltip("Khoảng cách từ center đến Exit point")]
    [SerializeField] private float exitOffset = 10f;

    [Tooltip("Size của Exit trigger")]
    [SerializeField] private Vector2 exitTriggerSize = new Vector2(2f, 4f);

    [Header("🎨 Visual Helpers")]
    [Tooltip("Hiển thị Entry/Exit points trong Scene view")]
    [SerializeField] private bool showGizmos = true;

    [Tooltip("Màu của Entry point")]
    [SerializeField] private Color entryColor = Color.green;

    [Tooltip("Màu của Exit point")]
    [SerializeField] private Color exitColor = Color.red;

    private Transform entryPoint;
    private Transform exitPoint;

    /// <summary>
    /// Tạo Entry và Exit points cho room
    /// </summary>
    [ContextMenu("Create Entry and Exit Points")]
    public void CreateEntryExitPoints()
    {
        // Tạo Entry point
        if (transform.Find("Entry") == null)
        {
            GameObject entryGO = new GameObject("Entry");
            entryGO.transform.SetParent(transform);
            entryGO.transform.localPosition = new Vector3(entryOffset, 0, 0);
            entryPoint = entryGO.transform;

            Debug.Log($"✅ Created Entry point at {entryGO.transform.position}");
        }
        else
        {
            entryPoint = transform.Find("Entry");
            Debug.Log("📍 Entry point already exists");
        }

        // Tạo Exit point với trigger
        if (transform.Find("Exit") == null)
        {
            GameObject exitGO = new GameObject("Exit");
            exitGO.transform.SetParent(transform);
            exitGO.transform.localPosition = new Vector3(exitOffset, 0, 0);

            // Thêm Collider2D trigger
            BoxCollider2D exitCollider = exitGO.AddComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exitCollider.size = exitTriggerSize;

            // Thêm ExitTrigger script
            exitGO.AddComponent<ExitTrigger>();

            exitPoint = exitGO.transform;

            Debug.Log($"✅ Created Exit point with trigger at {exitGO.transform.position}");
        }
        else
        {
            exitPoint = transform.Find("Exit");
            Debug.Log("📍 Exit point already exists");
        }
    }

    /// <summary>
    /// Validate room setup và hiển thị thông tin
    /// </summary>
    [ContextMenu("Validate Room Setup")]
    public void ValidateRoomSetup()
    {
        Debug.Log($"🔍 Validating room setup for {gameObject.name}:");

        bool isValid = true;

        // Kiểm tra Entry point
        Transform entry = transform.Find("Entry");
        if (entry == null)
        {
            Debug.LogError("❌ Missing Entry point! Use 'Create Entry and Exit Points' to fix.");
            isValid = false;
        }
        else
        {
            Debug.Log($"✅ Entry point found at {entry.position}");
        }

        // Kiểm tra Exit point
        Transform exit = transform.Find("Exit");
        if (exit == null)
        {
            Debug.LogError("❌ Missing Exit point! Use 'Create Entry and Exit Points' to fix.");
            isValid = false;
        }
        else
        {
            Debug.Log($"✅ Exit point found at {exit.position}");

            // Kiểm tra Exit có Collider2D trigger không
            Collider2D exitCollider = exit.GetComponent<Collider2D>();
            if (exitCollider == null)
            {
                Debug.LogError("❌ Exit point missing Collider2D!");
                isValid = false;
            }
            else if (!exitCollider.isTrigger)
            {
                Debug.LogError("❌ Exit Collider2D is not a trigger!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ Exit trigger collider properly configured");
            }

            // Kiểm tra ExitTrigger script
            ExitTrigger exitTrigger = exit.GetComponent<ExitTrigger>();
            if (exitTrigger == null)
            {
                Debug.LogError("❌ Exit point missing ExitTrigger script!");
                isValid = false;
            }
            else
            {
                Debug.Log("✅ ExitTrigger script found");
            }
        }

        if (isValid)
        {
            Debug.Log("🎉 Room setup is valid! Ready to be used as prefab.");
        }
        else
        {
            Debug.LogError("⚠️ Room setup has issues. Please fix before using as prefab.");
        }
    }

    /// <summary>
    /// Test spawn room tiếp theo (chỉ dùng khi có RoomManager trong scene)
    /// </summary>
    [ContextMenu("Test Spawn Next Room")]
    public void TestSpawnNextRoom()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ No RoomManager found in scene! Cannot test spawn.");
            return;
        }

        Debug.Log("🧪 Testing spawn next room...");
        RoomManager.Instance.SpawnNextRoom();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Tìm hoặc tạo reference đến Entry/Exit
        if (entryPoint == null)
            entryPoint = transform.Find("Entry");
        if (exitPoint == null)
            exitPoint = transform.Find("Exit");

        // Vẽ Entry point
        if (entryPoint != null)
        {
            Gizmos.color = entryColor;
            Gizmos.DrawWireSphere(entryPoint.position, 1f);
            Gizmos.DrawCube(entryPoint.position, Vector3.one * 0.5f);
        }
        else if (autoCreateEntryExit)
        {
            // Preview Entry position
            Vector3 previewEntryPos = transform.position + new Vector3(entryOffset, 0, 0);
            Gizmos.color = Color.Lerp(entryColor, Color.white, 0.5f);
            Gizmos.DrawWireSphere(previewEntryPos, 1f);
        }

        // Vẽ Exit point
        if (exitPoint != null)
        {
            Gizmos.color = exitColor;
            Gizmos.DrawWireSphere(exitPoint.position, 1f);
            Gizmos.DrawCube(exitPoint.position, Vector3.one * 0.5f);

            // Vẽ trigger area nếu có
            Collider2D exitCollider = exitPoint.GetComponent<Collider2D>();
            if (exitCollider != null)
            {
                Gizmos.color = Color.Lerp(exitColor, Color.white, 0.7f);
                Gizmos.DrawWireCube(exitCollider.bounds.center, exitCollider.bounds.size);
            }
        }
        else if (autoCreateEntryExit)
        {
            // Preview Exit position
            Vector3 previewExitPos = transform.position + new Vector3(exitOffset, 0, 0);
            Gizmos.color = Color.Lerp(exitColor, Color.white, 0.5f);
            Gizmos.DrawWireSphere(previewExitPos, 1f);
            Gizmos.DrawWireCube(previewExitPos, new Vector3(exitTriggerSize.x, exitTriggerSize.y, 1f));
        }

        // Vẽ connection line
        if (entryPoint != null && exitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(entryPoint.position, exitPoint.position);
        }
        else if (autoCreateEntryExit)
        {
            Vector3 previewEntryPos = transform.position + new Vector3(entryOffset, 0, 0);
            Vector3 previewExitPos = transform.position + new Vector3(exitOffset, 0, 0);
            Gizmos.color = Color.Lerp(Color.yellow, Color.white, 0.5f);
            Gizmos.DrawLine(previewEntryPos, previewExitPos);
        }
    }

    private void OnValidate()
    {
        // Auto-update khi values thay đổi trong Inspector
        if (autoCreateEntryExit && Application.isPlaying == false)
        {
            // Update existing points positions if they exist
            Transform entry = transform.Find("Entry");
            Transform exit = transform.Find("Exit");

            if (entry != null)
                entry.localPosition = new Vector3(entryOffset, 0, 0);
            if (exit != null)
                exit.localPosition = new Vector3(exitOffset, 0, 0);
        }
    }
}