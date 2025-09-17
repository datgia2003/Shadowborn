using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Enemy Configuration - ScriptableObject for managing enemy prefabs and spawn settings
/// Create via: Assets > Create > Shadowborn > Enemy Configuration
/// </summary>
[CreateAssetMenu(fileName = "EnemyConfiguration", menuName = "Shadowborn/Enemy Configuration")]
public class EnemyConfiguration : ScriptableObject
{
    [System.Serializable]
    public class EnemyData
    {
        public GameObject prefab;
        public string enemyName;
        public int difficultyRequirement = 1; // Min difficulty to spawn
        public float spawnWeight = 1f; // Higher = more likely to spawn
        public bool canSpawnInBossRoom = false;

        [Header("Scaling")]
        public bool scalesWithDifficulty = true;
        public float healthScaling = 0.1f; // 10% per difficulty level
        public float damageScaling = 0.1f;
    }

    [System.Serializable]
    public class BossData
    {
        public GameObject prefab;
        public string bossName;
        public int difficultyRequirement = 1;
        public float spawnWeight = 1f;

        [Header("Boss Specific")]
        public bool spawnsMinions = true;
        public int maxMinions = 2;
        public float minionSpawnDelay = 5f;
    }

    [Header("🦇 Normal Enemies")]
    [Tooltip("List of normal enemy configurations")]
    public List<EnemyData> normalEnemies = new List<EnemyData>();

    [Header("👑 Boss Enemies")]
    [Tooltip("List of boss configurations")]
    public List<BossData> bossEnemies = new List<BossData>();

    [Header("📊 Global Spawn Settings")]
    [Tooltip("Base enemy count for normal rooms")]
    public int baseEnemyCount = 3;

    [Tooltip("Max additional enemies from difficulty")]
    public int maxDifficultyBonus = 5;

    [Tooltip("Enemies per difficulty level")]
    public float enemiesPerDifficulty = 0.5f;

    [Header("🎯 Spawn Constraints")]
    [Tooltip("Min distance between enemy spawns")]
    public float minSpawnDistance = 2f;

    [Tooltip("Min distance from player spawn")]
    public float minPlayerDistance = 5f;

    [Tooltip("Max spawn attempts per enemy")]
    public int maxSpawnAttempts = 20;

    /// <summary>
    /// Get available normal enemies for difficulty level
    /// </summary>
    public List<EnemyData> GetAvailableNormalEnemies(int difficultyLevel)
    {
        List<EnemyData> available = new List<EnemyData>();

        foreach (var enemy in normalEnemies)
        {
            if (enemy.prefab != null && enemy.difficultyRequirement <= difficultyLevel)
            {
                available.Add(enemy);
            }
        }

        return available;
    }

    /// <summary>
    /// Get available boss enemies for difficulty level
    /// </summary>
    public List<BossData> GetAvailableBosses(int difficultyLevel)
    {
        List<BossData> available = new List<BossData>();

        foreach (var boss in bossEnemies)
        {
            if (boss.prefab != null && boss.difficultyRequirement <= difficultyLevel)
            {
                available.Add(boss);
            }
        }

        return available;
    }

    /// <summary>
    /// Select random normal enemy based on spawn weights
    /// </summary>
    public EnemyData GetRandomNormalEnemy(int difficultyLevel)
    {
        var available = GetAvailableNormalEnemies(difficultyLevel);
        if (available.Count == 0) return null;

        // Weighted selection
        float totalWeight = 0f;
        foreach (var enemy in available)
        {
            totalWeight += enemy.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var enemy in available)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemy;
            }
        }

        // Fallback
        return available[Random.Range(0, available.Count)];
    }

    /// <summary>
    /// Select random boss based on spawn weights
    /// </summary>
    public BossData GetRandomBoss(int difficultyLevel)
    {
        var available = GetAvailableBosses(difficultyLevel);
        if (available.Count == 0) return null;

        // Weighted selection
        float totalWeight = 0f;
        foreach (var boss in available)
        {
            totalWeight += boss.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var boss in available)
        {
            currentWeight += boss.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return boss;
            }
        }

        // Fallback
        return available[Random.Range(0, available.Count)];
    }

    /// <summary>
    /// Calculate enemy count for difficulty level
    /// </summary>
    public int CalculateEnemyCount(int difficultyLevel)
    {
        float difficultyBonus = difficultyLevel * enemiesPerDifficulty;
        int totalCount = baseEnemyCount + Mathf.RoundToInt(difficultyBonus);
        return Mathf.Min(totalCount, baseEnemyCount + maxDifficultyBonus);
    }

#if UNITY_EDITOR
    [Header("🛠️ Editor Tools")]
    [Tooltip("Auto-populate from prefabs in project")]
    public bool scanProjectForEnemies = false;

    void OnValidate()
    {
        if (scanProjectForEnemies)
        {
            ScanForEnemyPrefabs();
            scanProjectForEnemies = false;
        }
    }

    void ScanForEnemyPrefabs()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                string prefabName = prefab.name.ToLower();

                // Check if it's an enemy
                if (prefabName.Contains("enemy") || prefabName.Contains("bat") ||
                    prefabName.Contains("skeleton") || prefabName.Contains("spider"))
                {
                    // Check if already exists
                    bool exists = normalEnemies.Exists(e => e.prefab == prefab);

                    if (!exists)
                    {
                        EnemyData newEnemy = new EnemyData
                        {
                            prefab = prefab,
                            enemyName = prefab.name,
                            difficultyRequirement = 1,
                            spawnWeight = 1f,
                            scalesWithDifficulty = true
                        };

                        normalEnemies.Add(newEnemy);
                        Debug.Log($"Added enemy: {prefab.name}");
                    }
                }

                // Check if it's a boss
                if (prefabName.Contains("boss") || prefabName.Contains("igris"))
                {
                    bool exists = bossEnemies.Exists(b => b.prefab == prefab);

                    if (!exists)
                    {
                        BossData newBoss = new BossData
                        {
                            prefab = prefab,
                            bossName = prefab.name,
                            difficultyRequirement = 1,
                            spawnWeight = 1f,
                            spawnsMinions = true
                        };

                        bossEnemies.Add(newBoss);
                        Debug.Log($"Added boss: {prefab.name}");
                    }
                }
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}