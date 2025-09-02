using UnityEngine;
using System.Collections;

[System.Serializable]
public class FXSettings
{
    [Header("FX Configuration")]
    public GameObject prefab;
    public Vector3 offset = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public Vector3 scale = Vector3.one;

    [Header("Timing")]
    public float destroyDelay = 2f; // Thời gian tự hủy (seconds)
}

public class Projectile : MonoBehaviour
{
    [Header("Hit Settings")]
    public LayerMask enemyLayers;
    public float damage = 50f;
    public bool hasHit = false;

    [Header("Projectile Type")]
    public string attackType = "normal"; // "normal" hoặc "up"

    [Header("Area Damage")]
    public float areaRadius = 3f;        // Bán kính nổ lan
    public float areaDamageMultiplier = 0.5f; // Damage area = 50% damage trực tiếp

    [Header("Hit FX Settings")]
    public FXSettings fx1560Settings = new FXSettings(); // Main impact effect
    public FXSettings fx1570Settings = new FXSettings(); // Secondary effect  
    public FXSettings fx1360Settings = new FXSettings(); // Ground effect

    [Header("Legacy FX (Deprecated - Use FX Settings above)")]
    public GameObject hitFX1560;    // anim 1560 - main impact effect
    public GameObject hitFX1570;    // anim 1570 - secondary effect (chỉ dùng khi rơi xuống đất)
    public GameObject hitFX1360;    // anim 1360 - ground effect (chỉ dùng khi rơi xuống đất)

    [Header("Audio")]
    public AudioClip hitSound1;     // S5,32
    public AudioClip hitSound2;     // S5,53

    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Vector3 lastPosition;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure AudioSource for optimal sound quality
        audioSource.volume = 1f;
        audioSource.pitch = 1f;
        audioSource.priority = 128; // Default priority
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.playOnAwake = false;

        rb = GetComponent<Rigidbody2D>();

        // Set up collision detection for enemies only
        gameObject.layer = LayerMask.NameToLayer("ProjectileLayer"); // Tạo layer riêng cho projectile

        // Ignore collision with player layer
        int playerLayer = LayerMask.NameToLayer("Player");
        int projectileLayer = LayerMask.NameToLayer("ProjectileLayer");
        if (playerLayer != -1 && projectileLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(projectileLayer, playerLayer, true);
        }

        // Also find player GameObject and ignore collision directly
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D projectileCollider = GetComponent<Collider2D>();

            if (playerCollider != null && projectileCollider != null)
            {
                Physics2D.IgnoreCollision(projectileCollider, playerCollider, true);
            }
        }

        // Initialize FX settings with default values if not configured
        InitializeFXSettings();

        // Initialize position tracking
        lastPosition = transform.position;
    }

    void Update()
    {
        // Prevent ground penetration with raycast detection
        if (!hasHit && rb != null && !rb.isKinematic)
        {
            CheckGroundCollisionWithRaycast();
        }

        lastPosition = transform.position;
    }

    private void CheckGroundCollisionWithRaycast()
    {
        Vector3 currentPos = transform.position;
        Vector3 direction = (currentPos - lastPosition).normalized;
        float distance = Vector3.Distance(lastPosition, currentPos);

        // Only check if we're moving
        if (distance < 0.01f) return;

        // Cast a ray to detect ground before penetration
        RaycastHit2D hit = Physics2D.Raycast(lastPosition, direction, distance + 0.1f);

        if (hit.collider != null && !hasHit)
        {
            // Check if it's ground
            if (hit.collider.CompareTag("Ground") ||
                hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                hit.collider.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
                hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                hasHit = true;
                Debug.Log($"💥 RAYCAST Ground hit detected: {hit.collider.name} at {hit.point} - PREVENTING PENETRATION");

                // Set position to hit point to prevent penetration
                transform.position = hit.point;

                HitGround(hit.point);
            }
        }
    }

    private void InitializeFXSettings()
    {
        // Initialize fx1560Settings defaults - SHORT DURATION FOR SINGLE ANIMATION
        if (fx1560Settings.scale == Vector3.zero)
            fx1560Settings.scale = new Vector3(0.5f, 0.5f, 1f);
        if (fx1560Settings.destroyDelay == 0f)
            fx1560Settings.destroyDelay = 1f; // REDUCED: Single animation duration
        if (fx1560Settings.prefab == null)
            fx1560Settings.prefab = hitFX1560; // Fallback to legacy

        // Initialize fx1570Settings defaults - SHORT DURATION
        if (fx1570Settings.scale == Vector3.zero)
            fx1570Settings.scale = new Vector3(0.5f, 0.4f, 1f);
        if (fx1570Settings.destroyDelay == 0f)
            fx1570Settings.destroyDelay = 2.5f; // Kéo dài thời gian tồn tại FX1570
        if (fx1570Settings.offset == Vector3.zero)
            fx1570Settings.offset = new Vector3(0, 2f / 16f, 0); // MUGEN pos = 0,2
        if (fx1570Settings.prefab == null)
            fx1570Settings.prefab = hitFX1570; // Fallback to legacy

        // Initialize fx1360Settings defaults - SHORT DURATION
        if (fx1360Settings.scale == Vector3.zero)
            fx1360Settings.scale = new Vector3(0.25f, 0.07f, 1f);
        if (fx1360Settings.destroyDelay == 0f)
            fx1360Settings.destroyDelay = 1f; // REDUCED: Single animation
        if (fx1360Settings.offset == Vector3.zero)
            fx1360Settings.offset = new Vector3(0, 2f / 16f, 0); // MUGEN pos = 0,2
        if (fx1360Settings.prefab == null)
            fx1360Settings.prefab = hitFX1360; // Fallback to legacy

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Priority 1: Check if hit ground IMMEDIATELY (prevent tunneling)
        if (!hasHit && (other.CompareTag("Ground") ||
                       other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                       other.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
                       other.gameObject.layer == LayerMask.NameToLayer("Wall")))
        {
            hasHit = true;

            // Use projectile's current position for explosion (before penetration)
            Vector3 explosionPoint = transform.position;            // Stop movement IMMEDIATELY
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            HitGround(explosionPoint);
            return;
        }

        // Priority 2: Check if hit enemy
        if (IsEnemy(other.gameObject) && !hasHit)
        {
            HitEnemy(other, true); // true = direct hit
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Skip player collision - projectile should pass through player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // Don't return here, but ignore the collision physics
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), collision.collider, true);
            return;
        }

        // Hit ground - both normal and up attacks should explode on ground impact
        if (collision.gameObject.CompareTag("Ground") && !hasHit)
        {
            hasHit = true;
            HitGround(collision.contacts[0].point);
        }
        // Also check for terrain/walls by layer
        else if ((collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                 collision.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
                 collision.gameObject.layer == LayerMask.NameToLayer("Wall")) && !hasHit)
        {
            hasHit = true;
            HitGround(collision.contacts[0].point);
        }
        // Debug any other collision
        else if (!hasHit)
        {
            Debug.Log($"🤔 Projectile collided with: {collision.gameObject.name} (Tag: {collision.gameObject.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)})");
        }
    }

    private bool IsEnemy(GameObject obj)
    {
        // Check if object is in enemy layers
        bool isEnemy = ((1 << obj.layer) & enemyLayers) != 0;
        return isEnemy;
    }

    private void HitEnemy(Collider2D enemy, bool isDirectHit = true)
    {
        hasHit = true;

        // Stop projectile movement immediately for instant explosion
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Hide projectile visual immediately for instant explosion feel
        DisableProjectileVisual();

        // Get enemy center position for precise FX targeting
        Vector3 enemyPosition = enemy.bounds.center;

        // INSTANT explosion at enemy position - IMMEDIATE EXECUTION
        ExecuteHitLogicAtPosition(enemyPosition, true); // true = hit enemy

        // Apply damage to enemy using existing health components - INSTANT
        float damageAmount = isDirectHit ? damage : (damage * areaDamageMultiplier);
        ApplyDamageToEnemy(enemy.gameObject, damageAmount);

        // Area damage to nearby enemies - INSTANT (use enemy position as center)
        if (isDirectHit)
        {
            DealAreaDamage(enemyPosition, enemy.gameObject);
        }

        // Destroy projectile immediately but keep audio playing
        StartCoroutine(DestroyAfterAudio());
    }

    private void HitGround(Vector3 hitPoint)
    {
        // IMMEDIATE position correction to prevent ground penetration
        transform.position = new Vector3(hitPoint.x, hitPoint.y, transform.position.z);

        // Stop projectile movement immediately for instant explosion
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // Camera shake khi rơi xuống đất
        var cameraShake = Camera.main ? Camera.main.GetComponent<CameraShake>() : null;
        if (cameraShake == null)
            cameraShake = FindObjectOfType<CameraShake>();
        if (cameraShake != null)
            cameraShake.ShakeOnce(0.5f, 0.8f);

        // Hide projectile visual immediately for instant explosion feel
        DisableProjectileVisual();

        // INSTANT explosion with no delay - IMMEDIATE EXECUTION
        ExecuteHitLogic(false); // false = hit ground

        // Area damage around ground impact point - IMMEDIATE
        DealAreaDamage(hitPoint, null);

        // Destroy projectile immediately but keep audio playing
        StartCoroutine(DestroyAfterAudio());
    }

    private IEnumerator DestroyAfterAudio()
    {
        // Disable visual immediately but keep audio playing
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Calculate minimum time for audio to play
        float audioTime = GetMinAudioPlayTime();

        // Wait for audio to finish
        yield return new WaitForSeconds(audioTime);

        // Destroy the projectile
        Destroy(gameObject);
    }

    private float GetMaxAudioLength()
    {
        float maxLength = 0f;

        if (hitSound1 != null)
        {
            maxLength = Mathf.Max(maxLength, hitSound1.length);
        }

        if (hitSound2 != null)
        {
            maxLength = Mathf.Max(maxLength, hitSound2.length);
        }

        // Minimum delay to ensure audio plays even if very short
        return Mathf.Max(maxLength, 0.5f);
    }

    private float GetMinAudioPlayTime()
    {
        float minTime = 0.1f; // Minimum time for any audio

        if (hitSound1 != null)
        {
            minTime = Mathf.Max(minTime, hitSound1.length);
        }

        if (hitSound2 != null)
        {
            minTime = Mathf.Max(minTime, hitSound2.length);
        }

        // Cap at reasonable time to prevent too long delays
        return Mathf.Min(minTime, 2f);
    }
    private void DisableProjectileVisual()
    {
        // Hide projectile visual immediately for instant explosion feel
        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }

        // Disable colliders to prevent multiple hits
        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // Stop any particle systems
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Debug.Log("✅ Projectile visual disabled for instant explosion effect");
    }

    private void ApplyDamageToEnemy(GameObject enemy, float damageAmount)
    {
        int damageInt = Mathf.RoundToInt(damageAmount);

        // Try BatController first (most common enemy)
        var batController = enemy.GetComponent<BatController>();
        if (batController != null)
        {
            batController.TakeDamage(damageInt);
            return;
        }

        // Try PlayerResources (if projectile somehow hits player)
        var playerResources = enemy.GetComponent<PlayerResources>();
        if (playerResources != null)
        {
            playerResources.TakeDamage(damageInt);
            return;
        }

        // Try generic Damageable interface if available
        var damageable = enemy.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeHit(damageAmount); // Use float damage directly
            return;
        }
    }

    private void DealAreaDamage(Vector3 center, GameObject excludeTarget)
    {
        Debug.Log($"🌟 DEALING AREA DAMAGE at {center}, radius: {areaRadius}");

        // Find all enemies in area
        Collider2D[] enemiesInArea = Physics2D.OverlapCircleAll(center, areaRadius, enemyLayers);

        foreach (var enemyCollider in enemiesInArea)
        {
            // Skip the enemy that was hit directly
            if (excludeTarget != null && enemyCollider.gameObject == excludeTarget)
                continue;

            float areaDamage = damage * areaDamageMultiplier;
            ApplyDamageToEnemy(enemyCollider.gameObject, areaDamage);
            Debug.Log($"💥 Area damage {areaDamage} to {enemyCollider.name}");
        }
    }

    private void ExecuteHitLogic(bool hitEnemy)
    {
        Vector3 hitPos = transform.position;
        ExecuteHitLogicAtPosition(hitPos, hitEnemy);
    }

    private void ExecuteHitLogicAtPosition(Vector3 hitPos, bool hitEnemy)
    {
        Debug.Log($"🎯 EXECUTING HIT LOGIC at {hitPos} (HitEnemy: {hitEnemy}, AttackType: {attackType})");
        Debug.Log($"🔊 Audio: hitSound1={hitSound1 != null}, hitSound2={hitSound2 != null}, audioSource={audioSource != null}");
        Debug.Log($"🎆 FX: fx1560={fx1560Settings.prefab != null}, fx1570={fx1570Settings.prefab != null}, fx1360={fx1360Settings.prefab != null}");

        // 1. Play hit sounds with higher volume and ensure they're not cut off
        if (hitSound1 && audioSource)
        {
            audioSource.volume = 1f; // Max volume
            audioSource.PlayOneShot(hitSound1, 1f); // Full volume
            Debug.Log("✅ Played hitSound1 at full volume");
        }
        else
        {
            Debug.LogWarning($"❌ Cannot play hitSound1: sound={hitSound1 != null}, audioSource={audioSource != null}");
        }

        if (hitSound2 && audioSource)
        {
            audioSource.volume = 1f; // Max volume
            audioSource.PlayOneShot(hitSound2, 1f); // Full volume
            Debug.Log("✅ Played hitSound2 at full volume");
        }
        else
        {
            Debug.LogWarning($"❌ Cannot play hitSound2: sound={hitSound2 != null}, audioSource={audioSource != null}");
        }

        // 2. Environment shake (MUGEN: EnvShake time = 30)
        var cameraShake = Camera.main?.GetComponent<CameraShake>();
        if (cameraShake)
        {
            cameraShake.ShakeOnce(0.5f, 0.5f); // 30 frames = 0.5s duration, 0.5 magnitude
            Debug.Log("✅ Camera shake triggered");
        }
        else
        {
            Debug.LogWarning("❌ No CameraShake component found on Camera.main");
        }

        // 3. Spawn hit effects at specified position - different based on hit type
        SpawnHitFX(hitPos, hitEnemy);
    }

    private void SpawnHitFX(Vector3 hitPos, bool hitEnemy)
    {
        Debug.Log($"🎆 SPAWNING HIT FX at {hitPos} (HitEnemy: {hitEnemy}, AttackType: {attackType})");

        Vector3 basePosition;

        if (hitEnemy)
        {
            // When hitting enemy, use exact hit position (no offset for precise targeting)
            basePosition = hitPos;
            Debug.Log($"🎯 Enemy hit - using exact position: {basePosition}");
        }
        else
        {
            // When hitting ground, snap to ground level and use offset settings
            basePosition = new Vector3(hitPos.x, 0f, hitPos.z);
            Debug.Log($"� Ground hit - using ground position: {basePosition}");
        }

        // ALWAYS spawn fx1560 for main impact effect
        SpawnFX(fx1560Settings, basePosition, "FX1560", hitEnemy);

        // Fallback to legacy if new settings not configured
        if (fx1560Settings.prefab == null && hitFX1560 != null)
        {
            Vector3 fx1560Pos = hitEnemy ? basePosition : basePosition + new Vector3(0, 2f * (1f / 16f), 0);
            GameObject fx1 = Instantiate(hitFX1560, fx1560Pos, Quaternion.identity);
            fx1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            SetupFXDestruction(fx1, 1f);
            Debug.Log($"✅ Spawned legacy hitFX1560 at {fx1560Pos} (Enemy hit: {hitEnemy})");
        }

        // Spawn fx1570 and fx1360 ONLY for ground impacts (with offset)
        bool shouldSpawnGroundFX = !hitEnemy; // Only ground impacts get secondary FX

        if (shouldSpawnGroundFX)
        {
            Debug.Log("🌍 Spawning ground destruction FX (fx1570 + fx1360) with offset");

            // Spawn fx1570 with offset settings for ground impact
            SpawnFX(fx1570Settings, basePosition, "FX1570", false);

            // Fallback to legacy fx1570
            if (fx1570Settings.prefab == null && hitFX1570 != null)
            {
                Vector3 fx1570Pos = basePosition + new Vector3(0, 2f * (1f / 16f), 0);
                GameObject fx2 = Instantiate(hitFX1570, fx1570Pos, Quaternion.identity);
                fx2.transform.localScale = new Vector3(0.5f, 0.4f, 1f);
                SetupFXDestruction(fx2, 1f);
                Debug.Log($"✅ Spawned legacy hitFX1570 at {fx1570Pos}");
            }

            // Spawn fx1360 with offset settings for ground impact
            SpawnFX(fx1360Settings, basePosition, "FX1360", false);

            // Fallback to legacy fx1360
            if (fx1360Settings.prefab == null && hitFX1360 != null)
            {
                Vector3 fx1360Pos = basePosition + new Vector3(0, 2f * (1f / 16f), 0);
                GameObject fx3 = Instantiate(hitFX1360, fx1360Pos, Quaternion.identity);
                fx3.transform.localScale = new Vector3(0.25f, 0.07f, 1f);
                SetupFXDestruction(fx3, 1f);
                Debug.Log($"✅ Spawned legacy hitFX1360 at {fx1360Pos}");
            }
        }
        else
        {
            Debug.Log($"⏭️ Skipping ground FX - Enemy hit, only main FX spawned");
        }
    }

    private void SpawnFX(FXSettings fxSettings, Vector3 basePosition, string fxName, bool hitEnemy)
    {
        if (fxSettings.prefab == null)
        {
            Debug.Log($"⏭️ {fxName} prefab not assigned in FXSettings");
            return;
        }

        Vector3 finalPosition;

        if (hitEnemy)
        {
            // When hitting enemy, ignore offset - spawn exactly at hit position for precise targeting
            finalPosition = basePosition;
            Debug.Log($"🎯 {fxName} spawning at exact enemy position: {finalPosition} (no offset)");
        }
        else
        {
            // When hitting ground, apply offset settings for proper ground FX positioning
            finalPosition = basePosition + fxSettings.offset;
            Debug.Log($"🌍 {fxName} spawning with offset: {basePosition} + {fxSettings.offset} = {finalPosition}");
        }

        // Calculate final rotation
        Quaternion finalRotation = Quaternion.Euler(fxSettings.rotation);

        // Spawn the FX
        GameObject fx = Instantiate(fxSettings.prefab, finalPosition, finalRotation);

        // Apply scale
        fx.transform.localScale = fxSettings.scale;

        // Setup auto destruction with custom timing
        SetupFXDestruction(fx, fxSettings.destroyDelay);

        Debug.Log($"✅ Spawned {fxName} at {finalPosition} with rotation {fxSettings.rotation} and scale {fxSettings.scale} (HitEnemy: {hitEnemy})");
    }

    private void SetupFXDestruction(GameObject fx, float delay)
    {
        // Try to use existing AutoDestroyOnAnimationEnd component
        var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
        if (autoDestroy != null)
        {
            autoDestroy.destroyOnFirstLoop = true; // FORCE single animation loop
            autoDestroy.fallbackLifetime = delay;
            autoDestroy.startupGrace = 0.05f; // Reduce startup grace for faster response
            Debug.Log($"✅ Configured AutoDestroyOnAnimationEnd for {fx.name} (single loop, fallback: {delay}s)");
        }
        else
        {
            // Add AutoDestroyOnAnimationEnd component for proper single-loop destruction
            autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
            autoDestroy.destroyOnFirstLoop = true; // Single animation loop
            autoDestroy.fallbackLifetime = delay;
            autoDestroy.startupGrace = 0.05f;
            Debug.Log($"✅ Added AutoDestroyOnAnimationEnd to {fx.name} for single animation loop");
        }
    }

    // Visual debug for area damage radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, areaRadius * 0.5f);
    }
}
