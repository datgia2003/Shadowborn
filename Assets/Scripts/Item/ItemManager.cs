using UnityEngine;
using UnityEngine.InputSystem;

public class ItemManager : MonoBehaviour
{
    public int healthPotionCount = 0;
    public int manaPotionCount = 0;
    public int coinCount = 0;

    public int healthRestoreAmount = 50;
    public int manaRestoreAmount = 30;

    [Header("Input System")]
    public InputActionReference restoreHPAction;
    public InputActionReference restoreMPAction;

    private PlayerResources playerResources;

    void Awake()
    {
        playerResources = FindObjectOfType<PlayerResources>();
    }

    void OnEnable()
    {
        if (restoreHPAction != null)
            restoreHPAction.action.performed += OnRestoreHP;
        if (restoreMPAction != null)
            restoreMPAction.action.performed += OnRestoreMP;
    }

    void OnDisable()
    {
        if (restoreHPAction != null)
            restoreHPAction.action.performed -= OnRestoreHP;
        if (restoreMPAction != null)
            restoreMPAction.action.performed -= OnRestoreMP;
    }

    private void OnRestoreHP(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        UseHealthPotion();
    }
    private void OnRestoreMP(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        UseManaPotion();
    }

    public void AddHealthPotion(int amount = 1)
    {
        healthPotionCount += amount;
    }
    public void AddManaPotion(int amount = 1)
    {
        manaPotionCount += amount;
    }
    public void AddCoin(int amount = 1)
    {
        coinCount += amount;
    }

    public void UseHealthPotion()
    {
        if (healthPotionCount > 0 && playerResources != null)
        {
            playerResources.AddHealth(healthRestoreAmount);
            healthPotionCount--;
            Debug.Log($"🧪 Used Health Potion (+{healthRestoreAmount} HP), remaining: {healthPotionCount}");
        }
    }
    public void UseManaPotion()
    {
        if (manaPotionCount > 0 && playerResources != null)
        {
            playerResources.AddMana(manaRestoreAmount);
            manaPotionCount--;
            Debug.Log($"🔮 Used Mana Potion (+{manaRestoreAmount} MP), remaining: {manaPotionCount}");
        }
    }
}
