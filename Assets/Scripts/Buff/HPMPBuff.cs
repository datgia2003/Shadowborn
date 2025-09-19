using UnityEngine;

[CreateAssetMenu(menuName = "Buff/HPMPBuff")]
public class HPMPBuff : Buff
{
    public float percentIncrease = 0.3f;
    public override void Apply(PlayerController player)
    {
        if (player != null)
        {
            var res = player.GetComponent<PlayerResources>();
            if (res != null)
            {
                res.maxHealth = Mathf.RoundToInt(res.maxHealth * (1f + percentIncrease));
                res.maxMana = Mathf.RoundToInt(res.maxMana * (1f + percentIncrease));
                res.SetCurrentHealth(res.maxHealth);
                res.AddMana(res.maxMana); // Đảm bảo currentMana = maxMana
            }
        }
    }
}
