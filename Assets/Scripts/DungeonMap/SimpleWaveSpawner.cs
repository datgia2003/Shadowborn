using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Simple Wave Spawner - Spawn enemies theo từng đợt khi player trigger zones
/// Mỗi wave có nhiều spawn points, random chọn vài cái để spawn
/// </summary>
public class SimpleWaveSpawner : MonoBehaviour
{
    [Header("🌊 Wave Configuration")]
    [Tooltip("Enemy prefabs to spawn")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Tooltip("Number of enemies per wave")]
    public int enemiesPerWave = 3;

    [Tooltip("Difficulty multiplier per wave")]
    public float difficultyIncrease = 0.2f;

    [Header("📍 Spawn Points")]
    [Tooltip("All possible spawn points for this wave")]
    public Transform[] spawnPoints;

    [Tooltip("How many spawn points to use per wave (random selection)")]
    public int spawnPointsToUse = 2;

    [Header("⚙️ Settings")]
    [Tooltip("Player tag to detect")]
    public string playerTag = "Player";

    [Tooltip("Has this wave been triggered?")]
    [SerializeField] private bool isTriggered = false;

    [Tooltip("Current wave number")]
    [SerializeField] private int currentWave = 1;

    private BoxCollider2D triggerZone;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform playerTransform;

    void Awake()
    {
        triggerZone = GetComponent<BoxCollider2D>();
        if (triggerZone == null)
        {
            triggerZone = gameObject.AddComponent<BoxCollider2D>();
        }
        triggerZone.isTrigger = true;

        // Find player
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isTriggered && other.CompareTag(playerTag))
        {
            TriggerWave();
        }
    }

    /// <summary>
    /// Trigger spawn wave
    /// </summary>
    public void TriggerWave()
    {
        if (isTriggered) return;

        isTriggered = true;
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Spawn enemies for this wave
    /// </summary>
    IEnumerator SpawnWave()
    {
        Debug.Log($"🌊 Triggering Wave {currentWave} - Spawning {enemiesPerWave} enemies");

        // Get random spawn points
        List<Transform> selectedSpawnPoints = GetRandomSpawnPoints();

        if (selectedSpawnPoints.Count == 0)
        {
            Debug.LogError("❌ No spawn points available!");
            yield break;
        }

        // Calculate difficulty
        float difficultyMultiplier = 1f + ((currentWave - 1) * difficultyIncrease);

        // Calculate enemies per spawn point
        int enemiesPerSpawnPoint = Mathf.CeilToInt((float)enemiesPerWave / selectedSpawnPoints.Count);
        int totalSpawned = 0;

        // Spawn enemies at each selected spawn point
        for (int pointIndex = 0; pointIndex < selectedSpawnPoints.Count && totalSpawned < enemiesPerWave; pointIndex++)
        {
            Transform spawnPoint = selectedSpawnPoints[pointIndex];

            // Determine how many enemies to spawn at this point
            int enemiesToSpawnHere = Mathf.Min(enemiesPerSpawnPoint, enemiesPerWave - totalSpawned);

            for (int enemyIndex = 0; enemyIndex < enemiesToSpawnHere; enemyIndex++)
            {
                // Random enemy prefab
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

                // Spawn position around spawn point (circular distribution)
                float angle = (float)enemyIndex / enemiesToSpawnHere * 360f * Mathf.Deg2Rad;
                float radius = 1f + (enemyIndex * 0.3f); // Increasing radius per enemy
                Vector3 spawnPosition = spawnPoint.position + new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );

                // Spawn enemy
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, spawnPoint.rotation);

                // Set player reference for enemy AI
                SetEnemyPlayerTarget(enemy);

                // Apply difficulty scaling
                ApplyDifficultyScaling(enemy, difficultyMultiplier);

                // Track spawned enemy
                spawnedEnemies.Add(enemy);

                Debug.Log($"🦇 Spawned {enemy.name} at {spawnPoint.name} (difficulty: {difficultyMultiplier:F2}x)");

                totalSpawned++;

                // Small delay between spawns
                yield return new WaitForSeconds(0.1f);
            }

            // Slightly longer delay between spawn points
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log($"✅ Wave {currentWave} spawn complete! Total spawned: {totalSpawned}");
    }

    /// <summary>
    /// Get random selection of spawn points
    /// </summary>
    List<Transform> GetRandomSpawnPoints()
    {
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        List<Transform> selectedPoints = new List<Transform>();

        int pointsToSelect = Mathf.Min(spawnPointsToUse, availablePoints.Count);

        for (int i = 0; i < pointsToSelect; i++)
        {
            if (availablePoints.Count == 0) break;

            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        return selectedPoints;
    }

    /// <summary>
    /// Set player target for enemy AI
    /// </summary>
    void SetEnemyPlayerTarget(GameObject enemy)
    {
        if (playerTransform == null) return;

        // Try BatController specifically
        BatController batController = enemy.GetComponent<BatController>();
        if (batController != null)
        {
            batController.player = playerTransform;
            Debug.Log($"🎯 Set player target for BatController");
            return;
        }

        // Try other common enemy AI component names
        MonoBehaviour[] components = enemy.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in components)
        {
            // Try to set player target using reflection (works for most AI scripts)
            var targetField = component.GetType().GetField("target");
            if (targetField != null && targetField.FieldType == typeof(Transform))
            {
                targetField.SetValue(component, playerTransform);
                Debug.Log($"🎯 Set player target for {component.GetType().Name}");
                return;
            }

            var playerTargetField = component.GetType().GetField("playerTarget");
            if (playerTargetField != null && playerTargetField.FieldType == typeof(Transform))
            {
                playerTargetField.SetValue(component, playerTransform);
                Debug.Log($"🎯 Set player target for {component.GetType().Name}");
                return;
            }

            var playerField = component.GetType().GetField("player");
            if (playerField != null && playerField.FieldType == typeof(Transform))
            {
                playerField.SetValue(component, playerTransform);
                Debug.Log($"🎯 Set player target for {component.GetType().Name}");
                return;
            }

            // Try property instead of field
            var targetProperty = component.GetType().GetProperty("Target");
            if (targetProperty != null && targetProperty.PropertyType == typeof(Transform) && targetProperty.CanWrite)
            {
                targetProperty.SetValue(component, playerTransform);
                Debug.Log($"🎯 Set player target for {component.GetType().Name}");
                return;
            }
        }

        Debug.LogWarning($"⚠️ Could not find target field for {enemy.name}. Enemy may not follow player.");
    }

    /// <summary>
    /// Apply difficulty scaling to spawned enemy
    /// </summary>
    void ApplyDifficultyScaling(GameObject enemy, float multiplier)
    {
        if (multiplier <= 1f) return;

        // Add difficulty scaling component
        EnemyDifficultyScaling scaling = enemy.GetComponent<EnemyDifficultyScaling>();
        if (scaling == null)
        {
            scaling = enemy.AddComponent<EnemyDifficultyScaling>();
        }

        // Set scaling values
        scaling.SetScaling(currentWave, multiplier, multiplier);
    }

    /// <summary>
    /// Reset wave (for testing)
    /// </summary>
    [ContextMenu("Reset Wave")]
    public void ResetWave()
    {
        isTriggered = false;

        // Clean up spawned enemies
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
                DestroyImmediate(enemy);
        }
        spawnedEnemies.Clear();

        Debug.Log($"🔄 Reset Wave {currentWave}");
    }

    /// <summary>
    /// Check if all enemies are defeated
    /// </summary>
    public bool AreAllEnemiesDefeated()
    {
        spawnedEnemies.RemoveAll(enemy => enemy == null);
        return spawnedEnemies.Count == 0;
    }

    /// <summary>
    /// Check if this wave has been triggered
    /// </summary>
    public bool IsTriggered()
    {
        return isTriggered;
    }

    void OnDrawGizmosSelected()
    {
        // Draw trigger zone
        if (triggerZone != null)
        {
            Gizmos.color = isTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position + (Vector3)triggerZone.offset, triggerZone.size);
        }

        // Draw spawn points
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(spawnPoints[i].position, 0.5f);

#if UNITY_EDITOR
                    UnityEditor.Handles.Label(spawnPoints[i].position + Vector3.up, $"Spawn {i + 1}");
#endif
                }
            }
        }

        // Draw wave info
#if UNITY_EDITOR
        string waveInfo = $"Wave {currentWave}\n{enemiesPerWave} enemies\nDifficulty: {1f + ((currentWave - 1) * difficultyIncrease):F1}x";
        UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, waveInfo);
#endif
    }
}