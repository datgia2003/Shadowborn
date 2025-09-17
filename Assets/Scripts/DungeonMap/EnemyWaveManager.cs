using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Enemy Wave Manager - manages multiple wave zones in a room
/// Handles wave progression, timing, and conditions
/// </summary>
public class EnemyWaveManager : MonoBehaviour
{
    [Header("🌊 Wave Management")]
    [Tooltip("Wave progression mode")]
    public WaveProgressionMode progressionMode = WaveProgressionMode.PlayerTriggered;

    [Tooltip("Auto-start first wave when player enters room")]
    public bool autoStartFirstWave = false;

    [Tooltip("Delay before first wave auto-start")]
    public float firstWaveDelay = 1f;

    [Tooltip("Time between automatic waves")]
    public float timeBetweenWaves = 3f;

    [Header("🎯 Wave Conditions")]
    [Tooltip("Must clear previous wave before next can trigger")]
    public bool requirePreviousWaveCleared = true;

    [Tooltip("Maximum concurrent active waves")]
    public int maxConcurrentWaves = 1;

    [Tooltip("Show wave progress UI")]
    public bool showWaveProgress = true;

    [Header("🏆 Completion")]
    [Tooltip("Action when all waves completed")]
    public WaveCompletionAction completionAction = WaveCompletionAction.OpenExit;

    [Tooltip("Delay before completion action")]
    public float completionDelay = 2f;

    [Tooltip("Reward multiplier for completing all waves")]
    public float waveCompletionRewardMultiplier = 1.5f;

    [Header("⚙️ Debug")]
    public bool showDebugInfo = true;
    public bool showWaveGizmos = true;

    // Wave progression modes
    public enum WaveProgressionMode
    {
        PlayerTriggered,    // Player enters zones to trigger
        Sequential,         // Waves trigger automatically in sequence
        Timed,             // Waves trigger on timer
        Mixed              // Combination of triggers
    }

    public enum WaveCompletionAction
    {
        None,              // Do nothing
        OpenExit,          // Enable room exit
        SpawnReward,       // Spawn rewards
        TriggerEvent       // Custom event
    }

    // Private variables
    private List<EnemyWaveZone> waveZones = new List<EnemyWaveZone>();
    private List<EnemyWaveZone> activeWaves = new List<EnemyWaveZone>();
    private int currentWaveIndex = 0;
    private bool allWavesCompleted = false;
    private Coroutine autoWaveCoroutine;

    // Events
    public System.Action<EnemyWaveZone> OnWaveStarted;
    public System.Action<EnemyWaveZone> OnWaveCompleted;
    public System.Action<EnemyWaveZone> OnWaveCleared;
    public System.Action OnAllWavesCompleted;

    void Start()
    {
        // Find all wave zones in room
        FindWaveZones();

        // Setup wave progression
        SetupWaveProgression();

        if (showDebugInfo)
        {
            Debug.Log($"🌊 EnemyWaveManager initialized with {waveZones.Count} waves in {progressionMode} mode");
        }
    }

    void Update()
    {
        // Check wave completion status
        UpdateWaveStatus();

        // Handle automatic progression
        HandleAutoProgression();
    }

    /// <summary>
    /// Register a wave zone with this manager
    /// </summary>
    public void RegisterWaveZone(EnemyWaveZone waveZone)
    {
        if (!waveZones.Contains(waveZone))
        {
            waveZones.Add(waveZone);

            // Subscribe to wave events
            waveZone.OnWaveTriggered += HandleWaveTriggered;
            waveZone.OnWaveCompleted += HandleWaveCompleted;
            waveZone.OnWaveCleared += HandleWaveCleared;

            // Sort by wave ID
            waveZones = waveZones.OrderBy(w => w.waveId).ToList();

            if (showDebugInfo)
                Debug.Log($"📋 Registered wave zone: {waveZone.waveName} (ID: {waveZone.waveId})");
        }
    }

    /// <summary>
    /// Find and register all wave zones in the room
    /// </summary>
    private void FindWaveZones()
    {
        EnemyWaveZone[] zones = GetComponentsInChildren<EnemyWaveZone>();

        foreach (var zone in zones)
        {
            RegisterWaveZone(zone);
        }

        // Also search in current room if we're part of room system
        if (zones.Length == 0 && RoomManager.Instance != null)
        {
            GameObject currentRoom = RoomManager.Instance.GetCurrentRoom();
            if (currentRoom != null)
            {
                zones = currentRoom.GetComponentsInChildren<EnemyWaveZone>();
                foreach (var zone in zones)
                {
                    RegisterWaveZone(zone);
                }
            }
        }
    }

    /// <summary>
    /// Setup wave progression based on mode
    /// </summary>
    private void SetupWaveProgression()
    {
        switch (progressionMode)
        {
            case WaveProgressionMode.PlayerTriggered:
                // Waves trigger when player enters zones - no additional setup needed
                break;

            case WaveProgressionMode.Sequential:
                // Disable auto-trigger on all zones except first
                DisableAutoTriggers();
                if (autoStartFirstWave)
                {
                    StartCoroutine(AutoStartFirstWave());
                }
                break;

            case WaveProgressionMode.Timed:
                // Start automatic wave progression
                if (autoWaveCoroutine != null)
                    StopCoroutine(autoWaveCoroutine);
                autoWaveCoroutine = StartCoroutine(TimedWaveProgression());
                break;

            case WaveProgressionMode.Mixed:
                // Custom logic - implement based on specific needs
                break;
        }
    }

    /// <summary>
    /// Disable auto-triggers on wave zones for manual control
    /// </summary>
    private void DisableAutoTriggers()
    {
        foreach (var wave in waveZones)
        {
            wave.triggerOnPlayerEnter = false;
        }
    }

    /// <summary>
    /// Auto-start first wave after delay
    /// </summary>
    private IEnumerator AutoStartFirstWave()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        if (waveZones.Count > 0)
        {
            TriggerWave(waveZones[0]);
        }
    }

    /// <summary>
    /// Timed wave progression coroutine
    /// </summary>
    private IEnumerator TimedWaveProgression()
    {
        yield return new WaitForSeconds(firstWaveDelay);

        for (int i = 0; i < waveZones.Count; i++)
        {
            TriggerWave(waveZones[i]);

            // Wait for wave to complete or timeout
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    /// <summary>
    /// Trigger specific wave
    /// </summary>
    public void TriggerWave(EnemyWaveZone waveZone)
    {
        if (waveZone == null) return;

        // Check if we can trigger this wave
        if (!CanTriggerWave(waveZone))
        {
            if (showDebugInfo)
                Debug.Log($"❌ Cannot trigger wave {waveZone.waveName} - conditions not met");
            return;
        }

        waveZone.TriggerWave();
    }

    /// <summary>
    /// Trigger next wave in sequence
    /// </summary>
    public void TriggerNextWave()
    {
        if (currentWaveIndex < waveZones.Count)
        {
            TriggerWave(waveZones[currentWaveIndex]);
        }
    }

    /// <summary>
    /// Check if wave can be triggered based on conditions
    /// </summary>
    private bool CanTriggerWave(EnemyWaveZone waveZone)
    {
        // Check max concurrent waves
        if (activeWaves.Count >= maxConcurrentWaves)
        {
            return false;
        }

        // Check if previous wave must be cleared
        if (requirePreviousWaveCleared && waveZone.waveId > 1)
        {
            var previousWave = waveZones.FirstOrDefault(w => w.waveId == waveZone.waveId - 1);
            if (previousWave != null && !previousWave.IsWaveCleared())
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Handle wave triggered event
    /// </summary>
    private void HandleWaveTriggered(EnemyWaveZone waveZone)
    {
        activeWaves.Add(waveZone);
        OnWaveStarted?.Invoke(waveZone);

        if (showDebugInfo)
            Debug.Log($"🌊 Wave started: {waveZone.waveName} ({activeWaves.Count} active waves)");
    }

    /// <summary>
    /// Handle wave completed event
    /// </summary>
    private void HandleWaveCompleted(EnemyWaveZone waveZone)
    {
        OnWaveCompleted?.Invoke(waveZone);

        if (showDebugInfo)
            Debug.Log($"✅ Wave completed: {waveZone.waveName}");

        // Trigger next wave in sequential mode
        if (progressionMode == WaveProgressionMode.Sequential)
        {
            currentWaveIndex++;
            StartCoroutine(TriggerNextWaveDelayed());
        }
    }

    /// <summary>
    /// Handle wave cleared event
    /// </summary>
    private void HandleWaveCleared(EnemyWaveZone waveZone)
    {
        activeWaves.Remove(waveZone);
        OnWaveCleared?.Invoke(waveZone);

        if (showDebugInfo)
            Debug.Log($"🏆 Wave cleared: {waveZone.waveName} ({activeWaves.Count} waves remaining)");
    }

    /// <summary>
    /// Trigger next wave with delay
    /// </summary>
    private IEnumerator TriggerNextWaveDelayed()
    {
        yield return new WaitForSeconds(timeBetweenWaves);
        TriggerNextWave();
    }

    /// <summary>
    /// Update wave status and check completion
    /// </summary>
    private void UpdateWaveStatus()
    {
        // Update active waves list
        activeWaves.RemoveAll(wave => wave == null || wave.IsWaveCleared());

        // Check if all waves are completed
        if (!allWavesCompleted && AreAllWavesCompleted())
        {
            allWavesCompleted = true;
            StartCoroutine(HandleAllWavesCompleted());
        }
    }

    /// <summary>
    /// Handle automatic progression logic
    /// </summary>
    private void HandleAutoProgression()
    {
        // Custom auto-progression logic can go here
        // For example, triggering waves based on player position, time, etc.
    }

    /// <summary>
    /// Check if all waves are completed
    /// </summary>
    public bool AreAllWavesCompleted()
    {
        foreach (var wave in waveZones)
        {
            if (!wave.IsWaveCleared())
            {
                return false;
            }
        }
        return waveZones.Count > 0;
    }

    /// <summary>
    /// Handle all waves completed
    /// </summary>
    private IEnumerator HandleAllWavesCompleted()
    {
        if (showDebugInfo)
            Debug.Log($"🎉 All waves completed! Executing completion action: {completionAction}");

        OnAllWavesCompleted?.Invoke();

        // Delay before action
        yield return new WaitForSeconds(completionDelay);

        // Execute completion action
        switch (completionAction)
        {
            case WaveCompletionAction.OpenExit:
                OpenRoomExit();
                break;

            case WaveCompletionAction.SpawnReward:
                SpawnCompletionReward();
                break;

            case WaveCompletionAction.TriggerEvent:
                TriggerCompletionEvent();
                break;
        }
    }

    /// <summary>
    /// Open room exit
    /// </summary>
    private void OpenRoomExit()
    {
        // Find and activate room exit
        ExitTrigger exitTrigger = FindObjectOfType<ExitTrigger>();
        if (exitTrigger != null)
        {
            exitTrigger.gameObject.SetActive(true);
            if (showDebugInfo)
                Debug.Log($"🚪 Room exit opened!");
        }
    }

    /// <summary>
    /// Spawn completion rewards
    /// </summary>
    private void SpawnCompletionReward()
    {
        // TODO: Implement reward spawning
        if (showDebugInfo)
            Debug.Log($"💎 Spawning completion rewards (multiplier: {waveCompletionRewardMultiplier}x)");
    }

    /// <summary>
    /// Trigger custom completion event
    /// </summary>
    private void TriggerCompletionEvent()
    {
        // TODO: Implement custom event system
        if (showDebugInfo)
            Debug.Log($"🎭 Triggering custom completion event");
    }

    /// <summary>
    /// Get wave zone by ID
    /// </summary>
    public EnemyWaveZone GetWaveZone(int waveId)
    {
        return waveZones.FirstOrDefault(w => w.waveId == waveId);
    }

    /// <summary>
    /// Get all wave zones
    /// </summary>
    public List<EnemyWaveZone> GetAllWaveZones()
    {
        return new List<EnemyWaveZone>(waveZones);
    }

    /// <summary>
    /// Get active wave count
    /// </summary>
    public int GetActiveWaveCount()
    {
        return activeWaves.Count;
    }

    /// <summary>
    /// Get total enemy count across all waves
    /// </summary>
    public int GetTotalEnemyCount()
    {
        return waveZones.Sum(w => w.GetAliveEnemyCount());
    }

    /// <summary>
    /// Reset all waves
    /// </summary>
    [ContextMenu("Reset All Waves")]
    public void ResetAllWaves()
    {
        foreach (var wave in waveZones)
        {
            wave.ResetWave();
        }

        activeWaves.Clear();
        currentWaveIndex = 0;
        allWavesCompleted = false;

        if (autoWaveCoroutine != null)
        {
            StopCoroutine(autoWaveCoroutine);
        }

        SetupWaveProgression();

        if (showDebugInfo)
            Debug.Log($"🔄 All waves reset");
    }

    void OnDrawGizmos()
    {
        if (!showWaveGizmos) return;

        // Draw connections between sequential waves
        if (progressionMode == WaveProgressionMode.Sequential && waveZones.Count > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < waveZones.Count - 1; i++)
            {
                if (waveZones[i] != null && waveZones[i + 1] != null)
                {
                    Gizmos.DrawLine(waveZones[i].transform.position, waveZones[i + 1].transform.position);
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Find Wave Zones")]
    public void EditorFindWaveZones()
    {
        FindWaveZones();
    }

    [ContextMenu("Test Trigger All Waves")]
    public void TestTriggerAllWaves()
    {
        foreach (var wave in waveZones)
        {
            wave.ForceTrigger();
        }
    }
#endif
}