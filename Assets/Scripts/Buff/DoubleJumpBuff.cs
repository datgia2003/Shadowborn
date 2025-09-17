using UnityEngine;

[CreateAssetMenu(menuName = "Buff/DoubleJumpBuff")]
public class DoubleJumpBuff : Buff
{
    public override void Apply(PlayerController player)
    {
        if (player != null)
        {
            player.canDoubleJump = true;
        }
    }
}
