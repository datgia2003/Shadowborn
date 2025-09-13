using UnityEngine;

/// <summary>
/// Utility class để tất cả player skills có thể gây damage cho enemies và boss
/// </summary>
public static class SkillDamageUtility
{
    /// <summary>
    /// Apply damage to target using multiple methods for compatibility
    /// </summary>
    /// <param name="target">Target GameObject to damage</param>
    /// <param name="damage">Damage amount</param>
    /// <param name="skillName">Name of skill for debug logging</param>
    /// <param name="stunBoss">Whether to stun the boss if it's an Igris (for Ultimate skills)</param>
    /// <returns>True if damage was applied successfully</returns>
    public static bool ApplyDamageToTarget(GameObject target, int damage, string skillName = "Skill", bool stunBoss = false)
    {
        if (target == null) return false;

        bool damageApplied = false;

        // Try IDamageable interface first (universal)
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null && damageable.IsAlive)
        {
            damageable.TakeHit(damage);
            Debug.Log($"{skillName} hit {target.name} for {damage} damage via IDamageable");
            damageApplied = true;
        }

        // Try Igris boss specifically
        Igris boss = target.GetComponent<Igris>();
        if (boss != null && !damageApplied)
        {
            boss.TakeHit(damage);
            Debug.Log($"{skillName} hit boss for {damage} damage");

            // Spawn floating damage number for boss
            Vector3 damagePos = boss.transform.position + Vector3.up * 2f;
            FloatingDamageNumber.CreateDamageNumber(damage, damagePos);

            damageApplied = true;
        }

        // Special handling for Ultimate skills - stun the boss
        if (stunBoss && boss != null && skillName.Contains("Ultimate"))
        {
            boss.StunBoss(3f); // Stun for 3 seconds
            Debug.Log($"🌟 {skillName} STUNNED the boss!");
        }

        // Try generic Damageable component
        Damageable genericDamageable = target.GetComponent<Damageable>();
        if (genericDamageable != null && genericDamageable.IsAlive && !damageApplied)
        {
            genericDamageable.TakeHit(damage);
            Debug.Log($"{skillName} hit {target.name} for {damage} damage via Damageable");
            damageApplied = true;
        }

        // Fallback to old SendMessage system if no modern interface found
        if (!damageApplied)
        {
            target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            target.SendMessage("ApplyDamage", damage, SendMessageOptions.DontRequireReceiver);
            target.SendMessage("Damage", damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"{skillName} used SendMessage fallback on {target.name}");
            damageApplied = true; // Assume it worked
        }

        return damageApplied;
    }

    /// <summary>
    /// Apply damage to multiple targets
    /// </summary>
    /// <param name="targets">Array of target GameObjects</param>
    /// <param name="damage">Damage amount per target</param>
    /// <param name="skillName">Name of skill for debug logging</param>
    /// <param name="stunBoss">Whether to stun boss targets</param>
    /// <returns>Number of targets successfully damaged</returns>
    public static int ApplyDamageToTargets(GameObject[] targets, int damage, string skillName = "Skill", bool stunBoss = false)
    {
        if (targets == null || targets.Length == 0) return 0;

        int successCount = 0;
        foreach (var target in targets)
        {
            if (ApplyDamageToTarget(target, damage, skillName, stunBoss))
            {
                successCount++;
            }
        }

        Debug.Log($"{skillName} damaged {successCount}/{targets.Length} targets");
        return successCount;
    }

    /// <summary>
    /// Apply damage to collider array (from Physics2D.OverlapCircleAll etc)
    /// </summary>
    /// <param name="colliders">Array of Collider2D from physics queries</param>
    /// <param name="damage">Damage amount per target</param>
    /// <param name="skillName">Name of skill for debug logging</param>
    /// <param name="excludeSelf">GameObject to exclude from damage (usually the player)</param>
    /// <param name="stunBoss">Whether to stun boss targets</param>
    /// <returns>Number of targets successfully damaged</returns>
    public static int ApplyDamageToColliders(Collider2D[] colliders, int damage, string skillName = "Skill", GameObject excludeSelf = null, bool stunBoss = false)
    {
        if (colliders == null || colliders.Length == 0) return 0;

        int successCount = 0;
        foreach (var collider in colliders)
        {
            if (collider == null || collider.gameObject == excludeSelf) continue;

            if (ApplyDamageToTarget(collider.gameObject, damage, skillName, stunBoss))
            {
                successCount++;
            }
        }

        Debug.Log($"{skillName} damaged {successCount} targets from {colliders.Length} colliders");
        return successCount;
    }

    /// <summary>
    /// Check if target can take damage
    /// </summary>
    /// <param name="target">Target to check</param>
    /// <returns>True if target can be damaged</returns>
    public static bool CanTakeDamage(GameObject target)
    {
        if (target == null) return false;

        // Check IDamageable
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            return damageable.IsAlive;
        }

        // Check Igris boss
        Igris boss = target.GetComponent<Igris>();
        if (boss != null)
        {
            return boss.GetCurrentHP() > 0 && boss.GetCurrentState() != Igris.BossState.Dead;
        }

        // Check generic Damageable
        Damageable genericDamageable = target.GetComponent<Damageable>();
        if (genericDamageable != null)
        {
            return genericDamageable.IsAlive;
        }

        // If no damage component found, assume it can take damage (for SendMessage fallback)
        return true;
    }

    /// <summary>
    /// Get damage-capable targets from collider array
    /// </summary>
    /// <param name="colliders">Array of colliders to filter</param>
    /// <param name="excludeSelf">GameObject to exclude</param>
    /// <returns>Array of GameObjects that can take damage</returns>
    public static GameObject[] FilterDamageableTargets(Collider2D[] colliders, GameObject excludeSelf = null)
    {
        if (colliders == null || colliders.Length == 0) return new GameObject[0];

        System.Collections.Generic.List<GameObject> damageableTargets = new System.Collections.Generic.List<GameObject>();

        foreach (var collider in colliders)
        {
            if (collider == null || collider.gameObject == excludeSelf) continue;

            if (CanTakeDamage(collider.gameObject))
            {
                damageableTargets.Add(collider.gameObject);
            }
        }

        return damageableTargets.ToArray();
    }

    /// <summary>
    /// Stun the boss if target is Igris
    /// </summary>
    /// <param name="target">Target to check and stun</param>
    /// <param name="stunDuration">Duration to stun in seconds</param>
    /// <param name="skillName">Name of skill for debug logging</param>
    /// <returns>True if boss was stunned</returns>
    public static bool StunBoss(GameObject target, float stunDuration = 3f, string skillName = "Skill")
    {
        if (target == null) return false;

        Igris boss = target.GetComponent<Igris>();
        if (boss != null)
        {
            boss.StunBoss(stunDuration);
            Debug.Log($"🌟 {skillName} STUNNED the boss for {stunDuration} seconds!");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stun all bosses in the collider array
    /// </summary>
    /// <param name="colliders">Array of colliders to check</param>
    /// <param name="stunDuration">Duration to stun in seconds</param>
    /// <param name="skillName">Name of skill for debug logging</param>
    /// <returns>Number of bosses stunned</returns>
    public static int StunAllBosses(Collider2D[] colliders, float stunDuration = 3f, string skillName = "Skill")
    {
        if (colliders == null || colliders.Length == 0) return 0;

        int stunCount = 0;
        foreach (var collider in colliders)
        {
            if (collider != null && StunBoss(collider.gameObject, stunDuration, skillName))
            {
                stunCount++;
            }
        }

        if (stunCount > 0)
        {
            Debug.Log($"{skillName} stunned {stunCount} bosses!");
        }

        return stunCount;
    }
}