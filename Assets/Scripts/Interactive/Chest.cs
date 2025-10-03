using System.Collections;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    public bool isOpen = false;
    public GameObject[] rewards; // Items to spawn when opened
    public Transform spawnPoint; // Where to spawn items
    
    [Header("Animation")]
    public Animator chestAnimator;
    public string openAnimationTrigger = "Open";
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip creakSound; // Optional creak sound before opening
    
    [Header("Effects")]
    public GameObject openFX;
    public GameObject sparklesFX; // Continuous sparkles when closed
    
    [Header("Interaction")]
    public GameObject interactionPrompt; // UI prompt "Press F to open"
    public KeyCode interactionKey = KeyCode.F;
    
    private bool playerInRange = false;
    private bool isOpening = false;

    void Start()
    {
        // Show sparkles if chest is closed
        if (!isOpen && sparklesFX != null)
        {
            sparklesFX.SetActive(true);
        }
    }

    void Update()
    {
        // Handle player interaction
        if (playerInRange && !isOpen && !isOpening && Input.GetKeyDown(interactionKey))
        {
            OpenChest();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            playerInRange = true;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
        }
    }

    public void OpenChest()
    {
        if (isOpen || isOpening) return;
        
        isOpening = true;
        StartCoroutine(OpenChestSequence());
    }

    IEnumerator OpenChestSequence()
    {
        // Hide interaction prompt
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);

        // Play creak sound first (optional)
        if (creakSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySoundOneShot(creakSound, 0.8f);
            yield return new WaitForSeconds(0.2f);
        }

        // Trigger opening animation
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(openAnimationTrigger);
        }

        // Wait a bit for animation to start
        yield return new WaitForSeconds(0.3f);

        // Play main open sound
        if (AudioManager.Instance != null)
        {
            if (openSound != null)
                AudioManager.Instance.PlaySoundOneShot(openSound);
            else
                AudioManager.Instance.PlayChestOpen();
        }

        // Spawn opening effects
        if (openFX != null)
        {
            Instantiate(openFX, transform.position, Quaternion.identity);
        }

        // Hide sparkles
        if (sparklesFX != null)
        {
            sparklesFX.SetActive(false);
        }

        // Wait for animation peak
        yield return new WaitForSeconds(0.5f);

        // Spawn rewards
        SpawnRewards();

        // Mark as opened
        isOpen = true;
        isOpening = false;

        Debug.Log("Chest opened!");
    }

    void SpawnRewards()
    {
        Vector3 baseSpawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up;
        
        for (int i = 0; i < rewards.Length; i++)
        {
            if (rewards[i] != null)
            {
                // Spread items out slightly
                Vector3 spawnPos = baseSpawnPos + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0f, 0.3f),
                    0f
                );
                
                GameObject reward = Instantiate(rewards[i], spawnPos, Quaternion.identity);
                
                // Add slight random velocity to make items scatter
                Rigidbody2D rb = reward.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = new Vector2(
                        Random.Range(-2f, 2f),
                        Random.Range(1f, 3f)
                    );
                }
            }
        }
    }

    // Public method to open chest from external scripts
    public void ForceOpen()
    {
        if (!isOpen)
        {
            OpenChest();
        }
    }
}