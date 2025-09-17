using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy Spawner System for Infinity Dungeon
/// Handles random enemy spawning based on room type and difficulty
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("🦇 Enemy Prefabs")]
    [Tooltip("Global enemy configuration (recommended)")]
    public EnemyConfiguration enemyConfig;

    [Tooltip("List of enemy prefabs for normal rooms (fallback if no config)")]
    public List<GameObject> normalEnemyPrefabs = new List<GameObject>();

    [Tooltip("List of enemy prefabs for boss rooms (fallback if no config)")]
    public List<GameObject> bossEnemyPrefabs = new List<GameObject>();

    [Header("📊 Spawn Settings")]
    [Tooltip("Base number of enemies to spawn in normal rooms")]
    [SerializeField] private int baseEnemyCount = 3;

    [Tooltip("Max additional enemies based on difficulty")]
    [SerializeField] private int maxDifficultyBonus = 5;

    [Tooltip("Difficulty scaling factor (enemies per difficulty level)")]
    [SerializeField] private float difficultyScaling = 0.5f;

    [Header("🎯 Spawn Configuration")]
    [Tooltip("Layer mask for valid spawn positions")]
    [SerializeField] private LayerMask obstacleLayer = -1;

    [Tooltip("Radius to check for obstacles when spawning")]
    [SerializeField] private float obstacleCheckRadius = 1f;

    [Tooltip("Max attempts to find valid spawn position")]
    [SerializeField] private int maxSpawnAttempts = 20;

    [Tooltip("Distance from player spawn to avoid")]
    [SerializeField] private float minDistanceFromPlayer = 5f;

    [Header("🔄 Spawn Points")]
    [Tooltip("Predefined spawn points (if empty, will spawn randomly in room bounds)")]
    [SerializeField] private Transform[] spawnPoints;

    [Tooltip("Room bounds for random spawning (auto-detected if null)")]
    [SerializeField] private BoxCollider2D roomBounds;

    [Header("⚙️ Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private bool logSpawnDetails = true;

    // Private variables
    private Transform playerTransform;
    private int lastDifficultyLevel = 0;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // Static instance for easy access
    public static EnemySpawner Current { get; private set; }

    void Awake()
    {
        Current = this;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;

        // Auto-detect room bounds if not assigned
        if (roomBounds == null)
            roomBounds = GetComponentInChildren<BoxCollider2D>();
    }

    /// <summary>
    /// Spawn enemies for a room based on type and difficulty
    /// </summary>
    /// <param name="roomType">Room type (Normal/Boss)</param>
    /// <param name="difficultyLevel">Current difficulty level</param>
    public void SpawnEnemiesForRoom(string roomType, int difficultyLevel)
    {
        if (logSpawnDetails)
            Debug.Log($"🦇 EnemySpawner: Spawning enemies for {roomType} room (Difficulty: {difficultyLevel})");

        // Clear any existing spawned enemies
        ClearSpawnedEnemies();

        lastDifficultyLevel = difficultyLevel;

        if (roomType.ToLower() == "boss")
        {
            SpawnBossEnemies(difficultyLevel);
        }
        else
        {
            SpawnNormalEnemies(difficultyLevel);
        }

        if (logSpawnDetails)
            Debug.Log($"✅ Spawned {spawnedEnemies.Count} enemies in {roomType} room");
    }

    /// <summary>
    /// Spawn enemies for normal rooms
    /// </summary>
    private void SpawnNormalEnemies(int difficultyLevel)
    {
        // Use enemy configuration if available
        if (enemyConfig != null)
        {
            SpawnNormalEnemiesFromConfig(difficultyLevel);
            return;
        }

        // Fallback to manual prefab lists
        if (normalEnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemySpawner: No normal enemy prefabs assigned and no config found!");
            return;
        }

        // Calculate enemy count based on difficulty
        int enemyCount = CalculateEnemyCount(difficultyLevel);

        for (int i = 0; i < enemyCount; i++)
        {
            // Select random enemy prefab
            GameObject enemyPrefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Count)];

            // Find spawn position
            Vector3 spawnPos = GetValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                // Spawn enemy
                GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                spawnedEnemy.name = $"{enemyPrefab.name}_D{difficultyLevel}_{i + 1}";

                // Add to tracking list
                spawnedEnemies.Add(spawnedEnemy);

                if (logSpawnDetails)
                    Debug.Log($"🦇 Spawned {enemyPrefab.name} at {spawnPos}");
            }
            else
            {
                Debug.LogWarning($"⚠️ EnemySpawner: Could not find valid spawn position for enemy {i + 1}");
            }
        }
    }

    /// <summary>
    /// Spawn normal enemies using enemy configuration
    /// </summary>
    private void SpawnNormalEnemiesFromConfig(int difficultyLevel)
    {
        int enemyCount = enemyConfig.CalculateEnemyCount(difficultyLevel);

        for (int i = 0; i < enemyCount; i++)
        {
            var enemyData = enemyConfig.GetRandomNormalEnemy(difficultyLevel);
            if (enemyData == null || enemyData.prefab == null)
            {
                Debug.LogWarning($"⚠️ No valid enemy found for difficulty {difficultyLevel}");
                continue;
            }

            Vector3 spawnPos = GetValidSpawnPosition();

            if (spawnPos != Vector3.zero)
            {
                GameObject spawnedEnemy = Instantiate(enemyData.prefab, spawnPos, Quaternion.identity);
                spawnedEnemy.name = $"{enemyData.enemyName}_D{difficultyLevel}_{i + 1}";

                // Apply difficulty scaling if needed
                ApplyDifficultyScaling(spawnedEnemy, enemyData, difficultyLevel);

                spawnedEnemies.Add(spawnedEnemy);

                if (logSpawnDetails)
                    Debug.Log($"🦇 Spawned {enemyData.enemyName} at {spawnPos} (Weight: {enemyData.spawnWeight})");
            }
        }
    }

    /// <summary>
    /// Apply difficulty scaling to spawned enemy
    /// </summary>
    private void ApplyDifficultyScaling(GameObject enemy, EnemyConfiguration.EnemyData enemyData, int difficultyLevel)
    {
        if (!enemyData.scalesWithDifficulty || difficultyLevel <= 1) return;

        // Basic difficulty scaling - you can expand based on your enemy structure
        float healthMultiplier = 1f + (difficultyLevel - 1) * enemyData.healthScaling;
        float damageMultiplier = 1f + (difficultyLevel - 1) * enemyData.damageScaling;

        // Add scaling component to enemy for reference
        var scalingComponent = enemy.GetComponent<EnemyDifficultyScaling>();
        if (scalingComponent == null)
        {
            scalingComponent = enemy.AddComponent<EnemyDifficultyScaling>();
        }

        scalingComponent.healthMultiplier = healthMultiplier;
        scalingComponent.damageMultiplier = damageMultiplier;
        scalingComponent.difficultyLevel = difficultyLevel;

        if (logSpawnDetails)
            Debug.Log($"🔧 Applied scaling to {enemy.name}: Health {healthMultiplier:F2}x, Damage {damageMultiplier:F2}x");
    }
    private void SpawnBossEnemies(int difficultyLevel)
    {
        if (bossEnemyPrefabs.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemySpawner: No boss enemy prefabs assigned!");
            return;
        }

        // Boss rooms typically spawn 1 main boss + some minions
        // Select random boss
        GameObject bossPrefab = bossEnemyPrefabs[Random.Range(0, bossEnemyPrefabs.Count)];

        // Spawn boss at center or designated spawn point
        Vector3 bossSpawnPos = GetBossSpawnPosition();

        if (bossSpawnPos != Vector3.zero)
        {
            GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPos, Quaternion.identity);
            spawnedBoss.name = $"Boss_{bossPrefab.name}_D{difficultyLevel}";
            spawnedEnemies.Add(spawnedBoss);

            if (logSpawnDetails)
                Debug.Log($"👑 Spawned boss {bossPrefab.name} at {bossSpawnPos}");
        }

        // Optionally spawn some minions for higher difficulty
        if (difficultyLevel > 2 && normalEnemyPrefabs.Count > 0)
        {
            int minionCount = Mathf.Min(2, difficultyLevel / 3);

            for (int i = 0; i < minionCount; i++)
            {
                GameObject minionPrefab = normalEnemyPrefabs[Random.Range(0, normalEnemyPrefabs.Count)];
                Vector3 minionSpawnPos = GetValidSpawnPosition();

                if (minionSpawnPos != Vector3.zero)
                {
                    GameObject spawnedMinion = Instantiate(minionPrefab, minionSpawnPos, Quaternion.identity);
                    spawnedMinion.name = $"Minion_{minionPrefab.name}_D{difficultyLevel}_{i + 1}";
                    spawnedEnemies.Add(spawnedMinion);

                    if (logSpawnDetails)
                        Debug.Log($"🦇 Spawned minion {minionPrefab.name} at {minionSpawnPos}");
                }
            }
        }
    }

    /// <summary>
    /// Calculate number of enemies based on difficulty
    /// </summary>
    private int CalculateEnemyCount(int difficultyLevel)
    {
        float difficultyBonus = difficultyLevel * difficultyScaling;
        int totalCount = baseEnemyCount + Mathf.RoundToInt(difficultyBonus);

        // Cap at max
        return Mathf.Min(totalCount, baseEnemyCount + maxDifficultyBonus);
    }

    /// <summary>
    /// Get valid spawn position avoiding obstacles and player
    /// </summary>
    private Vector3 GetValidSpawnPosition()
    {
        // Try predefined spawn points first
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            List<Transform> validSpawnPoints = new List<Transform>();

            foreach (Transform point in spawnPoints)
            {
                if (point != null && IsValidSpawnPosition(point.position))
                {
                    validSpawnPoints.Add(point);
                }
            }

            if (validSpawnPoints.Count > 0)
            {
                return validSpawnPoints[Random.Range(0, validSpawnPoints.Count)].position;
            }
        }

        // Try random positions within room bounds
        for (int attempts = 0; attempts < maxSpawnAttempts; attempts++)
        {
            Vector3 randomPos = GetRandomPositionInRoom();

            if (IsValidSpawnPosition(randomPos))
            {
                return randomPos;
            }
        }

        return Vector3.zero; // Failed to find valid position
    }

    /// <summary>
    /// Get boss spawn position (center or designated point)
    /// </summary>
    private Vector3 GetBossSpawnPosition()
    {
        // Look for designated boss spawn point
        Transform bossSpawn = transform.Find("BossSpawn");
        if (bossSpawn != null)
        {
            return bossSpawn.position;
        }

        // Default to room center
        if (roomBounds != null)
        {
            return roomBounds.bounds.center;
        }

        return transform.position;
    }

    /// <summary>
    /// Get random position within room bounds
    /// </summary>
    private Vector3 GetRandomPositionInRoom()
    {
        if (roomBounds != null)
        {
            Bounds bounds = roomBounds.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector3(x, y, 0f);
        }

        // Fallback to area around spawner
        Vector2 randomOffset = Random.insideUnitCircle * 10f;
        return transform.position + (Vector3)randomOffset;
    }

    /// <summary>
    /// Check if spawn position is valid
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position)
    {
        // Check for obstacles
        Collider2D obstacle = Physics2D.OverlapCircle(position, obstacleCheckRadius, obstacleLayer);
        if (obstacle != null)
        {
            return false;
        }

        // Check distance from player
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(position, playerTransform.position);
            if (distanceToPlayer < minDistanceFromPlayer)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Clear all spawned enemies
    /// </summary>
    public void ClearSpawnedEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                DestroyImmediate(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    /// <summary>
    /// Get count of alive enemies
    /// </summary>
    public int GetAliveEnemyCount()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count;
    }

    /// <summary>
    /// Check if all enemies are defeated
    /// </summary>
    public bool AreAllEnemiesDefeated()
    {
        return GetAliveEnemyCount() == 0;
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        // Draw room bounds
        if (roomBounds != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(roomBounds.bounds.center, roomBounds.bounds.size);
        }

        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.green;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }

        // Draw obstacle check radius
        Gizmos.color = Color.red;
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, obstacleCheckRadius);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Spawn Normal Enemies")]
    public void TestSpawnNormalEnemies()
    {
        SpawnNormalEnemies(1);
    }

    [ContextMenu("Test Spawn Boss Enemies")]
    public void TestSpawnBossEnemies()
    {
        SpawnBossEnemies(1);
    }

    [ContextMenu("Clear All Enemies")]
    public void TestClearEnemies()
    {
        ClearSpawnedEnemies();
    }
#endif
}