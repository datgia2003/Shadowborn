using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// ReadOnly attribute for inspector display
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

/// <summary>
/// Enhanced player resource management system with Level & EXP
/// Centralized resource tracking with event system for UI updates
/// Features: Health, Mana, Energy, Level, Experience
/// </summary>
public class PlayerResources : MonoBehaviour
{
    // Singleton pattern
    public static PlayerResources Instance { get; private set; }

    [Header("Health Settings")]
    public int baseMaxHealth = 100;
    [SerializeField, ReadOnly] private int maxHealth = 100;
    public float healthRegenRate = 2f; // HP per second when not in combat
    public float healthRegenDelay = 3f; // Delay after taking damage before regen starts
    public float invincibleTime = 0.5f; // Invincibility duration after taking damage

    [Header("Mana Settings")]
    public int baseMaxMana = 100;
    [SerializeField, ReadOnly] private int maxMana = 100;
    public float manaRegenRate = 1f; // Mana per second (adjustable in inspector)
    public int sliceUpManaCost = 20;
    public int summonManaCost = 30;
    public int ultimateManaCost = 50;

    [Header("Energy Settings (for defensive abilities)")]
    public int maxEnergy = 50;
    public float energyRegenRate = 8f; // Energy per second
    public int dodgeEnergyCost = 15;

    [Header("Level & Experience Settings")]
    [Space(5)]
    [Header("⚠️ DEPRECATED: Use ExperienceSystem instead")]
    [SerializeField, ReadOnly] public int currentLevel = 1; // For compatibility only
    [SerializeField, ReadOnly] public int currentExp = 0; // For compatibility only  
    [SerializeField, ReadOnly] public int baseExpToNextLevel = 100; // For compatibility only
    [SerializeField, ReadOnly] public float expGrowthRate = 1.2f; // For compatibility only

    // Current values
    private int currentHealth;
    private int currentMana;
    private int currentEnergy;
    private int expToNextLevel;

    // Combat state tracking
    private float lastDamageTime;
    private bool isInCombat = false;
    private bool isInvincible = false;
    private float lastHitTime;

    // Resource regeneration timers
    private float manaRegenTimer = 0f;
    private float healthRegenTimer = 0f;
    private float energyRegenTimer = 0f;

    // Fractional accumulation for regeneration
    private float manaRegenAccumulated = 0f;
    private float energyRegenAccumulated = 0f;

    // Animation system
    private Animator anim;

    // Event system for UI updates
    public static event Action<int, int> OnHealthChanged; // current, max
    public static event Action<int, int> OnManaChanged;   // current, max
    public static event Action<int, int> OnEnergyChanged; // current, max
    public static event Action<int, int> OnExpChanged;    // current exp, exp needed for next level
    public static event Action<int> OnLevelUp;            // new level
    public static event Action OnPlayerDied;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize resources
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentEnergy = maxEnergy;

        // Initialize level system
        CalculateExpToNextLevel();

        Debug.Log($"[PlayerResources] Awake - Initialized all resources as standalone system");
    }

    void Start()
    {
        // Initialize resource regeneration timers
        manaRegenTimer = 1f; // mana regens every 1 second
        healthRegenTimer = healthRegenDelay;
        energyRegenTimer = 0.5f; // energy regens faster

        // Get animator component for death/hit animations
        anim = GetComponent<Animator>();

        // Fire initial events to set up UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        OnExpChanged?.Invoke(currentExp, expToNextLevel);

        Debug.Log($"[PlayerResources] Start - Max Health: {maxHealth}, Current Health: {currentHealth}");
        Debug.Log($"[PlayerResources] Start - Max Mana: {maxMana}, Current Mana: {currentMana}");
        Debug.Log($"[PlayerResources] Start - Max Energy: {maxEnergy}, Current Energy: {currentEnergy}");
    }

    void Update()
    {
        // Update combat state
        isInCombat = Time.time - lastDamageTime < healthRegenDelay;

        // Regenerate resources
        RegenerateResources();
    }

    private void CalculateExpToNextLevel()
    {
        expToNextLevel = Mathf.RoundToInt(baseExpToNextLevel * Mathf.Pow(expGrowthRate, currentLevel - 1));
    }

    private void RegenerateResources()
    {
        // Health regen (only when not in combat)
        if (!isInCombat && currentHealth < maxHealth)
        {
            healthRegenTimer += Time.deltaTime;
            if (healthRegenTimer >= 1f) // Every 1 second
            {
                AddHealth(Mathf.RoundToInt(healthRegenRate));
                healthRegenTimer = 0f;
            }
        }

        // Mana regen (always) - supports fractional rates
        if (currentMana < maxMana)
        {
            manaRegenAccumulated += manaRegenRate * Time.deltaTime;

            // Add whole numbers when accumulated enough
            if (manaRegenAccumulated >= 1f)
            {
                int manaToAdd = Mathf.FloorToInt(manaRegenAccumulated);
                AddMana(manaToAdd);
                manaRegenAccumulated -= manaToAdd; // Keep the fractional part

                Debug.Log($"🔮 Mana regen: +{manaToAdd} (rate: {manaRegenRate}/sec, accumulated: {manaRegenAccumulated:F2})");
            }
        }
        else
        {
            manaRegenAccumulated = 0f; // Reset when full
        }

        // Energy regen (always) - supports fractional rates  
        if (currentEnergy < maxEnergy)
        {
            energyRegenAccumulated += energyRegenRate * Time.deltaTime;

            // Add whole numbers when accumulated enough
            if (energyRegenAccumulated >= 1f)
            {
                int energyToAdd = Mathf.FloorToInt(energyRegenAccumulated);
                AddEnergy(energyToAdd);
                energyRegenAccumulated -= energyToAdd; // Keep the fractional part
            }
        }
        else
        {
            energyRegenAccumulated = 0f; // Reset when full
        }
    }

    // Health management  
    public void TakeDamage(int damage)
    {
        // Check invincibility frames
        if (isInvincible)
        {
            return;
        }

        int previousHealth = currentHealth;
        currentHealth = Mathf.Max(0, currentHealth - damage);
        lastDamageTime = Time.time;
        lastHitTime = Time.time;

        // Start invincibility frames
        if (invincibleTime > 0)
        {
            isInvincible = true;
            StartCoroutine(InvincibilityFrames());
        }

        // Trigger hit animation
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        // Fire health changed event
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Check for death
        if (currentHealth <= 0 && previousHealth > 0)
        {
            Debug.Log("💀 PlayerResources: Player died!");

            // Trigger death animation
            if (anim != null)
            {
                anim.SetTrigger("Death");
            }

            OnPlayerDied?.Invoke();
        }
    }

    private IEnumerator InvincibilityFrames()
    {
        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    public void AddHealth(int amount)
    {
        int previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (previousHealth != currentHealth)
        {
            Debug.Log($"🩸 AddHealth: {previousHealth} → {currentHealth} (added: {amount})");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    // Mana management
    public bool TryConsumeMana(int amount)
    {
        if (currentMana >= amount)
        {
            int previousMana = currentMana;
            currentMana -= amount;

            Debug.Log($"🔮 TryConsumeMana: {previousMana} → {currentMana} (consumed: {amount})");

            OnManaChanged?.Invoke(currentMana, maxMana);
            return true;
        }
        else
        {
            Debug.Log($"❌ TryConsumeMana failed: Not enough mana ({currentMana}/{amount})");
            return false;
        }
    }

    public void AddMana(int amount)
    {
        int previousMana = currentMana;
        currentMana = Mathf.Min(maxMana, currentMana + amount);

        if (previousMana != currentMana)
        {
            Debug.Log($"🔮 AddMana: {previousMana} → {currentMana} (added: {amount})");
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    // Energy management
    public bool TryConsumeEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }
        return false;
    }

    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    // Level & Experience management
    public void AddExperience(int amount)
    {
        currentExp += amount;

        // Check for level up
        while (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        currentLevel++;

        // Recalculate exp needed for next level
        CalculateExpToNextLevel();

        // Level up bonuses
        ApplyLevelUpBonuses();

        // Fire level up event
        OnLevelUp?.Invoke(currentLevel);
    }

    private void ApplyLevelUpBonuses()
    {
        // Increase max health by 10 per level
        maxHealth += 10;

        // Increase max mana by 5 per level
        maxMana += 5;

        // Restore health and mana on level up
        currentHealth = maxHealth;
        currentMana = maxMana;

        // Fire events to update UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    // Getters for skill systems
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public int GetCurrentMana() => currentMana;
    public int GetMaxMana() => maxMana;
    public int GetCurrentEnergy() => currentEnergy;
    public int GetMaxEnergy() => maxEnergy;
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExp() => currentExp;
    public int GetExpToNextLevel() => expToNextLevel;

    public bool HasManaFor(int amount) => currentMana >= amount;
    public bool HasEnergyFor(int amount) => currentEnergy >= amount;

    // Skill-specific resource checks
    public bool CanUseSliceUp() => HasManaFor(sliceUpManaCost);
    public bool CanUseSummon() => HasManaFor(summonManaCost);
    public bool CanUseUltimate() => HasManaFor(ultimateManaCost);
    public bool CanUseDodge() => HasEnergyFor(dodgeEnergyCost);

    // Public methods for external systems to give exp
    public void GiveExpForKill(string enemyType)
    {
        int expReward = enemyType switch
        {
            "SmallEnemy" => 10,
            "MediumEnemy" => 25,
            "LargeEnemy" => 50,
            "Boss" => 100,
            _ => 5
        };

        AddExperience(expReward);
    }

    public void GiveExpForAction(string actionType)
    {
        int expReward = actionType switch
        {
            "SkillUsed" => 2,
            "ComboHit" => 5,
            "PerfectDodge" => 8,
            "QuestComplete" => 50,
            _ => 1
        };

        AddExperience(expReward);
    }

    #region Stats Integration Methods

    public void AddMaxHealth(int bonusAmount)
    {
        maxHealth += bonusAmount;

        // If current health is higher than new max, adjust it
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"Max health increased by {bonusAmount}: {maxHealth}");
    }

    public void AddMaxMana(int bonusAmount)
    {
        maxMana += bonusAmount;

        // If current mana is higher than new max, adjust it
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        OnManaChanged?.Invoke(currentMana, maxMana);
        Debug.Log($"Max mana increased by {bonusAmount}: {maxMana}");
    }

    #endregion

    // Compatibility method for external scripts expecting PlayerHealth interface
    public int GetHP() => currentHealth;

    // Property for max HP compatibility
    public int maxHP
    {
        get => maxHealth;
        set => maxHealth = value;
    }
}
