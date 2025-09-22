using UnityEngine;
using UnityEngine.InputSystem;

public class ChestController : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference interactionActionRef; // Kéo action Interaction vào đây
    public string openPanelName = "OpenPanel"; // Tên panel trong Main Canvas

    [Header("Chest Sprites")]
    public Sprite closedChestSprite;
    public Sprite openedChestSprite;
    public SpriteRenderer chestRenderer;

    [Header("Loot Settings")]
    public GameObject[] lootPrefabs;
    [Range(0, 1)] public float[] lootRates;
    public Transform lootSpawnPoint;

    [Header("UI")]
    public GameObject openPanel; // Panel hiện thông báo (child của prefab, có thể là Canvas World Space hoặc TextMeshPro)

    private bool isOpened = false;
    private bool playerNearby = false;

    void Awake()
    {
        if (chestRenderer == null)
            chestRenderer = GetComponent<SpriteRenderer>();
        // Nếu openPanel chưa gán từ inspector, tự tìm trong Main Canvas
        if (openPanel == null)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                openPanel = canvas.transform.Find(openPanelName)?.gameObject;
                Debug.Log(openPanel != null ? $"Chest found panel: {openPanel.name}" : "Chest panel not found!");
            }
        }
    }

    void Update()
    {
        if (playerNearby && !isOpened)
        {
            if (openPanel != null)
            {
                openPanel.SetActive(true);
                Debug.Log($"OpenPanel enabled for chest: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"OpenPanel is null for chest: {gameObject.name}");
            }
            if (interactionActionRef != null && interactionActionRef.action != null && interactionActionRef.action.WasPressedThisFrame())
            {
                OpenChest();
            }
        }
        // Không tắt panel trong Update nếu đã mở rương, chỉ tắt khi player rời khỏi vùng trigger hoặc khi mở rương
    }

    private void OpenChest()
    {
        isOpened = true;
        if (chestRenderer != null && openedChestSprite != null)
            chestRenderer.sprite = openedChestSprite;

        // Spawn tối đa 2 vật phẩm theo tỉ lệ
        int maxLoot = 2;
        int lootSpawned = 0;
        bool[] spawned = new bool[lootPrefabs.Length];
        for (int i = 0; i < lootPrefabs.Length && lootSpawned < maxLoot; i++)
        {
            if (Random.value < lootRates[i])
            {
                var lootObj = Instantiate(lootPrefabs[i], lootSpawnPoint ? lootSpawnPoint.position : transform.position, Quaternion.identity);
                var rb = lootObj.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(8f, 12f));
                }
                // Nếu là Coin, random số lượng coin
                var coin = lootObj.GetComponent<CoinPickup>();
                if (coin != null)
                {
                    coin.amount = Random.Range(20, 51); // 20-50 coin
                }
                lootSpawned++;
                spawned[i] = true;
            }
        }
        // Nếu không spawn được vật phẩm nào, đảm bảo luôn có ít nhất 1 loot
        if (lootSpawned == 0)
        {
            int lootIndex = GetRandomLootIndex();
            var lootObj = Instantiate(lootPrefabs[lootIndex], lootSpawnPoint ? lootSpawnPoint.position : transform.position, Quaternion.identity);
            var rb = lootObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(8f, 12f));
            }
            var coin = lootObj.GetComponent<CoinPickup>();
            if (coin != null)
            {
                coin.amount = Random.Range(20, 51);
            }
        }

        if (openPanel != null)
        {
            openPanel.SetActive(false);
            Debug.Log($"OpenPanel disabled for chest: {gameObject.name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log($"Player entered chest trigger: {gameObject.name}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (openPanel != null)
            {
                openPanel.SetActive(false);
                Debug.Log($"OpenPanel disabled for chest: {gameObject.name} (player exited)");
            }
        }
    }

    private int GetRandomLootIndex()
    {
        if (lootRates == null || lootRates.Length != lootPrefabs.Length) return 0;
        float rand = Random.value;
        float sum = 0f;
        for (int i = 0; i < lootRates.Length; i++)
        {
            sum += lootRates[i];
            if (rand <= sum)
                return i;
        }
        return lootRates.Length - 1;
    }
}

internal class CoinPickup
{
    public int amount;
}