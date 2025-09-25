using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Player dodge/dash skill system with perfect dodge mechanics
/// Features: Dodge dash, invincibility frames, perfect dodge with time slow
/// </summary>
public class DodgeSkill : MonoBehaviour
{

    [Header("Dodge Settings")]

    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float cooldownTime = 2f;
    [SerializeField] private int manaCost = 10;

    // Double dodge logic
    private int currentDodgeCharges = 1;
    private float dodgeRechargeTimer = 0f;
    private float dodgeRechargeDelay = 0.2f; // Delay before recharge after landing

    [Header("Air Dodge Physics")]
    [SerializeField] private bool maintainHeightDuringAirDodge = true;
    [SerializeField] private float airDodgeHangTime = 0.1f; // Extra hang time after air dodge

    [Header("Perfect Dodge")]
    [SerializeField] private float perfectDodgeWindow = 0.2f; // Time window for perfect dodge
    [SerializeField] private float timeSlowDuration = 1f;
    [SerializeField] private float timeSlowScale = 0.3f;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.4f;

    [Header("Animation Settings")]
    [SerializeField] private bool useCustomAnimationEffects = true;
    [SerializeField] private bool forceIdleAfterDodge = true; // Force return to idle state
    [SerializeField] private bool useExaggeratedScaleForTesting = true; // Make scale effects more visible
    [SerializeField] private float dodgeScaleEffect = 0.8f; // Ground dodge scale effect (more visible)
    [SerializeField] private float airDodgeScaleX = 1.3f; // Air dodge scale X (more visible)
    [SerializeField] private float airDodgeScaleY = 0.7f; // Air dodge scale Y (more visible)
    [SerializeField] private float scaleAnimationDuration = 0.2f;

    [Header("Enhanced Visual Effects (MUGEN-style)")]
    [SerializeField] private GameObject dashTrailEffect;
    [SerializeField] private GameObject perfectDodgeEffect;
    [SerializeField] private Color dashTrailColor = Color.cyan;
    [SerializeField] private bool enablePalFXEffects = true;
    [SerializeField] private Color palFXTintColor = new Color(0f, 0f, 0.8f, 0.3f); // Blue tint (0,0,200)
    [SerializeField] private Color palFXMultiplyColor = new Color(2f, 2f, 2f, 1f); // mul = 200,200,200
    [SerializeField] private float palFXInitialDuration = 0.067f; // time < 2 frames
    [SerializeField] private float palFXExtendedDuration = 0.167f; // time = 5 frames

    [Header("Helper/Trail Effects (MUGEN-style)")]
    [SerializeField] private bool enableHelperTrails = true;
    [SerializeField] private GameObject helperTrailPrefab;
    [SerializeField] private float helperSpawnInterval2 = 0.067f; // Time%2 = 0 (every 2 frames)
    [SerializeField] private float helperSpawnInterval4 = 0.133f; // Time%4 = 0 (every 4 frames)
    [SerializeField] private int maxHelperTrails = 8;
    [SerializeField] private float helperTrailLifetime = 0.5f;


    [Header("Explod Effects (MUGEN Main FX)")]
    [SerializeField] private bool enableExplodEffects = true;


    [System.Serializable]
    public class ExplodFXSettings
    {
        [Header("Prefab & Enable")]
        public GameObject prefab;
        public bool enabled = true;

        [Header("Transform Settings")]
        public Vector3 positionOffset = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public Vector3 rotation = Vector3.zero;

        [Header("Physics (if has Rigidbody2D)")]
        public Vector2 velocity = Vector2.zero;
        public Vector2 acceleration = Vector2.zero;

        [Header("Visual")]
        public Color tintColor = Color.white;
        public bool enableSubEffect = true; // For trans = sub versions
        public Color subEffectColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    }

    [SerializeField]
    private ExplodFXSettings dustParticlesSettings = new ExplodFXSettings
    {
        positionOffset = Vector3.zero,
        scale = new Vector3(0.2f, 0.1f, 1f),
        rotation = new Vector3(0f, 0f, 90f),
        velocity = new Vector2(1f, 0f),
        enabled = true
    };

    [SerializeField]
    private ExplodFXSettings energyTrailsSettings = new ExplodFXSettings
    {
        positionOffset = Vector3.zero,
        scale = new Vector3(0.2f, 0.1f, 1f),
        rotation = new Vector3(0f, 0f, 90f),
        enabled = true
    };

    [SerializeField]
    private ExplodFXSettings mainDodgeEffectSettings = new ExplodFXSettings
    {
        positionOffset = new Vector3(-0.85f, 0.05f, 0f),
        scale = new Vector3(0.4f, 0.4f, 1f),
        enabled = true
    };

    [SerializeField]
    private ExplodFXSettings dashLinesSettings = new ExplodFXSettings
    {
        positionOffset = new Vector3(0.75f, 0.1f, 0f),
        scale = new Vector3(0.2f, 0.25f, 1f),
        enabled = true
    };

    [SerializeField]
    private ExplodFXSettings finalEffectSettings = new ExplodFXSettings
    {
        positionOffset = new Vector3(0f, -1.25f, 0f),
        scale = new Vector3(0.1f, 0.1f, 1f),
        velocity = new Vector2(0.3f, 0f),
        acceleration = new Vector2(0.15f, 0f),
        enabled = true
    };

    // Legacy fields (kept for backward compatibility but deprecated)
    [HideInInspector][SerializeField] private GameObject dustParticlesPrefab; // anim = 40200
    [HideInInspector][SerializeField] private GameObject energyTrailsPrefab; // anim = 40199  
    [HideInInspector][SerializeField] private GameObject mainDodgeEffectPrefab; // anim = 40065
    [HideInInspector][SerializeField] private GameObject dashLinesPrefab; // anim = 40060
    [HideInInspector][SerializeField] private GameObject finalEffectPrefab; // anim = 6213
    [HideInInspector][SerializeField] private float explodLifetime = 2f; // RemoveTime = -2

    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip perfectDodgeSound;

    [Header("Enhanced Audio (MUGEN-style)")]
    [SerializeField] private AudioClip[] dodgeRandomSounds = new AudioClip[3]; // s8888, 0-2 variants
    [SerializeField] private AudioClip dodgePrimarySound; // s160, 1
    [SerializeField] private AudioClip dodgeSecondarySound; // s160, 3
    [SerializeField] private float dodgeSoundVolume = 1.0f; // volumescale = 999 -> max volume
    [SerializeField] private bool enableMugenStyleSounds = true;

    // Components
    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerResources playerResources;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private AudioSource audioSource;
    private SkillCooldownManager skillCooldownManager;
    private Animator animator;

    // State tracking
    private bool isDodging = false;
    private bool isInvincible = false;
    private float lastDamageTime = 0f;
    private Vector2 dodgeDirection;
    private Vector2 originalPosition;
    private Vector2 currentMoveInput;
    private Vector3 originalScale;
    private Vector2 originalColliderOffset;
    private Vector2 originalColliderSize;
    private float dodgeFlipDirection = 1f; // Track the flip direction set during dodge

    public bool isIntroLock = false;

    // Skill constants
    private const string SKILL_NAME = "Dodge";

    #region Unity Lifecycle

    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerResources = GetComponent<PlayerResources>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
        skillCooldownManager = FindObjectOfType<SkillCooldownManager>();
        animator = GetComponent<Animator>();

        if (skillCooldownManager == null)
        {
            Debug.LogError("DodgeSkill: SkillCooldownManager not found! Cooldown UI will not work.");
        }

        if (animator == null)
        {
            Debug.LogError("DodgeSkill: Animator not found! Dodge animations will not play.");
        }

        // Store original transform values
        originalScale = transform.localScale;

        if (playerCollider != null)
        {
            if (playerCollider is BoxCollider2D boxCollider)
            {
                originalColliderOffset = boxCollider.offset;
                originalColliderSize = boxCollider.size;
            }
        }
    }
    private void Update()
    {
        // Recharge dodge charges when grounded
        if (playerController != null && playerController.isGrounded)
        {
            if (currentDodgeCharges < playerController.dodgeCharges)
            {
                dodgeRechargeTimer += Time.deltaTime;
                if (dodgeRechargeTimer >= dodgeRechargeDelay)
                {
                    currentDodgeCharges = playerController.dodgeCharges;
                    dodgeRechargeTimer = 0f;
                }
            }
        }
        else
        {
            dodgeRechargeTimer = 0f;
        }
    }

    private void Start()
    {
        // No need for manual UI setup - SkillCooldownManager handles this
    }

    #endregion

    #region Input Handling - Input System Callbacks

    // This method will be called by PlayerInput component when Dodge action is triggered
    void OnDodge(InputValue value)
    {
        if (isIntroLock) return;
        if (value.isPressed && CanDodge())
        {
            PerformDodge();
        }
    }

    // Also support for move input to get current movement direction
    void OnMove(InputValue value)
    {
        currentMoveInput = value.Get<Vector2>();
    }

    #endregion

    #region Dodge Logic

    private bool CanDodge()
    {
        if (isDodging) return false;

        // Double dodge: check charges
        if (currentDodgeCharges <= 0) return false;

        // Use SkillCooldownManager to check if skill can be used
        if (skillCooldownManager != null)
        {
            return skillCooldownManager.CanUseSkill(SKILL_NAME);
        }

        // Fallback to manual checks if SkillCooldownManager not available
        if (playerResources != null && playerResources.GetCurrentMana() < manaCost) return false;

        return true;
    }

    private void PerformDodge()
    {
        // Double dodge: consume a charge
        if (currentDodgeCharges > 0)
            currentDodgeCharges--;

        // Use SkillCooldownManager to handle skill usage (cooldown + mana consumption)
        if (skillCooldownManager != null)
        {
            if (!skillCooldownManager.TryUseSkill(SKILL_NAME))
            {
                return;
            }
        }
        else
        {
            // Fallback: manual mana consumption if SkillCooldownManager not available
            if (playerResources != null)
            {
                if (!playerResources.TryConsumeMana(manaCost))
                {
                    return;
                }
            }
        }

        // CRITICAL: Disable PlayerController animator updates IMMEDIATELY
        // This prevents any interference with our custom animation handling
        if (playerController != null)
        {
            playerController.disableAnimatorUpdates = true;
            // ALSO completely disable PlayerController to prevent ALL interference
            playerController.enabled = false;
            Debug.Log("DodgeSkill: EARLY disable of PlayerController (COMPLETE DISABLE)");
        }

        // Determine dodge direction based on current movement input
        Debug.Log($"DodgeSkill: DEBUG currentMoveInput: {currentMoveInput}, magnitude: {currentMoveInput.magnitude}");

        if (currentMoveInput.magnitude > 0.1f)
        {
            dodgeDirection = currentMoveInput.normalized;
            Debug.Log($"DodgeSkill: Using input direction: {dodgeDirection}");
        }
        else
        {
            // Default to facing direction or right if no input
            float currentFacing = playerController != null ? playerController.transform.localScale.x : 1f;
            dodgeDirection = new Vector2(currentFacing > 0 ? 1 : -1, 0);
            Debug.Log($"DodgeSkill: No input, using facing direction: {dodgeDirection} (currentFacing: {currentFacing})");
        }

        // Handle sprite flipping for dodge direction
        if (Mathf.Abs(dodgeDirection.x) > 0.01f)
        {
            dodgeFlipDirection = Mathf.Sign(dodgeDirection.x);
            transform.localScale = new Vector3(dodgeFlipDirection, 1f, 1f);
            Debug.Log($"DodgeSkill: Flipping sprite to direction: {dodgeFlipDirection} (dodgeDirection.x: {dodgeDirection.x})");
        }
        else
        {
            // Keep current flip direction if no x movement
            dodgeFlipDirection = Mathf.Sign(transform.localScale.x);
            Debug.Log($"DodgeSkill: Keeping current flip direction: {dodgeFlipDirection}");
        }

        // Play appropriate dodge animation based on grounded state
        PlayDodgeAnimation();

        StartCoroutine(DodgeCoroutine());
        StartCoroutine(InvincibilityCoroutine());

        // Check for perfect dodge
        CheckPerfectDodge();

        // Play effects
        PlayDashEffects();

        // Play enhanced MUGEN-style sounds
        if (enableMugenStyleSounds)
        {
            StartCoroutine(PlayEnhancedDodgeSounds());
        }

        // Apply enhanced MUGEN-style PalFX effects
        if (enablePalFXEffects)
        {
            StartCoroutine(ApplyPalFXEffects());
        }

        // Spawn enhanced MUGEN-style helper trails
        if (enableHelperTrails)
        {
            StartCoroutine(SpawnHelperTrails());
        }

        // Spawn enhanced MUGEN-style explod effects (Main FX)
        if (enableExplodEffects)
        {
            StartCoroutine(SpawnExplodEffects());
        }
    }

    private void PlayDodgeAnimation()
    {
        if (animator == null && playerController?.anim == null)
        {
            return;
        }

        // Use PlayerController's animator if available for consistency
        Animator activeAnimator = playerController?.anim ?? animator;

        // Check if player is grounded to determine animation type
        bool isGrounded = playerController != null ? playerController.isGrounded : Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Ground"));

        // Reset any existing animation states first
        activeAnimator.ResetTrigger("Dodge");
        activeAnimator.ResetTrigger("AirDodge");

        if (isGrounded)
        {
            // Ground dodge animation (Action 65)
            Debug.Log("DodgeSkill: Triggering Dodge animation (player grounded)");
            Debug.Log($"DodgeSkill: useCustomAnimationEffects = {useCustomAnimationEffects}");

            // Reset AirDodge trigger to prevent dual activation
            activeAnimator.ResetTrigger("AirDodge");
            activeAnimator.SetTrigger("Dodge");

            if (useCustomAnimationEffects)
            {
                Debug.Log("DodgeSkill: Starting GroundDodgeEffects coroutine");
                StartCoroutine(GroundDodgeEffects());
                SetDodgeCollisionBox(isGrounded: true);
            }
            else
            {
                Debug.LogWarning("DodgeSkill: Custom animation effects disabled, no scale effects will show");
            }
        }
        else
        {
            // Air dodge animation (Action 70)
            Debug.Log("DodgeSkill: Triggering AirDodge animation (player in air)");

            // Log ALL current animator parameters for debugging
            Debug.Log($"DodgeSkill: PRE-CHANGE Animator State: IsGrounded={activeAnimator.GetBool("IsGrounded")}, " +
                     $"YVel={activeAnimator.GetFloat("YVel")}, Speed={activeAnimator.GetFloat("Speed")}");

            AnimatorStateInfo preState = activeAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"DodgeSkill: Current State Info - Name: {preState.shortNameHash}, " +
                     $"IsName('Air'): {preState.IsName("Air")}, NormalizedTime: {preState.normalizedTime}");

            // CRITICAL: Force YVel to 0 to prevent BlendTree from overriding AirDodge
            // This stops the BlendTree from continuously updating based on velocity
            activeAnimator.SetFloat("YVel", 0f);
            activeAnimator.SetFloat("Speed", 0f);
            activeAnimator.SetBool("IsGrounded", false);

            Debug.Log($"DodgeSkill: IMMEDIATE parameter override - YVel: 0, Speed: 0, IsGrounded: false");

            // Wait one frame for parameters to take effect, then set trigger
            StartCoroutine(DelayedAirDodgeTriggerWithBlendTreeFix(activeAnimator));

            if (useCustomAnimationEffects)
            {
                StartCoroutine(AirDodgeEffects());
                SetDodgeCollisionBox(isGrounded: false);
            }
        }
    }

    private void ResetDodgeAnimation()
    {
        // Use PlayerController's animator if available for consistency
        Animator activeAnimator = playerController?.anim ?? animator;

        if (activeAnimator == null)
        {
            return;
        }

        // Reset dodge animation triggers to prevent staying in dodge state
        activeAnimator.ResetTrigger("Dodge");
        activeAnimator.ResetTrigger("AirDodge");

        // Sync with PlayerController's animation parameters to force proper state transition
        if (playerController != null)
        {
            // Update animation parameters to current player state
            activeAnimator.SetBool("IsGrounded", playerController.isGrounded);
            activeAnimator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
            activeAnimator.SetFloat("YVel", rb.velocity.y);

            // Optional: Force idle state trigger if available
            if (forceIdleAfterDodge)
            {
                try
                {
                    activeAnimator.SetTrigger("Idle");
                }
                catch
                {
                    // If "Idle" trigger doesn't exist, that's fine
                }
            }
        }
    }

    /// <summary>
    /// Delayed air dodge trigger with BlendTree fix to prevent continuous parameter updates
    /// NEW APPROACH: Force direct state transition instead of triggers
    /// </summary>
    private IEnumerator DelayedAirDodgeTriggerWithBlendTreeFix(Animator activeAnimator)
    {
        Debug.Log("DodgeSkill: Starting DelayedAirDodgeTriggerWithBlendTreeFix - NEW DIRECT STATE APPROACH");

        // PlayerController is already disabled in ExecuteDodge, no need to disable again
        Debug.Log("DodgeSkill: PlayerController already disabled, proceeding with state transition");

        // Wait 2 frames to ensure parameters are stable
        yield return null;
        yield return null;

        // Method 1: Try direct state transition using CrossFade
        bool crossFadeSuccess = false;
        try
        {
            Debug.Log("DodgeSkill: Attempting direct CrossFade to AirDodge state");
            activeAnimator.CrossFade("AirDodge", 0.1f, 0); // Force transition with 0.1s blend
            crossFadeSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DodgeSkill: CrossFade failed: {e.Message}");
        }

        if (crossFadeSuccess)
        {
            yield return new WaitForSeconds(0.1f);
            AnimatorStateInfo afterCrossfade = activeAnimator.GetCurrentAnimatorStateInfo(0);
            if (afterCrossfade.IsName("AirDodge"))
            {
                Debug.Log("DodgeSkill: ✅ SUCCESS with CrossFade!");
                yield break; // Exit coroutine successfully
            }
        }

        // Method 2: Try Play() method to directly play the state
        bool playSuccess = false;
        try
        {
            Debug.Log("DodgeSkill: Attempting direct Play() to AirDodge state");
            activeAnimator.Play("AirDodge", 0, 0f); // Force play from start
            playSuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DodgeSkill: Play() failed: {e.Message}");
        }

        if (playSuccess)
        {
            yield return null;
            AnimatorStateInfo afterPlay = activeAnimator.GetCurrentAnimatorStateInfo(0);
            if (afterPlay.IsName("AirDodge"))
            {
                Debug.Log("DodgeSkill: ✅ SUCCESS with Play()!");
                yield break; // Exit coroutine successfully
            }
        }

        // Method 3: Try to find AirDodge state by hash
        bool hashPlaySuccess = false;
        int airDodgeHash = 0;
        try
        {
            Debug.Log("DodgeSkill: Attempting Play() with state hash");
            airDodgeHash = Animator.StringToHash("AirDodge");
            activeAnimator.Play(airDodgeHash, 0, 0f);
            hashPlaySuccess = true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"DodgeSkill: State hash Play() failed: {e.Message}");
        }

        if (hashPlaySuccess)
        {
            yield return null;
            AnimatorStateInfo afterHashPlay = activeAnimator.GetCurrentAnimatorStateInfo(0);
            if (afterHashPlay.shortNameHash == airDodgeHash)
            {
                Debug.Log("DodgeSkill: ✅ SUCCESS with state hash!");
                yield break; // Exit coroutine successfully
            }
        }

        // Method 4: Fallback to original trigger method with extreme parameter locking
        Debug.LogError("DodgeSkill: All direct state methods failed, falling back to trigger with EXTREME locking");

        // NUCLEAR OPTION: Temporarily disable and re-enable animator
        Debug.Log("DodgeSkill: NUCLEAR OPTION - Temporarily disabling Animator");
        activeAnimator.enabled = false;
        yield return null;

        activeAnimator.enabled = true;
        yield return null;

        // Completely override BlendTree by setting extreme values
        activeAnimator.SetFloat("YVel", -999f);    // Force way out of Air blend range
        activeAnimator.SetFloat("Speed", -999f);   // Force way out of movement range
        activeAnimator.SetBool("IsGrounded", true); // Force to ground to exit Air BlendTree

        yield return null;

        // Now set back to air dodge values and trigger
        activeAnimator.SetFloat("YVel", 0f);
        activeAnimator.SetFloat("Speed", 0f);
        activeAnimator.SetBool("IsGrounded", false);
        activeAnimator.ResetTrigger("AirDodge");
        activeAnimator.SetTrigger("AirDodge");

        Debug.Log("DodgeSkill: Post-nuclear parameters set, monitoring for success...");

        // Monitor for 20 frames with aggressive parameter forcing
        for (int frame = 0; frame < 20; frame++)
        {
            // Super aggressive parameter locking
            activeAnimator.SetFloat("YVel", 0f);
            activeAnimator.SetFloat("Speed", 0f);
            activeAnimator.SetBool("IsGrounded", false);

            // Re-trigger every 2 frames
            if (frame % 2 == 0)
            {
                activeAnimator.SetTrigger("AirDodge");
            }

            AnimatorStateInfo currentState = activeAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"DodgeSkill: NUCLEAR Frame {frame} - State: {currentState.shortNameHash}, " +
                     $"IsAirDodge: {currentState.IsName("AirDodge")}, YVel: {activeAnimator.GetFloat("YVel")}");

            if (currentState.IsName("AirDodge"))
            {
                Debug.Log($"DodgeSkill: ✅ NUCLEAR SUCCESS at frame {frame}!");
                break;
            }

            yield return null;
        }

        // Keep parameters locked for animation duration
        yield return new WaitForSeconds(0.3f);

        // Re-enable PlayerController (both enabled and animator updates)
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.disableAnimatorUpdates = false;
            Debug.Log("DodgeSkill: Re-enabled PlayerController (COMPLETE RE-ENABLE)");
        }

        // Final state check
        AnimatorStateInfo finalState = activeAnimator.GetCurrentAnimatorStateInfo(0);
        if (finalState.IsName("AirDodge"))
        {
            Debug.Log("DodgeSkill: ✅ AirDodge animation successfully started!");
        }
        else
        {
            Debug.LogError($"DodgeSkill: ❌ COMPLETE FAILURE! Final state: {finalState.shortNameHash}");
            Debug.LogError("DodgeSkill: BlendTree is completely overriding our transitions!");
            LogAllAnimatorParameters(activeAnimator);
        }
    }

    /// <summary>
    /// Set collision box based on dodge type (matching your animation setup)
    /// </summary>
    private void SetDodgeCollisionBox(bool isGrounded)
    {
        if (playerCollider == null) return;

        if (playerCollider is BoxCollider2D boxCollider)
        {
            if (isGrounded)
            {
                // Ground dodge collision: Clsn2[0] = -9, -54, 5, 0
                // Convert to Unity BoxCollider2D format
                float width = 5 - (-9); // 14 units wide
                float height = 0 - (-54); // 54 units tall
                boxCollider.offset = new Vector2(-2f, -27f); // Center of the box
                boxCollider.size = new Vector2(width * 0.1f, height * 0.1f); // Scale down for Unity units
            }
            else
            {
                // Air dodge collision: Clsn2[0] = -20, -51, 18, 0  
                float width = 18 - (-20); // 38 units wide
                float height = 0 - (-51); // 51 units tall
                boxCollider.offset = new Vector2(-1f, -25.5f); // Center of the box
                boxCollider.size = new Vector2(width * 0.1f, height * 0.1f); // Scale down for Unity units
            }
        }
    }

    /// <summary>
    /// Restore original collision box after dodge ends
    /// </summary>
    private void RestoreOriginalCollisionBox()
    {
        if (playerCollider != null && playerCollider is BoxCollider2D boxCollider)
        {
            boxCollider.offset = originalColliderOffset;
            boxCollider.size = originalColliderSize;
        }
    }

    /// <summary>
    /// Ground dodge scale effects (Action 65)
    /// </summary>
    private IEnumerator GroundDodgeEffects()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        Debug.Log($"DodgeSkill: Starting GroundDodgeEffects - originalScale: {originalScale}");

        // Phase 1: Scale down effect (0.99 -> 0.95)
        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scaleAnimationDuration;

            // Interpolate scale from 0.99 to 0.95 (or more dramatic for testing)
            float startScaleY = useExaggeratedScaleForTesting ? 1.0f : 0.99f;
            float targetScaleY = useExaggeratedScaleForTesting ? 0.5f : dodgeScaleEffect;
            float scaleY = Mathf.Lerp(startScaleY, targetScaleY, progress);

            // Preserve flip direction during scale effects
            float scaleXMultiplier = useExaggeratedScaleForTesting ? 1.0f : 0.99f;
            Vector3 newScale = new Vector3(startScale.x * scaleXMultiplier * dodgeFlipDirection, startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

            // Add slight color tint for visibility
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.cyan, progress * 0.3f);
            }

            yield return null;
        }

        Debug.Log($"DodgeSkill: Ground dodge scale applied: {transform.localScale}");

        // Hold the scale effect for dodge duration
        yield return new WaitForSeconds(dashDuration - scaleAnimationDuration);

        // Restore original scale but preserve sprite flip direction
        transform.localScale = new Vector3(originalScale.x * dodgeFlipDirection, originalScale.y, originalScale.z);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log($"DodgeSkill: Ground dodge effects completed, scale restored to: {transform.localScale} (preserving flip: {dodgeFlipDirection})");
    }

    /// <summary>
    /// Air dodge scale effects (Action 70)
    /// </summary>
    private IEnumerator AirDodgeEffects()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        Debug.Log($"DodgeSkill: Starting AirDodgeEffects - originalScale: {originalScale}");

        // Phase 1: Initial scale effect (1.1, 0.9)
        while (elapsedTime < scaleAnimationDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (scaleAnimationDuration * 0.5f);

            float scaleX = Mathf.Lerp(1f, useExaggeratedScaleForTesting ? 2.0f : airDodgeScaleX, progress);
            float scaleY = Mathf.Lerp(1f, useExaggeratedScaleForTesting ? 0.5f : airDodgeScaleY, progress);
            Vector3 newScale = new Vector3(startScale.x * scaleX * dodgeFlipDirection, startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

            // Add slight color tint for visibility
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, progress * 0.4f);
            }

            yield return null;
        }

        // Phase 2: Interpolate to final scale (0.95, 1.05)
        elapsedTime = 0f;
        while (elapsedTime < scaleAnimationDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (scaleAnimationDuration * 0.5f);

            float scaleX = Mathf.Lerp(airDodgeScaleX, 0.95f, progress);
            float scaleY = Mathf.Lerp(airDodgeScaleY, 1.05f, progress);
            Vector3 newScale = new Vector3(startScale.x * scaleX * dodgeFlipDirection, startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

            yield return null;
        }

        Debug.Log($"DodgeSkill: Air dodge scale applied: {transform.localScale}");

        // Hold effect for remaining duration
        yield return new WaitForSeconds(dashDuration - scaleAnimationDuration);

        // Restore original scale but preserve sprite flip direction
        transform.localScale = new Vector3(originalScale.x * dodgeFlipDirection, originalScale.y, originalScale.z);
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log($"DodgeSkill: Air dodge effects completed, scale restored to: {transform.localScale} (preserving flip: {dodgeFlipDirection})");
    }

    private void CheckPerfectDodge()
    {
        // Check if player was about to take damage (recently damaged or enemy attack imminent)
        float timeSinceLastDamage = Time.time - lastDamageTime;

        if (timeSinceLastDamage <= perfectDodgeWindow)
        {
            TriggerPerfectDodge();
        }
        else
        {
            // Check for nearby enemy attacks (advanced perfect dodge detection)
            CheckNearbyThreats();
        }
    }

    private void CheckNearbyThreats()
    {
        // Find enemies in range that might be attacking
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, 3f);

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject &&
                enemy.CompareTag("Enemy"))
            {
                // Simple threat detection - if enemy is very close, consider it a perfect dodge
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= 2f)
                {
                    TriggerPerfectDodge();
                    break;
                }
            }
        }
    }

    private void TriggerPerfectDodge()
    {
        // Trigger time slow effect
        StartCoroutine(TimeSlowEffect());

        // Play perfect dodge effects
        PlayPerfectDodgeEffects();

        // Additional perfect dodge benefits (could add mana restore, damage boost, etc.)
        if (playerResources != null)
        {
            playerResources.AddMana(manaCost / 2); // Restore half mana cost
        }
    }

    #endregion

    #region Coroutines

    private IEnumerator DodgeCoroutine()
    {
        isDodging = true;
        originalPosition = transform.position;

        float elapsedTime = 0f;
        Vector2 startPos = transform.position;

        // Enhanced velocity system (MUGEN-style)
        // velset x = cond(var(2) > 0, (const(velocity.run.back.x) * 1.2), const(velocity.run.back.x))
        float baseVelocity = playerController != null ? playerController.runSpeed : 8f;
        float enhancedVelocityMultiplier = 1.2f; // Enhanced speed multiplier from MUGEN
        float dodgeVelocity = baseVelocity * enhancedVelocityMultiplier;

        Vector2 targetPos = startPos + (dodgeDirection * dashDistance);

        // Check if this is an air dodge
        bool isAirDodge = playerController != null ? !playerController.isGrounded : !Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Ground"));

        // Store original gravity and velocity for air dodge
        float originalGravityScale = rb.gravityScale;
        Vector2 originalVelocity = rb.velocity;

        Debug.Log($"DodgeSkill: Enhanced velocity - base: {baseVelocity}, enhanced: {dodgeVelocity}, multiplier: {enhancedVelocityMultiplier}");

        if (isAirDodge)
        {
            // For air dodge: suspend gravity and maintain height
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Stop vertical movement

            // Only use horizontal component of dodge direction for air dodge
            Vector2 horizontalDodgeDirection = new Vector2(dodgeDirection.x, 0f).normalized;
            targetPos = new Vector2(startPos.x + (horizontalDodgeDirection.x * dashDistance), startPos.y);

            // Apply enhanced velocity for air dodge
            rb.velocity = new Vector2(horizontalDodgeDirection.x * dodgeVelocity, 0f);
        }
        else
        {
            // Apply enhanced velocity for ground dodge  
            rb.velocity = dodgeDirection * dodgeVelocity;
        }

        // Disable player controller movement during dash
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        while (elapsedTime < dashDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / dashDuration;

            // Use smooth curve for dash movement
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, smoothProgress);

            if (isAirDodge)
            {
                // For air dodge, maintain the Y position strictly
                currentPos.y = startPos.y;
                rb.velocity = new Vector2(0f, 0f); // Keep velocity zero during air dodge
            }

            rb.MovePosition(currentPos);

            yield return null;
        }

        // Restore gravity and physics for air dodge
        if (isAirDodge)
        {
            if (maintainHeightDuringAirDodge && airDodgeHangTime > 0f)
            {
                // Brief hang time before gravity resumes
                yield return new WaitForSeconds(airDodgeHangTime);
            }

            rb.gravityScale = originalGravityScale;
            // Don't restore original velocity - let natural falling resume
            rb.velocity = new Vector2(0f, 0f);
        }

        // Re-enable player controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Restore original collision box
        RestoreOriginalCollisionBox();

        // Reset animation state to prevent it from staying in dodge animation
        ResetDodgeAnimation();

        isDodging = false;

        // Safety: Always re-enable PlayerController and animator updates when dodge ends
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.disableAnimatorUpdates = false;
            Debug.Log("DodgeSkill: SAFETY re-enable of PlayerController");
        }
    }

    /// <summary>
    /// Continuously lock animator parameters during dodge to prevent any interference
    /// ENHANCED: Higher frequency locking with detailed monitoring
    /// </summary>
    private IEnumerator ContinuousParameterLock(Animator activeAnimator)
    {
        Debug.Log("DodgeSkill: Starting ENHANCED ContinuousParameterLock");

        int frameCount = 0;
        while (isDodging)
        {
            frameCount++;

            // Force lock critical parameters every frame with logging
            float currentYVel = activeAnimator.GetFloat("YVel");
            float currentSpeed = activeAnimator.GetFloat("Speed");
            bool currentGrounded = activeAnimator.GetBool("IsGrounded");

            // Detect if parameters were overridden
            bool wasOverridden = (Mathf.Abs(currentYVel) > 0.1f || Mathf.Abs(currentSpeed) > 0.1f || currentGrounded);

            if (wasOverridden || frameCount % 10 == 0) // Log every 10 frames or when overridden
            {
                Debug.Log($"DodgeSkill: Frame {frameCount} - BEFORE lock: YVel={currentYVel:F2}, Speed={currentSpeed:F2}, Grounded={currentGrounded}");

                if (wasOverridden)
                {
                    Debug.LogWarning($"DodgeSkill: PARAMETERS WERE OVERRIDDEN! Forcing back to locked values.");
                }
            }

            // Force parameters back to locked values
            activeAnimator.SetFloat("YVel", 0f);
            activeAnimator.SetFloat("Speed", 0f);
            activeAnimator.SetBool("IsGrounded", false);

            // Check current animation state
            AnimatorStateInfo currentState = activeAnimator.GetCurrentAnimatorStateInfo(0);

            if (frameCount % 10 == 0 || wasOverridden) // Log state every 10 frames
            {
                Debug.Log($"DodgeSkill: Frame {frameCount} - Current State: {currentState.shortNameHash}, " +
                         $"IsAirDodge: {currentState.IsName("AirDodge")}, NormalizedTime: {currentState.normalizedTime:F2}");
            }

            // If we're not in AirDodge state, try to force back
            if (!currentState.IsName("AirDodge") && frameCount > 5) // Allow first few frames for transition
            {
                Debug.LogError($"DodgeSkill: CRITICAL - Not in AirDodge state at frame {frameCount}! Attempting recovery...");

                // Try to force back to AirDodge using multiple methods
                activeAnimator.ResetTrigger("AirDodge");
                activeAnimator.SetTrigger("AirDodge");

                // Also try CrossFade as backup
                try
                {
                    activeAnimator.CrossFade("AirDodge", 0.05f, 0);
                    Debug.Log("DodgeSkill: Recovery CrossFade attempted");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"DodgeSkill: Recovery CrossFade failed: {e.Message}");
                }
            }

            yield return null; // Wait one frame
        }

        Debug.Log($"DodgeSkill: ContinuousParameterLock ended after {frameCount} frames (dodge finished)");
    }

    /// <summary>
    /// Debug method to log all available animator parameters and triggers
    /// </summary>
    private void LogAllAnimatorParameters(Animator animator)
    {
        Debug.Log("=== ANIMATOR DEBUG INFO ===");

        // Log all parameters
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            string value = "";
            switch (param.type)
            {
                case AnimatorControllerParameterType.Bool:
                    value = animator.GetBool(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Float:
                    value = animator.GetFloat(param.name).ToString("F2");
                    break;
                case AnimatorControllerParameterType.Int:
                    value = animator.GetInteger(param.name).ToString();
                    break;
                case AnimatorControllerParameterType.Trigger:
                    value = "(trigger)";
                    break;
            }
            Debug.Log($"Parameter: {param.name} ({param.type}) = {value}");
        }

        // Log current state info
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(layer);
            Debug.Log($"Layer {layer}: State Hash: {state.shortNameHash}, NormalizedTime: {state.normalizedTime:F2}");
        }

        Debug.Log("=== END ANIMATOR DEBUG ===");
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        // Visual feedback for invincibility
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashEffect());
        }

        yield return new WaitForSeconds(invincibilityDuration);

        isInvincible = false;
    }

    private IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        float flashDuration = 0.1f;
        int flashCount = Mathf.FloorToInt(invincibilityDuration / (flashDuration * 2));

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }

        // Ensure normal color at end
        spriteRenderer.color = Color.white;
    }

    private IEnumerator TimeSlowEffect()
    {
        // Store original time scale
        float originalTimeScale = Time.timeScale;

        // Apply time slow
        Time.timeScale = timeSlowScale;

        // Wait for slow duration (using unscaled time)
        yield return new WaitForSecondsRealtime(timeSlowDuration);

        // Restore normal time
        Time.timeScale = originalTimeScale;

        Debug.Log("Time slow effect ended");
    }

    #endregion

    #region Effects and Audio

    private void PlayDashEffects()
    {
        // Play dash sound
        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }

        // Create dash trail effect
        if (dashTrailEffect != null)
        {
            GameObject trail = Instantiate(dashTrailEffect, transform.position, Quaternion.identity);

            // Rotate trail to match dodge direction
            float angle = Mathf.Atan2(dodgeDirection.y, dodgeDirection.x) * Mathf.Rad2Deg;
            trail.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Parent to player temporarily
            trail.transform.SetParent(transform, true);

            // Destroy trail after some time
            Destroy(trail, 2f);
        }
        else
        {
            // Create simple trail effect using DashTrailEffect script
            GameObject trailObj = new GameObject("DashTrail");
            trailObj.transform.position = transform.position;
            trailObj.transform.rotation = transform.rotation;

            DashTrailEffect trailEffect = trailObj.AddComponent<DashTrailEffect>();
            trailEffect.StartTrail();
        }
    }

    private void PlayPerfectDodgeEffects()
    {
        // Play perfect dodge sound
        if (audioSource != null && perfectDodgeSound != null)
        {
            audioSource.PlayOneShot(perfectDodgeSound);
        }

        // Create perfect dodge effect
        if (perfectDodgeEffect != null)
        {
            GameObject effect = Instantiate(perfectDodgeEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        else
        {
            // Use static factory method to create effect
            GameObject effect = PerfectDodgeEffect.CreatePerfectDodgeEffect(transform.position);
        }

        Debug.Log("🌟 Perfect Dodge Effect Played!");
    }

    #endregion

    #region UI Integration

    #endregion

    #region Public Interface

    public bool IsInvincible()
    {
        return isInvincible;
    }

    public bool IsDodging()
    {
        return isDodging;
    }

    public void OnDamageTaken()
    {
        // Called by damage system to track damage timing for perfect dodge detection
        lastDamageTime = Time.time;
    }

    public float GetCooldownTime()
    {
        // Get cooldown from SkillCooldownManager if available
        if (skillCooldownManager != null)
        {
            var skillData = skillCooldownManager.GetSkillData(SKILL_NAME);
            return skillData?.cooldownDuration ?? cooldownTime;
        }
        return cooldownTime;
    }

    public int GetManaCost()
    {
        // Get mana cost from SkillCooldownManager if available
        if (skillCooldownManager != null)
        {
            var skillData = skillCooldownManager.GetSkillData(SKILL_NAME);
            return skillData?.manaCost ?? manaCost;
        }
        return manaCost;
    }

    #endregion

    #region Enhanced Effects (MUGEN-style)

    /// <summary>
    /// Utility method to spawn explod FX with proper settings and AutoDestroyOnAnimationEnd
    /// </summary>
    private GameObject SpawnExplodFX(ExplodFXSettings settings, string effectName, bool applyFlip = true)
    {
        if (settings?.prefab == null || !settings.enabled)
            return null;

        // Calculate spawn position with mirrored offset for left dodge
        Vector3 mirroredOffset = settings.positionOffset;
        if (applyFlip && dodgeFlipDirection < 0)
        {
            // Mirror X offset for left dodge to make effects symmetrical
            mirroredOffset.x = -mirroredOffset.x;
        }
        Vector3 spawnPos = transform.position + mirroredOffset;

        // Apply random offset for particle effects
        if (effectName.Contains("Particles") || effectName.Contains("Trails"))
        {
            spawnPos += new Vector3(
                Random.Range(-1.5f, 1.5f),
                Random.Range(-2f, 2f),
                0f
            );
        }

        // Calculate mirrored rotation for flip
        Vector3 finalRotation = settings.rotation;
        if (applyFlip && dodgeFlipDirection < 0)
        {
            // You may want to adjust rotation for left dodge if needed
            // finalRotation.z = -finalRotation.z; // Uncomment if rotation needs flipping
        }

        // Spawn main effect
        GameObject effect = Instantiate(settings.prefab, spawnPos, Quaternion.Euler(finalRotation));

        // Apply scale with flip direction
        Vector3 finalScale = settings.scale;
        if (applyFlip)
        {
            finalScale.x *= dodgeFlipDirection;
        }
        effect.transform.localScale = finalScale;

        // Apply tint color
        if (effect.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.color = settings.tintColor;
        }

        // Apply physics if available with flipped velocity
        if (effect.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector2 finalVelocity = settings.velocity;
            Vector2 finalAcceleration = settings.acceleration;

            if (applyFlip && dodgeFlipDirection < 0)
            {
                // Flip X velocity and acceleration for left dodge
                finalVelocity.x = -finalVelocity.x;
                finalAcceleration.x = -finalAcceleration.x;
            }

            rb.velocity = finalVelocity;
            if (finalAcceleration != Vector2.zero)
            {
                StartCoroutine(ApplyAcceleration(rb, finalAcceleration));
            }
        }

        // Add AutoDestroyOnAnimationEnd component
        if (!effect.GetComponent<AutoDestroyOnAnimationEnd>())
        {
            var autoDestroy = effect.AddComponent<AutoDestroyOnAnimationEnd>();
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.1f;
            autoDestroy.fallbackLifetime = 5f;
        }

        Debug.Log($"DodgeSkill: Spawned {effectName} at {spawnPos} with scale {finalScale} (flip: {dodgeFlipDirection}, mirrored offset: {mirroredOffset})");

        // Spawn sub effect if enabled
        if (settings.enableSubEffect)
        {
            GameObject subEffect = Instantiate(settings.prefab, spawnPos, Quaternion.Euler(finalRotation));
            subEffect.transform.localScale = finalScale;

            if (subEffect.TryGetComponent<SpriteRenderer>(out var subRenderer))
            {
                subRenderer.color = settings.subEffectColor;
            }

            if (!subEffect.GetComponent<AutoDestroyOnAnimationEnd>())
            {
                var autoDestroy = subEffect.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = true;
                autoDestroy.startupGrace = 0.1f;
                autoDestroy.fallbackLifetime = 5f;
            }
        }

        return effect;
    }

    /// <summary>
    /// Create MUGEN-style helper trail effects during dodge
    /// </summary>
    private IEnumerator SpawnHelperTrails()
    {
        if (!enableHelperTrails || helperTrailPrefab == null)
            yield break;

        Debug.Log("DodgeSkill: Starting helper trail spawning");

        float elapsedTime = 0f;
        int trailCount = 0;

        // Spawn trails during dodge duration
        while (elapsedTime < dashDuration && trailCount < maxHelperTrails)
        {
            // Spawn helper every 2 frames (Time%2 = 0)
            if (Time.time % helperSpawnInterval2 < 0.033f) // Half frame tolerance
            {
                SpawnHelperTrail(trailCount);
                trailCount++;
                Debug.Log($"DodgeSkill: Spawned helper trail #{trailCount} (2-frame interval)");
            }

            // Additional spawn every 4 frames (Time%4 = 0) 
            if (Time.time % helperSpawnInterval4 < 0.033f && trailCount < maxHelperTrails)
            {
                SpawnHelperTrail(trailCount);
                trailCount++;
                Debug.Log($"DodgeSkill: Spawned helper trail #{trailCount} (4-frame interval)");
            }

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"DodgeSkill: Helper trail spawning completed, total spawned: {trailCount}");
    }

    private void SpawnHelperTrail(int index)
    {
        if (helperTrailPrefab == null) return;

        // Spawn at current position with slight offset
        Vector3 spawnPos = transform.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.2f, 0.2f),
            0f
        );

        GameObject trail = Instantiate(helperTrailPrefab, spawnPos, Quaternion.identity);

        // Set trail properties to match MUGEN helper behavior
        if (trail.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.color = new Color(1f, 1f, 1f, 0.7f - (index * 0.1f)); // Fade with age
        }

        // Auto-destroy after lifetime
        Destroy(trail, helperTrailLifetime);
    }

    /// <summary>
    /// Spawn MUGEN-style explod effects during dodge (Simplified with Settings)
    /// </summary>
    private IEnumerator SpawnExplodEffects()
    {
        if (!enableExplodEffects)
            yield break;

        Debug.Log($"DodgeSkill: Starting MUGEN explod effects spawning (flip direction: {dodgeFlipDirection})");

        // Wait for animelem = 3 timing
        yield return new WaitForSeconds(0.1f);

        // Spawn main dodge effect (40065) - Single spawn
        SpawnExplodFX(mainDodgeEffectSettings, "MainDodgeEffect", true);

        // Spawn final effect (6213) - Single spawn  
        SpawnExplodFX(finalEffectSettings, "FinalEffect", true);

        // Only spawn ground effects if on ground
        bool isGrounded = playerController != null ? playerController.isGrounded : Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Ground"));
        if (isGrounded)
        {
            SpawnExplodFX(dashLinesSettings, "DashLines", true);
        }

        // Spawn particle effects (limited spawning)
        StartCoroutine(SpawnParticleEffects());

        Debug.Log("DodgeSkill: All explod effects spawned using new settings system");
    }

    /// <summary>
    /// Spawn particle effects with limited count (not continuous)
    /// </summary>
    private IEnumerator SpawnParticleEffects()
    {
        // Spawn 2-3 dust particles only
        for (int i = 0; i < 3; i++)
        {
            SpawnExplodFX(dustParticlesSettings, "DustParticles", false);
            yield return new WaitForSeconds(0.067f); // 2 frames
        }

        // Spawn 2-3 energy trails only
        for (int i = 0; i < 3; i++)
        {
            SpawnExplodFX(energyTrailsSettings, "EnergyTrails", false);
            yield return new WaitForSeconds(0.067f); // 2 frames
        }
    }

    /// <summary>
    /// Apply acceleration to a Rigidbody2D over time
    /// </summary>
    private IEnumerator ApplyAcceleration(Rigidbody2D rb, Vector2 acceleration)
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration && rb != null)
        {
            rb.velocity += acceleration * Time.fixedDeltaTime;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    /// <summary>
    /// Apply MUGEN-style PalFX color effects during dodge
    /// </summary>
    private IEnumerator ApplyPalFXEffects()
    {
        if (!enablePalFXEffects || spriteRenderer == null)
            yield break;

        Color originalColor = spriteRenderer.color;

        // Phase 1: Initial color effect (time < 2)
        Debug.Log("DodgeSkill: Applying PalFX initial effect (blue tint)");
        spriteRenderer.color = Color.Lerp(originalColor, palFXTintColor, 0.5f);

        yield return new WaitForSeconds(palFXInitialDuration);

        // Phase 2: Extended color multiplication effect (time = 5)  
        Debug.Log("DodgeSkill: Applying PalFX extended effect (color multiplication)");
        Color enhancedColor = new Color(
            originalColor.r * palFXMultiplyColor.r,
            originalColor.g * palFXMultiplyColor.g,
            originalColor.b * palFXMultiplyColor.b,
            originalColor.a
        );
        spriteRenderer.color = enhancedColor;

        yield return new WaitForSeconds(palFXExtendedDuration);

        // Restore original color (handled by existing scale effects)
        Debug.Log("DodgeSkill: PalFX effects completed");
    }

    /// <summary>
    /// Play MUGEN-style dodge sounds with proper timing (animelem = 3)
    /// </summary>
    private IEnumerator PlayEnhancedDodgeSounds()
    {
        if (!enableMugenStyleSounds || audioSource == null)
            yield break;

        // Wait for animation frame 3 equivalent (estimated timing)
        float animFrameDelay = 0.1f; // Approximate time for 3 animation frames
        yield return new WaitForSeconds(animFrameDelay);

        // Play random dodge sound (s8888, 0+Random%3)
        if (dodgeRandomSounds != null && dodgeRandomSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, dodgeRandomSounds.Length);
            AudioClip randomSound = dodgeRandomSounds[randomIndex];
            if (randomSound != null)
            {
                audioSource.PlayOneShot(randomSound, dodgeSoundVolume);
                Debug.Log($"DodgeSkill: Playing random dodge sound {randomIndex}");
            }
        }

        // Play primary dodge sound (s160, 1)
        if (dodgePrimarySound != null)
        {
            audioSource.PlayOneShot(dodgePrimarySound, dodgeSoundVolume);
            Debug.Log("DodgeSkill: Playing primary dodge sound");
        }

        // Play secondary dodge sound (s160, 3)  
        if (dodgeSecondarySound != null)
        {
            audioSource.PlayOneShot(dodgeSecondarySound, dodgeSoundVolume);
            Debug.Log("DodgeSkill: Playing secondary dodge sound");
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        // Draw dodge range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashDistance);

        // Draw perfect dodge detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 3f);
    }

    #endregion
}