using UnityEngine;

[CreateAssetMenu(menuName = "Buff/StatPointBuff")]
public class StatPointBuff : Buff
{
    public int statPoints = 15;
    public override void Apply(PlayerController player)
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddAvailablePoints(statPoints);
        }
    }
}
