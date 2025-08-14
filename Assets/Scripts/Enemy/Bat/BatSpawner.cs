using UnityEngine;

public class BatSpawner : MonoBehaviour
{
    public GameObject batPrefab;
    public int batCount = 5;
    public Vector2 spawnAreaMin = new Vector2(-5, 0);
    public Vector2 spawnAreaMax = new Vector2(5, 3);

    void Start()
    {
        for (int i = 0; i < batCount; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );
            Instantiate(batPrefab, pos, Quaternion.identity);
        }
    }
}
