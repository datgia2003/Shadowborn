using UnityEngine;

[CreateAssetMenu(menuName = "Buff/HPMPBuff")]
public class HPMPBuff : Buff
{
    public float percentIncrease = 0.3f;
    public override void Apply(PlayerController player)
    {
        if (player != null)
        {
            player.maxHP = Mathf.RoundToInt(player.maxHP * (1f + percentIncrease));
            player.maxMP = Mathf.RoundToInt(player.maxMP * (1f + percentIncrease));
            player.currentHP = player.maxHP;
            player.currentMP = player.maxMP;
        }
    }
}
