using UnityEngine;
using System;

/// <summary>
/// Core player resource management system (Health, Mana, Energy)
/// Centralized resource tracking with event system for UI updates
/// </summary>
public class PlayerResources : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public float healthRegenRate = 2f; // HP per second when not in combat
    public float healthRegenDelay = 3f; // Delay after taking damage before regen starts

    [Header("Mana Settings")]
    public int maxMana = 100;
    public float manaRegenRate = 5f; // Mana per second
    public int sliceUpManaCost = 20;
    public int summonManaCost = 30;
    public int ultimateManaCost = 50;

    [Header("Energy Settings (for defensive abilities)")]
    public int maxEnergy = 50;
    public float energyRegenRate = 8f; // Energy per second
    public int dodgeEnergyCost = 15;

    // Current values
    private int currentHealth;
    private int currentMana;
    private int currentEnergy;

    // Combat state tracking
    private float lastDamageTime;
    private bool isInCombat = false;

    // Event system for UI updates
    public static event Action<int, int> OnHealthChanged; // current, max
    public static event Action<int, int> OnManaChanged;   // current, max
    public static event Action<int, int> OnEnergyChanged; // current, max
    public static event Action OnPlayerDied;

    // References
    private PlayerHealth playerHealth;

    void Awake()
    {
        // Initialize resources
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentEnergy = maxEnergy;

        // Get existing PlayerHealth component
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // Sync with existing health system
            maxHealth = playerHealth.maxHP;
            currentHealth = playerHealth.GetHP();
        }
    }

    void Start()
    {
        // Fire initial events to set up UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    void Update()
    {
        // Update combat state
        isInCombat = Time.time - lastDamageTime < healthRegenDelay;

        // Regenerate resources
        RegenerateResources();

        // Sync with PlayerHealth if exists
        SyncWithPlayerHealth();
    }

    private void RegenerateResources()
    {
        // Health regen (only when not in combat)
        if (!isInCombat && currentHealth < maxHealth)
        {
            AddHealth(Mathf.RoundToInt(healthRegenRate * Time.deltaTime));
        }

        // Mana regen (always)
        if (currentMana < maxMana)
        {
            AddMana(Mathf.RoundToInt(manaRegenRate * Time.deltaTime));
        }

        // Energy regen (always)
        if (currentEnergy < maxEnergy)
        {
            AddEnergy(Mathf.RoundToInt(energyRegenRate * Time.deltaTime));
        }
    }

    private void SyncWithPlayerHealth()
    {
        if (playerHealth != null)
        {
            int healthFromOldSystem = playerHealth.GetHP();
            if (healthFromOldSystem != currentHealth)
            {
                currentHealth = healthFromOldSystem;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }
    }

    // Public resource modification methods
    public bool TryConsumeMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana = Mathf.Max(0, currentMana - amount);
            OnManaChanged?.Invoke(currentMana, maxMana);
            return true;
        }
        return false;
    }

    public bool TryConsumeEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy = Mathf.Max(0, currentEnergy - amount);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }
        return false;
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        lastDamageTime = Time.time;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            OnPlayerDied?.Invoke();
        }

        // Also update old system if it exists
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(amount);
        }
    }

    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddMana(int amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    // Getters for skill systems
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;
    public int GetCurrentEnergy() => currentEnergy;
    public int GetMaxEnergy() => maxEnergy;

    public bool HasManaFor(int amount) => currentMana >= amount;
    public bool HasEnergyFor(int amount) => currentEnergy >= amount;

    // Skill-specific resource checks
    public bool CanUseSliceUp() => HasManaFor(sliceUpManaCost);
    public bool CanUseSummon() => HasManaFor(summonManaCost);
    public bool CanUseUltimate() => HasManaFor(ultimateManaCost);
    public bool CanDodge() => HasEnergyFor(dodgeEnergyCost);
}
