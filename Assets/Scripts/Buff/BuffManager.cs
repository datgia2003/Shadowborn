using UnityEngine;
using System.Collections.Generic;

public class BuffManager : MonoBehaviour
{
    public List<Buff> allBuffs;
    public Buff statPointBuff;

    // Danh sách buff đã chọn
    public List<Buff> chosenBuffs = new List<Buff>();
    /// <summary>
    /// Kiểm tra buff đã được chọn chưa
    /// </summary>
    public bool IsBuffChosen(Buff buff)
    {
        return buff != null && chosenBuffs.Contains(buff);
    }

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
