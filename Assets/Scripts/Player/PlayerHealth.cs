using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 10;
    public float invincibleTime = 0.5f;

    private int currentHP;
    private bool isInvincible = false;
    private float lastHitTime;

    private Animator anim;

    void Awake()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || currentHP <= 0) return;

        currentHP -= amount;
        anim?.SetTrigger("Hurt");

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            isInvincible = true;
            lastHitTime = Time.time;
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
