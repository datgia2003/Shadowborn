
using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    public int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        var itemManager = FindObjectOfType<ItemManager>();
        if (itemManager != null)
        {
            itemManager.AddHealthPotion(amount);
            Destroy(gameObject);
        }
    }
}
