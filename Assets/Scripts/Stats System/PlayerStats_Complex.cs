// using UnityEngine;
// using System;
// using System.Reflection;

// /// <summary>
// /// Solo Leveling Style Player Stats System
// /// Manages 5 core stats: VIT, STR, INT, AGI, CRIT with allocation points
// /// </summary>
// public class PlayerStats : MonoBehaviour
// {
//     [Header("🎯 Solo Leveling Stats System")]
//     [Space(5)]

//     [Header("📊 Core Stats")]
//     [SerializeField] private int vitality = 10;
//     [SerializeField] private int strength = 10;
//     [SerializeField] private int intelligence = 10;
//     [SerializeField] private int agility = 10;
//     [SerializeField] private int criticalChance = 5;

//     [Header("📈 Stat Points")]
//     [SerializeField] private int availablePoints = 0;

//     [Header("⚙️ Stat Multipliers")]
//     [SerializeField] private float vitHealthMultiplier = 20f;
//     [SerializeField] private float vitDefenseMultiplier = 1f;
//     [SerializeField] private float strAttackMultiplier = 2f;
//     [SerializeField] private float intManaMultiplier = 15f;
//     [SerializeField] private float intSkillDamageMultiplier = 1.5f;
//     [SerializeField] private float agiSpeedMultiplier = 0.1f;
//     [SerializeField] private float agiAttackSpeedMultiplier = 0.05f;

//     // Singleton Instance
//     public static PlayerStats Instance { get; private set; }

//     // Events
//     public static event Action<StatType, int> OnStatChanged;
//     public static event Action<int> OnPointsChanged;
//     public static event Action<int> OnPointsAwarded;

//     // Stat Types Enum
//     public enum StatType
//     {
//         Vitality,
//         Strength,
//         Intelligence,
//         Agility,
//         Critical
//     }

//     void Awake()
//     {
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     void Start()
//     {
//         // Wait 1 frame để đảm bảo PlayerResources đã initialize
//         StartCoroutine(DelayedInitialization());
//     }

//     private System.Collections.IEnumerator DelayedInitialization()
//     {
//         yield return null; // Wait 1 frame
        
//         // Initialize system and force update all player systems  
//         Debug.Log("🎯 PlayerStats DelayedInitialization() - Force updating all player systems");
//         UpdatePlayerSystems();
//         Debug.Log("🎯 PlayerStats System Initialized");
//     }

//     // Properties for calculated values (Health handled directly by PlayerResources)
//     public int MaxMana => Mathf.RoundToInt(100 + (intelligence * intManaMultiplier));
//     public int AttackDamage => Mathf.RoundToInt(10 + (strength * strAttackMultiplier));
//     public float MovementSpeed => 5f + (agility * agiSpeedMultiplier);
//     public float AttackSpeed => 1f + (agility * agiAttackSpeedMultiplier);
//     public float DefenseReduction => vitality * vitDefenseMultiplier;
//     public float SkillDamageBonus => intelligence * intSkillDamageMultiplier;
//     public float CriticalChancePercent => criticalChance;

//     // Getters
//     public int Vitality => vitality;
//     public int Strength => strength;
//     public int Intelligence => intelligence;
//     public int Agility => agility;
//     public int Critical => criticalChance;
//     public int AvailablePoints => availablePoints;

//     /// <summary>
//     /// Add available stat points (called by ExperienceSystem)
//     /// </summary>
//     public void AddAvailablePoints(int points)
//     {
//         Debug.Log($"PlayerStats: AddAvailablePoints called with {points} points");
//         availablePoints += points;
//         OnPointsChanged?.Invoke(availablePoints);

//         Debug.Log($"PlayerStats: Firing OnPointsAwarded event with {points} points");
//         OnPointsAwarded?.Invoke(points);

//         Debug.Log($"🎯 Received {points} stat points! Total available: {availablePoints}");
//     }

//     /// <summary>
//     /// Allocate a point to a specific stat
//     /// </summary>
//     public bool AllocatePoint(StatType statType)
//     {
//         if (availablePoints <= 0) return false;

//         availablePoints--;

//         int oldStatValue = GetStatValue(statType);
//         int oldMaxMana = MaxMana;

//         switch (statType)
//         {
//             case StatType.Vitality:
//                 vitality++;
//                 break;
//             case StatType.Strength:
//                 strength++;
//                 break;
//             case StatType.Intelligence:
//                 intelligence++;
//                 break;
//             case StatType.Agility:
//                 agility++;
//                 break;
//             case StatType.Critical:
//                 criticalChance++;
//                 break;
//         }

//         int newStatValue = GetStatValue(statType);
//         int newMaxMana = MaxMana;

//         OnStatChanged?.Invoke(statType, newStatValue);
//         OnPointsChanged?.Invoke(availablePoints);

//         Debug.Log($"🎯 BEFORE UPDATE: {statType} {oldStatValue}→{newStatValue}, MP: {oldMaxMana}→{newMaxMana}");

//         UpdatePlayerSystems();

//         Debug.Log($"🎯 Allocated point to {statType}. New value: {newStatValue}, Remaining points: {availablePoints}");
//         return true;
//     }

//     /* DISABLED - Health now handled in UpdatePlayerSystems like Mana
//     /// <summary>
//     /// Update PlayerResources health when VIT increases (same logic as mana)
//     /// </summary>
//     private void UpdateVitalityBonus()
//     {
//         var playerResources = FindObjectOfType<PlayerResources>();
//         if (playerResources != null)
//         {
//             // Store current health before updating max values  
//             int currentHealth = playerResources.GetCurrentHealth();
//             int oldMaxHealth = playerResources.maxHealth;
//             int newMaxHealth = 100 + (vitality * (int)vitHealthMultiplier); // Direct calculation

//             Debug.Log($"💚 PLAYERRESOURCES BEFORE: HP={currentHealth}/{oldMaxHealth}");

//             // Update max health
//             playerResources.maxHealth = newMaxHealth;
            
//             Debug.Log($"💚 SET NEW MAX HEALTH: HP={playerResources.maxHealth}");

//             // Calculate health difference
//             int healthDifference = newMaxHealth - oldMaxHealth;

//             Debug.Log($"💚 DIFFERENCE: Health +{healthDifference}");
            
//             if (healthDifference > 0)
//             {
//                 Debug.Log($"💚 CALLING AddHealth({healthDifference}) - Before: {playerResources.GetCurrentHealth()}/{oldMaxHealth}");
//                 playerResources.AddHealth(healthDifference);
//                 Debug.Log($"💚 AddHealth completed - After: {playerResources.GetCurrentHealth()}/{playerResources.maxHealth}");
//             }

//             Debug.Log($"💚 FINAL STATE: HP={playerResources.GetCurrentHealth()}/{playerResources.maxHealth}");
//         }
//         else
//         {
//             Debug.LogError("❌ UpdateVitalityBonus: PlayerResources not found!");
//         }
//     }
//     */

//     /// <summary>
//     /// Get current value of a stat
//     /// </summary>
//     public int GetStatValue(StatType statType)
//     {
//         return statType switch
//         {
//             StatType.Vitality => vitality,
//             StatType.Strength => strength,
//             StatType.Intelligence => intelligence,
//             StatType.Agility => agility,
//             StatType.Critical => criticalChance,
//             _ => 0
//         };
//     }

//     /// <summary>
//     /// Update all connected player systems with new stat values
//     /// </summary>
//     private void UpdatePlayerSystems()
//     {
//         Debug.Log($"🔄 UpdatePlayerSystems called - VIT: {vitality}, INT: {intelligence}");
        
//         // Update PlayerResources health and mana with same logic
//         var playerResources = FindObjectOfType<PlayerResources>();
//         if (playerResources != null)
//         {
//             Debug.Log($"📊 FOUND PlayerResources component!");
//             Debug.Log($"📊 STATS: VIT={vitality}, STR={strength}, INT={intelligence}, AGI={agility}, CRIT={criticalChance}");
//             Debug.Log($"📊 CALCULATED: MaxHealth={baseHealthFromLevels + (vitality * (int)vitHealthMultiplier)} (base: {baseHealthFromLevels} + VIT: {vitality * (int)vitHealthMultiplier}), MaxMana={MaxMana}");

//             // Store current health and mana before updating max values  
//             int currentHealth = playerResources.GetCurrentHealth();
//             int currentMana = playerResources.GetCurrentMana();
//             int oldMaxHealth = playerResources.maxHealth;
//             int oldMaxMana = playerResources.maxMana;

//             Debug.Log($"📊 PLAYERRESOURCES BEFORE: HP={currentHealth}/{oldMaxHealth}, MP={currentMana}/{oldMaxMana}");

//             // Update max health and mana
//             // Calculate base health including level bonuses
//             var expSystem = FindObjectOfType<ExperienceSystem>();
//             int baseHealthFromLevels = 100; // Starting health
//             if (expSystem != null)
//             {
//                 int levelsGained = expSystem.GetCurrentLevel() - 1; // Level 1 = 0 bonus levels
//                 baseHealthFromLevels += levelsGained * 10; // healthBonusPerLevel from ExperienceSystem
//                 Debug.Log($"📊 LEVEL CALCULATION: Level {expSystem.GetCurrentLevel()}, base health from levels = {baseHealthFromLevels}");
//             }
            
//             int newMaxHealth = baseHealthFromLevels + (vitality * (int)vitHealthMultiplier);
//             playerResources.maxHealth = newMaxHealth;
//             playerResources.maxMana = MaxMana;
            
//             Debug.Log($"📊 SET NEW MAX VALUES: HP={playerResources.maxHealth}, MP={playerResources.maxMana}");

//             // Calculate health and mana differences
//             int healthDifference = newMaxHealth - oldMaxHealth;
//             int manaDifference = MaxMana - oldMaxMana;

//             Debug.Log($"📊 DIFFERENCE: Health +{healthDifference}, Mana +{manaDifference}");
            
//             if (healthDifference > 0)
//             {
//                 Debug.Log($"💚 CALLING AddHealth({healthDifference}) - Before: {playerResources.GetCurrentHealth()}/{oldMaxHealth}");
//                 playerResources.AddHealth(healthDifference);
//                 Debug.Log($"💚 AddHealth completed - After: {playerResources.GetCurrentHealth()}/{playerResources.maxHealth}");
//             }
            
//             if (manaDifference > 0)
//             {
//                 Debug.Log($"� CALLING AddMana({manaDifference}) - Before: {playerResources.GetCurrentMana()}/{oldMaxMana}");
//                 playerResources.AddMana(manaDifference);
//                 Debug.Log($"� AddMana completed - After: {playerResources.GetCurrentMana()}/{playerResources.maxMana}");
//             }

//             Debug.Log($"📊 FINAL STATE: HP={playerResources.GetCurrentHealth()}/{playerResources.maxHealth}, MP={playerResources.GetCurrentMana()}/{playerResources.maxMana}");
//         }
//         else
//         {
//             Debug.LogError("❌ PlayerResources not found! Cannot update health/mana from stats.");
//         }

//         // Update PlayerController movement and combat speeds
//         UpdatePlayerController();

//         // Update PlayerCombat attack speeds
//         UpdatePlayerCombat();

//         Debug.Log($"📊 Stats Updated - HP: {(expSystem != null ? (100 + (expSystem.GetCurrentLevel() - 1) * 10 + (vitality * (int)vitHealthMultiplier)) : (100 + vitality * (int)vitHealthMultiplier))}, MP: {MaxMana}, ATK: {AttackDamage}, SPD: {MovementSpeed:F1}");
//     }

//     /// <summary>
//     /// Update PlayerController with AGI-based speeds
//     /// </summary>
//     private void UpdatePlayerController()
//     {
//         var playerController = FindObjectOfType<PlayerController>();
//         if (playerController != null)
//         {
//             Debug.Log($"🏃 BEFORE PlayerController update - AGI: {agility}, MovementSpeed: {MovementSpeed:F1}");

//             // Access public fields directly (no reflection needed)
//             float oldWalkSpeed = playerController.walkSpeed;
//             float oldRunSpeed = playerController.runSpeed;

//             playerController.walkSpeed = MovementSpeed;
//             playerController.runSpeed = MovementSpeed * 1.5f;

//             Debug.Log($"🏃 PlayerController updated:");
//             Debug.Log($"   Walk Speed: {oldWalkSpeed:F1} → {playerController.walkSpeed:F1}");
//             Debug.Log($"   Run Speed: {oldRunSpeed:F1} → {playerController.runSpeed:F1}");

//             // Also try to update jumpForce if it exists
//             var controllerType = typeof(PlayerController);
//             var jumpForceField = controllerType.GetField("jumpForce", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//             if (jumpForceField != null)
//             {
//                 float oldJumpForce = (float)jumpForceField.GetValue(playerController);
//                 float newJumpForce = 14f + (agility * 0.2f);
//                 jumpForceField.SetValue(playerController, newJumpForce);
//                 Debug.Log($"   Jump Force: {oldJumpForce:F1} → {newJumpForce:F1}");
//             }
//         }
//         else
//         {
//             Debug.LogWarning("🏃 PlayerController not found!");
//         }

//         // Update PlayerCombat attack speed
//         UpdatePlayerCombat();
//     }

//     /// <summary>
//     /// Update PlayerCombat with AGI-based attack speed
//     /// </summary>
//     private void UpdatePlayerCombat()
//     {
//         var playerCombat = FindObjectOfType<PlayerCombat>();
//         if (playerCombat != null)
//         {
//             Debug.Log($"⚔️ BEFORE PlayerCombat update - AGI: {agility}, AttackSpeed: {AttackSpeed:F2}");

//             // Access public field directly (no reflection needed)
//             float oldAttackSpeed = playerCombat.attackSpeed;
//             playerCombat.attackSpeed = AttackSpeed;

//             Debug.Log($"⚔️ PlayerCombat updated:");
//             Debug.Log($"   Attack Speed: {oldAttackSpeed:F2} → {playerCombat.attackSpeed:F2}");
//         }
//         else
//         {
//             Debug.LogWarning("⚔️ PlayerCombat not found!");
//         }
//     }

//     /// <summary>
//     /// Reset all stats to base values (for testing)
//     /// </summary>
//     [ContextMenu("Reset All Stats")]
//     public void ResetStats()
//     {
//         vitality = 10;
//         strength = 10;
//         intelligence = 10;
//         agility = 10;
//         criticalChance = 5;
//         availablePoints = 0;

//         UpdatePlayerSystems();

//         OnStatChanged?.Invoke(StatType.Vitality, vitality);
//         OnStatChanged?.Invoke(StatType.Strength, strength);
//         OnStatChanged?.Invoke(StatType.Intelligence, intelligence);
//         OnStatChanged?.Invoke(StatType.Agility, agility);
//         OnStatChanged?.Invoke(StatType.Critical, criticalChance);
//         OnPointsChanged?.Invoke(availablePoints);

//         Debug.Log("🔄 All stats reset to base values");
//     }

//     /// <summary>
//     /// Add test points for debugging
//     /// </summary>
//     [ContextMenu("Add 10 Test Points")]
//     public void AddTestPoints()
//     {
//         AddAvailablePoints(10);
//     }

//     /// <summary>
//     /// Test AGI allocation specifically
//     /// </summary>
//     [ContextMenu("Test AGI +5")]
//     public void TestAgilityBoost()
//     {
//         Debug.Log("🧪 Testing AGI boost - Adding 5 AGI points");
//         agility += 5;
//         UpdatePlayerSystems();
//         Debug.Log($"🧪 AGI Test Complete - New AGI: {agility}, MovementSpeed: {MovementSpeed:F1}, AttackSpeed: {AttackSpeed:F2}");
//     }

//     /// <summary>
//     /// Force update all systems (for testing)
//     /// </summary>
//     [ContextMenu("Force Update Systems")]
//     public void ForceUpdateSystems()
//     {
//         Debug.Log("🔄 Force updating all player systems...");
//         UpdatePlayerSystems();
//     }
// }
