using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 10;
    public float invincibleTime = 0.5f;

    private int currentHP;
    public bool isInvincible = false;
    private float lastHitTime;

    private Animator anim;

    void Awake()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || currentHP <= 0)
        {
            Debug.Log($"🛡️ PlayerHealth damage blocked - Invincible: {isInvincible}, HP: {currentHP}");
            return;
        }

        int previousHP = currentHP;
        currentHP -= amount;

        Debug.Log($"❤️ PlayerHealth.TakeDamage: {previousHP} → {currentHP} (damage: {amount})");

        anim?.SetTrigger("Hurt");

        // Notify PlayerResources of damage taken (but prevent infinite loop)
        var playerResources = GetComponent<PlayerResources>();
        if (playerResources != null)
        {
            // Update PlayerResources health directly to avoid double damage
            int newResourceHealth = playerResources.GetCurrentHealth() - amount;
            newResourceHealth = Mathf.Max(0, newResourceHealth);
            // Don't call TakeDamage to avoid loop
            Debug.Log($"🔗 Syncing PlayerResources health to {newResourceHealth}");
        }

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            lastHitTime = Time.time;
            Debug.Log($"🛡️ Player invincible for {invincibleTime}s");
        }
    }

    void Update()
    {
        if (isInvincible && Time.time - lastHitTime >= invincibleTime)
        {
            isInvincible = false;
        }
    }

    void Die()
    {
        anim?.SetTrigger("Die");
        // TODO: Disable movement, trigger game over, etc.
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
    }

    public int GetHP() => currentHP;
}
