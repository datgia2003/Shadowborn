using UnityEngine;

/// <summary>
/// Component attached to enemies spawned by wave zones
/// Tracks which wave zone spawned this enemy
/// </summary>
public class EnemyWaveInfo : MonoBehaviour
{
    [ReadOnly] public EnemyWaveZone waveZone;
    [ReadOnly] public string waveName;
    [ReadOnly] public int waveId;
    [ReadOnly] public int waveDifficulty;
    
    void OnDestroy()
    {
        // Notify wave zone when enemy is destroyed
        if (waveZone != null)
        {
            // The wave zone will handle checking if wave is cleared
        }
    }
}