using UnityEngine;

[CreateAssetMenu(menuName = "Buff/DoubleDodgeBuff")]
public class DoubleDodgeBuff : Buff
{
    public float cooldownMultiplier = 0.5f; // giảm 50% thời gian hồi
    public override void Apply(PlayerController player)
    {
        if (player != null)
        {
            var cooldownManager = GameObject.FindObjectOfType<SkillCooldownManager>();
            if (cooldownManager != null)
            {
                cooldownManager.SetSkillCooldownMultiplier("Dodge", cooldownMultiplier);
            }
        }
    }
}
