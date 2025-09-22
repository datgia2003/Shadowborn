using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    public GameObject chestPrefab;
    public Transform[] spawnPoints; // Kéo 5 vị trí vào inspector
    public int chestCount = 2;

    void Start()
    {
        SpawnRandomChests();
    }

    void SpawnRandomChests()
    {
        // Shuffle mảng vị trí
        var points = spawnPoints.Clone() as Transform[];
        for (int i = 0; i < points.Length; i++)
        {
            int rnd = Random.Range(i, points.Length);
            var temp = points[i];
            points[i] = points[rnd];
            points[rnd] = temp;
        }
        // Spawn chest tại 3 vị trí đầu
        for (int i = 0; i < Mathf.Min(chestCount, points.Length); i++)
        {
            Instantiate(chestPrefab, points[i].position, Quaternion.identity);
        }
    }
}