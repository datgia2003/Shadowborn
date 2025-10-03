using UnityEngine;

public class Pickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Coin;
    public int value = 1;

    [Header("Audio (Optional - uses AudioManager defaults)")]
    public AudioClip customPickupSound;

    [Header("Effects")]
    public GameObject pickupFX;
    public float destroyDelay = 0.1f;

    private bool hasBeenCollected = false;

    public enum PickupType
    {
        Coin,
        Item,
        Chest,
        Key,
        Potion,
        Upgrade
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenCollected) return;

        if (other.CompareTag("Player"))
        {
            CollectPickup();
        }
    }

    void CollectPickup()
    {
        hasBeenCollected = true;

        // Play pickup sound
        PlayPickupSound();

        // Spawn pickup effect
        if (pickupFX != null)
        {
            Instantiate(pickupFX, transform.position, Quaternion.identity);
        }

        // Handle pickup logic based on type
        HandlePickupLogic();

        // Destroy pickup object
        Destroy(gameObject, destroyDelay);
    }

    void PlayPickupSound()
    {
        if (AudioManager.Instance == null) return;

        if (customPickupSound != null)
        {
            AudioManager.Instance.PlaySoundOneShot(customPickupSound);
        }
        else
        {
            switch (pickupType)
            {
                case PickupType.Coin:
                    AudioManager.Instance.PlayCoinPickup();
                    break;
                case PickupType.Chest:
                    AudioManager.Instance.PlayChestOpen();
                    break;
                case PickupType.Item:
                case PickupType.Key:
                case PickupType.Potion:
                case PickupType.Upgrade:
                    AudioManager.Instance.PlayItemPickup();
                    break;
            }
        }
    }

    void HandlePickupLogic()
    {
        switch (pickupType)
        {
            case PickupType.Coin:
                // Add coins to player inventory
                if (PlayerStats.Instance != null)
                {
                    // PlayerStats.Instance.AddCoins(value);
                    Debug.Log($"Collected {value} coins!");
                }
                break;

            case PickupType.Item:
                Debug.Log($"Collected item: {gameObject.name}");
                break;

            case PickupType.Chest:
                Debug.Log("Opened chest!");
                break;

            case PickupType.Key:
                Debug.Log("Collected key!");
                break;

            case PickupType.Potion:
                Debug.Log("Collected potion!");
                break;

            case PickupType.Upgrade:
                Debug.Log("Collected upgrade!");
                break;
        }
    }
}