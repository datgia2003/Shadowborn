// using UnityEngine;

// /// <summary>
// /// Test script to verify default values match expectations
// /// </summary>
// public class TestDefaultValues : MonoBehaviour
// {
//     [ContextMenu("Test All Default Values")]
//     public void TestDefaults()
//     {
//         var playerStats = FindObjectOfType<PlayerStats>();
//         var playerController = FindObjectOfType<PlayerController>();
//         var playerCombat = FindObjectOfType<PlayerCombat>();
        
//         if (playerStats == null)
//         {
//             Debug.LogError("❌ PlayerStats not found!");
//             return;
//         }
        
//         Debug.Log("🧪 TESTING DEFAULT VALUES (Level 1, All Stats = 0)");
//         Debug.Log("=".PadRight(50, '='));
        
//         // Test PlayerStats calculated values
//         Debug.Log($"📊 PLAYER STATS:");
//         Debug.Log($"   VIT: {playerStats.Vitality} (should be 0)");
//         Debug.Log($"   STR: {playerStats.Strength} (should be 0)");
//         Debug.Log($"   INT: {playerStats.Intelligence} (should be 0)");
//         Debug.Log($"   AGI: {playerStats.Agility} (should be 0)");
//         Debug.Log($"   CRIT: {playerStats.CriticalChance} (should be 0)");
//         Debug.Log($"   Available Points: {playerStats.AvailablePoints}");
        
//         Debug.Log($"📊 CALCULATED VALUES:");
//         Debug.Log($"   MaxHealth: {playerStats.MaxHealth} (should be 100)");
//         Debug.Log($"   MaxMana: {playerStats.MaxMana} (should be 100)");
//         Debug.Log($"   AttackDamage: {playerStats.AttackDamage} (should be 10)");
//         Debug.Log($"   MovementSpeed: {playerStats.MovementSpeed:F1} (should be 7.0)");
//         Debug.Log($"   AttackSpeed: {playerStats.AttackSpeed:F2} (should be 1.00)");
        
//         // Test PlayerController values
//         if (playerController != null)
//         {
//             Debug.Log($"🏃 PLAYER CONTROLLER:");
//             Debug.Log($"   walkSpeed: {playerController.walkSpeed:F1} (should be 7.0)");
//             Debug.Log($"   runSpeed: {playerController.runSpeed:F1} (should be ~17.5)");
//             Debug.Log($"   jumpForce: {playerController.jumpForce:F1}");
//         }
        
//         // Test PlayerCombat values
//         if (playerCombat != null)
//         {
//             Debug.Log($"⚔️ PLAYER COMBAT:");
//             Debug.Log($"   attackSpeed: {playerCombat.attackSpeed:F2} (should be 1.00)");
//         }
        
//         // Test with 10 AGI
//         Debug.Log("\n🧪 TESTING WITH 10 AGI:");
//         Debug.Log("=".PadRight(30, '='));
//         float testMovementSpeed = 7f + (10 * 0.1f);
//         float testAttackSpeed = 1f + (10 * 0.02f);
//         Debug.Log($"   MovementSpeed with 10 AGI: {testMovementSpeed:F1} (should be 8.0)");
//         Debug.Log($"   AttackSpeed with 10 AGI: {testAttackSpeed:F2} (should be 1.20)");
//         Debug.Log($"   RunSpeed with 10 AGI: {testMovementSpeed * 2.5f:F1} (should be 20.0)");
//     }
// }
