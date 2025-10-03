using UnityEngine;

public class SimplePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public bool isCoin = false; // true = coin, false = item
    public string playerTag = "Player";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"[SimplePickup] Player picked up {(isCoin ? "coin" : "item")}");

            // Play appropriate sound
            if (SimpleAudioManager.Instance != null)
            {
                if (isCoin)
                {
                    Debug.Log("[SimplePickup] Playing coin pickup sound");
                    SimpleAudioManager.Instance.PlayCoinPickup();
                }
                else
                {
                    Debug.Log("[SimplePickup] Playing item pickup sound");
                    SimpleAudioManager.Instance.PlayItemPickup();
                }
            }
            else
            {
                Debug.LogError("[SimplePickup] SimpleAudioManager.Instance is null!");
            }

            // Add your pickup logic here (increase coin count, add to inventory, etc.)

            // Destroy pickup
            Destroy(gameObject);
        }
    }
}