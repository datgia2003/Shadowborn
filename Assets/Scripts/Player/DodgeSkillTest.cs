using UnityEngine;

/// <summary>
/// Test script for dodge skill functionality
/// Attach to player to test dodge mechanics
/// </summary>
public class DodgeSkillTest : MonoBehaviour
{
    [Header("Test Settings")]
    public int testDamage = 20;
    public float testInterval = 2f;
    
    private DodgeSkill dodgeSkill;
    private PlayerResources playerResources;
    private float lastTestTime;

    private void Start()
    {
        dodgeSkill = GetComponent<DodgeSkill>();
        playerResources = GetComponent<PlayerResources>();
        
        if (dodgeSkill == null)
        {
            Debug.LogError("DodgeSkillTest: No DodgeSkill component found!");
        }
        
        if (playerResources == null)
        {
            Debug.LogError("DodgeSkillTest: No PlayerResources component found!");
        }
    }

    private void Update()
    {
        // Press T to test damage (for perfect dodge testing)
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestDamage();
        }

        // Press Y to test invincibility status
        if (Input.GetKeyDown(KeyCode.Y))
        {
            TestInvincibilityStatus();
        }

        // Press U to start auto damage test
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartAutoDamageTest();
        }

        // Auto damage test
        if (Input.GetKey(KeyCode.U) && Time.time - lastTestTime > testInterval)
        {
            TestDamage();
            lastTestTime = Time.time;
        }
    }

    private void TestDamage()
    {
        if (playerResources != null)
        {
            Debug.Log("🗡️ Testing damage on player...");
            playerResources.TakeDamage(testDamage);
        }
    }

    private void TestInvincibilityStatus()
    {
        if (dodgeSkill != null)
        {
            bool isInvincible = dodgeSkill.IsInvincible();
            bool isDodging = dodgeSkill.IsDodging();
            
            Debug.Log($"🛡️ Dodge Status - Invincible: {isInvincible}, Dodging: {isDodging}");
        }
        
        if (playerResources != null)
        {
            int currentHealth = playerResources.GetCurrentHealth();
            int currentMana = playerResources.GetCurrentMana();
            
            Debug.Log($"💙 Player Status - HP: {currentHealth}, Mana: {currentMana}");
        }
    }

    private void StartAutoDamageTest()
    {
        Debug.Log("🎯 Auto damage test started! Hold U to continue, release to stop");
        lastTestTime = Time.time;
    }

    private void OnGUI()
    {
        // Display test instructions
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");
        
        GUILayout.Label("=== DODGE SKILL TEST ===");
        GUILayout.Label("Left Shift: Dodge (or configured key)");
        GUILayout.Label("T: Test Damage");
        GUILayout.Label("Y: Check Status");
        GUILayout.Label("U: Auto Damage Test (hold)");
        
        if (dodgeSkill != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Dodging: {dodgeSkill.IsDodging()}");
            GUILayout.Label($"Invincible: {dodgeSkill.IsInvincible()}");
            GUILayout.Label($"Cooldown: {dodgeSkill.GetCooldownTime()}s");
            GUILayout.Label($"Mana Cost: {dodgeSkill.GetManaCost()}");
        }
        
        if (playerResources != null)
        {
            GUILayout.Space(10);
            GUILayout.Label($"HP: {playerResources.GetCurrentHealth()}");
            GUILayout.Label($"Mana: {playerResources.GetCurrentMana()}");
        }
        
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}