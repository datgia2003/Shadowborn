using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Wave Manager - Quản lý nhiều waves trong room
/// Mỗi wave trigger theo thứ tự khi player đi qua
/// </summary>
public class SimpleWaveManager : MonoBehaviour
{
    [Header("🌊 Wave Management")]
    [Tooltip("All wave spawners in this room")]
    public SimpleWaveSpawner[] waveSpawners;

    [Tooltip("Auto-find wave spawners in children")]
    public bool autoFindWaves = true;

    [Header("📊 Progress")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool allWavesComplete = false;

    [Header("⚙️ Settings")]
    [Tooltip("Show debug info")]
    public bool showDebugInfo = true;

    void Start()
    {
        if (autoFindWaves)
        {
            waveSpawners = GetComponentsInChildren<SimpleWaveSpawner>();
        }

        // Block exit initially
        BlockExit();

        if (showDebugInfo)
        {
            Debug.Log($"🏠 Room initialized with {waveSpawners.Length} waves - Exit blocked");
        }
    }

    void Update()
    {
        CheckWaveProgress();
    }

    /// <summary>
    /// Block exit trigger so player can't leave until waves complete
    /// </summary>
    void BlockExit()
    {
        ExitTrigger exitTrigger = GetComponentInChildren<ExitTrigger>();
        if (exitTrigger != null)
        {
            // Disable the collider to prevent triggering
            Collider2D exitCollider = exitTrigger.GetComponent<Collider2D>();
            if (exitCollider != null)
            {
                exitCollider.enabled = false;
                if (showDebugInfo)
                {
                    Debug.Log("🚫 Exit blocked - must clear all waves first!");
                }
            }
        }
    }

    /// <summary>
    /// Unblock exit trigger when all waves are complete
    /// </summary>
    void UnblockExit()
    {
        ExitTrigger exitTrigger = GetComponentInChildren<ExitTrigger>();
        if (exitTrigger != null)
        {
            // Re-enable the collider
            Collider2D exitCollider = exitTrigger.GetComponent<Collider2D>();
            if (exitCollider != null)
            {
                exitCollider.enabled = true;
                exitTrigger.ResetTrigger(); // Reset to allow triggering
                if (showDebugInfo)
                {
                    Debug.Log("✅ Exit unblocked - player can now proceed!");
                }
            }
        }
    }

    /// <summary>
    /// Check wave completion progress
    /// </summary>
    void CheckWaveProgress()
    {
        if (allWavesComplete) return;

        // Check if current wave is complete and there are still active enemies
        if (currentWaveIndex < waveSpawners.Length)
        {
            SimpleWaveSpawner currentWave = waveSpawners[currentWaveIndex];

            // Check if current wave is triggered and enemies are defeated
            if (currentWave.IsTriggered() && currentWave.AreAllEnemiesDefeated())
            {
                OnWaveComplete(currentWaveIndex);
                currentWaveIndex++;

                if (currentWaveIndex >= waveSpawners.Length)
                {
                    // Double check - make sure ALL waves are truly clear
                    if (AreAllWavesCompletelyCleared())
                    {
                        OnAllWavesComplete();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if all waves are completely cleared (no enemies remaining)
    /// </summary>
    bool AreAllWavesCompletelyCleared()
    {
        foreach (SimpleWaveSpawner wave in waveSpawners)
        {
            if (!wave.AreAllEnemiesDefeated())
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Called when a wave is completed
    /// </summary>
    void OnWaveComplete(int waveIndex)
    {
        if (showDebugInfo)
        {
            Debug.Log($"✅ Wave {waveIndex + 1} completed!");
        }
    }

    /// <summary>
    /// Called when all waves are completed
    /// </summary>
    void OnAllWavesComplete()
    {
        allWavesComplete = true;

        if (showDebugInfo)
        {
            Debug.Log($"🎉 All waves complete! Room cleared!");
        }

        // Unblock room exit
        UnblockExit();
    }

    /// <summary>
    /// Get current wave progress
    /// </summary>
    public string GetWaveProgress()
    {
        return $"{currentWaveIndex}/{waveSpawners.Length}";
    }

    /// <summary>
    /// Are all waves complete?
    /// </summary>
    public bool IsComplete()
    {
        return allWavesComplete;
    }

    /// <summary>
    /// Reset all waves (for testing)
    /// </summary>
    [ContextMenu("Reset All Waves")]
    public void ResetAllWaves()
    {
        currentWaveIndex = 0;
        allWavesComplete = false;

        foreach (SimpleWaveSpawner wave in waveSpawners)
        {
            if (wave != null)
                wave.ResetWave();
        }

        Debug.Log($"🔄 Reset all waves in {gameObject.name}");
    }

    void OnDrawGizmosSelected()
    {
        // Draw wave progression
        if (waveSpawners != null)
        {
#if UNITY_EDITOR
            string progressInfo = $"Room Progress: {GetWaveProgress()}\n" +
                                $"Current Wave: {currentWaveIndex + 1}\n" +
                                $"Complete: {(allWavesComplete ? "Yes" : "No")}";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 5f, progressInfo);
#endif
        }
    }
}