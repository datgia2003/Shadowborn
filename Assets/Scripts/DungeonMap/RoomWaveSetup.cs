using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Auto-setup script for Wave-based enemy spawning in room prefabs
/// Helps create and configure wave zones in long rooms
/// </summary>
public class RoomWaveSetup : MonoBehaviour
{
    [Header("🌊 Wave Room Configuration")]
    [Tooltip("Room type (Normal/Boss) - auto-detected if empty")]
    public string roomType = "";

    [Tooltip("Use wave-based spawning instead of instant spawning")]
    public bool useWaveSpawning = true;

    [Header("🎯 Auto Wave Generation")]
    [Tooltip("Automatically create wave zones based on room length")]
    public bool autoCreateWaveZones = true;

    [Tooltip("Number of wave zones to create")]
    [Range(1, 10)]
    public int numberOfWaves = 3;

    [Tooltip("Wave progression mode")]
    public EnemyWaveManager.WaveProgressionMode progressionMode = EnemyWaveManager.WaveProgressionMode.PlayerTriggered;

    [Header("📏 Wave Zone Layout")]
    [Tooltip("Wave zone width (auto-calculated if 0)")]
    public float waveZoneWidth = 0f;

    [Tooltip("Wave zone height (auto-calculated if 0)")]
    public float waveZoneHeight = 0f;

    [Tooltip("Overlap between wave zones")]
    [Range(0f, 0.5f)]
    public float waveZoneOverlap = 0.1f;

    [Tooltip("Margin from room edges")]
    public float roomMargin = 2f;

    [Header("⚙️ Wave Settings")]
    [Tooltip("Base enemies per wave")]
    public int baseEnemiesPerWave = 2;

    [Tooltip("Enemy count variation per wave")]
    public int enemyCountVariation = 1;

    [Tooltip("Difficulty multiplier progression per wave")]
    public float difficultyProgression = 0.1f;

    [Header("🔧 Debug")]
    public bool showSetupLogs = true;
    public bool showWaveGizmos = true;

    // Components
    private EnemyWaveManager waveManager;
    private EnemySpawner enemySpawner;
    private BoxCollider2D roomBounds;

    void Awake()
    {
        if (useWaveSpawning)
        {
            SetupWaveSystem();
        }
    }

    /// <summary>
    /// Setup complete wave system for this room
    /// </summary>
    void SetupWaveSystem()
    {
        // Get room bounds
        roomBounds = GetComponent<BoxCollider2D>();
        if (roomBounds == null)
        {
            roomBounds = GetComponentInChildren<BoxCollider2D>();
        }

        if (roomBounds == null)
        {
            Debug.LogError($"❌ RoomWaveSetup: No BoxCollider2D found for room bounds in {gameObject.name}");
            return;
        }

        // Auto-detect room type
        if (string.IsNullOrEmpty(roomType))
        {
            string roomName = gameObject.name.ToLower();
            roomType = roomName.Contains("boss") ? "Boss" : "Normal";
        }

        // Setup wave manager
        SetupWaveManager();

        // Setup enemy spawner (needed by wave zones)
        SetupEnemySpawner();

        // Create wave zones if auto-create is enabled
        if (autoCreateWaveZones)
        {
            CreateWaveZones();
        }

        if (showSetupLogs)
        {
            Debug.Log($"🌊 Wave system setup complete for {roomType} room: {gameObject.name}");
        }
    }

    /// <summary>
    /// Setup wave manager component
    /// </summary>
    void SetupWaveManager()
    {
        waveManager = GetComponent<EnemyWaveManager>();
        if (waveManager == null)
        {
            waveManager = gameObject.AddComponent<EnemyWaveManager>();
        }

        // Configure wave manager
        waveManager.progressionMode = progressionMode;
        waveManager.showDebugInfo = showSetupLogs;
        waveManager.showWaveGizmos = showWaveGizmos;

        // Adjust settings based on room type
        if (roomType == "Boss")
        {
            waveManager.completionAction = EnemyWaveManager.WaveCompletionAction.OpenExit;
            waveManager.waveCompletionRewardMultiplier = 2f;
        }
        else
        {
            waveManager.completionAction = EnemyWaveManager.WaveCompletionAction.OpenExit;
            waveManager.waveCompletionRewardMultiplier = 1.5f;
        }
    }

    /// <summary>
    /// Setup enemy spawner component
    /// </summary>
    void SetupEnemySpawner()
    {
        enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner == null)
        {
            enemySpawner = gameObject.AddComponent<EnemySpawner>();
        }

        // Note: EnemySpawner properties are private, so we rely on default values
        // Wave zones will handle specific spawning configuration

        if (showSetupLogs)
        {
            Debug.Log($"🎯 Enemy spawner ready for wave system in {gameObject.name}");
        }
    }

    /// <summary>
    /// Create wave zones automatically based on room layout
    /// </summary>
    void CreateWaveZones()
    {
        if (roomBounds == null) return;

        // Clear existing wave zones if any
        EnemyWaveZone[] existingZones = GetComponentsInChildren<EnemyWaveZone>();
        foreach (var zone in existingZones)
        {
            if (Application.isPlaying)
                Destroy(zone.gameObject);
            else
                DestroyImmediate(zone.gameObject);
        }

        // Create wave zones parent
        GameObject waveZonesParent = new GameObject("WaveZones");
        waveZonesParent.transform.SetParent(transform);

        Bounds bounds = roomBounds.bounds;

        // Calculate zone dimensions
        float zoneWidth = waveZoneWidth > 0 ? waveZoneWidth : (bounds.size.x - roomMargin * 2) / numberOfWaves;
        float zoneHeight = waveZoneHeight > 0 ? waveZoneHeight : bounds.size.y - roomMargin * 2;

        // Calculate step size with overlap
        float stepSize = zoneWidth * (1f - waveZoneOverlap);

        // Create zones
        for (int i = 0; i < numberOfWaves; i++)
        {
            CreateWaveZone(i, waveZonesParent.transform, bounds, zoneWidth, zoneHeight, stepSize);
        }

        if (showSetupLogs)
        {
            Debug.Log($"📍 Created {numberOfWaves} wave zones in {gameObject.name}");
        }
    }

    /// <summary>
    /// Create individual wave zone
    /// </summary>
    void CreateWaveZone(int waveIndex, Transform parent, Bounds roomBounds, float zoneWidth, float zoneHeight, float stepSize)
    {
        // Create wave zone object
        GameObject waveZoneObj = new GameObject($"WaveZone_{waveIndex + 1:D2}");
        waveZoneObj.transform.SetParent(parent);

        // Position zone
        float startX = roomBounds.min.x + roomMargin + (zoneWidth * 0.5f);
        float posX = startX + (stepSize * waveIndex);
        float posY = roomBounds.center.y;

        waveZoneObj.transform.position = new Vector3(posX, posY, 0);

        // Add and configure wave zone component
        EnemyWaveZone waveZone = waveZoneObj.AddComponent<EnemyWaveZone>();
        waveZone.waveId = waveIndex + 1;
        waveZone.waveName = $"Wave {waveIndex + 1}";
        waveZone.triggerOnPlayerEnter = (progressionMode == EnemyWaveManager.WaveProgressionMode.PlayerTriggered);

        // Configure enemy count with variation
        int enemyCount = baseEnemiesPerWave + Random.Range(-enemyCountVariation, enemyCountVariation + 1);
        waveZone.enemyCount = Mathf.Max(1, enemyCount);

        // Apply difficulty progression
        waveZone.difficultyMultiplier = 1f + (waveIndex * difficultyProgression);

        // Boss wave settings
        if (roomType == "Boss" && waveIndex == numberOfWaves - 1)
        {
            waveZone.spawnBossEnemies = true;
            waveZone.enemyCount = 1; // Boss + possible minions
            waveZone.difficultyMultiplier *= 1.5f;
        }

        // Add trigger collider
        BoxCollider2D zoneCollider = waveZoneObj.AddComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;
        zoneCollider.size = new Vector2(zoneWidth, zoneHeight);

        // Configure wave zone settings
        waveZone.randomSpawnInBounds = true;
        waveZone.spawnMargin = 1f;
        waveZone.showDebugInfo = showSetupLogs;
        waveZone.showGizmos = showWaveGizmos;
    }

    /// <summary>
    /// Get wave zones in this room
    /// </summary>
    public EnemyWaveZone[] GetWaveZones()
    {
        return GetComponentsInChildren<EnemyWaveZone>();
    }

    /// <summary>
    /// Get wave manager
    /// </summary>
    public EnemyWaveManager GetWaveManager()
    {
        return waveManager;
    }

    void OnDrawGizmosSelected()
    {
        if (!showWaveGizmos) return;

        // Draw room bounds
        if (roomBounds != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(roomBounds.bounds.center, roomBounds.bounds.size);
        }

        // Draw planned wave zones
        if (autoCreateWaveZones && roomBounds != null && !Application.isPlaying)
        {
            DrawPlannedWaveZones();
        }
    }

    /// <summary>
    /// Draw planned wave zone positions in editor
    /// </summary>
    void DrawPlannedWaveZones()
    {
        Bounds bounds = roomBounds.bounds;

        float zoneWidth = waveZoneWidth > 0 ? waveZoneWidth : (bounds.size.x - roomMargin * 2) / numberOfWaves;
        float zoneHeight = waveZoneHeight > 0 ? waveZoneHeight : bounds.size.y - roomMargin * 2;
        float stepSize = zoneWidth * (1f - waveZoneOverlap);

        Gizmos.color = Color.yellow * 0.7f;

        for (int i = 0; i < numberOfWaves; i++)
        {
            float startX = bounds.min.x + roomMargin + (zoneWidth * 0.5f);
            float posX = startX + (stepSize * i);
            float posY = bounds.center.y;

            Vector3 zonePos = new Vector3(posX, posY, 0);
            Vector3 zoneSize = new Vector3(zoneWidth, zoneHeight, 1f);

            Gizmos.DrawWireCube(zonePos, zoneSize);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(zonePos + Vector3.up * (zoneHeight * 0.6f), $"Wave {i + 1}");
#endif
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Setup Wave System")]
    public void ManualSetupWaveSystem()
    {
        SetupWaveSystem();
    }

    [ContextMenu("Create Wave Zones")]
    public void ManualCreateWaveZones()
    {
        CreateWaveZones();
    }

    [ContextMenu("Clear Wave Zones")]
    public void ClearWaveZones()
    {
        EnemyWaveZone[] zones = GetComponentsInChildren<EnemyWaveZone>();
        foreach (var zone in zones)
        {
            DestroyImmediate(zone.gameObject);
        }

        Transform waveZonesParent = transform.Find("WaveZones");
        if (waveZonesParent != null)
        {
            DestroyImmediate(waveZonesParent.gameObject);
        }

        Debug.Log($"🧹 Cleared wave zones from {gameObject.name}");
    }
#endif
}