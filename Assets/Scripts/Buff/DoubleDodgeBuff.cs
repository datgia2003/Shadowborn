using UnityEngine;

[CreateAssetMenu(menuName = "Buff/DoubleDodgeBuff")]
public class DoubleDodgeBuff : Buff
{
    public override void Apply(PlayerController player)
    {
        if (player != null)
        {
            player.dodgeCharges = 2;
        }
    }
}
