using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý hệ thống Endless Room - sinh ra các phòng liên tiếp khi player di chuyển
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("🏠 Room Configuration")]
    [Tooltip("Danh sách các prefab room có thể spawn")]
    public List<GameObject> roomPrefabs = new List<GameObject>();

    [Header("🎮 Gameplay Settings")]
    [Tooltip("Số lượng room tối đa được giữ active cùng lúc")]
    [SerializeField] private int maxActiveRooms = 3;

    [Tooltip("Vị trí spawn room đầu tiên")]
    [SerializeField] private Vector3 startPosition = Vector3.zero;

    [Header("📊 Debug Info")]
    [Tooltip("Level độ khó hiện tại (tăng dần mỗi room)")]
    [SerializeField] private int difficultyLevel = 1;

    [Tooltip("Tổng số room đã spawn")]
    [SerializeField] private int totalRoomsSpawned = 0;

    // Private variables
    private readonly List<GameObject> activeRooms = new List<GameObject>(); // Danh sách room đang active
    private GameObject currentRoom; // Room hiện tại player đang ở
    private Vector3 nextSpawnPosition; // Vị trí để spawn room tiếp theo (thông tin debug)

    // Singleton pattern để dễ truy cập từ ExitTrigger
    public static RoomManager Instance { get; private set; }

    private void Awake()
    {
        // Đảm bảo chỉ có 1 RoomManager trong scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Kiểm tra xem có room prefab nào được gán không
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("❌ RoomManager: Không có room prefab nào! Hãy gán room prefabs vào list.");
            return;
        }

        // Spawn room đầu tiên tại vị trí start
        SpawnFirstRoom();
    }

    /// <summary>
    /// Spawn room đầu tiên khi game bắt đầu
    /// </summary>
    private void SpawnFirstRoom()
    {
        Debug.Log("🏠 RoomManager: Spawning first room...");

        // Chọn room đầu tiên (có thể random hoặc fixed)
        GameObject firstRoomPrefab = roomPrefabs[0];
        if (firstRoomPrefab == null)
        {
            Debug.LogError("❌ RoomManager: Room prefab đầu tiên null!");
            return;
        }

        // Spawn room tại vị trí start
        GameObject firstRoom = Instantiate(firstRoomPrefab, startPosition, Quaternion.identity);
        firstRoom.name = $"Room_01_Difficulty_{difficultyLevel}";

        // Thêm vào danh sách active rooms
        activeRooms.Add(firstRoom);
        currentRoom = firstRoom;
        totalRoomsSpawned++;

        // Ghi log spawn đầu tiên
        Debug.Log($"✅ First room spawned at {startPosition}.");
    }

    /// <summary>
    /// Spawn room tiếp theo khi player chạm exit trigger
    /// Được gọi từ ExitTrigger.cs
    /// </summary>
    public void SpawnNextRoom()
    {
        if (roomPrefabs == null || roomPrefabs.Count == 0)
        {
            Debug.LogError("❌ RoomManager: Không có room prefab để spawn!");
            return;
        }

        Debug.Log($"🚪 RoomManager: Player reached exit! Spawning next room (Difficulty {difficultyLevel + 1})...");

        // Tăng difficulty level
        difficultyLevel++;

        // Chọn random một room prefab
        int randomIndex = Random.Range(0, roomPrefabs.Count);
        GameObject roomPrefab = roomPrefabs[randomIndex];
        if (roomPrefab == null)
        {
            Debug.LogError("❌ RoomManager: Room prefab random null!");
            return;
        }

        // Tính toán vị trí spawn dựa trên Exit của room hiện tại và Entry offset của prefab được chọn
        Vector3 spawnPos = CalculateSpawnPositionForPrefab(roomPrefab);
        nextSpawnPosition = spawnPos; // lưu lại để debug

        // Spawn room mới tại vị trí đã tính toán
        GameObject newRoom = Instantiate(roomPrefab, spawnPos, Quaternion.identity);
        newRoom.name = $"Room_{totalRoomsSpawned + 1:D2}_Difficulty_{difficultyLevel}";

        // Thêm room mới vào active list
        activeRooms.Add(newRoom);
        currentRoom = newRoom;
        totalRoomsSpawned++;

        // Quản lý số lượng room active (xóa room cũ nếu cần)
        ManageActiveRooms();

        Debug.Log($"✅ New room spawned! Total rooms: {totalRoomsSpawned}, Active rooms: {activeRooms.Count}, Difficulty: {difficultyLevel}");

        // Trigger event cho các system khác (spawn enemies, etc.)
        OnNewRoomSpawned(difficultyLevel);
    }

    /// <summary>
    /// Tính toán vị trí spawn cho prefab được chọn dựa trên Exit point của room hiện tại
    /// </summary>
    /// <param name="roomPrefabToSpawn">Prefab của room chuẩn bị spawn</param>
    /// <returns>Vị trí spawn thích hợp</returns>
    private Vector3 CalculateSpawnPositionForPrefab(GameObject roomPrefabToSpawn)
    {
        if (currentRoom == null)
        {
            // Nếu chưa có room nào (edge case), dùng startPosition
            return startPosition;
        }

        // Tìm Exit point trong room hiện tại
        Transform exitPoint = FindExitPoint(currentRoom);
        if (exitPoint == null)
        {
            Debug.LogError($"❌ Room {currentRoom.name} không có Exit point! Hãy tạo Empty GameObject tên 'Exit' trong room prefab.");
            return startPosition;
        }

        // Tìm Entry point của room sắp spawn
        Transform entryPoint = FindEntryPoint(roomPrefabToSpawn);
        if (entryPoint == null)
        {
            Debug.LogError($"❌ Room prefab {roomPrefabToSpawn.name} không có Entry point! Hãy tạo Empty GameObject tên 'Entry' trong room prefab.");
            return exitPoint.position; // fallback
        }

        // Tính toán offset từ prefab position đến entry point
        Vector3 entryOffset = entryPoint.localPosition;

        // Vị trí spawn = Exit của room hiện tại - Entry offset của room mới
        Vector3 spawnPos = exitPoint.position - entryOffset;

        Debug.Log($"📍 Spawn position calculated: {spawnPos} (Exit: {exitPoint.position}, Entry offset: {entryOffset})");
        return spawnPos;
    }

    /// <summary>
    /// Quản lý số lượng room active - xóa room cũ nhất nếu vượt quá maxActiveRooms
    /// </summary>
    private void ManageActiveRooms()
    {
        // Nếu số room active vượt quá giới hạn
        if (activeRooms.Count > maxActiveRooms)
        {
            // Xóa room cũ nhất (index 0)
            GameObject oldestRoom = activeRooms[0];
            activeRooms.RemoveAt(0);

            Debug.Log($"🗑️ Destroying oldest room: {oldestRoom.name}");
            Destroy(oldestRoom);
        }
    }

    /// <summary>
    /// Tìm Exit point trong room (child object có tên "Exit")
    /// </summary>
    /// <param name="room">Room object để tìm</param>
    /// <returns>Transform của Exit point, null nếu không tìm thấy</returns>
    private Transform FindExitPoint(GameObject room)
    {
        Transform exitPoint = room.transform.Find("Exit");
        if (exitPoint == null)
        {
            // Thử tìm trong children
            for (int i = 0; i < room.transform.childCount; i++)
            {
                Transform child = room.transform.GetChild(i);
                if (child.name.ToLower().Contains("exit"))
                {
                    return child;
                }
            }
        }
        return exitPoint;
    }

    /// <summary>
    /// Tìm Entry point trong room prefab (child object có tên "Entry")
    /// </summary>
    /// <param name="roomPrefab">Room prefab để tìm</param>
    /// <returns>Transform của Entry point, null nếu không tìm thấy</returns>
    private Transform FindEntryPoint(GameObject roomPrefab)
    {
        Transform entryPoint = roomPrefab.transform.Find("Entry");
        if (entryPoint == null)
        {
            // Thử tìm trong children
            for (int i = 0; i < roomPrefab.transform.childCount; i++)
            {
                Transform child = roomPrefab.transform.GetChild(i);
                if (child.name.ToLower().Contains("entry"))
                {
                    return child;
                }
            }
        }
        return entryPoint;
    }

    /// <summary>
    /// Event được gọi khi room mới được spawn - có thể dùng để spawn enemies, áp dụng difficulty, etc.
    /// </summary>
    /// <param name="difficulty">Level độ khó hiện tại</param>
    private void OnNewRoomSpawned(int difficulty)
    {
        // TODO: Implement logic based on difficulty
        // Ví dụ: spawn enemies, thay đổi lighting, tăng reward, etc.

        Debug.Log($"🎯 Room spawned with difficulty {difficulty}. You can implement enemy spawning here!");

        // Ví dụ implementation:
        // - Spawn (difficulty * 2) enemies
        // - Increase enemy stats by (difficulty * 10%)
        // - Add special effects based on difficulty
    }

    /// <summary>
    /// Get current difficulty level (có thể dùng bởi các script khác)
    /// </summary>
    public int GetCurrentDifficulty()
    {
        return difficultyLevel;
    }

    /// <summary>
    /// Get total rooms spawned (cho statistics/UI)
    /// </summary>
    public int GetTotalRoomsSpawned()
    {
        return totalRoomsSpawned;
    }

    /// <summary>
    /// Get current room (có thể cần cho camera follow, etc.)
    /// </summary>
    public GameObject GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Reset room system (có thể dùng khi player chết và restart)
    /// </summary>
    [ContextMenu("Reset Room System")]
    public void ResetRoomSystem()
    {
        Debug.Log("🔄 Resetting room system...");

        // Xóa tất cả room active
        foreach (GameObject room in activeRooms)
        {
            if (room != null)
                Destroy(room);
        }

        activeRooms.Clear();
        currentRoom = null;
        difficultyLevel = 1;
        totalRoomsSpawned = 0;
        nextSpawnPosition = Vector3.zero;

        // Spawn lại room đầu tiên
        SpawnFirstRoom();

        Debug.Log("✅ Room system reset complete!");
    }
}

/*
========================================
🛠️ HƯỚNG DẪN SETUP ENDLESS ROOM SYSTEM
========================================

📋 BƯỚC 1: TẠO ROOM PREFAB
--------------------------
1. Tạo Empty GameObject, đặt tên "Room_01"
2. Thêm các object con: Background, Walls, Props, Enemies, etc.
3. Tạo 2 Empty GameObject con:
   - "Entry": Điểm bắt đầu của room (thường ở bên trái)
   - "Exit": Điểm kết thúc của room (thường ở bên phải)
4. Thêm Collider2D (isTrigger = true) tại vị trí Exit
5. Attach script ExitTrigger.cs vào Collider2D của Exit
6. Kéo thả room vào Project để tạo prefab

📋 BƯỚC 2: SETUP PLAYER
-----------------------
1. Đảm bảo Player có Collider2D (không cần isTrigger)
2. Đảm bảo Player có tag = "Player"
3. Player phải có thể di chuyển và va chạm với trigger

📋 BƯỚC 3: SETUP ROOM MANAGER
-----------------------------
1. Tạo Empty GameObject trong Scene, đặt tên "RoomManager"
2. Attach script RoomManager.cs
3. Trong Inspector:
   - Gán các Room Prefab vào list "Room Prefabs"
   - Đặt "Max Active Rooms" = 3 (hoặc số khác)
   - Đặt "Start Position" = (0, 0, 0) hoặc vị trí mong muốn

📋 BƯỚC 4: TEST SYSTEM
----------------------
1. Chạy game, room đầu tiên sẽ spawn tại Start Position
2. Di chuyển Player đến Exit trigger của room
3. Room mới sẽ spawn, Entry của room mới khớp với Exit của room cũ
4. Difficulty level sẽ tăng dần mỗi room
5. Room cũ sẽ bị xóa khi vượt quá Max Active Rooms

📋 BƯỚC 5: TÙYÝ CHỈNH (TÙY CHỌN)
---------------------------------
1. Thêm logic spawn enemies trong OnNewRoomSpawned()
2. Tạo UI hiển thị difficulty level, rooms cleared, etc.
3. Thêm effects khi spawn room mới
4. Implement save/load system cho progress
5. Thêm boss rooms ở difficulty levels đặc biệt

⚠️ LƯU Ý QUAN TRỌNG:
- Entry và Exit points phải được đặt chính xác để room nối tiếp nhau
- Player phải có Collider2D và tag "Player"
- Room prefabs phải có Entry và Exit objects
- ExitTrigger phải được đặt chính xác tại vị trí Exit
*/