using UnityEngine;

/// <summary>
/// Demo script để test Enemy Health Bar system
/// Attach vào một GameObject để tự động tạo enemies và test health bars
/// </summary>
public class EnemyHealthBarDemo : MonoBehaviour
{
    [Header("Demo Configuration")]
    public bool createDemoEnemies = true;
    public int batCount = 3;
    public int skeletonCount = 2;
    
    [Header("Spawn Settings")]
    public Vector2 spawnAreaMin = new Vector2(-5, 0);
    public Vector2 spawnAreaMax = new Vector2(5, 3);
    
    [Header("Prefab References")]
    public GameObject batPrefab;
    public GameObject skeletonPrefab;
    
    [Header("Test Controls")]
    [Range(1, 10)] public int damageAmount = 1;
    public KeyCode damageKey = KeyCode.Space;
    public KeyCode healKey = KeyCode.H;
    
    private GameObject[] spawnedEnemies;

    void Start()
    {
        if (createDemoEnemies)
        {
            CreateDemoEnemies();
        }
    }

    void Update()
    {
        // Test controls
        if (Input.GetKeyDown(damageKey))
        {
            DamageAllEnemies();
        }
        
        if (Input.GetKeyDown(healKey))
        {
            HealAllEnemies();
        }
    }

    void CreateDemoEnemies()
    {
        var enemies = new System.Collections.Generic.List<GameObject>();
        
        // Create bats
        for (int i = 0; i < batCount; i++)
        {
            Vector2 spawnPos = GetRandomSpawnPosition();
            GameObject bat = CreateEnemy("DemoBat", spawnPos);
            
            // Add BatController if prefab not available
            if (batPrefab == null)
            {
                BatController batController = bat.GetComponent<BatController>();
                if (batController == null)
                {
                    batController = bat.AddComponent<BatController>();
                    batController.maxHealth = 3;
                }
            }
            else
            {
                bat = Instantiate(batPrefab, spawnPos, Quaternion.identity);
            }
            
            // Add health bar setup
            EnemyHealthBarSetup setup = bat.GetComponent<EnemyHealthBarSetup>();
            if (setup == null)
            {
                setup = bat.AddComponent<EnemyHealthBarSetup>();
                setup.showOnlyWhenDamaged = false; // Always show for demo
                setup.alwaysVisible = true;
            }
            
            enemies.Add(bat);
        }
        
        // Create skeletons
        for (int i = 0; i < skeletonCount; i++)
        {
            Vector2 spawnPos = GetRandomSpawnPosition();
            GameObject skeleton = CreateEnemy("DemoSkeleton", spawnPos);
            
            // Add SkeletonController if prefab not available
            if (skeletonPrefab == null)
            {
                SkeletonController skeletonController = skeleton.GetComponent<SkeletonController>();
                if (skeletonController == null)
                {
                    skeletonController = skeleton.AddComponent<SkeletonController>();
                    skeletonController.maxHealth = 5;
                }
            }
            else
            {
                skeleton = Instantiate(skeletonPrefab, spawnPos, Quaternion.identity);
            }
            
            // Add health bar setup
            EnemyHealthBarSetup setup = skeleton.GetComponent<EnemyHealthBarSetup>();
            if (setup == null)
            {
                setup = skeleton.AddComponent<EnemyHealthBarSetup>();
                setup.showOnlyWhenDamaged = false; // Always show for demo
                setup.alwaysVisible = true;
            }
            
            enemies.Add(skeleton);
        }
        
        spawnedEnemies = enemies.ToArray();
        
        Debug.Log($"[EnemyHealthBarDemo] Created {enemies.Count} demo enemies with health bars");
    }

    GameObject CreateEnemy(string name, Vector2 position)
    {
        GameObject enemy = new GameObject(name);
        enemy.transform.position = position;
        
        // Add basic components that might be needed
        if (enemy.GetComponent<Rigidbody2D>() == null)
            enemy.AddComponent<Rigidbody2D>();
        
        if (enemy.GetComponent<Animator>() == null)
            enemy.AddComponent<Animator>();
            
        if (enemy.GetComponent<SpriteRenderer>() == null)
        {
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            // Create a simple colored square sprite for demo
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = name.Contains("Bat") ? Color.gray : Color.white;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            sr.sprite = sprite;
        }
        
        return enemy;
    }

    Vector2 GetRandomSpawnPosition()
    {
        return new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );
    }

    void DamageAllEnemies()
    {
        if (spawnedEnemies == null) return;
        
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null) continue;
            
            BatController bat = enemy.GetComponent<BatController>();
            if (bat != null)
            {
                bat.TakeDamage(damageAmount);
                continue;
            }
            
            SkeletonController skeleton = enemy.GetComponent<SkeletonController>();
            if (skeleton != null)
            {
                skeleton.TakeDamage(damageAmount);
                continue;
            }
            
            // Fallback: direct health bar damage
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                healthBar.TakeDamage(damageAmount);
            }
        }
        
        Debug.Log($"[EnemyHealthBarDemo] Damaged all enemies by {damageAmount}");
    }

    void HealAllEnemies()
    {
        if (spawnedEnemies == null) return;
        
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy == null) continue;
            
            EnemyHealthBar healthBar = enemy.GetComponent<EnemyHealthBar>();
            if (healthBar != null)
            {
                // Set to full health
                BatController bat = enemy.GetComponent<BatController>();
                SkeletonController skeleton = enemy.GetComponent<SkeletonController>();
                
                int maxHealth = 100;
                if (bat != null) maxHealth = bat.maxHealth;
                else if (skeleton != null) maxHealth = skeleton.maxHealth;
                
                healthBar.SetHealth(maxHealth, maxHealth);
            }
        }
        
        Debug.Log("[EnemyHealthBarDemo] Healed all enemies to full health");
    }

    void OnDrawGizmosSelected()
    {
        // Draw spawn area
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2, (spawnAreaMin.y + spawnAreaMax.y) / 2, 0);
        Vector3 size = new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 1);
        Gizmos.DrawWireCube(center, size);
    }

    [ContextMenu("Create Demo Enemies")]
    public void ManualCreateDemoEnemies()
    {
        CreateDemoEnemies();
    }

    [ContextMenu("Damage All Enemies")]
    public void ManualDamageAllEnemies()
    {
        DamageAllEnemies();
    }

    [ContextMenu("Heal All Enemies")]
    public void ManualHealAllEnemies()
    {
        HealAllEnemies();
    }
}