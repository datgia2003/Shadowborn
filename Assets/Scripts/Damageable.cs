using UnityEngine;

public interface IDamageable
{
    void TakeHit(float dmg);
    bool IsAlive { get; }
}

public class Damageable : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float MaxHP = 100f;
    public float CurrentHP = 100f;
    public bool IsAlive => CurrentHP > 0f;

    void Awake() { if (CurrentHP <= 0f) CurrentHP = MaxHP; }

    public void TakeHit(float dmg)
    {
        if (!IsAlive) return;
        CurrentHP -= dmg;
        if (CurrentHP <= 0f) { CurrentHP = 0f; OnDeath(); }
        Debug.Log($"{name} took {dmg} dmg → HP {CurrentHP}/{MaxHP}");
    }

    void OnDeath() { Debug.Log($"{name} died"); /* TODO: FX / Destroy */ }
}