using UnityEngine;

/// <summary>
/// Auto-setup script for EnemySpawner in room prefabs
/// This script helps automatically configure EnemySpawner when added to a room
/// </summary>
public class RoomEnemySetup : MonoBehaviour
{
    [Header("🏠 Room Configuration")]
    [Tooltip("Room type (Normal/Boss) - auto-detected if empty")]
    public string roomType = "";

    [Header("🦇 Enemy Configuration")]
    [Tooltip("Override enemy count for this specific room")]
    public int customEnemyCount = -1; // -1 means use default

    [Tooltip("Custom enemy prefabs for this room (overrides spawner defaults)")]
    public GameObject[] customEnemyPrefabs;

    [Header("🎯 Spawn Points")]
    [Tooltip("Auto-create spawn points based on room size")]
    public bool autoCreateSpawnPoints = true;

    [Tooltip("Number of spawn points to create automatically")]
    public int autoSpawnPointCount = 8;

    [Tooltip("Margin from room edges when creating spawn points")]
    public float spawnMargin = 2f;

    private EnemySpawner enemySpawner;

    void Awake()
    {
        SetupEnemySpawner();
    }

    void SetupEnemySpawner()
    {
        // Get or create EnemySpawner
        enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner == null)
        {
            enemySpawner = gameObject.AddComponent<EnemySpawner>();
            Debug.Log($"✅ Auto-created EnemySpawner for room: {gameObject.name}");
        }

        // Auto-detect room type from name if not specified
        if (string.IsNullOrEmpty(roomType))
        {
            string roomName = gameObject.name.ToLower();
            if (roomName.Contains("boss"))
                roomType = "Boss";
            else
                roomType = "Normal";
        }

        // Setup custom enemy prefabs if provided
        if (customEnemyPrefabs != null && customEnemyPrefabs.Length > 0)
        {
            if (roomType == "Boss")
            {
                // Add to boss enemy prefabs
                foreach (GameObject prefab in customEnemyPrefabs)
                {
                    if (prefab != null && !enemySpawner.bossEnemyPrefabs.Contains(prefab))
                    {
                        enemySpawner.bossEnemyPrefabs.Add(prefab);
                    }
                }
            }
            else
            {
                // Add to normal enemy prefabs
                foreach (GameObject prefab in customEnemyPrefabs)
                {
                    if (prefab != null && !enemySpawner.normalEnemyPrefabs.Contains(prefab))
                    {
                        enemySpawner.normalEnemyPrefabs.Add(prefab);
                    }
                }
            }
        }

        // Auto-create spawn points if enabled
        if (autoCreateSpawnPoints)
        {
            CreateAutoSpawnPoints();
        }

        // Auto-detect room bounds
        BoxCollider2D roomBounds = GetComponent<BoxCollider2D>();
        if (roomBounds == null)
        {
            // Try to find room bounds in children
            roomBounds = GetComponentInChildren<BoxCollider2D>();
        }

        if (roomBounds != null)
        {
            // Set room bounds in spawner using reflection or direct access
            var field = typeof(EnemySpawner).GetField("roomBounds",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(enemySpawner, roomBounds);
            }
        }

        Debug.Log($"🏠 Room setup complete: {gameObject.name} ({roomType})");
    }

    void CreateAutoSpawnPoints()
    {
        // Look for existing spawn points first
        Transform spawnPointsParent = transform.Find("SpawnPoints");
        if (spawnPointsParent != null && spawnPointsParent.childCount > 0)
        {
            Debug.Log($"📍 Using existing spawn points ({spawnPointsParent.childCount} found)");
            return;
        }

        // Create spawn points parent
        if (spawnPointsParent == null)
        {
            GameObject spawnPointsObj = new GameObject("SpawnPoints");
            spawnPointsObj.transform.SetParent(transform);
            spawnPointsParent = spawnPointsObj.transform;
        }

        // Get room bounds
        BoxCollider2D bounds = GetComponent<BoxCollider2D>();
        if (bounds == null)
        {
            bounds = GetComponentInChildren<BoxCollider2D>();
        }

        if (bounds != null)
        {
            Bounds roomBounds = bounds.bounds;

            // Create spawn points in a grid pattern
            int pointsPerRow = Mathf.CeilToInt(Mathf.Sqrt(autoSpawnPointCount));
            int pointsPerCol = Mathf.CeilToInt((float)autoSpawnPointCount / pointsPerRow);

            float stepX = (roomBounds.size.x - spawnMargin * 2) / (pointsPerRow - 1);
            float stepY = (roomBounds.size.y - spawnMargin * 2) / (pointsPerCol - 1);

            int pointIndex = 0;
            for (int row = 0; row < pointsPerCol && pointIndex < autoSpawnPointCount; row++)
            {
                for (int col = 0; col < pointsPerRow && pointIndex < autoSpawnPointCount; col++)
                {
                    Vector3 spawnPos = new Vector3(
                        roomBounds.min.x + spawnMargin + col * stepX,
                        roomBounds.min.y + spawnMargin + row * stepY,
                        0f
                    );

                    GameObject spawnPoint = new GameObject($"SpawnPoint_{pointIndex + 1:D2}");
                    spawnPoint.transform.SetParent(spawnPointsParent);
                    spawnPoint.transform.position = spawnPos;

                    pointIndex++;
                }
            }

            Debug.Log($"📍 Created {pointIndex} auto spawn points for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ No BoxCollider2D found for room {gameObject.name} - cannot create auto spawn points");
        }
    }

    /// <summary>
    /// Get spawn points as Transform array for EnemySpawner
    /// </summary>
    public Transform[] GetSpawnPoints()
    {
        Transform spawnPointsParent = transform.Find("SpawnPoints");
        if (spawnPointsParent != null)
        {
            Transform[] points = new Transform[spawnPointsParent.childCount];
            for (int i = 0; i < spawnPointsParent.childCount; i++)
            {
                points[i] = spawnPointsParent.GetChild(i);
            }
            return points;
        }

        return new Transform[0];
    }

#if UNITY_EDITOR
    [ContextMenu("Setup Enemy Spawner")]
    public void ManualSetup()
    {
        SetupEnemySpawner();
    }

    [ContextMenu("Create Spawn Points")]
    public void ManualCreateSpawnPoints()
    {
        CreateAutoSpawnPoints();
    }

    void OnDrawGizmosSelected()
    {
        // Draw room bounds
        BoxCollider2D bounds = GetComponent<BoxCollider2D>();
        if (bounds == null)
            bounds = GetComponentInChildren<BoxCollider2D>();

        if (bounds != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.bounds.center, bounds.bounds.size);

            // Draw spawn margin
            Vector3 marginSize = bounds.bounds.size - Vector3.one * spawnMargin * 2;
            if (marginSize.x > 0 && marginSize.y > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(bounds.bounds.center, marginSize);
            }
        }

        // Draw spawn points
        Transform spawnPointsParent = transform.Find("SpawnPoints");
        if (spawnPointsParent != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in spawnPointsParent)
            {
                Gizmos.DrawWireSphere(point.position, 0.3f);
                Gizmos.DrawLine(point.position, point.position + Vector3.up * 1f);
            }
        }
    }
#endif
}