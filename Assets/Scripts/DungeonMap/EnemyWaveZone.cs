using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy Wave Zone - defines a trigger area that spawns enemies when player enters
/// Used for long rooms where enemies spawn in segments/waves
/// </summary>
public class EnemyWaveZone : MonoBehaviour
{
    [Header("🌊 Wave Configuration")]
    [Tooltip("Wave ID for identification and ordering")]
    public int waveId = 1;

    [Tooltip("Wave name for debugging")]
    public string waveName = "Wave 1";

    [Tooltip("Trigger this wave when player enters")]
    public bool triggerOnPlayerEnter = true;

    [Tooltip("Auto-trigger this wave after previous wave is cleared")]
    public bool triggerOnPreviousComplete = false;

    [Tooltip("Delay before spawning enemies (seconds)")]
    [Range(0f, 5f)]
    public float spawnDelay = 0.5f;

    [Header("🦇 Enemy Configuration")]
    [Tooltip("Number of enemies to spawn in this wave")]
    public int enemyCount = 3;

    [Tooltip("Override global enemy prefabs for this wave")]
    public GameObject[] customEnemyPrefabs;

    [Tooltip("Use boss enemies for this wave")]
    public bool spawnBossEnemies = false;

    [Tooltip("Enemy difficulty multiplier for this wave")]
    [Range(0.5f, 3f)]
    public float difficultyMultiplier = 1f;

    [Header("📍 Spawn Configuration")]
    [Tooltip("Spawn enemies at specific points (if empty, uses zone bounds)")]
    public Transform[] waveSpawnPoints;

    [Tooltip("Random spawn within zone bounds")]
    public bool randomSpawnInBounds = true;

    [Tooltip("Margin from zone edges for random spawn")]
    public float spawnMargin = 1f;

    [Header("🎯 Trigger Settings")]
    [Tooltip("Player must be in trigger for this duration to activate")]
    [Range(0f, 2f)]
    public float triggerDuration = 0.1f;

    [Tooltip("Only trigger once per room visit")]
    public bool triggerOnce = true;

    [Tooltip("Show warning UI when wave is about to spawn")]
    public bool showWaveWarning = true;

    [Tooltip("Warning display time")]
    public float warningDuration = 1f;

    [Header("⚙️ Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;

    // Private variables
    private bool hasTriggered = false;
    private bool playerInZone = false;
    private float playerInZoneTime = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private BoxCollider2D zoneBounds;
    private EnemyWaveManager waveManager;
    private Coroutine spawnCoroutine;

    // Events
    public System.Action<EnemyWaveZone> OnWaveTriggered;
    public System.Action<EnemyWaveZone> OnWaveCompleted;
    public System.Action<EnemyWaveZone> OnWaveCleared;

    void Awake()
    {
        // Get zone bounds
        zoneBounds = GetComponent<BoxCollider2D>();
        if (zoneBounds == null)
        {
            zoneBounds = gameObject.AddComponent<BoxCollider2D>();
            zoneBounds.isTrigger = true;
            Debug.LogWarning($"⚠️ EnemyWaveZone {waveName}: Added missing BoxCollider2D trigger");
        }

        // Ensure it's a trigger
        zoneBounds.isTrigger = true;

        // Find wave manager
        waveManager = GetComponentInParent<EnemyWaveManager>();
        if (waveManager == null)
        {
            waveManager = FindObjectOfType<EnemyWaveManager>();
        }
    }

    void Start()
    {
        // Register with wave manager
        if (waveManager != null)
        {
            waveManager.RegisterWaveZone(this);
        }

        // Set default spawn points if none assigned
        if (waveSpawnPoints == null || waveSpawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }

        if (showDebugInfo)
        {
            Debug.Log($"🌊 Wave Zone '{waveName}' initialized with {enemyCount} enemies");
        }
    }

    void Update()
    {
        // Handle trigger duration
        if (triggerOnPlayerEnter && playerInZone && !hasTriggered)
        {
            playerInZoneTime += Time.deltaTime;

            if (playerInZoneTime >= triggerDuration)
            {
                TriggerWave();
            }
        }
    }

    /// <summary>
    /// Trigger this wave to spawn enemies
    /// </summary>
    public void TriggerWave()
    {
        if (hasTriggered && triggerOnce)
        {
            if (showDebugInfo)
                Debug.Log($"🌊 Wave {waveName} already triggered, skipping");
            return;
        }

        hasTriggered = true;

        if (showDebugInfo)
            Debug.Log($"🌊 Triggering wave: {waveName} ({enemyCount} enemies)");

        OnWaveTriggered?.Invoke(this);

        // Start spawn coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
    }

    /// <summary>
    /// Force trigger this wave regardless of conditions
    /// </summary>
    public void ForceTrigger()
    {
        bool originalTriggerOnce = triggerOnce;
        triggerOnce = false;
        TriggerWave();
        triggerOnce = originalTriggerOnce;
    }

    /// <summary>
    /// Coroutine to handle wave spawning with delays and effects
    /// </summary>
    private IEnumerator SpawnWaveCoroutine()
    {
        // Show warning if enabled
        if (showWaveWarning)
        {
            ShowWaveWarning();
            yield return new WaitForSeconds(warningDuration);
        }

        // Spawn delay
        if (spawnDelay > 0f)
        {
            yield return new WaitForSeconds(spawnDelay);
        }

        // Spawn enemies
        yield return StartCoroutine(SpawnEnemiesCoroutine());

        // Wave completed
        OnWaveCompleted?.Invoke(this);

        if (showDebugInfo)
            Debug.Log($"✅ Wave {waveName} spawn completed. {spawnedEnemies.Count} enemies spawned.");
    }

    /// <summary>
    /// Spawn enemies for this wave
    /// </summary>
    private IEnumerator SpawnEnemiesCoroutine()
    {
        EnemySpawner spawner = GetEnemySpawner();
        if (spawner == null)
        {
            Debug.LogError($"❌ EnemyWaveZone {waveName}: No EnemySpawner found!");
            yield break;
        }

        // Clear previous enemies
        ClearWaveEnemies();

        // Calculate difficulty
        int currentDifficulty = GetCurrentDifficulty();
        int waveDifficulty = Mathf.RoundToInt(currentDifficulty * difficultyMultiplier);

        // Spawn enemies one by one with small delays
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = GetSpawnPosition(i);
            GameObject enemyPrefab = SelectEnemyPrefab(waveDifficulty);

            if (enemyPrefab != null && spawnPosition != Vector3.zero)
            {
                GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                spawnedEnemy.name = $"{waveName}_{enemyPrefab.name}_{i + 1}";

                // Apply difficulty scaling
                ApplyWaveScaling(spawnedEnemy, waveDifficulty);

                spawnedEnemies.Add(spawnedEnemy);

                if (showDebugInfo)
                    Debug.Log($"🦇 Spawned {enemyPrefab.name} at {spawnPosition} for wave {waveName}");

                // Small delay between spawns for visual effect
                if (i < enemyCount - 1)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }

    /// <summary>
    /// Get spawn position for enemy index
    /// </summary>
    private Vector3 GetSpawnPosition(int enemyIndex)
    {
        // Use specific spawn points if available
        if (waveSpawnPoints != null && waveSpawnPoints.Length > 0)
        {
            int pointIndex = enemyIndex % waveSpawnPoints.Length;
            if (waveSpawnPoints[pointIndex] != null)
            {
                return waveSpawnPoints[pointIndex].position;
            }
        }

        // Random spawn within bounds
        if (randomSpawnInBounds && zoneBounds != null)
        {
            return GetRandomSpawnPositionInBounds();
        }

        // Fallback to zone center
        return transform.position;
    }

    /// <summary>
    /// Get random spawn position within zone bounds
    /// </summary>
    private Vector3 GetRandomSpawnPositionInBounds()
    {
        if (zoneBounds == null) return transform.position;

        Bounds bounds = zoneBounds.bounds;
        float margin = spawnMargin;

        float x = Random.Range(bounds.min.x + margin, bounds.max.x - margin);
        float y = Random.Range(bounds.min.y + margin, bounds.max.y - margin);

        return new Vector3(x, y, transform.position.z);
    }

    /// <summary>
    /// Select enemy prefab for this wave
    /// </summary>
    private GameObject SelectEnemyPrefab(int difficulty)
    {
        // Use custom prefabs if assigned
        if (customEnemyPrefabs != null && customEnemyPrefabs.Length > 0)
        {
            return customEnemyPrefabs[Random.Range(0, customEnemyPrefabs.Length)];
        }

        // Use enemy spawner's selection
        EnemySpawner spawner = GetEnemySpawner();
        if (spawner != null && spawner.enemyConfig != null)
        {
            if (spawnBossEnemies)
            {
                var bossData = spawner.enemyConfig.GetRandomBoss(difficulty);
                return bossData?.prefab;
            }
            else
            {
                var enemyData = spawner.enemyConfig.GetRandomNormalEnemy(difficulty);
                return enemyData?.prefab;
            }
        }

        return null;
    }

    /// <summary>
    /// Apply wave-specific scaling to enemy
    /// </summary>
    private void ApplyWaveScaling(GameObject enemy, int waveDifficulty)
    {
        // Add wave info to enemy
        var waveInfo = enemy.AddComponent<EnemyWaveInfo>();
        waveInfo.waveZone = this;
        waveInfo.waveName = waveName;
        waveInfo.waveId = waveId;
        waveInfo.waveDifficulty = waveDifficulty;

        // Apply difficulty scaling
        var scaling = enemy.GetComponent<EnemyDifficultyScaling>();
        if (scaling == null)
        {
            scaling = enemy.AddComponent<EnemyDifficultyScaling>();
        }

        float healthMultiplier = 1f + (waveDifficulty - 1) * 0.1f * difficultyMultiplier;
        float damageMultiplier = 1f + (waveDifficulty - 1) * 0.1f * difficultyMultiplier;

        scaling.SetScaling(waveDifficulty, healthMultiplier, damageMultiplier);
    }

    /// <summary>
    /// Check if wave is cleared (all enemies defeated)
    /// </summary>
    public bool IsWaveCleared()
    {
        // Remove null references
        spawnedEnemies.RemoveAll(enemy => enemy == null);

        bool cleared = spawnedEnemies.Count == 0;

        if (cleared && hasTriggered)
        {
            OnWaveCleared?.Invoke(this);
            if (showDebugInfo)
                Debug.Log($"🏆 Wave {waveName} cleared!");
        }

        return cleared;
    }

    /// <summary>
    /// Get count of alive enemies in this wave
    /// </summary>
    public int GetAliveEnemyCount()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count;
    }

    /// <summary>
    /// Clear all enemies spawned by this wave
    /// </summary>
    public void ClearWaveEnemies()
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
    /// Reset wave to be triggered again
    /// </summary>
    public void ResetWave()
    {
        hasTriggered = false;
        playerInZone = false;
        playerInZoneTime = 0f;
        ClearWaveEnemies();

        if (showDebugInfo)
            Debug.Log($"🔄 Wave {waveName} reset");
    }

    /// <summary>
    /// Show wave warning UI
    /// </summary>
    private void ShowWaveWarning()
    {
        // TODO: Implement wave warning UI
        // For now, just log
        if (showDebugInfo)
            Debug.Log($"⚠️ Wave incoming: {waveName}!");
    }

    /// <summary>
    /// Create default spawn points if none assigned
    /// </summary>
    private void CreateDefaultSpawnPoints()
    {
        if (zoneBounds == null) return;

        // Create 4 spawn points at zone corners
        List<Transform> points = new List<Transform>();
        Bounds bounds = zoneBounds.bounds;

        Vector3[] positions = new Vector3[]
        {
            new Vector3(bounds.min.x + spawnMargin, bounds.min.y + spawnMargin, 0),
            new Vector3(bounds.max.x - spawnMargin, bounds.min.y + spawnMargin, 0),
            new Vector3(bounds.min.x + spawnMargin, bounds.max.y - spawnMargin, 0),
            new Vector3(bounds.max.x - spawnMargin, bounds.max.y - spawnMargin, 0)
        };

        GameObject spawnPointsParent = new GameObject($"{waveName}_SpawnPoints");
        spawnPointsParent.transform.SetParent(transform);

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = new GameObject($"SpawnPoint_{i + 1}");
            point.transform.SetParent(spawnPointsParent.transform);
            point.transform.position = positions[i];
            points.Add(point.transform);
        }

        waveSpawnPoints = points.ToArray();
    }

    /// <summary>
    /// Get current difficulty level
    /// </summary>
    private int GetCurrentDifficulty()
    {
        if (RoomManager.Instance != null)
        {
            return RoomManager.Instance.GetCurrentDifficulty();
        }
        return 1;
    }

    /// <summary>
    /// Get enemy spawner reference
    /// </summary>
    private EnemySpawner GetEnemySpawner()
    {
        EnemySpawner spawner = GetComponentInParent<EnemySpawner>();
        if (spawner == null)
        {
            spawner = FindObjectOfType<EnemySpawner>();
        }
        return spawner;
    }

    // Trigger events
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            playerInZoneTime = 0f;

            if (showDebugInfo)
                Debug.Log($"👤 Player entered wave zone: {waveName}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            playerInZoneTime = 0f;

            if (showDebugInfo)
                Debug.Log($"👤 Player exited wave zone: {waveName}");
        }
    }

    // Gizmos for visualization
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw zone bounds
        if (zoneBounds != null)
        {
            Gizmos.color = hasTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(zoneBounds.bounds.center, zoneBounds.bounds.size);

            // Draw zone label
            Vector3 labelPos = zoneBounds.bounds.center + Vector3.up * (zoneBounds.bounds.size.y * 0.6f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"Wave {waveId}: {waveName}");
#endif
        }

        // Draw spawn points
        if (waveSpawnPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Transform point in waveSpawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.3f);
                }
            }
        }

        // Draw spawned enemies
        Gizmos.color = Color.red;
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw spawn margin
        if (zoneBounds != null)
        {
            Gizmos.color = Color.green * 0.3f;
            Vector3 marginSize = zoneBounds.bounds.size - Vector3.one * spawnMargin * 2;
            if (marginSize.x > 0 && marginSize.y > 0)
            {
                Gizmos.DrawWireCube(zoneBounds.bounds.center, marginSize);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Trigger Wave")]
    public void TestTrigger()
    {
        ForceTrigger();
    }

    [ContextMenu("Reset Wave")]
    public void TestReset()
    {
        ResetWave();
    }

    [ContextMenu("Clear Wave Enemies")]
    public void TestClearEnemies()
    {
        ClearWaveEnemies();
    }
#endif
}