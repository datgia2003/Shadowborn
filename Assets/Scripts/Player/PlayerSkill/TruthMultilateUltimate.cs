using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// TruthMultilateUltimate - MUGEN-accurate Ultimate System with 4-state progression
/// States: 3000(Portrait) → 3010(Dash) → 3020(Setup) → 3030(Attack)
/// 
/// FX SPAWNING SYSTEM:
/// 1. SpawnCinematicFX() → Screen-wide effects at camera center
/// 2. SpawnPlayerFX() → Player-relative effects  
/// 3. SpawnFX() → Standard system for intro effects
/// 
/// CAMERA SYSTEM:
/// • Auto-zoom during ultimate to show all FX
/// • Focus on primary target (first enemy hit)
/// • Smooth restore after ultimate ends
/// 
/// AREA TARGETING:
/// • Hits all enemies within camera bounds
/// • No distance requirement from player
/// • Primary target used for camera focus
/// 
/// Port from M.U.G.E.N Ultimate sequence with 60 FPS timing
/// </summary>
public class TruthMultilateUltimate : MonoBehaviour
{
    /// <summary>
    /// Settings for individual FX with customizable properties
    /// INSPECTOR USAGE: All FX settings (offset, scale, rotation, lifetime) can be adjusted in Inspector.
    /// The code will respect these settings and apply any dynamic scaling on top of Inspector values.
    /// </summary>
    [System.Serializable]
    public class UltimateFXSettings
    {
        public GameObject prefab;
        [Tooltip("Position offset from spawn point (Inspector controllable)")]
        public Vector3 offset = Vector3.zero;
        [Tooltip("Base scale for FX (Inspector controllable - dynamic scaling will multiply this)")]
        public Vector3 scale = Vector3.one;
        [Tooltip("Base rotation in degrees (Inspector controllable)")]
        public float rotation = 0f;
        [Tooltip("How long FX lasts in seconds (Inspector controllable)")]
        public float lifetime = 5f;
        [Tooltip("Follow player transform during spawn")]
        public bool followPlayer = false;
    }
    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public AudioSource audioSource;
    public AudioSource voiceSource;     // Riêng cho voice lines
    public CameraShake camShake;
    [Tooltip("FX spawn point - Used for non-ultimate FX only. During ultimate, FX center on primary target")]
    public Transform fxSpawnPoint;      // Điểm spawn FX khi không trong ultimate (ultimate sẽ dùng primary target position)

    [Header("Cinematic Camera")]
    [Tooltip("Main camera for cinematic zoom during ultimate")]
    public Camera mainCamera;
    [Tooltip("Cinemachine Virtual Camera for proper zoom control")]
    public CinemachineVirtualCamera virtualCamera;
    [Tooltip("Camera zoom out size during ultimate (default camera size * zoomMultiplier)")]
    public float cameraZoomMultiplier = 2.5f;
    [Tooltip("Speed of camera zoom transition")]
    public float cameraZoomSpeed = 2f;

    [Header("Player Movement During Ultimate")]
    [Tooltip("Player walk speed during sndImpact2 (when moving away from enemy)")]
    public float ultimateWalkSpeed = 2f;

    [Header("Audio Clips")]
    public AudioClip voiceIntro;        // S950,1 - Voice intro
    public AudioClip voiceDash;         // S1,39 - Voice dash  
    public AudioClip sndImpact1;        // S0,26 - Impact sound 1
    public AudioClip sndImpact2;        // S0,27 - Impact sound 2
    public AudioClip sndImpact3;        // S0,28 - Impact sound 3
    public AudioClip sndImpact4;        // S0,29 - Impact sound 4
    public AudioClip sndSlash1;         // S5,45 - Slash sound 1
    public AudioClip sndSlash2;         // S5,51 - Slash sound 2
    public AudioClip sndAmbient1;       // S2,9 - Ambient loop
    public AudioClip sndAmbient2;       // S3,3 - Ambient loop 2

    [Header("FX Prefabs - Phase 0: Ultimate Portrait (3000)")]
    [Tooltip("MUGEN: Helper 8050 - Ultimate Portrait at pos(0,-70), shows character portrait during startup")]
    public UltimateFXSettings fxPortrait = new UltimateFXSettings { offset = new Vector3(0f, -1.17f, 0f), scale = Vector3.one, lifetime = 1.2f };

    [Tooltip("MUGEN: Helper 50903/50904/50905 - Light/Normal/Dark Color auras, trigger every 4 frames at player pos")]
    public UltimateFXSettings fxColorCycle = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one, lifetime = 1.7f };

    [Tooltip("MUGEN: Helper 30850 - Academy Special VFX, size.mid.pos(-1,-38), scale(0.3,0.3)")]
    public UltimateFXSettings fxAcademy = new UltimateFXSettings { offset = new Vector3(-0.017f, -0.63f, 0f), scale = Vector3.one * 0.3f, lifetime = 1.2f };

    [Tooltip("MUGEN: Explod 30513 - Intro Ground Effect at pos(0,2), scale(0.3,0.4), sprpriority(-10)")]
    public UltimateFXSettings fxIntroGround = new UltimateFXSettings { offset = new Vector3(0f, 0.033f, 0f), scale = new Vector3(0.3f, 0.4f, 1f), lifetime = 1.7f };

    [Tooltip("MUGEN: Explod 6221 - Intro Energy Ring at pos(0,-2), scale(0.6,0.3), sprpriority(5)")]
    public UltimateFXSettings fxIntroRing = new UltimateFXSettings { offset = new Vector3(0f, -0.033f, 0f), scale = new Vector3(0.6f, 0.3f, 1f), lifetime = 1.7f };

    [Header("FX Prefabs - Phase 1: Dash Phase (3010)")]
    [Tooltip("MUGEN: Explod 6850 - Background Overlay at screen center, scale(1.5,1), trans(sub), sprpriority(6)")]
    public UltimateFXSettings fxDashBackground = new UltimateFXSettings { offset = Vector3.zero, scale = new Vector3(1.5f, 1f, 1f), lifetime = 2.0f };

    [Tooltip("Custom: Dash Trail Effect - Following player during dash movement")]
    public UltimateFXSettings fxDashTrail = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one, lifetime = 0.8f, followPlayer = true };

    [Header("FX Prefabs - Phase 2: Setup (3020)")]
    [Tooltip("MUGEN: Explod 3030 - Background Layer 1 at screen center, scale(0.63,0.63), sprpriority(-5)")]
    public UltimateFXSettings fxBackOverlay1 = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.63f, lifetime = 5.0f };

    [Tooltip("MUGEN: Explod 3040 - Background Layer 2 at screen center, scale(0.63,0.63), sprpriority(50)")]
    public UltimateFXSettings fxBackOverlay2 = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.63f, lifetime = 4.5f };

    [Tooltip("MUGEN: Explod 3050 - Background Layer 3 at screen center, scale(0.63,0.63), sprpriority(-50)")]
    public UltimateFXSettings fxBackOverlay3 = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.63f, lifetime = 5.0f };

    [Tooltip("MUGEN: Explod 40198 - Player Weapon Glow 1 at pos(-6,-25), scale(0.8,0.3), sprpriority(21)")]
    public UltimateFXSettings fxWeaponGlow1 = new UltimateFXSettings { offset = new Vector3(-0.1f, -0.42f, 0f), scale = new Vector3(0.8f, 0.3f, 1f), lifetime = 3.0f };

    [Tooltip("MUGEN: Explod 40198 - Player Weapon Glow 2 at pos(10,-20), scale(0.8,0.1), sprpriority(21)")]
    public UltimateFXSettings fxWeaponGlow2 = new UltimateFXSettings { offset = new Vector3(0.167f, -0.33f, 0f), scale = new Vector3(0.8f, 0.1f, 1f), lifetime = 3.0f };

    [Tooltip("MUGEN: Explod 6045 - Player Aura at pos(0,0), scale(0.5,0.5), sprpriority(-2)")]
    public UltimateFXSettings fxPlayerAura = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.5f, lifetime = 3.0f };

    [Header("FX Prefabs - Phase 3: Attack Beams (3030)")]
    [Tooltip("MUGEN: Explod 3060 - Energy Beam 1 at pos(10,200), scale(0.4,0.6), angle(-10°), sprpriority(-4)")]
    public UltimateFXSettings fxBeam1 = new UltimateFXSettings { offset = new Vector3(0.167f, 3.33f, 0f), scale = new Vector3(0.4f, 0.6f, 1f), rotation = -10f, lifetime = 1.0f };

    [Tooltip("MUGEN: Explod 3090 - Energy Core 1 at pos(10,200), scale(0.07,0.6), angle(-10°), sprpriority(-3)")]
    public UltimateFXSettings fxBeam2 = new UltimateFXSettings { offset = new Vector3(0.167f, 3.33f, 0f), scale = new Vector3(0.07f, 0.6f, 1f), rotation = -10f, lifetime = 1.0f };

    [Tooltip("MUGEN: Explod 3080 - Screen Flash Effect at Left pos(0,0), scale(0.4,0.4), sprpriority(70)")]
    public UltimateFXSettings fxSlash = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.4f, lifetime = 1.0f };

    [Tooltip("MUGEN: Explod 7013 - Final Explosion at back pos(0,0), scale(0.2,0.2), sprpriority(70)")]
    public UltimateFXSettings fxExplosion = new UltimateFXSettings { offset = Vector3.zero, scale = Vector3.one * 0.2f, lifetime = 2.0f };

    [Tooltip("Custom: Hit Spark Effect - For enemy damage feedback")]
    public UltimateFXSettings fxHitSpark = new UltimateFXSettings { offset = Vector3.up * 0.5f, scale = Vector3.one * 0.5f, lifetime = 0.3f };

    [Tooltip("Custom: Ground Impact Effect - For area damage")]
    public UltimateFXSettings fxGroundImpact = new UltimateFXSettings { offset = Vector3.down * 0.5f, scale = Vector3.one * 0.8f, lifetime = 1.0f };

    [Header("Ultimate Settings")]
    public float dashDistance = 8f;     // Dash distance in Unity units
    public float dashDuration = 0.1f;   // Dash duration - very fast
    public int totalDamage = 315;       // Total damage (5 hits x 63)
    public int hitCount = 5;            // Number of hits
    public LayerMask enemyLayers;       // Enemy detection layers
    public float hitRadius = 2f;        // Hit detection radius
    public float activationDistance = 4f; // Distance from enemy to activate ultimate

    [Header("Invincibility Settings")]
    [Tooltip("Make player invincible during ultimate (nothitby = 1)")]
    public bool makeInvincible = true;
    [Tooltip("Player collider to disable during ultimate")]
    public Collider2D playerCollider;
    [Tooltip("Invincible layer (enemies can't hit this layer)")]
    public int invincibleLayer = 8; // Default layer for invincible state
    [Tooltip("Original player layer to restore after ultimate")]
    public int originalLayer = 0;   // Default layer

    [Header("Animation Settings")]
    [Tooltip("Animation name for Ultimate intro/portrait phase (anim 3000)")]
    public string ultimateAnimationName = "Ultimate";
    [Tooltip("Animation name for Ultimate attack phase when hitting target (anim 3020)")]
    public string ultimateP2AnimationName = "Ultimate_p2";

    [Header("Requirements")]
    [Tooltip("Require player to be grounded before activating ultimate")]
    public bool requireGrounded = true;
    [Tooltip("Require enemy target to activate ultimate")]
    public bool requiresTarget = true;
    public float maxTargetDistance = 10f;   // Max distance to target

    [Header("Timing Settings (seconds)")]
    public float introDuration = 1.2f;      // Phase 1: Intro
    public float dashPhaseDuration = 0.5f;  // Phase 2: Dash
    public float setupDuration = 1.0f;      // Phase 3: Setup
    public float attackDuration = 4.0f;     // Phase 4: Main attack

    // Runtime state
    private bool isUltimateActive = false;
    private float ultimateTimer = 0f;
    private int currentPhase = 0;           // 0=intro, 1=dash, 2=setup, 3=attack
    private Transform targetEnemy = null;
    private Vector3 originalPosition;
    private Vector3 ultimateCenterPosition; // Center position based on first enemy hit
    private Transform primaryTarget = null; // First enemy hit by ultimate - FX center point
    private List<GameObject> spawnedFX = new List<GameObject>();
    private bool hasHitTarget = false;
    private int damagePerHit;
    private List<MonoBehaviour> disabledComponents = new List<MonoBehaviour>(); // Track disabled components

    // Camera cinematic state
    private float originalCameraSize;
    private Vector3 originalCameraPosition;
    private float originalVirtualCameraSize;
    private bool isCameraZoomed = false;
    private Transform originalCameraFollow = null; // Store original follow target

    // Player movement during ultimate
    private bool isMovingAwayFromEnemy = false;
    private Vector3 moveDirection = Vector3.zero;

    // Component references
    private MonoBehaviour playerController;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (camShake == null) camShake = FindObjectOfType<CameraShake>();
        if (fxSpawnPoint == null) fxSpawnPoint = transform;
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (virtualCamera == null) virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        // Store original layer
        originalLayer = gameObject.layer;

        // Setup invincible layer if not set
        if (invincibleLayer == 0)
        {
            invincibleLayer = LayerMask.NameToLayer("PlayerInvincible");
            if (invincibleLayer == -1)
                invincibleLayer = 8; // Fallback to layer 8
        }

        // Tìm player controller để disable trong lúc ultimate
        playerController = GetComponent<MonoBehaviour>();
        var pc = GetComponent("PlayerController");
        if (pc != null) playerController = pc as MonoBehaviour;

        // Calculate damage per hit
        damagePerHit = totalDamage / hitCount;

        // Default enemy layers
        if (enemyLayers.value == 0)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) enemyLayers = 1 << enemyLayer;
        }
    }

    void FixedUpdate()
    {
        // Handle player movement during ultimate
        if (isUltimateActive && isMovingAwayFromEnemy && rb != null)
        {
            // Move player at walk speed in the calculated direction
            Vector3 movement = moveDirection * ultimateWalkSpeed * Time.fixedDeltaTime;
            rb.MovePosition(transform.position + movement);
        }
    }

    /// <summary>
    /// ❌ DISABLED: Direct input bypass - Skills should go through UI system only!
    /// This method was causing skills to execute without mana/cooldown checks
    /// </summary>
    /*
    public void OnUltimate(InputValue value)
    {
        if (value.isPressed && !isUltimateActive)
        {
            // Check requirements
            if (!CanActivateUltimate()) return;

            StartUltimate();
        }
    }
    */

    /// <summary>
    /// Public method for UI system to trigger the skill
    /// </summary>
    public void PlaySkill()
    {
        if (!isUltimateActive)
        {
            // Check requirements (excluding mana - already checked by UI system)
            if (CanActivateUltimate())
            {
                StartUltimate();
            }
        }
    }

    /// <summary>
    /// Public method for UI system to check if skill can be used BEFORE consuming mana/cooldown
    /// </summary>
    public bool CanUseSkill()
    {
        if (isUltimateActive) return false;
        return CanActivateUltimate();
    }

    /// <summary>
    /// Public method to start ultimate (for AI or other systems)
    /// </summary>
    public void StartUltimate()
    {
        if (isUltimateActive) return;

        // Find all enemies in camera bounds for area ultimate
        List<Transform> enemiesInCamera = FindAllEnemiesInCameraBounds();

        // Set primary target for camera focus (first enemy found or nearest)
        if (enemiesInCamera.Count > 0)
        {
            // Use nearest enemy as primary target for camera focus and facing
            targetEnemy = FindNearestEnemy();
            primaryTarget = targetEnemy != null ? targetEnemy : enemiesInCamera[0];

            // Make player face the primary target
            if (primaryTarget != null)
            {
                Vector3 directionToTarget = (primaryTarget.position - transform.position).normalized;
                if (directionToTarget.x < 0)
                {
                    // Target is to the left, face left
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    // Target is to the right, face right
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                }
                Debug.Log($"Ultimate: Player facing towards {primaryTarget.name}. Direction: {(directionToTarget.x < 0 ? "Left" : "Right")}");
            }

            Debug.Log($"Ultimate: Primary target for camera focus set to {primaryTarget.name}. Total enemies in camera: {enemiesInCamera.Count}");
        }
        else
        {
            // No enemies found - ultimate can still activate for cinematic effect
            targetEnemy = null;
            primaryTarget = null;
            Debug.Log("Ultimate: No enemies in camera bounds - activating cinematic ultimate");
        }
        Debug.Log("Ultimate: Starting Truth Multilate Ultimate!");

        // Initialize ultimate
        isUltimateActive = true;
        ultimateTimer = 0f;
        currentPhase = 0;
        originalPosition = transform.position;

        // Set FX center based on primary target if available, otherwise use player position
        if (primaryTarget != null)
        {
            ultimateCenterPosition = primaryTarget.position;
            Debug.Log($"Ultimate center position set to primary target: {ultimateCenterPosition}");
        }
        else
        {
            ultimateCenterPosition = transform.position;
            Debug.Log($"Ultimate center position set to player position: {ultimateCenterPosition}");
        }

        hasHitTarget = false;
        spawnedFX.Clear();

        // Enable invincibility (nothitby = 1) using layer change
        if (makeInvincible)
        {
            gameObject.layer = invincibleLayer;
            Debug.Log($"Player switched to invincible layer: {invincibleLayer}");
        }

        // Disable player movement AND all controls
        if (playerController != null)
            playerController.enabled = false;

        // Disable all input components to prevent any actions
        disabledComponents.Clear();
        var inputComponents = GetComponents<MonoBehaviour>();
        foreach (var component in inputComponents)
        {
            if (component != this && component != null && component.enabled)
            {
                string componentName = component.GetType().Name;
                if (componentName.Contains("Input") || componentName.Contains("Controller") ||
                    componentName.Contains("Combat") || componentName.Contains("Attack") ||
                    componentName.Contains("Skill") || componentName.Contains("Player"))
                {
                    component.enabled = false;
                    disabledComponents.Add(component);
                    Debug.Log($"Disabled component: {componentName}");
                }
            }
        }

        // Stop current velocity
        if (rb != null)
            rb.velocity = Vector2.zero;

        // Start ultimate sequence
        StartCoroutine(UltimateSequence());
    }

    bool CanActivateUltimate()
    {
        // Debug ground check only if required
        if (requireGrounded)
        {
            bool grounded = IsGrounded();
            Debug.Log($"Ultimate CanActivate: Grounded={grounded}, Y={transform.position.y}, VelY={rb?.velocity.y}");

            if (!grounded)
            {
                Debug.Log("Ultimate: Must be grounded!");
                return false;
            }
        }

        // Check if target exists and in range (optional for area ultimate)
        if (requiresTarget)
        {
            var target = FindNearestEnemy();
            if (target != null)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance > maxTargetDistance)
                {
                    Debug.Log($"Ultimate: Primary target too far ({distance:F1} > {maxTargetDistance}), but area ultimate can still activate!");
                    // Don't return false - area ultimate can work without close target
                }
            }
        }

        // Area ultimate requires at least one enemy in camera bounds to prevent animation issues
        List<Transform> enemiesInCamera = FindAllEnemiesInCameraBounds();
        if (enemiesInCamera.Count == 0)
        {
            Debug.Log("Ultimate: No enemies found in camera bounds - cannot activate to prevent animation issues!");
            return false;
        }

        Debug.Log($"Ultimate: All requirements met for area ultimate! Found {enemiesInCamera.Count} enemies in camera.");
        return true;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, maxTargetDistance, enemyLayers);

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (enemy.transform == transform) continue; // Skip self

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy.transform;
            }
        }

        // Fallback: find by tag if no layer setup
        if (nearest == null && enemyLayers.value == 0)
        {
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in taggedEnemies)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist <= maxTargetDistance && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy.transform;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// Find all enemies within camera bounds for area-of-effect ultimate
    /// </summary>
    List<Transform> FindAllEnemiesInCameraBounds()
    {
        List<Transform> enemiesInCamera = new List<Transform>();

        // Get camera bounds
        Camera activeCamera = virtualCamera != null ? mainCamera : Camera.main;
        if (activeCamera == null) return enemiesInCamera;

        float cameraHeight = (virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : activeCamera.orthographicSize) * 2f;
        float cameraWidth = cameraHeight * activeCamera.aspect;

        Vector3 cameraCenter = activeCamera.transform.position;

        // Define camera bounds
        float leftBound = cameraCenter.x - cameraWidth * 0.5f;
        float rightBound = cameraCenter.x + cameraWidth * 0.5f;
        float bottomBound = cameraCenter.y - cameraHeight * 0.5f;
        float topBound = cameraCenter.y + cameraHeight * 0.5f;

        // Find all enemies using layer mask
        Collider2D[] allEnemies = Physics2D.OverlapAreaAll(
            new Vector2(leftBound, bottomBound),
            new Vector2(rightBound, topBound),
            enemyLayers
        );

        foreach (var enemy in allEnemies)
        {
            if (enemy.transform == transform) continue; // Skip self
            enemiesInCamera.Add(enemy.transform);
        }

        // Fallback: find by tag if no layer setup
        if (enemiesInCamera.Count == 0 && enemyLayers.value == 0)
        {
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in taggedEnemies)
            {
                Vector3 enemyPos = enemy.transform.position;
                // Check if enemy is within camera bounds
                if (enemyPos.x >= leftBound && enemyPos.x <= rightBound &&
                    enemyPos.y >= bottomBound && enemyPos.y <= topBound)
                {
                    enemiesInCamera.Add(enemy.transform);
                }
            }
        }

        Debug.Log($"Found {enemiesInCamera.Count} enemies in camera bounds");
        return enemiesInCamera;
    }

    /// <summary>
    /// Find a safe position for player to appear during attack phase
    /// - Within camera bounds for visibility
    /// - Away from enemies to prevent getting stuck
    /// - Close enough to primary target for combat flow
    /// </summary>
    Vector3 FindSafePlayerPosition()
    {
        if (primaryTarget == null)
        {
            return transform.position; // Fallback to current position
        }

        // Get camera bounds for positioning
        Camera cam = Camera.main;
        if (cam == null) return transform.position;

        float cameraHeight = 2f * cam.orthographicSize;
        float cameraWidth = cameraHeight * cam.aspect;
        Vector3 cameraCenter = cam.transform.position;

        // Define safe distance from enemies
        float safeDistance = 2.5f;

        // Try positions in order of preference: left, right, behind
        Vector3[] candidatePositions = new Vector3[]
        {
            primaryTarget.position + Vector3.left * 4f,   // Left side (original direction)
            primaryTarget.position + Vector3.right * 4f,  // Right side
            primaryTarget.position + Vector3.left * 6f,   // Further left
            primaryTarget.position + Vector3.right * 6f,  // Further right
            primaryTarget.position + Vector3.down * 3f,   // Below target
        };

        // Check each candidate position
        foreach (Vector3 candidate in candidatePositions)
        {
            Vector3 adjustedPos = new Vector3(candidate.x, 0f, transform.position.z); // Keep on ground

            // Check if position is within camera bounds
            if (adjustedPos.x < cameraCenter.x - cameraWidth / 2 || adjustedPos.x > cameraCenter.x + cameraWidth / 2)
                continue;

            // Check if position is far enough from all enemies
            bool isSafe = true;
            List<Transform> allEnemies = FindAllEnemiesInCameraBounds();
            foreach (Transform enemy in allEnemies)
            {
                if (Vector3.Distance(adjustedPos, enemy.position) < safeDistance)
                {
                    isSafe = false;
                    break;
                }
            }

            if (isSafe)
            {
                Debug.Log($"Found safe player position: {adjustedPos}");
                return adjustedPos;
            }
        }

        // Fallback: use left position but ensure it's in camera
        Vector3 fallbackPos = primaryTarget.position + Vector3.left * 3.33f;
        fallbackPos = new Vector3(
            Mathf.Clamp(fallbackPos.x, cameraCenter.x - cameraWidth / 2 + 1f, cameraCenter.x + cameraWidth / 2 - 1f),
            0f,
            transform.position.z
        );

        Debug.Log($"Using fallback safe position: {fallbackPos}");
        return fallbackPos;
    }

    bool IsGrounded()
    {
        Debug.Log($"Ground check: pos={transform.position.y}, rb.vel={rb?.velocity.y}");

        // Improved ground check using raycast
        float rayDistance = 0.5f;
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * 0.1f; // Start slightly inside player

        // Cast ray downward to check for ground
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayDistance);

        Debug.Log($"Raycast hit: {hit.collider?.name}, distance: {hit.distance}");

        if (hit.collider != null)
        {
            // Check if we hit ground/platform layer
            int groundLayer = LayerMask.NameToLayer("Ground");
            int platformLayer = LayerMask.NameToLayer("Platform");

            bool validGround = hit.collider.gameObject.layer == groundLayer ||
                              hit.collider.gameObject.layer == platformLayer ||
                              hit.collider.CompareTag("Ground") ||
                              hit.collider.CompareTag("Platform");

            Debug.Log($"Ground valid: {validGround}, layer: {hit.collider.gameObject.layer}, tag: {hit.collider.tag}");

            if (validGround)
            {
                return true;
            }
        }

        // Fallback: simple Y position and velocity check
        bool fallback = (rb == null || Mathf.Abs(rb.velocity.y) < 0.5f) && transform.position.y <= 1f;
        Debug.Log($"Fallback ground check: {fallback}");

        return fallback;
    }

    /// <summary>
    /// Main ultimate sequence coroutine - Following MUGEN states 3000->3030
    /// </summary>
    IEnumerator UltimateSequence()
    {
        Debug.Log("=== ULTIMATE SEQUENCE START ===");

        // State 3000: Portrait + Intro (Time 0-72 frames = 1.2s)
        yield return StartCoroutine(State3000_Portrait());

        // State 3010: Dash Phase (Time 0-14 frames = 0.23s)
        yield return StartCoroutine(State3010_Dash());

        // State 3020: Setup Phase (Time 0-40 frames = 0.67s)
        yield return StartCoroutine(State3020_Setup());

        // State 3030: Main Attack (Time 0-300+ frames = 5s+)
        yield return StartCoroutine(State3030_Attack());

        // End ultimate
        EndUltimate();
    }

    /// <summary>
    /// State 3000: Ultimate Portrait Phase  
    /// Animation: Ultimate (anim 3000) - Portrait sequence with scale interpolation
    /// Duration: 72 frames at 60fps = 1.2 seconds
    /// </summary>
    IEnumerator State3000_Portrait()
    {
        Debug.Log("State 3000: Portrait Phase - Ultimate Animation");
        currentPhase = 0;

        // Play Ultimate animation (anim = 3000)
        // Frame sequence: 200,49 -> 200,50 (scale changes) -> LoopStart with 200,51
        if (animator != null)
        {
            animator.Play(ultimateAnimationName);

            // Start animation scale interpolation handling
            StartCoroutine(HandleUltimateAnimationScaling());
        }

        // Frame 0: Immediate effects
        /* MUGEN State 3000 Frame 0 (!Time trigger):
         * [State 0, Ultimate Portrait] Type = Helper ID = 8050 Pos = 0,-70 - Portrait during ultimate
         * [state 0, Academy Special VFX] type = helper stateno = 30850 id = 003 pos = -1,-38 scale = .3,.3
         * [State 0] Type = PlaySnd Value = S950,1 - Voice intro sound
         * [State 0] type = Explod anim = 30513 pos = 0,2 scale = .3,.4 removetime = 100 - Ground aura
         * [State 0] type = Explod anim = 6221 pos = 0,-2 scale = .6,.3 removetime = 100 - Energy ring
         */

        // Ultimate Portrait Helper (MUGEN: Helper ID 8050, pos 0,-70)
        if (fxPortrait.prefab != null)
            SpawnFX(fxPortrait, lifetimeOverride: 1.67f); // MUGEN: removetime = 100 frames

        // Academy Special VFX (MUGEN: Helper ID 003, stateno 30850, size.mid.pos -1,-38, scale 0.3,0.3)
        if (fxAcademy.prefab != null)
            SpawnFX(fxAcademy, lifetimeOverride: 1.67f);

        // Intro Sound (MUGEN: S950,1)
        PlayVoice(voiceIntro);

        // Ground effects (MUGEN: Explod anim 30513 pos 0,2 scale 0.3,0.4 + Explod anim 6221 pos 0,-2 scale 0.6,0.3)
        if (fxIntroGround.prefab != null)
            SpawnFX(fxIntroGround, lifetimeOverride: 1.67f); // MUGEN: removetime = 100
        if (fxIntroRing.prefab != null)
            SpawnFX(fxIntroRing, lifetimeOverride: 1.67f);   // MUGEN: removetime = 100

        /* MUGEN Color cycling helpers (time % 4 = 0 trigger):
         * [State 0] type = HELPER trigger1 = time % 4 = 0 name = "Light Color" ID = 8011 stateno = 50903
         * [State 0] type = HELPER trigger1 = time % 4 = 0 name = "Normal Color" ID = 8011 stateno = 50904  
         * [State 0] type = HELPER trigger1 = time % 4 = 0 name = "Dark Color" ID = 8011 stateno = 50905
         */
        StartCoroutine(ColorCycleLoop());

        // Wait for animation to finish (MUGEN: !animtime trigger)
        yield return new WaitForSeconds(1.2f); // 72 frames
    }

    /// <summary>
    /// Handle Ultimate animation scaling (anim 3000 interpolation)
    /// Frame 0-5: scale 1.0
    /// Frame 5-10: scale 1.1,0.9 (Interpolate Scale)
    /// Frame 10-15: scale 0.95,1.05 (Interpolate Scale) 
    /// Frame 15-20: scale 1.0
    /// Frame 20-100: scale 1.0 (hold)
    /// Frame 100+: LoopStart with AS50D20 effect
    /// </summary>
    IEnumerator HandleUltimateAnimationScaling()
    {
        // Get current scale AFTER facing direction has been set
        Vector3 currentScale = transform.localScale;
        float facingDirection = Mathf.Sign(currentScale.x); // Preserve current facing direction

        Debug.Log($"Animation scaling: Starting with facing direction {(facingDirection > 0 ? "Right" : "Left")} (scale.x = {currentScale.x})");

        // Frame 0-5: Normal scale (5 frames)
        yield return new WaitForSeconds(5f / 60f);

        // Frame 5-10: Scale to 1.1,0.9 with interpolation (5 frames)
        Vector3 targetScale1 = new Vector3(facingDirection * Mathf.Abs(currentScale.x) * 1.1f, currentScale.y * 0.9f, currentScale.z);
        yield return StartCoroutine(InterpolateScale(currentScale, targetScale1, 5f / 60f));

        // Frame 10-15: Scale to 0.95,1.05 with interpolation (5 frames)
        Vector3 targetScale2 = new Vector3(facingDirection * Mathf.Abs(currentScale.x) * 0.95f, currentScale.y * 1.05f, currentScale.z);
        yield return StartCoroutine(InterpolateScale(transform.localScale, targetScale2, 5f / 60f));

        // Frame 15-20: Scale back to normal (5 frames)
        Vector3 normalScale = new Vector3(facingDirection * Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        yield return StartCoroutine(InterpolateScale(transform.localScale, normalScale, 5f / 60f));

        Debug.Log($"Animation scaling: Finished with facing direction {(Mathf.Sign(transform.localScale.x) > 0 ? "Right" : "Left")} (scale.x = {transform.localScale.x})");

        // Frame 20-100: Hold normal scale (80 frames)
        yield return new WaitForSeconds(80f / 60f);

        // Frame 100+: Start loop with AS50D20 effect
        StartCoroutine(UltimateLoopEffect());
    }

    /// <summary>
    /// Interpolate scale smoothly over duration
    /// </summary>
    IEnumerator InterpolateScale(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        transform.localScale = endScale;
    }

    /// <summary>
    /// Ultimate loop effect (AS50D20 - AddSub with 50 alpha, 20 destination)
    /// </summary>
    IEnumerator UltimateLoopEffect()
    {
        // Create AddSub visual effect loop
        while (currentPhase == 0 && isUltimateActive)
        {
            // Simulate AS50D20 effect with alpha/color modulation
            StartCoroutine(AddSubFlashEffect());
            yield return new WaitForSeconds(5f / 60f); // Every 5 frames
        }
    }

    /// <summary>
    /// AddSub flash effect simulation
    /// </summary>
    IEnumerator AddSubFlashEffect()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Color flashColor = new Color(originalColor.r + 0.2f, originalColor.g + 0.2f, originalColor.b + 0.2f, originalColor.a);

            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(1f / 60f); // 1 frame flash
            spriteRenderer.color = originalColor;
        }
    }

    /// <summary>
    /// Color cycling effect during portrait phase
    /// </summary>
    IEnumerator ColorCycleLoop()
    {
        float cycleInterval = 4f / 60f; // Every 4 frames at 60fps
        int cycles = 18; // About 72 frames / 4

        for (int i = 0; i < cycles; i++)
        {
            if (fxColorCycle.prefab != null)
                SpawnFX(fxColorCycle, lifetimeOverride: cycleInterval * 2f);
            yield return new WaitForSeconds(cycleInterval);
        }
    }

    /// <summary>
    /// State 3010: Dash Phase  
    /// Duration: 14 frames at 60fps = 0.23 seconds
    /// </summary>
    IEnumerator State3010_Dash()
    {
        Debug.Log("State 3010: Dash Phase");
        currentPhase = 1;

        // Play run animation (anim = 6)
        if (animator != null)
            animator.Play(ultimateAnimationName);

        // Frame 0: Velocity (x = 30, y = 0) + Screen shake
        if (rb != null)
            rb.velocity = new Vector2(15f, 0f); // Much faster dash velocity

        // Environmental shake (time = 10, freq = 110, ampl = -7)
        if (camShake != null)
            camShake.ShakeOnce(10f / 60f, 7f);

        // Background overlay (SCREEN CENTER - cinematic)
        if (fxDashBackground.prefab != null)
            SpawnCinematicFX(fxDashBackground, Vector3.zero, 2f); // Screen overlay during dash

        // Dash trail effect (follows player)
        if (fxDashTrail.prefab != null)
            SpawnPlayerFX(fxDashTrail, Vector3.zero, 0.5f); // Trail following player

        // Start dash coroutine - use primaryTarget for area ultimate
        if (primaryTarget != null)
            StartCoroutine(DashToTarget());
        else
            StartCoroutine(DashForward());

        // Very short duration - for area ultimate, always force activation
        float elapsed = 0f;
        while (elapsed < 0.23f && !hasHitTarget)
        {
            // For area ultimate, check if we have any enemies in camera (not distance-dependent)
            if (primaryTarget != null)
            {
                // Area ultimate: don't require being close to specific target
                List<Transform> enemiesInCamera = FindAllEnemiesInCameraBounds();
                if (enemiesInCamera.Count > 0)
                {
                    hasHitTarget = true;
                    Debug.Log($"Ultimate: Area activation with {enemiesInCamera.Count} enemies in camera! Player disappears and starts loạn trảm");

                    // Player disappears immediately when activating area ultimate
                    MakePlayerInvisible();
                    Debug.Log("Player disappeared for loạn trảm phase");
                    break;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Force activation for area ultimate - always proceed if we have primary target
        if (primaryTarget != null && !hasHitTarget)
        {
            hasHitTarget = true;
            // Also disappear in force activation
            MakePlayerInvisible();
            Debug.Log("Ultimate: Force activating area ultimate after dash phase - player disappeared");
        }
    }

    /// <summary>
    /// State 3020: Setup Phase
    /// Duration: 40 frames at 60fps = 0.67 seconds
    /// Player remains invisible during this phase (loạn trảm continues)
    /// </summary>
    IEnumerator State3020_Setup()
    {
        Debug.Log("State 3020: Setup Phase - Player invisible during loạn trảm");
        currentPhase = 2;

        // Player should remain invisible during setup phase
        // (Already invisible from dash phase when hitting enemy)

        // Play setup animation (anim = 3010)
        if (animator != null)
            animator.Play(ultimateAnimationName);

        // Frame 0: Setup sounds
        PlayVoice(voiceDash); // MUGEN: S1,39

        // Start cinematic camera zoom to cover FX 3030/3040 damage area
        StartCoroutine(StartCinematicCameraZoom());

        // Wait for camera to position properly before spawning FX
        yield return new WaitForSeconds(0.2f);

        /* MUGEN State 3020 Frame 0 (!time trigger):
         * [State 0] type = Explod anim = 3030 pos = screen center scale = .63,.63 sprpriority = -5 removetime = 325
         * [State 0] type = Explod anim = 3040 pos = screen center scale = .63,.63 sprpriority = 50 removetime = 300  
         * [State 0] type = Explod anim = 3050 pos = screen center scale = .63,.63 sprpriority = -50 removetime = 325
         * [State 0] type = Explod anim = 40198 pos = -6,-25 scale = .8,.3 sprpriority = 21 removetime = -2
         * [State 0] type = Explod anim = 40198 pos = 10,-20 scale = .8,.1 sprpriority = 21 removetime = -2
         * [State 0] type = Explod anim = 6045 pos = 0,0 scale = .5,.5 sprpriority = -2 removetime = -2
         */

        // Frame 0: Background overlays (SCREEN CENTER - focused on enemy) - extended to last most of ultimate for cinematic effect
        if (fxBackOverlay1.prefab != null)
            SpawnCinematicFX(fxBackOverlay1, Vector3.zero, 6.5f); // FX 3030 at camera center (enemy-focused)
        if (fxBackOverlay2.prefab != null)
            SpawnCinematicFX(fxBackOverlay2, Vector3.zero, 6.0f); // FX 3040 at camera center (enemy-focused)  
        if (fxBackOverlay3.prefab != null)
            SpawnCinematicFX(fxBackOverlay3, Vector3.zero, 6.5f); // FX 3050 at camera center (enemy-focused)

        // Freeze all enemies immediately when background FX spawn (fx 3030 & 3040)
        Debug.Log("Freezing all enemies during setup phase (fx 3030 & 3040 spawn)");
        FreezeAllEnemiesInArea();

        // Frame 0: Player weapon glows and aura (at player position)
        if (fxWeaponGlow1.prefab != null)
            SpawnPlayerFX(fxWeaponGlow1, Vector3.zero, 10f); // MUGEN: Explod 40198 pos -6,-25, removetime = -2 (permanent)
        if (fxWeaponGlow2.prefab != null)
            SpawnPlayerFX(fxWeaponGlow2, Vector3.zero, 10f); // MUGEN: Explod 40198 pos 10,-20, removetime = -2 (permanent)
        if (fxPlayerAura.prefab != null)
            SpawnPlayerFX(fxPlayerAura, Vector3.zero, 10f);  // MUGEN: Explod 6045 pos 0,0, removetime = -2 (permanent)

        // Frame 30: Impact sounds moved to appear timing - no setup sounds needed
        // Start ambient loop
        StartCoroutine(PlayAmbientLoop());

        // Duration: 40 frames
        yield return new WaitForSeconds(0.67f);
    }

    /// <summary>
    /// Sound schedule for State 3020
    /// </summary>
    /// <summary>
    /// Ambient sound loop during setup and attack phases
    /// </summary>
    IEnumerator PlayAmbientLoop()
    {
        // Play ambient sounds in loop during cinematic (timemod = 6,0)
        // Only during State 3020 (setup phase), not attack phase
        while (currentPhase == 2 && isUltimateActive)
        {
            if (sndAmbient1 != null)
                audioSource.PlayOneShot(sndAmbient1, 0.5f); // S2,9 - reduced volume
            if (sndAmbient2 != null)
                audioSource.PlayOneShot(sndAmbient2, 0.5f); // S3,3 - reduced volume

            yield return new WaitForSeconds(6f / 60f); // Every 6 frames at 60fps
        }
    }

    /// <summary>
    /// State 3030: Main Attack Phase
    /// Animation: Ultimate_p2 (anim 3020) - Attack sequence with multiple phases
    /// Duration: 300+ frames at 60fps = 5+ seconds
    /// </summary>
    IEnumerator State3030_Attack()
    {
        Debug.Log("State 3030: Main Attack - Ultimate_p2 Animation");
        currentPhase = 3;

        // Check if we have target for attack sequence (use primaryTarget for area ultimate)
        bool hasValidTarget = primaryTarget != null && hasHitTarget;

        if (hasValidTarget)
        {
            // Player is already invisible from dash phase - continue with loạn trảm
            // No need to make invisible again since it happened during dash
            Debug.Log("Continuing loạn trảm phase - player already invisible");

            // Very short loop/slash phase - faster transition
            yield return new WaitForSeconds(0.3f); // Reduced from 1.5f to 0.3f

            // Player reappears and spawns appear effect (FX 40198)
            MakePlayerVisible();

            // Spawn appear FX (40198) at player position when reappearing
            if (fxWeaponGlow1.prefab != null)
            {
                // Use SpawnPlayerFX to respect Inspector settings
                SpawnPlayerFX(fxWeaponGlow1, Vector3.zero, 2f); // Use proper FX system with Inspector settings

                // Play impact sound when FX 40198 spawns (moved from setup phase)
                PlaySound(sndImpact1); // S0,26
                PlaySound(sndImpact1); // Double hit

                Debug.Log($"Player appear effect spawned using Inspector settings");
            }

            // Play Ultimate_p2 animation (anim = 3020) when hitting target
            if (animator != null)
            {
                animator.Play(ultimateP2AnimationName);

                // Start Ultimate_p2 animation sequence handling
                StartCoroutine(HandleUltimate_p2_Animation());
            }            // Position adjustment - find safe position away from enemies but in camera
            Vector3 safePos = FindSafePlayerPosition();
            transform.position = safePos;

            // Start all attack components
            StartCoroutine(AttackSoundSchedule());
            StartCoroutine(AttackFXSchedule());
            StartCoroutine(AttackHitSchedule());
            StartCoroutine(AttackShakeSchedule());

            // Duration: 300+ frames (5+ seconds)
            yield return new WaitForSeconds(5f);
        }
        else
        {
            // No target - continue with Ultimate animation but shorter duration
            if (animator != null)
                animator.Play(ultimateAnimationName);

            Debug.Log("No target for Ultimate_p2 sequence - using fallback");
            yield return new WaitForSeconds(2f); // Shorter sequence
        }
    }

    /// <summary>
    /// Handle Ultimate_p2 animation sequence (anim 3020)
    /// Frame sequence from MUGEN:
    /// 10,3 -> 10,2 -> 10,1 -> 10,0 (50+4+4+4 frames, H flip)
    /// 0,3 (150 frames, H flip) 
    /// 120,3 (8 frames, H flip)
    /// 200,35 (30 frames, H flip)
    /// 0,2 (5 frames)
    /// </summary>
    IEnumerator HandleUltimate_p2_Animation()
    {
        // Get original facing direction
        bool originalFacingRight = transform.localScale.x > 0;

        // Phase 1: 10,3 -> 10,2 -> 10,1 -> 10,0 sequence (62 frames total, H flipped)
        SetFacing(!originalFacingRight); // H flip

        // 10,3 for 50 frames
        yield return new WaitForSeconds(50f / 60f);

        // 10,2 for 4 frames  
        yield return new WaitForSeconds(4f / 60f);

        // 10,1 for 4 frames
        yield return new WaitForSeconds(4f / 60f);

        // 10,0 for 4 frames
        yield return new WaitForSeconds(4f / 60f);

        // Phase 2: 0,3 for 150 frames (H flipped)
        yield return new WaitForSeconds(150f / 60f);

        // Phase 3: 120,3 for 8 frames (H flipped) 
        yield return new WaitForSeconds(8f / 60f);

        // Phase 4: 200,35 for 30 frames (H flipped)
        yield return new WaitForSeconds(30f / 60f);

        // Phase 5: 0,2 for 5 frames (no flip specified)
        SetFacing(originalFacingRight); // Return to original facing
        yield return new WaitForSeconds(5f / 60f);

        Debug.Log("Ultimate_p2 animation sequence completed");
    }

    /// <summary>
    /// Set character facing direction
    /// </summary>
    void SetFacing(bool facingRight)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1 : -1);
        transform.localScale = scale;
    }

    /// <summary>
    /// Attack sound schedule based on MUGEN timing
    /// </summary>
    IEnumerator AttackSoundSchedule()
    {
        // Frame 40: Impact sounds (0.67s) - Back to original timing
        yield return new WaitForSeconds(40f / 60f);
        PlaySound(sndImpact2); // S0,27
        PlaySound(sndImpact2); // Double hit

        // Frame 80: Start player movement away from enemy (frame 80-130 as requested)
        yield return new WaitForSeconds(40f / 60f); // Wait additional 40 frames to reach frame 80
        StartMovingAwayFromEnemy();

        // Frame 130: Stop movement (frame 80-130 movement duration as requested)
        yield return new WaitForSeconds(50f / 60f); // Wait 50 frames (80-130)
        StopMovingAwayFromEnemy(); // Stop movement at frame 130

        // Frame 150: Impact sound (2.5s) - Back to original timing
        yield return new WaitForSeconds(20f / 60f); // Wait additional 20 frames to reach frame 150
        PlaySound(sndImpact3); // S0,28

        // Frame 180: Impact sounds (3s)
        yield return new WaitForSeconds(30f / 60f); // Wait additional 30 frames
        PlaySound(sndImpact4); // S0,29
        PlaySound(sndImpact4); // Double hit

        // Frame 220, 225, 230, 240: Slash sounds (3.67s - 4s)
        yield return new WaitForSeconds(40f / 60f); // Wait to frame 220
        PlaySound(sndSlash1); // S5,45
        PlaySound(sndSlash1); // Double
        PlaySound(sndSlash2, 0.5f); // S5,51 with volume 50%

        yield return new WaitForSeconds(5f / 60f); // Frame 225
        PlaySound(sndSlash1);
        PlaySound(sndSlash1);
        PlaySound(sndSlash2, 0.5f);

        yield return new WaitForSeconds(5f / 60f); // Frame 230
        PlaySound(sndSlash1);
        PlaySound(sndSlash1);
        PlaySound(sndSlash2, 0.5f);

        yield return new WaitForSeconds(10f / 60f); // Frame 240
        PlaySound(sndSlash1);
        PlaySound(sndSlash1);
        PlaySound(sndSlash2, 0.5f);
    }

    /// <summary>
    /// Attack FX schedule based on MUGEN timing
    /// </summary>
    IEnumerator AttackFXSchedule()
    {
        // Frame 0: Player aura effect (animelem = 6, around frame 6)
        yield return new WaitForSeconds(6f / 60f);
        if (fxPlayerAura.prefab != null)
            SpawnFX(fxPlayerAura, new Vector3(-0.12f, -0.68f, 0f), new Vector3(0.1f, 0.1f, 1f), 0f, 10f); // pos -7,-41, scale 0.1

        // Wait until damage phase starts (Frame 220) before spawning beams
        yield return new WaitForSeconds(214f / 60f); // Wait to frame 220 when damage starts

        /* MUGEN Frame 220: First beam set - animelem = 7 triggers
         * [state 0] type = explod trigger1 = animelem = 7 anim = 3060 pos = 10, 200 angle = -10 scale = .4,.6 sprpriority = -4
         * [state 0] type = explod trigger1 = animelem = 7 anim = 3090 pos = 10, 200 angle = -10 scale = .07,.6 sprpriority = -3
         * [state 0] type = explod trigger1 = animelem = 7 anim = 3060 pos = 300, 200 angle = 7 scale = .4,.6 sprpriority = -4
         * [state 0] type = explod trigger1 = animelem = 7 anim = 3090 pos = 300, 200 angle = 7 scale = .07,.6 sprpriority = -3
         * Note: MUGEN uses postype = back (absolute screen coordinates)
         */
        SpawnBeamPair(new Vector3(10, 200, 0f), -10f, 0.4f, 0.6f); // MUGEN: absolute pos 10,200, scale 0.4,0.6, angle -10°
        SpawnBeamPair(new Vector3(300, 200, 0f), 7f, 0.4f, 0.6f);  // MUGEN: absolute pos 300,200, scale 0.4,0.6, angle 7°

        /* MUGEN Frame 225: Second beam set - time = 225 trigger
         * [state 0] type = explod trigger1 = time = 225 anim = 3060 pos = 90, 200 angle = -35 scale = .3,.6 sprpriority = -4  
         * [state 0] type = explod trigger1 = time = 225 anim = 3090 pos = 90, 200 angle = -35 scale = .07,.6 sprpriority = -3
         */
        yield return new WaitForSeconds(5f / 60f); // Wait 5 more frames to frame 225
        SpawnBeamPair(new Vector3(90, 200, 0f), -35f, 0.3f, 0.6f);  // MUGEN: absolute pos 90,200, scale 0.3,0.6, angle -35°

        /* MUGEN Frame 230: Third beam set + large overlays - time = 230 trigger
         * [state 0] type = explod trigger1 = time = 230 anim = 3060 pos = 230, 200 angle = 35 scale = .3,.6 sprpriority = -4
         * [state 0] type = explod trigger1 = time = 230 anim = 3090 pos = 230, 200 angle = 35 scale = .07,.6 sprpriority = -3
         * [state 0] type = explod trigger1 = time = 230 anim = 3060 pos = 10, 200 angle = -20 scale = .6,.6 sprpriority = 60
         */
        yield return new WaitForSeconds(5f / 60f);
        SpawnBeamPair(new Vector3(230, 200, 0f), 35f, 0.3f, 0.6f);  // MUGEN: absolute pos 230,200, scale 0.3,0.6, angle 35°
        SpawnLargeOverlayFX(new Vector3(10, 200, 0f), -20f, 0.6f, 0.6f); // MUGEN: absolute pos 10,200, scale 0.6,0.6, angle -20°

        /* MUGEN Frame 240: Final beam set + explosion - time = 240 trigger
         * [state 0] type = explod trigger1 = time = 240 anim = 3060 pos = 155, 200 scale = .7,.7 sprpriority = 60
         * [state 0] type = explod trigger1 = time = 240 anim = 3060 pos = 310, 200 angle = 20 scale = .6,.6 sprpriority = 60
         * [State 0] type = Helper trigger1 = time = 240 stateno = 3040/3050 
         * [state 0] type = explod trigger1 = time = 240 anim = 7013 pos = 0,0 scale = .2,.2 sprpriority = 70
         */
        yield return new WaitForSeconds(10f / 60f);
        SpawnLargeOverlayFX(new Vector3(155, 200, 0f), 0f, 0.7f, 0.7f);   // MUGEN: absolute pos 155,200, scale 0.7,0.7
        SpawnLargeOverlayFX(new Vector3(310, 200, 0f), 20f, 0.6f, 0.6f);  // MUGEN: absolute pos 310,200, scale 0.6,0.6, angle 20°

        // Final explosion (MUGEN: anim 7013, pos 0,0, scale 0.2,0.2, sprpriority 70)
        if (fxExplosion.prefab != null)
            SpawnCinematicFX(fxExplosion, Vector3.zero, 3f); // Center screen explosion

        // Screen flash effects (animelem = 8, around frame 240+)
        yield return new WaitForSeconds(8f / 60f);
        if (fxSlash.prefab != null)
        {
            SpawnCinematicFX(fxSlash, Vector3.zero, 63f / 60f); // MUGEN: Explod 3080, pos 0,0 Left, center screen
            SpawnCinematicFX(fxSlash, Vector3.zero, 63f / 60f); // Double effect with sub trans
        }
    }

    /// <summary>
    /// Attack hit schedule - damage timing
    /// </summary>
    IEnumerator AttackHitSchedule()
    {
        // Pre-damage hits (time < 215, every 20 frames)
        for (int i = 0; i < 10; i++) // Until frame 200
        {
            yield return new WaitForSeconds(20f / 60f); // Every 20 frames
            ApplyPreDamageHit(); // damage = 0, just stun
        }

        // Main damage hits (time >= 220, every 4 frames)
        yield return new WaitForSeconds(20f / 60f); // Wait to frame 220
        for (int i = 0; i < hitCount; i++)
        {
            ApplyDamageHit(); // damage = 63 per hit
            yield return new WaitForSeconds(4f / 60f); // Every 4 frames
        }

        // Target state transition (frame 255+)
        yield return new WaitForSeconds(35f / 60f); // Wait to frame 255
        if (hasHitTarget && primaryTarget != null)
        {
            // Trigger special target state (p2stateno = 6000 in MUGEN) - use primaryTarget for area ultimate
            var targetScript = primaryTarget.GetComponent<MonoBehaviour>();
            if (targetScript != null)
                targetScript.SendMessage("EnterUltimateHitState", SendMessageOptions.DontRequireReceiver);
        }
    }

    /// <summary>
    /// Camera shake schedule during attack
    /// </summary>
    IEnumerator AttackShakeSchedule()
    {
        // Wait until main damage phase (frame 215+)
        yield return new WaitForSeconds(215f / 60f);

        // Continuous shake during damage (every 4 frames, time > 215 && time < 300)
        for (int i = 0; i < 21; i++) // 85 frames / 4 = 21 shakes
        {
            if (camShake != null)
                camShake.ShakeOnce(20f / 60f, 6f); // time = 20, ampl = -6, freq = 70
            yield return new WaitForSeconds(4f / 60f);
        }
    }

    IEnumerator DashToTarget()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = primaryTarget.position; // Use primaryTarget for area ultimate

        // Adjust target position (don't dash exactly on enemy)
        float direction = Mathf.Sign(targetPos.x - startPos.x);
        targetPos.x -= direction * 1.5f; // Stop 1.5 units before enemy

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;

            // Ease out movement
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
    }

    IEnumerator DashForward()
    {
        Vector3 startPos = transform.position;
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 targetPos = startPos + Vector3.right * direction * dashDistance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
    }

    void ApplyDamageHit()
    {
        // Find all enemies in camera bounds for true area-of-effect targeting
        List<Transform> enemiesInCamera = FindAllEnemiesInCameraBounds();

        if (enemiesInCamera.Count == 0)
        {
            Debug.Log("No enemies found in camera bounds for damage");
            return;
        }

        // Apply damage to all enemies in camera view
        foreach (var enemyTransform in enemiesInCamera)
        {
            if (enemyTransform != null)
            {
                ApplyDamageToEnemy(enemyTransform.gameObject);
            }
        }

        Debug.Log($"Applied damage to {enemiesInCamera.Count} enemies in camera bounds");
    }
    void ApplyDamageToEnemy(GameObject enemy)
    {
        if (enemy == null || enemy == gameObject) return;

        // Use the centralized damage utility with boss stun for Ultimate
        SkillDamageUtility.ApplyDamageToTarget(enemy, damagePerHit, "TruthMultilateUltimate", stunBoss: true);

        // Check if it's a boss - if so, let the StunBoss handle everything
        Igris boss = enemy.GetComponent<Igris>();
        if (boss != null)
        {
            Debug.Log($"Ultimate hit boss for {damagePerHit} damage and stunned it!");
            hasHitTarget = true;

            // Spawn hit FX for boss
            if (fxHitSpark.prefab != null)
            {
                Vector3 hitPos = enemy.transform.position;
                SpawnFX(fxHitSpark, hitPos - transform.position);
            }
            return; // Boss stun handling is done by SkillDamageUtility
        }

        // For regular enemies, apply the original disable logic
        var enemyComponents = enemy.GetComponents<MonoBehaviour>();
        foreach (var component in enemyComponents)
        {
            if (component != null)
            {
                string componentName = component.GetType().Name;
                // Disable movement, AI, combat, and controller components
                if (componentName.Contains("Movement") || componentName.Contains("AI") ||
                    componentName.Contains("Controller") || componentName.Contains("Combat") ||
                    componentName.Contains("Enemy") || componentName.Contains("Patrol") ||
                    componentName.Contains("Follow") || componentName.Contains("Chase") ||
                    componentName.Contains("Attack") || componentName.Contains("Behavior"))
                {
                    component.enabled = false;
                    Debug.Log($"Disabled enemy component: {componentName} on {enemy.name}");
                }
            }
        }

        // Freeze regular enemies
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector2.zero;
            enemyRb.isKinematic = true;
        }

        // Force hurt animation for regular enemies
        Animator enemyAnimator = enemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            // Try different hurt animation names
            if (enemyAnimator.HasState(0, Animator.StringToHash("Bat_Hurt")))
            {
                enemyAnimator.Play("Bat_Hurt");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Bat_Hurt"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Hurt")))
            {
                enemyAnimator.Play("Hurt");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Hurt"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Hit")))
            {
                enemyAnimator.Play("Hit");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Hit"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Damaged")))
            {
                enemyAnimator.Play("Damaged");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Damaged"));
            }

            Debug.Log($"Playing and locking hurt animation on {enemy.name}");
        }

        // Alternative: Try SendMessage for animation and stun
        enemy.SendMessage("PlayHurtAnimation", SendMessageOptions.DontRequireReceiver);
        enemy.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
        enemy.SendMessage("OnDamage", SendMessageOptions.DontRequireReceiver);
        enemy.SendMessage("ForceStun", 2.5f, SendMessageOptions.DontRequireReceiver);

        hasHitTarget = true;

        // Spawn hit FX
        if (fxHitSpark.prefab != null)
        {
            Vector3 hitPos = enemy.transform.position;
            SpawnFX(fxHitSpark, hitPos - transform.position);
        }

        Debug.Log($"Ultimate hit enemy {enemy.name}: {damagePerHit} damage, disabled enemy logic");
    }
    GameObject SpawnFX(UltimateFXSettings fxSettings, Vector3 additionalOffset = default, Vector3 scaleOverride = default, float rotationOverride = float.NaN, float lifetimeOverride = -1f)
    {
        if (fxSettings.prefab == null) return null;

        // Use FX settings with optional overrides
        Vector3 finalOffset = fxSettings.offset + additionalOffset;
        Vector3 finalScale = scaleOverride != default ? scaleOverride : fxSettings.scale;
        float finalRotation = !float.IsNaN(rotationOverride) ? rotationOverride : fxSettings.rotation;
        float finalLifetime = lifetimeOverride > 0f ? lifetimeOverride : fxSettings.lifetime;

        return SpawnFXInternal(fxSettings.prefab, finalOffset, finalScale, finalRotation, finalLifetime, fxSettings.followPlayer);
    }

    // Legacy SpawnFX method for backward compatibility
    GameObject SpawnFX(GameObject prefab, Vector3 localOffset, Vector3 scale, float angle, float lifetime)
    {
        if (prefab == null) return null;
        return SpawnFXInternal(prefab, localOffset, scale, angle, lifetime, false);
    }

    // Unified internal spawning logic
    GameObject SpawnFXInternal(GameObject prefab, Vector3 localOffset, Vector3 scale, float angle, float lifetime, bool followPlayer)
    {
        // Determine spawn position based on context
        Vector3 basePos;
        if (isUltimateActive && primaryTarget != null)
        {
            basePos = primaryTarget.position;
        }
        else if (isUltimateActive)
        {
            basePos = ultimateCenterPosition;
        }
        else
        {
            basePos = fxSpawnPoint.position;
        }

        Vector3 spawnPos = basePos + localOffset;
        GameObject fx = Instantiate(prefab, spawnPos, Quaternion.identity);

        fx.transform.localScale = scale;
        if (angle != 0f)
            fx.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Follow player if specified
        if (followPlayer)
            fx.transform.SetParent(transform);

        // Add AutoDestroy if available
        var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
        if (autoDestroy == null)
            autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();

        // Manual cleanup as backup
        if (lifetime > 0f)
            Destroy(fx, lifetime);

        spawnedFX.Add(fx);
        return fx;
    }

    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    void PlayVoice(AudioClip clip, float volume = 1f)
    {
        if (clip != null && voiceSource != null)
            voiceSource.PlayOneShot(clip, volume);
        else if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    void EndUltimate()
    {
        Debug.Log("Ultimate: Ending Truth Multilate Ultimate");

        isUltimateActive = false;
        currentPhase = -1;

        // Restore camera zoom
        StartCoroutine(RestoreCameraZoom());

        // Restore invincibility (restore original layer)
        if (makeInvincible)
        {
            gameObject.layer = originalLayer;
            Debug.Log($"Player restored to original layer: {originalLayer}");
        }

        // Restore player control and all disabled components
        if (playerController != null)
            playerController.enabled = true;

        // Re-enable all previously disabled components
        foreach (var component in disabledComponents)
        {
            if (component != null)
            {
                component.enabled = true;
                Debug.Log($"Re-enabled component: {component.GetType().Name}");
            }
        }
        disabledComponents.Clear();

        // Restore player visibility
        MakePlayerVisible();

        // Stop any ultimate movement
        StopMovingAwayFromEnemy();

        // Return to appropriate animation
        if (animator != null)
        {
            if (IsGrounded())
                animator.Play("Player_Idle");
            else
                animator.Play("Player_Falling");
        }

        // Clean up beam FX immediately (3060, 3090)
        DestroyBeamFXImmediately();

        // Clean up any remaining FX with delay for other effects
        StartCoroutine(CleanupFX());

        Debug.Log($"Ultimate completed! Hit target: {hasHitTarget}");
    }

    IEnumerator CleanupFX()
    {
        yield return new WaitForSeconds(2f); // Wait a bit before cleanup

        foreach (var fx in spawnedFX)
        {
            if (fx != null)
                Destroy(fx);
        }
        spawnedFX.Clear();
    }

    /// <summary>
    /// Immediately destroy beam FX (3060, 3090) when ultimate ends
    /// </summary>
    void DestroyBeamFXImmediately()
    {
        List<GameObject> fxToDestroy = new List<GameObject>();

        foreach (var fx in spawnedFX)
        {
            if (fx != null)
            {
                // Check if this is a beam FX by name or by FX settings reference
                string fxName = fx.name.ToLower();
                if (fxName.Contains("beam") || fxName.Contains("3060") || fxName.Contains("3090"))
                {
                    fxToDestroy.Add(fx);
                    Debug.Log($"Immediately destroying beam FX: {fx.name}");
                }
            }
        }

        // Destroy beam FX immediately
        foreach (var fx in fxToDestroy)
        {
            spawnedFX.Remove(fx);
            Destroy(fx);
        }

        Debug.Log($"Destroyed {fxToDestroy.Count} beam FX immediately");
    }

    /// <summary>
    /// Freeze all enemies in area (called during setup phase when fx 3030 & 3040 spawn)
    /// </summary>
    void FreezeAllEnemiesInArea()
    {
        // Use primary target position as area center, fallback to ultimateCenterPosition
        Vector3 areaCenter = primaryTarget != null ? primaryTarget.position : ultimateCenterPosition;

        // Find all enemies in area using primary target position for visual consistency
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(areaCenter, hitRadius * 4f, enemyLayers);

        if (enemiesInRange.Length == 0)
        {
            // Fallback: try finding by tag
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in taggedEnemies)
            {
                float distance = Vector3.Distance(areaCenter, enemy.transform.position);
                if (distance <= hitRadius * 4f)
                {
                    ApplyStunToEnemy(enemy);
                }
            }
        }
        else
        {
            // Apply stun to all enemies in range
            foreach (var enemyCollider in enemiesInRange)
            {
                ApplyStunToEnemy(enemyCollider.gameObject);
            }
        }

        Debug.Log($"Froze all enemies in area during setup phase (center: {areaCenter})");
    }

    /// <summary>
    /// Public method to cancel ultimate (for interrupts)
    /// </summary>
    public void CancelUltimate()
    {
        if (!isUltimateActive) return;

        StopAllCoroutines();

        // Restore invincibility immediately (restore original layer)
        if (makeInvincible)
            gameObject.layer = originalLayer;

        // Re-enable all previously disabled components immediately
        foreach (var component in disabledComponents)
        {
            if (component != null)
                component.enabled = true;
        }
        disabledComponents.Clear();

        EndUltimate();
    }

    // ========== HELPER METHODS FOR NEW STATE SYSTEM ==========

    /// <summary>
    /// Spawn beam pair (main beam + core beam) within camera view
    /// MUGEN positions converted to camera-relative coordinates for cinematic view
    /// </summary>
    void SpawnBeamPair(Vector3 mugenPosition, float angle, float scaleX, float scaleY)
    {
        // Use main camera for positioning (Cinemachine controls the camera but we still need the transform)
        Camera activeCamera = mainCamera != null ? mainCamera : Camera.main;
        if (activeCamera == null) return;

        // Convert MUGEN position (10-300, 200) to camera-relative position
        // MUGEN screen: ~320x240, our positions: 10-300 horizontally, 200 vertically
        float normalizedX = (mugenPosition.x - 155f) / 155f; // Center around screen middle (155)
        float normalizedY = (mugenPosition.y - 120f) / 120f; // Center around screen middle (120)

        // Calculate world position relative to camera bounds
        Vector3 cameraCenter = activeCamera.transform.position;
        float cameraHeight = (virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : activeCamera.orthographicSize) * 2f;
        float cameraWidth = cameraHeight * activeCamera.aspect;

        Vector3 worldPos = new Vector3(
            cameraCenter.x + (normalizedX * cameraWidth * 0.4f),  // Scale down to fit in frame nicely
            cameraCenter.y + (normalizedY * cameraHeight * 0.3f), // Scale down and offset
            cameraCenter.z + 5f // In front of camera
        );

        // Main beam (anim 3060) - use Inspector settings with dynamic scaling
        if (fxBeam1.prefab != null)
        {
            GameObject beam = Instantiate(fxBeam1.prefab, worldPos, Quaternion.Euler(0, 0, angle));
            // Apply Inspector scale as base, then multiply by dynamic scaling
            Vector3 inspectorScale = fxBeam1.scale;
            beam.transform.localScale = new Vector3(inspectorScale.x * scaleX, inspectorScale.y * scaleY, inspectorScale.z);
            Destroy(beam, 3f);
            spawnedFX.Add(beam);
        }

        // Core beam (anim 3090) - use Inspector settings with dynamic scaling
        if (fxBeam2.prefab != null)
        {
            GameObject core = Instantiate(fxBeam2.prefab, worldPos, Quaternion.Euler(0, 0, angle));
            // Apply Inspector scale as base, then multiply by dynamic scaling  
            Vector3 inspectorScale = fxBeam2.scale;
            core.transform.localScale = new Vector3(inspectorScale.x, inspectorScale.y * scaleY, inspectorScale.z);
            Destroy(core, 3f);
            spawnedFX.Add(core);
        }
    }

    /// <summary>
    /// Spawn large overlay FX for final beams within camera view
    /// </summary>
    void SpawnLargeOverlayFX(Vector3 mugenPosition, float angle, float scaleX, float scaleY)
    {
        // Use main camera for positioning (Cinemachine controls the camera but we still need the transform)
        Camera activeCamera = mainCamera != null ? mainCamera : Camera.main;
        if (activeCamera == null) return;

        // Convert MUGEN position to camera-relative position (same logic as SpawnBeamPair)
        float normalizedX = (mugenPosition.x - 155f) / 155f;
        float normalizedY = (mugenPosition.y - 120f) / 120f;

        Vector3 cameraCenter = activeCamera.transform.position;
        float cameraHeight = (virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : activeCamera.orthographicSize) * 2f;
        float cameraWidth = cameraHeight * activeCamera.aspect;

        Vector3 worldPos = new Vector3(
            cameraCenter.x + (normalizedX * cameraWidth * 0.4f),
            cameraCenter.y + (normalizedY * cameraHeight * 0.3f),
            cameraCenter.z + 5f
        );

        if (fxBeam1.prefab != null)
        {
            GameObject overlay = Instantiate(fxBeam1.prefab, worldPos, Quaternion.Euler(0, 0, angle));
            // Apply Inspector scale as base, then multiply by dynamic scaling
            Vector3 inspectorScale = fxBeam1.scale;
            overlay.transform.localScale = new Vector3(inspectorScale.x * scaleX, inspectorScale.y * scaleY, inspectorScale.z);
            Destroy(overlay, 0.73f); // MUGEN: removetime = 44 frames
            spawnedFX.Add(overlay);
        }
    }

    /// <summary>
    /// Apply pre-damage hit (stun only, no damage)
    /// </summary>
    void ApplyPreDamageHit()
    {
        // Find all enemies in camera bounds for area-of-effect stun
        List<Transform> enemiesInCamera = FindAllEnemiesInCameraBounds();

        if (enemiesInCamera.Count == 0)
        {
            Debug.Log("No enemies found in camera bounds for stun");
            return;
        }

        // Apply stun to all enemies in camera view
        foreach (var enemyTransform in enemiesInCamera)
        {
            if (enemyTransform != null)
            {
                ApplyStunToEnemy(enemyTransform.gameObject);
            }
        }

        Debug.Log($"Applied stun to {enemiesInCamera.Count} enemies in camera bounds");
    }
    void ApplyStunToEnemy(GameObject enemy)
    {
        if (enemy == null || enemy == gameObject) return;

        // Completely disable enemy logic by disabling all components  
        var enemyComponents = enemy.GetComponents<MonoBehaviour>();
        foreach (var component in enemyComponents)
        {
            if (component != null)
            {
                string componentName = component.GetType().Name;
                // Disable movement, AI, combat, and controller components
                if (componentName.Contains("Movement") || componentName.Contains("AI") ||
                    componentName.Contains("Controller") || componentName.Contains("Combat") ||
                    componentName.Contains("Enemy") || componentName.Contains("Patrol") ||
                    componentName.Contains("Follow") || componentName.Contains("Chase") ||
                    componentName.Contains("Attack") || componentName.Contains("Behavior"))
                {
                    component.enabled = false;
                }
            }
        }

        // Also freeze rigidbody
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.velocity = Vector2.zero;
            enemyRb.isKinematic = true;
        }

        // Try multiple hurt animation methods
        Animator enemyAnimator = enemy.GetComponent<Animator>();
        if (enemyAnimator != null)
        {
            // Try different hurt animation names and lock them
            if (enemyAnimator.HasState(0, Animator.StringToHash("Bat_Hurt")))
            {
                enemyAnimator.Play("Bat_Hurt");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Bat_Hurt"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Hurt")))
            {
                enemyAnimator.Play("Hurt");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Hurt"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Hit")))
            {
                enemyAnimator.Play("Hit");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Hit"));
            }
            else if (enemyAnimator.HasState(0, Animator.StringToHash("Damaged")))
            {
                enemyAnimator.Play("Damaged");
                StartCoroutine(LockEnemyAnimation(enemyAnimator, "Damaged"));
            }
        }

        // Alternative: Try SendMessage for animation and stun
        enemy.SendMessage("PlayHurtAnimation", SendMessageOptions.DontRequireReceiver);
        enemy.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
        enemy.SendMessage("ForceStun", 2.5f, SendMessageOptions.DontRequireReceiver);

        // Spawn hit spark
        if (fxHitSpark.prefab != null)
            SpawnFX(fxHitSpark, enemy.transform.position + Vector3.up * 0.5f, lifetimeOverride: 0.3f);

        Debug.Log($"Ultimate pre-hit: completely disabled {enemy.name} logic");
    }

    /// <summary>
    /// Lock enemy animation to hurt state - prevents any animation transitions
    /// </summary>
    IEnumerator LockEnemyAnimation(Animator enemyAnimator, string animationName)
    {
        if (enemyAnimator == null) yield break;

        // Keep forcing the hurt animation for the duration of ultimate
        float lockDuration = 5f; // Lock for 5 seconds (entire ultimate duration)
        float elapsed = 0f;

        while (elapsed < lockDuration && isUltimateActive)
        {
            // Force the animation to stay in hurt state
            if (enemyAnimator != null && enemyAnimator.gameObject != null)
            {
                // Check if current animation is not the hurt animation
                AnimatorStateInfo currentState = enemyAnimator.GetCurrentAnimatorStateInfo(0);
                if (!currentState.IsName(animationName))
                {
                    // Force back to hurt animation
                    enemyAnimator.Play(animationName);
                    Debug.Log($"Forced {enemyAnimator.gameObject.name} back to {animationName}");
                }

                // Also disable animator speed to freeze the animation
                enemyAnimator.speed = 0.1f; // Very slow animation speed
            }

            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }

        // Restore animator speed when done (if enemy still exists)
        if (enemyAnimator != null && enemyAnimator.gameObject != null)
        {
            enemyAnimator.speed = 1f;
        }
    }

    /// <summary>
    /// Make player completely invisible (all SpriteRenderers)
    /// </summary>
    void MakePlayerInvisible()
    {
        // Find all SpriteRenderers on player and children
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                sr.color = new Color(color.r, color.g, color.b, 0f); // Completely transparent
            }
        }
        Debug.Log("Player became completely invisible");
    }

    /// <summary>
    /// Make player visible again (all SpriteRenderers)
    /// </summary>
    void MakePlayerVisible()
    {
        // Find all SpriteRenderers on player and children
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color color = sr.color;
                sr.color = new Color(color.r, color.g, color.b, 1f); // Fully opaque
            }
        }
        Debug.Log("Player became visible again");
    }
    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // Draw target detection range
        Gizmos.color = Color.red;
        Handles.DrawWireDisc(transform.position, Vector3.forward, maxTargetDistance);

        // Draw activation range
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Handles.DrawWireDisc(transform.position, Vector3.forward, activationDistance);

        // Draw hit range
        Gizmos.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.forward, hitRadius * 3f); // Area damage range

        // Draw dash distance
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetEnemy.position);
        }
        else
        {
            float direction = transform.localScale.x >= 0 ? 1f : -1f;
            Vector3 dashEnd = transform.position + Vector3.right * direction * dashDistance;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, dashEnd);
        }
#endif
    }

    // === CAMERA CINEMATIC SYSTEM ===

    /// <summary>
    /// Start cinematic camera zoom to cover FX 3030/3040 damage area
    /// </summary>
    IEnumerator StartCinematicCameraZoom()
    {
        // Prefer Cinemachine Virtual Camera for proper zoom
        if (virtualCamera != null)
        {
            // Store original virtual camera settings
            originalVirtualCameraSize = virtualCamera.m_Lens.OrthographicSize;

            // DISABLE FOLLOW TARGET to prevent camera from following player
            originalCameraFollow = virtualCamera.Follow;
            virtualCamera.Follow = null; // Disable follow during ultimate

            // Calculate cinematic position centered on PRIMARY TARGET (first enemy hit)
            Vector3 cinematicPosition = Vector3.zero;
            if (primaryTarget != null)
            {
                // Focus directly on the enemy that was hit first
                cinematicPosition = new Vector3(
                    primaryTarget.position.x,
                    primaryTarget.position.y, // Same Y level as enemy for better focus
                    virtualCamera.transform.position.z // Keep original Z
                );
            }
            else
            {
                // Fallback to ultimate center if no primary target
                cinematicPosition = new Vector3(
                    ultimateCenterPosition.x,
                    ultimateCenterPosition.y,
                    virtualCamera.transform.position.z
                );
            }

            // Target cinematic size to ZOOM IN but cover all FX properly
            float damageAreaRadius = hitRadius * 3f; // Larger radius to cover all FX
            float targetSize = Mathf.Max(3f, originalVirtualCameraSize * 0.7f); // Less zoom = larger size to show more area

            // IMMEDIATELY set camera position (no transition for positioning)
            virtualCamera.transform.position = cinematicPosition;

            // Smooth zoom transition for size only
            float elapsed = 0f;
            float duration = 0.3f; // Faster zoom
            float startSize = virtualCamera.m_Lens.OrthographicSize;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }

            virtualCamera.m_Lens.OrthographicSize = targetSize;
            isCameraZoomed = true;

        }
        else if (mainCamera != null)
        {
            // Fallback to regular camera if no Cinemachine
            originalCameraSize = mainCamera.orthographicSize;
            originalCameraPosition = mainCamera.transform.position;

            // Calculate cinematic position for regular camera - focus on PRIMARY TARGET
            Vector3 cinematicPosition = originalCameraPosition;
            if (primaryTarget != null)
            {
                // Focus directly on the enemy that was hit first
                cinematicPosition = new Vector3(
                    primaryTarget.position.x,
                    primaryTarget.position.y, // Same Y level as enemy for better focus
                    originalCameraPosition.z
                );
            }
            else
            {
                // Fallback to ultimate center
                cinematicPosition = new Vector3(
                    ultimateCenterPosition.x,
                    ultimateCenterPosition.y,
                    originalCameraPosition.z
                );
            }

            float damageAreaRadius = hitRadius * 3f; // Larger radius to cover all FX
            float targetSize = Mathf.Max(3f, originalCameraSize * 0.7f); // Less zoom = larger size to show more area

            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 startPos = mainCamera.transform.position;
            float startSize = mainCamera.orthographicSize;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                mainCamera.transform.position = Vector3.Lerp(startPos, cinematicPosition, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                yield return null;
            }

            mainCamera.transform.position = cinematicPosition;
            mainCamera.orthographicSize = targetSize;
            isCameraZoomed = true;

        }
    }

    /// <summary>
    /// Restore camera to original position and zoom after ultimate
    /// </summary>
    IEnumerator RestoreCameraZoom()
    {
        if (!isCameraZoomed) yield break;

        if (virtualCamera != null)
        {
            // Restore Cinemachine Virtual Camera
            float elapsed = 0f;
            float duration = 1f;
            float startSize = virtualCamera.m_Lens.OrthographicSize;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, originalVirtualCameraSize, t);
                yield return null;
            }

            virtualCamera.m_Lens.OrthographicSize = originalVirtualCameraSize;

            // RESTORE FOLLOW TARGET
            virtualCamera.Follow = originalCameraFollow;

            isCameraZoomed = false;

        }
        else if (mainCamera != null)
        {
            // Restore regular camera
            float elapsed = 0f;
            float duration = 1f;
            Vector3 startPos = mainCamera.transform.position;
            float startSize = mainCamera.orthographicSize;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                mainCamera.transform.position = Vector3.Lerp(startPos, originalCameraPosition, t);
                mainCamera.orthographicSize = Mathf.Lerp(startSize, originalCameraSize, t);
                yield return null;
            }

            mainCamera.transform.position = originalCameraPosition;
            mainCamera.orthographicSize = originalCameraSize;
            isCameraZoomed = false;

            Debug.Log("Regular Camera restored to original settings after ultimate");
        }
    }

    /// <summary>
    /// Spawn cinematic FX at camera center for screen-wide effects (7013, 3080)
    /// </summary>
    void SpawnCinematicFX(UltimateFXSettings fxSettings, Vector3 offset, float lifetime)
    {
        if (fxSettings.prefab == null) return;

        // Use main camera for positioning (Cinemachine controls the camera but we still need the transform)
        Camera activeCamera = mainCamera != null ? mainCamera : Camera.main;
        if (activeCamera == null) return;

        // Spawn at camera center for screen-wide effects
        Vector3 cameraCenter = activeCamera.transform.position;
        Vector3 spawnPos = new Vector3(
            cameraCenter.x + offset.x,
            cameraCenter.y + offset.y,
            cameraCenter.z + 5f // In front of camera
        );

        GameObject fx = Instantiate(fxSettings.prefab, spawnPos, Quaternion.identity);
        fx.transform.localScale = fxSettings.scale;

        // Scale effect to fill camera view for screen-wide impact
        float cameraSize = virtualCamera != null ? virtualCamera.m_Lens.OrthographicSize : activeCamera.orthographicSize;
        fx.transform.localScale = fx.transform.localScale * (cameraSize / 5f); // Scale with camera size

        if (fxSettings.rotation != 0f)
            fx.transform.rotation = Quaternion.Euler(0f, 0f, fxSettings.rotation);

        // Auto destroy
        var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
        if (autoDestroy == null)
            autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();

        if (lifetime > 0f)
            Destroy(fx, lifetime);

        spawnedFX.Add(fx);
        Debug.Log($"Spawned cinematic FX {fxSettings.prefab.name} at camera center");
    }

    /// <summary>
    /// Spawn FX at player position during ultimate (for weapon glow, player aura)
    /// </summary>
    void SpawnPlayerFX(UltimateFXSettings fxSettings, Vector3 offset, float lifetime)
    {
        if (fxSettings.prefab == null) return;

        // Always spawn at player position for weapon/aura effects
        Vector3 spawnPos = transform.position + fxSettings.offset + offset;

        GameObject fx = Instantiate(fxSettings.prefab, spawnPos, Quaternion.identity);
        fx.transform.localScale = fxSettings.scale;

        if (fxSettings.rotation != 0f)
            fx.transform.rotation = Quaternion.Euler(0f, 0f, fxSettings.rotation);

        // Follow player if needed
        if (fxSettings.followPlayer)
            fx.transform.SetParent(transform);

        // Auto destroy
        var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
        if (autoDestroy == null)
            autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();

        if (lifetime > 0f)
            Destroy(fx, lifetime);

        spawnedFX.Add(fx);
        Debug.Log($"Spawned player FX {fxSettings.prefab.name} at player position");
    }

    // === PLAYER MOVEMENT DURING ULTIMATE ===

    /// <summary>
    /// Start moving player away from enemy during sndImpact2
    /// </summary>
    void StartMovingAwayFromEnemy()
    {
        if (primaryTarget != null)
        {
            // Calculate direction away from enemy
            Vector3 directionToEnemy = (primaryTarget.position - transform.position).normalized;
            moveDirection = -directionToEnemy; // Move opposite direction
        }
        else
        {
            // Fallback: move in facing direction  
            float facingDirection = transform.localScale.x >= 0 ? -1f : 1f; // Move opposite to facing
            moveDirection = new Vector3(facingDirection, 0f, 0f);
        }

        isMovingAwayFromEnemy = true;
        Debug.Log($"Player started moving away from enemy. Direction: {moveDirection}");
    }

    /// <summary>
    /// Stop moving player away from enemy when sndImpact3 starts
    /// </summary>
    void StopMovingAwayFromEnemy()
    {
        isMovingAwayFromEnemy = false;
        moveDirection = Vector3.zero;
        Debug.Log("Player stopped moving away from enemy");
    }
}
