using UnityEngine;
using System.Collections.Generic;

public class BuffManager : MonoBehaviour
{
    public List<Buff> allBuffs;
    public Buff statPointBuff;

    public Buff[] GetRandomBuffs(int count)
    {
        List<Buff> pool = new List<Buff>(allBuffs);
        pool.Remove(statPointBuff); // StatPointBuff is only for conversion
        Buff[] result = new Buff[count];
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result[i] = pool[idx];
            pool.RemoveAt(idx);
        }
        return result;
    }
}
