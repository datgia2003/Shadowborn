using UnityEngine;

/// <summary>
/// Demo script để test và showcase Endless Room system
/// </summary>
public class EndlessRoomDemo : MonoBehaviour
{
    [Header("🎮 Demo Controls")]
    [Tooltip("Key để force spawn room tiếp theo")]
    [SerializeField] private KeyCode forceSpawnKey = KeyCode.N;

    [Tooltip("Key để reset toàn bộ room system")]
    [SerializeField] private KeyCode resetSystemKey = KeyCode.R;

    [Tooltip("Key để teleport player to exit (for testing)")]
    [SerializeField] private KeyCode teleportToExitKey = KeyCode.T;

    [Header("📊 Demo Info")]
    [SerializeField] private bool showDemoUI = true;
    [SerializeField] private bool enableKeyControls = true;

    private void Update()
    {
        if (!enableKeyControls) return;

        // Force spawn next room
        if (Input.GetKeyDown(forceSpawnKey))
        {
            ForceSpawnNextRoom();
        }

        // Reset room system
        if (Input.GetKeyDown(resetSystemKey))
        {
            ResetRoomSystem();
        }

        // Teleport to exit for testing
        if (Input.GetKeyDown(teleportToExitKey))
        {
            TeleportPlayerToExit();
        }
    }

    /// <summary>
    /// Force spawn room tiếp theo (for testing)
    /// </summary>
    [ContextMenu("Force Spawn Next Room")]
    public void ForceSpawnNextRoom()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ No RoomManager found!");
            return;
        }

        Debug.Log("🔧 DEMO: Force spawning next room...");
        RoomManager.Instance.SpawnNextRoom();
    }

    /// <summary>
    /// Reset toàn bộ room system
    /// </summary>
    [ContextMenu("Reset Room System")]
    public void ResetRoomSystem()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ No RoomManager found!");
            return;
        }

        Debug.Log("🔄 DEMO: Resetting room system...");
        RoomManager.Instance.ResetRoomSystem();
    }

    /// <summary>
    /// Teleport player đến exit của room hiện tại (for testing)
    /// </summary>
    [ContextMenu("Teleport Player to Exit")]
    public void TeleportPlayerToExit()
    {
        // Tìm player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("❌ No Player found with tag 'Player'!");
            return;
        }

        // Tìm room hiện tại
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ No RoomManager found!");
            return;
        }

        GameObject currentRoom = RoomManager.Instance.GetCurrentRoom();
        if (currentRoom == null)
        {
            Debug.LogError("❌ No current room found!");
            return;
        }

        // Tìm exit point
        Transform exitPoint = currentRoom.transform.Find("Exit");
        if (exitPoint == null)
        {
            Debug.LogError("❌ No Exit point found in current room!");
            return;
        }

        // Teleport player
        Vector3 teleportPos = exitPoint.position + Vector3.left * 2f; // Offset để không trigger ngay lập tức
        player.transform.position = teleportPos;

        Debug.Log($"📍 DEMO: Teleported player to {teleportPos} (near exit)");
    }

    /// <summary>
    /// Show demo statistics
    /// </summary>
    [ContextMenu("Show Demo Statistics")]
    public void ShowDemoStatistics()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogError("❌ No RoomManager found!");
            return;
        }

        Debug.Log("📊 ENDLESS ROOM DEMO STATISTICS:");
        Debug.Log("=".PadRight(40, '='));
        Debug.Log($"🏠 Total Rooms Spawned: {RoomManager.Instance.GetTotalRoomsSpawned()}");
        Debug.Log($"🎯 Current Difficulty: {RoomManager.Instance.GetCurrentDifficulty()}");
        Debug.Log($"🏃 Rooms Cleared: {Mathf.Max(0, RoomManager.Instance.GetTotalRoomsSpawned() - 1)}");

        // Tìm player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Debug.Log($"📍 Player Position: {player.transform.position}");
        }

        // Tìm active triggers
        ExitTrigger[] triggers = FindObjectsOfType<ExitTrigger>();
        Debug.Log($"🚪 Active Exit Triggers: {triggers.Length}");

        int triggeredCount = 0;
        foreach (var trigger in triggers)
        {
            if (trigger.HasBeenTriggered())
                triggeredCount++;
        }
        Debug.Log($"✅ Triggered Exits: {triggeredCount}/{triggers.Length}");
    }

    private void OnGUI()
    {
        if (!showDemoUI) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("🎮 ENDLESS ROOM DEMO CONTROLS", GUI.skin.box);

        if (enableKeyControls)
        {
            GUILayout.Label($"[{forceSpawnKey}] Force Spawn Next Room");
            GUILayout.Label($"[{resetSystemKey}] Reset Room System");
            GUILayout.Label($"[{teleportToExitKey}] Teleport to Exit");
        }

        GUILayout.Space(10);

        if (RoomManager.Instance != null)
        {
            GUILayout.Label($"🏠 Rooms: {RoomManager.Instance.GetTotalRoomsSpawned()}");
            GUILayout.Label($"🎯 Difficulty: {RoomManager.Instance.GetCurrentDifficulty()}");
            GUILayout.Label($"🏃 Cleared: {Mathf.Max(0, RoomManager.Instance.GetTotalRoomsSpawned() - 1)}");
        }
        else
        {
            GUILayout.Label("❌ No RoomManager found!");
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Force Spawn"))
        {
            ForceSpawnNextRoom();
        }

        if (GUILayout.Button("Reset System"))
        {
            ResetRoomSystem();
        }

        if (GUILayout.Button("Show Stats"))
        {
            ShowDemoStatistics();
        }

        GUILayout.EndArea();
    }
}