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
    [SerializeField] private bool useExaggeratedScaleForTesting = false; // Make scale effects more visible
    [SerializeField] private float dodgeScaleEffect = 0.95f; // Ground dodge scale effect (0.99 -> 0.95)
    [SerializeField] private float airDodgeScaleX = 1.1f; // Air dodge scale X (1.1)
    [SerializeField] private float airDodgeScaleY = 0.9f; // Air dodge scale Y (0.9)
    [SerializeField] private float scaleAnimationDuration = 0.2f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject dashTrailEffect;
    [SerializeField] private GameObject perfectDodgeEffect;
    [SerializeField] private Color dashTrailColor = Color.cyan;

    [Header("Audio")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip perfectDodgeSound;

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

    private void Start()
    {
        // No need for manual UI setup - SkillCooldownManager handles this
    }

    #endregion

    #region Input Handling - Input System Callbacks

    // This method will be called by PlayerInput component when Dodge action is triggered
    void OnDodge(InputValue value)
    {
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

        // Determine dodge direction based on current movement input
        if (currentMoveInput.magnitude > 0.1f)
        {
            dodgeDirection = currentMoveInput.normalized;
        }
        else
        {
            // Default to facing direction or right if no input
            dodgeDirection = playerController != null ?
                new Vector2(playerController.transform.localScale.x > 0 ? 1 : -1, 0) :
                Vector2.right;
        }

        // Play appropriate dodge animation based on grounded state
        PlayDodgeAnimation();

        StartCoroutine(DodgeCoroutine());
        StartCoroutine(InvincibilityCoroutine());

        // Check for perfect dodge
        CheckPerfectDodge();

        // Play effects
        PlayDashEffects();
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
            activeAnimator.SetTrigger("Dodge");

            if (useCustomAnimationEffects)
            {
                StartCoroutine(GroundDodgeEffects());
                SetDodgeCollisionBox(isGrounded: true);
            }
        }
        else
        {
            // Air dodge animation (Action 70)
            activeAnimator.SetTrigger("AirDodge");

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

        // Phase 1: Scale down effect (0.99 -> 0.95)
        while (elapsedTime < scaleAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / scaleAnimationDuration;

            // Interpolate scale from 0.99 to 0.95 (or more dramatic for testing)
            float startScaleY = useExaggeratedScaleForTesting ? 1.0f : 0.99f;
            float targetScaleY = useExaggeratedScaleForTesting ? 0.5f : dodgeScaleEffect;
            float scaleY = Mathf.Lerp(startScaleY, targetScaleY, progress);
            Vector3 newScale = new Vector3(startScale.x * (useExaggeratedScaleForTesting ? 1.0f : 0.99f), startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

            yield return null;
        }

        // Hold the scale effect for dodge duration
        yield return new WaitForSeconds(dashDuration - scaleAnimationDuration);

        // Restore original scale
        transform.localScale = originalScale;
    }

    /// <summary>
    /// Air dodge scale effects (Action 70)
    /// </summary>
    private IEnumerator AirDodgeEffects()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;

        // Phase 1: Initial scale effect (1.1, 0.9)
        while (elapsedTime < scaleAnimationDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (scaleAnimationDuration * 0.5f);

            float scaleX = Mathf.Lerp(1f, useExaggeratedScaleForTesting ? 2.0f : airDodgeScaleX, progress);
            float scaleY = Mathf.Lerp(1f, useExaggeratedScaleForTesting ? 0.5f : airDodgeScaleY, progress);
            Vector3 newScale = new Vector3(startScale.x * scaleX, startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

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
            Vector3 newScale = new Vector3(startScale.x * scaleX, startScale.y * scaleY, startScale.z);
            transform.localScale = newScale;

            yield return null;
        }

        // Hold effect for remaining duration
        yield return new WaitForSeconds(dashDuration - scaleAnimationDuration);

        // Restore original scale
        transform.localScale = originalScale;
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
                (enemy.CompareTag("Enemy") || enemy.CompareTag("Boss")))
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
        Vector2 targetPos = startPos + (dodgeDirection * dashDistance);

        // Check if this is an air dodge
        bool isAirDodge = playerController != null ? !playerController.isGrounded : !Physics2D.OverlapCircle(transform.position, 0.1f, LayerMask.GetMask("Ground"));

        // Store original gravity and velocity for air dodge
        float originalGravityScale = rb.gravityScale;
        Vector2 originalVelocity = rb.velocity;

        if (isAirDodge)
        {
            // For air dodge: suspend gravity and maintain height
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Stop vertical movement

            // Keep target position at same Y level as start for air dodge
            // Only use horizontal component of dodge direction for air dodge
            Vector2 horizontalDodgeDirection = new Vector2(dodgeDirection.x, 0f).normalized;
            targetPos = new Vector2(startPos.x + (horizontalDodgeDirection.x * dashDistance), startPos.y);
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