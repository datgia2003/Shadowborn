using UnityEngine;
using System.Collections;

public class Caster : MonoBehaviour
{
    [Header("FX Prefabs")]
    public GameObject fxAura;        // anim 6219 (pos 0,-6)
    public GameObject fxCircle1;     // anim 6217 (pos 13,-95) - spawn 1 lần khi bắt đầu tụ lực
    public GameObject fxCircle2;     // anim 1880 (pos 12,-67) - spawn khi tụ lực
    public GameObject fxCircle3;     // anim 1850 (pos 12,-94) - spawn khi tụ lực
    public GameObject fxCircle4;     // anim 1860 (pos 12,-94) - spawn khi tụ lực
    public GameObject fxImpact;      // anim 1870 (pos 12,-77) - spawn khi release
    public GameObject projectilePrefab; // helper 1820 (pos 12,-80) - ĐẠN PHÉP bay ra gây damage cho enemy

    [Header("Projectile FX")]
    public GameObject fx1840;        // anim 1840 - effect khi spawn projectile 
    public GameObject fx1850;        // anim 1850 - additional effect khi spawn projectile

    [Header("Hit FX - MUGEN State 1830")]
    public GameObject hitFX1560;     // anim 1560 - main impact effect (scale .5,.5)
    public GameObject hitFX1570;     // anim 1570 - secondary effect (scale .5,.4)
    public GameObject hitFX1360;     // anim 1360 - ground effect (scale .25,.07)

    [Header("Audio")]
    public AudioClip sfxCharge;      // S5,48 (khi bắt đầu tụ lực)
    public AudioClip sfxRelease;     // S5,46 (khi release)
    public AudioClip hitSound1;      // S5,32 (hit sound 1)
    public AudioClip hitSound2;      // S5,53 (hit sound 2)
    public AudioSource audioSource;

    [Header("AI Settings")]
    public float idleDuration = 1.5f;        // Thời gian đứng im giữa các attack
    public float normalAttackChargeTime = 2f;   // Thời gian tụ lực attack ngang
    public float upAttackChargeTime = 3.5f;     // Thời gian tụ lực attack up
    public float recoveryDuration = 1f;         // Thời gian recovery sau khi release
    public float lifeTime = 22f;           // 1050 ticks / 60 = 17.5s
    public LayerMask enemyLayers;            // Để detect enemy

    [System.Serializable]
    public class FXSettings
    {
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
    }

    [System.Serializable]
    public class ProjectileSettings
    {
        [Header("Projectile Transform")]
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;

        [Header("Spawn FX Settings")]
        public Vector3 fx1840Offset;
        public Vector3 fx1840Rotation; // Thêm rotation cho fx1840
        public Vector3 fx1840Scale = Vector3.one;
        public Vector3 fx1850Offset;
        public Vector3 fx1850Scale = new Vector3(0.25f, 0.25f, 1f);

        [Header("Movement")]
        public float speed = 12f;

        [Header("Up Attack Targeting")]
        public float targetRadius = 30f;     // Bán kính tìm enemy
        public float dropSpread = 15f;       // Độ rộng vùng rơi
        public float enemyBias = 0.7f;       // Xu hướng target enemy (0-1)
    }

    [Header("FX Settings - Manual Adjustment")]
    [Header("Normal Attack FX Settings")]
    public FXSettings fxAuraSettings = new FXSettings();
    public FXSettings fxCircle1Settings_Normal = new FXSettings();
    public FXSettings fxCircle2Settings_Normal = new FXSettings();
    public FXSettings fxCircle3Settings_Normal = new FXSettings();
    public FXSettings fxCircle4Settings_Normal = new FXSettings();
    public FXSettings fxImpactSettings_Normal = new FXSettings();

    [Header("Up Attack FX Settings")]
    public FXSettings fxCircle1Settings_Up = new FXSettings();
    public FXSettings fxCircle2Settings_Up = new FXSettings();
    public FXSettings fxCircle3Settings_Up = new FXSettings();
    public FXSettings fxCircle4Settings_Up = new FXSettings();
    public FXSettings fxImpactSettings_Up = new FXSettings();

    [Header("Projectile Settings")]
    [Header("Normal Attack Projectile")]
    public ProjectileSettings projectileSettings_Normal = new ProjectileSettings();

    [Header("Up Attack Projectile")]
    public ProjectileSettings projectileSettings_Up = new ProjectileSettings();

    // AI State Machine
    private enum CasterState
    {
        Spawning,    // Vừa spawn, đang idle
        Idle,        // Đứng im giữa các attack
        Charging,    // Đang tụ lực
        Releasing,   // Đang release đòn đánh
        Recovering   // Recovery sau khi release (1s) trước khi về idle
    }

    private CasterState currentState = CasterState.Spawning;
    private Animator animator;
    private Rigidbody2D rb;
    private float stateTimer = 0f;
    private float totalLifeTimer = 0f;

    // Attack variables
    private string currentAttackType = ""; // "normal" or "up"
    private float currentChargeTime = 0f;
    private bool hasSpawnedChargeOnce = false;
    private bool isCompletingAttack = false; // Flag để prevent despawn khi đang attack
    private GameObject[] chargeEffects = new GameObject[4]; // Circle1, Circle2, Circle3, Circle4
    private GameObject currentAura;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Ensure AudioSource is available
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f; // 2D sound
                audioSource.playOnAwake = false;
            }
        }

        // MUGEN: type = S, physics = S, velset = 0,0
        if (rb)
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0f; // physics = S (không rơi)
        }

        // Bắt đầu ở trạng thái spawning
        StartSpawning();

        // Smart despawn: check mỗi frame thay vì auto-destroy cứng nhắc
        StartCoroutine(CheckLifeTimeCoroutine());
    }

    void Update()
    {
        stateTimer += Time.deltaTime;
        totalLifeTimer += Time.deltaTime;

        switch (currentState)
        {
            case CasterState.Spawning:
                UpdateSpawning();
                break;
            case CasterState.Idle:
                UpdateIdle();
                break;
            case CasterState.Charging:
                UpdateCharging();
                break;
            case CasterState.Releasing:
                UpdateReleasing();
                break;
            case CasterState.Recovering:
                UpdateRecovering();
                break;
        }
    }

    // ==================== SPAWNING STATE ====================

    private void StartSpawning()
    {
        currentState = CasterState.Spawning;
        stateTimer = 0f;

        if (animator)
        {
            animator.Play("Caster"); // Animation idle
        }

        // Spawn aura ngay lập tức
        SpawnAura();
    }

    private void UpdateSpawning()
    {
        // Sau 1.5s thì chuyển sang idle để chuẩn bị attack
        if (stateTimer >= idleDuration)
        {
            StartIdle();
        }
    }

    // ==================== IDLE STATE ====================

    private void StartIdle()
    {
        currentState = CasterState.Idle;
        stateTimer = 0f;

        if (animator)
        {
            animator.Play("Caster"); // Animation idle
        }
    }

    private void UpdateIdle()
    {
        // Sau 1.5s idle thì bắt đầu attack
        if (stateTimer >= idleDuration)
        {
            StartRandomAttack();
        }
    }

    // ==================== ATTACKING STATE ====================

    private void StartRandomAttack()
    {
        // Random giữa 2 loại attack
        bool useUpAttack = Random.Range(0f, 1f) > 0.5f;
        currentAttackType = useUpAttack ? "up" : "normal";
        currentChargeTime = useUpAttack ? upAttackChargeTime : normalAttackChargeTime;

        // Bắt đầu phase tụ lực
        StartCharging();
    }

    // ==================== CHARGING STATE ====================

    private void StartCharging()
    {
        currentState = CasterState.Charging;
        stateTimer = 0f;
        hasSpawnedChargeOnce = false;
        isCompletingAttack = true; // Bắt đầu attack sequence - không được despawn

        if (animator)
        {
            // Play animation tương ứng với loại attack
            string animName = currentAttackType == "up" ? "Caster_UpAttack" : "Caster_Attack";
            animator.Play(animName);
        }
    }

    private void UpdateCharging()
    {
        // Spawn charge effects 1 lần ngay khi bắt đầu tụ lực
        if (!hasSpawnedChargeOnce)
        {
            OnChargeStart();
            hasSpawnedChargeOnce = true;
        }

        // Sau khi tụ lực đủ thời gian thì release
        if (stateTimer >= currentChargeTime)
        {
            StartReleasing();
        }
    }

    // ==================== RELEASING STATE ====================

    private void StartReleasing()
    {
        currentState = CasterState.Releasing;
        stateTimer = 0f;

        // Release ngay lập tức
        OnRelease();

        // Cleanup charge effects
        CleanupChargeEffects();
    }

    private void UpdateReleasing()
    {
        // Release phase rất ngắn, chỉ để spawn effects rồi chuyển sang recovery
        if (stateTimer >= 0.1f) // 100ms
        {
            StartRecovering();
        }
    }

    // ==================== CHARGE START ====================

    private void OnChargeStart()
    {

        // Play charge sound
        if (audioSource && sfxCharge)
        {
            audioSource.volume = 0.7f;
            audioSource.PlayOneShot(sfxCharge);
        }

        // Spawn circle1 (6217) - chỉ 1 lần khi bắt đầu tụ lực
        SpawnCircle1();

        // Spawn charge circles (1880, 1850, 1860) - theo hướng attack
        SpawnChargeCircles();
    }

    // ==================== RELEASE ====================

    private void OnRelease()
    {

        // Stop any current audio first để không bị ghi đè
        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Play release sound với enhanced sound fix
        if (audioSource && sfxRelease)
        {
            audioSource.volume = 1f; // Max volume
            audioSource.pitch = 1f;  // Normal pitch
            audioSource.PlayOneShot(sfxRelease);

            // Force play nếu PlayOneShot không hoạt động
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sfxRelease;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f;
                audioSource.playOnAwake = false;
            }
        }

        // Camera shake
        var camShake = FindObjectOfType<CameraShake>();
        if (camShake)
        {
            camShake.ShakeOnce(10f / 60f, 0.4f);
        }

        // Spawn impact effect và projectile
        SpawnImpactEffect();
        SpawnProjectile();
    }

    // ==================== CLEANUP ====================

    // ==================== RECOVERY STATE ====================

    private void StartRecovering()
    {
        currentState = CasterState.Recovering;
        stateTimer = 0f;

        if (animator)
        {
            animator.Play("Caster"); // Back to idle animation
        }
    }

    private void UpdateRecovering()
    {
        // Sau recovery time thì về idle và cho phép despawn
        if (stateTimer >= recoveryDuration)
        {
            isCompletingAttack = false; // Attack sequence hoàn tất
            StartIdle();
        }
    }

    // ==================== SMART LIFETIME MANAGEMENT ====================

    private IEnumerator CheckLifeTimeCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f); // Check mỗi 100ms

            // Nếu đã hết lifetime
            if (totalLifeTimer >= lifeTime)
            {
                // Nếu không đang attack thì despawn ngay
                if (!isCompletingAttack)
                {
                    CleanupAllEffects();
                    Destroy(gameObject);
                    yield break;
                }
                else
                {
                    // Tiếp tục đợi đến khi attack sequence hoàn tất
                }
            }
        }
    }

    private void CleanupChargeEffects()
    {

        // Remove charge circles (1880, 1850, 1860) - nhưng giữ circle1 (6217)
        for (int i = 1; i < chargeEffects.Length; i++) // Bắt đầu từ index 1
        {
            if (chargeEffects[i] != null)
            {
                // Unparent trước khi destroy
                chargeEffects[i].transform.SetParent(null);
                Destroy(chargeEffects[i]);
                chargeEffects[i] = null;
            }
        }
    }

    private void CleanupAllEffects()
    {
        // Cleanup aura
        if (currentAura != null)
        {
            // Unparent trước khi destroy để tránh lỗi
            currentAura.transform.SetParent(null);
            Destroy(currentAura);
            currentAura = null;
        }

        // Cleanup all charge effects including circle1
        for (int i = 0; i < chargeEffects.Length; i++)
        {
            if (chargeEffects[i] != null)
            {
                // Unparent trước khi destroy
                chargeEffects[i].transform.SetParent(null);
                Destroy(chargeEffects[i]);
                chargeEffects[i] = null;
            }
        }

        // Find và cleanup các FX còn lại có thể đã SetParent(transform)
        var childFX = GetComponentsInChildren<AutoDestroyOnAnimationEnd>();
        foreach (var fx in childFX)
        {
            if (fx != null && fx.gameObject != this.gameObject)
            {
                fx.transform.SetParent(null);
                Destroy(fx.gameObject);
            }
        }
    }

    // ==================== FX SPAWNING METHODS ====================

    private void SpawnAura()
    {
        if (fxAura)
        {
            Vector3 pos = transform.position + new Vector3(0, 6f / 16f, 0); // (0, +0.375)
            GameObject fx = Instantiate(fxAura, pos, Quaternion.identity);
            fx.transform.localScale = new Vector3(0.4f, 0.3f, 1);
            fx.transform.SetParent(transform);

            // Add auto-destroy component
            var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = false; // Aura loop liên tục
                autoDestroy.fallbackLifetime = lifeTime + 1f; // Fallback theo lifetime của caster
            }

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -4;

            // Store reference để cleanup khi cần
            currentAura = fx;
        }
    }

    // ==================== SEPARATED FX SPAWNING METHODS ====================

    private void SpawnCircle1()
    {
        // Circle1 (6217): pos = 13,+95
        if (fxCircle1)
        {
            Vector3 basePos = new Vector3(13f / 16f, 95f / 16f, 0);
            FXSettings settings = currentAttackType == "up" ? fxCircle1Settings_Up : fxCircle1Settings_Normal;
            Vector3 pos = transform.position + ApplyFXSettings(basePos, settings);
            Vector3 rotation = ApplyFXRotation(settings);

            GameObject fx = Instantiate(fxCircle1, pos, Quaternion.Euler(rotation));
            fx.transform.localScale = Vector3.Scale(new Vector3(0.15f, 0.15f, 1), settings.scale);
            fx.transform.SetParent(transform);

            // Add auto-destroy component
            var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = true;
                autoDestroy.fallbackLifetime = 3f;
            }

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -1;

            chargeEffects[0] = fx;
        }
    }

    private void SpawnChargeCircles()
    {
        // Circle2 (1880): pos = 12,+67
        if (fxCircle2)
        {
            Vector3 basePos = new Vector3(12f / 16f, 67f / 16f, 0);
            FXSettings settings = currentAttackType == "up" ? fxCircle2Settings_Up : fxCircle2Settings_Normal;
            Vector3 pos = transform.position + ApplyFXSettings(basePos, settings);
            Vector3 rotation = ApplyFXRotation(settings);

            GameObject fx = Instantiate(fxCircle2, pos, Quaternion.Euler(rotation));
            fx.transform.localScale = Vector3.Scale(new Vector3(0.2f, 0.2f, 1), settings.scale);

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -2;

            chargeEffects[1] = fx;
        }

        // Circle3 (1850): pos = 12,+94  
        if (fxCircle3)
        {
            Vector3 basePos = new Vector3(12f / 16f, 94f / 16f, 0);
            FXSettings settings = currentAttackType == "up" ? fxCircle3Settings_Up : fxCircle3Settings_Normal;
            Vector3 pos = transform.position + ApplyFXSettings(basePos, settings);
            Vector3 rotation = ApplyFXRotation(settings);

            GameObject fx = Instantiate(fxCircle3, pos, Quaternion.Euler(rotation));
            fx.transform.localScale = Vector3.Scale(new Vector3(0.25f, 0.25f, 1), settings.scale);

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -2;

            chargeEffects[2] = fx;
        }

        // Circle4 (1860): pos = 12,+94
        if (fxCircle4)
        {
            Vector3 basePos = new Vector3(12f / 16f, 94f / 16f, 0);
            FXSettings settings = currentAttackType == "up" ? fxCircle4Settings_Up : fxCircle4Settings_Normal;
            Vector3 pos = transform.position + ApplyFXSettings(basePos, settings);
            Vector3 rotation = ApplyFXRotation(settings);

            GameObject fx = Instantiate(fxCircle4, pos, Quaternion.Euler(rotation));
            fx.transform.localScale = Vector3.Scale(new Vector3(0.3f, 0.3f, 1), settings.scale);

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -3;

            chargeEffects[3] = fx;
        }
    }

    private Vector3 ApplyFXSettings(Vector3 basePos, FXSettings settings)
    {
        // Lấy facing direction
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        // Nếu đang face left (facingDir < 0), flip toàn bộ
        if (facingDir < 0)
        {
            // Flip base position X
            basePos.x = -basePos.x;

            // Flip settings offset X (vì user đã set manual cho facing right)
            Vector3 flippedOffset = settings.offset;
            flippedOffset.x = -flippedOffset.x;

            return basePos + flippedOffset;
        }
        else
        {
            // Facing right - dùng settings như bình thường
            return basePos + settings.offset;
        }
    }

    private Vector3 ApplyFXRotation(FXSettings settings)
    {
        Vector3 rotation = settings.rotation;

        // Flip rotation Y khi facing left
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        if (facingDir < 0)
        {
            // Mirror Y rotation để FX quay đúng hướng
            rotation.y = 180f - rotation.y;
        }

        return rotation;
    }

    private Vector3 ApplyProjectileSettings(Vector3 basePos, ProjectileSettings settings)
    {
        // Lấy facing direction
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        // Nếu đang face left (facingDir < 0), flip toàn bộ
        if (facingDir < 0)
        {
            // Flip base position X
            basePos.x = -basePos.x;

            // Flip settings offset X (vì user đã set manual cho facing right)
            Vector3 flippedOffset = settings.offset;
            flippedOffset.x = -flippedOffset.x;

            return basePos + flippedOffset;
        }
        else
        {
            // Facing right - dùng settings như bình thường
            return basePos + settings.offset;
        }
    }

    private Vector3 ApplyProjectileRotation(ProjectileSettings settings)
    {
        Vector3 rotation = settings.rotation;

        // Flip rotation Y khi facing left
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;
        if (facingDir < 0)
        {
            // Mirror Y rotation để projectile quay đúng hướng
            rotation.y = 180f - rotation.y;
        }

        return rotation;
    }

    private void SpawnProjectileFX(Vector3 spawnPos, ProjectileSettings settings, GameObject projectile)
    {
        float facingDir = transform.localScale.x > 0 ? 1f : -1f;

        // MUGEN: Explod anim = 1840, pos = 0,30
        if (fx1840 != null)
        {
            Vector3 fx1840Pos = new Vector3(0, 30f / 16f, 0); // pos = 0,30 relative to projectile
            Vector3 adjustedOffset = settings.fx1840Offset;
            if (facingDir < 0) adjustedOffset.x = -adjustedOffset.x;

            fx1840Pos += adjustedOffset;

            // Apply rotation from settings
            Quaternion fx1840Rotation = Quaternion.Euler(settings.fx1840Rotation);

            GameObject fx = Instantiate(fx1840, projectile.transform.position + fx1840Pos, fx1840Rotation);
            fx.transform.localScale = settings.fx1840Scale;
            fx.transform.SetParent(projectile.transform); // Follow projectile
            fx.transform.localPosition = fx1840Pos; // Set relative position
            fx.transform.localRotation = fx1840Rotation; // Set relative rotation

            // Auto-destroy với lifetime dài hơn
            var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = false; // Không destroy khi loop
                autoDestroy.fallbackLifetime = 10f; // Lifetime dài hơn
            }
            else
            {
                autoDestroy.destroyOnFirstLoop = false;
                autoDestroy.fallbackLifetime = 10f;
            }
        }

        // MUGEN: Explod anim = 1850, pos = 0,0, scale = .25,.25
        if (fx1850 != null)
        {
            Vector3 fx1850Pos = Vector3.zero; // pos = 0,0 relative to projectile
            Vector3 adjustedOffset = settings.fx1850Offset;
            if (facingDir < 0) adjustedOffset.x = -adjustedOffset.x;

            fx1850Pos += adjustedOffset;

            GameObject fx = Instantiate(fx1850, projectile.transform.position + fx1850Pos, Quaternion.identity);
            fx.transform.localScale = settings.fx1850Scale; // Default: .25,.25,1
            fx.transform.SetParent(projectile.transform); // Follow projectile
            fx.transform.localPosition = fx1850Pos; // Set relative position

            // Auto-destroy với lifetime dài hơn
            var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = false; // Không destroy khi loop
                autoDestroy.fallbackLifetime = 10f; // Lifetime dài hơn
            }
            else
            {
                autoDestroy.destroyOnFirstLoop = false;
                autoDestroy.fallbackLifetime = 10f;
            }
        }
    }

    private void SpawnImpactEffect()
    {
        if (fxImpact)
        {
            Vector3 basePos = new Vector3(12f / 16f, 77f / 16f, 0);
            FXSettings settings = currentAttackType == "up" ? fxImpactSettings_Up : fxImpactSettings_Normal;
            Vector3 adjustedPos = ApplyFXSettings(basePos, settings);
            Vector3 adjustedRotation = ApplyFXRotation(settings);

            Vector3 pos = transform.position + adjustedPos;
            GameObject fx = Instantiate(fxImpact, pos, Quaternion.Euler(adjustedRotation));
            fx.transform.localScale = Vector3.Scale(new Vector3(0.6f, 0.7f, 1), settings.scale);
            fx.transform.SetParent(transform);

            // Add auto-destroy component
            var autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = true;
                autoDestroy.fallbackLifetime = 2f;
            }

            var renderer = fx.GetComponent<SpriteRenderer>();
            if (renderer) renderer.sortingOrder = -3;
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab)
        {
            Vector3 basePos = new Vector3(12f / 16f, 80f / 16f, 0);
            ProjectileSettings settings = currentAttackType == "up" ? projectileSettings_Up : projectileSettings_Normal;
            float facingDir = transform.localScale.x > 0 ? 1f : -1f;

            GameObject proj; // Declare proj variable here

            if (currentAttackType == "up")
            {
                // UP ATTACK: Theo logic MUGEN state 1820
                // 1. Spawn projectile ở caster position trước
                Vector3 pos = transform.position + ApplyProjectileSettings(basePos, settings);
                Vector3 rotation = ApplyProjectileRotation(settings);

                proj = Instantiate(projectilePrefab, pos, Quaternion.Euler(rotation));
                proj.transform.localScale = Vector3.Scale(new Vector3(0.4f, 0.4f, 1), settings.scale);

                // Setup Projectile component for collision detection
                SetupProjectileComponent(proj);

                // 1.5. Spawn FX effects attached to projectile (MUGEN: explod anim 1840, 1850)
                SpawnProjectileFX(pos, settings, proj);

                // 2. Add component để handle MUGEN state logic
                var stateHandler = proj.AddComponent<ProjectileStateHandler>();
                stateHandler.Initialize(currentAttackType, facingDir);
            }
            else
            {
                // NORMAL ATTACK: Projectile bay ngang từ caster (không theo state 1820)
                Vector3 pos = transform.position + ApplyProjectileSettings(basePos, settings);
                Vector3 rotation = ApplyProjectileRotation(settings);

                proj = Instantiate(projectilePrefab, pos, Quaternion.Euler(rotation));
                proj.transform.localScale = Vector3.Scale(new Vector3(0.4f, 0.4f, 1), settings.scale);

                // Setup Projectile component for collision detection
                SetupProjectileComponent(proj);

                // Spawn FX effects attached to projectile
                SpawnProjectileFX(pos, settings, proj);

                var rb2d = proj.GetComponent<Rigidbody2D>();
                if (rb2d)
                {
                    rb2d.velocity = new Vector2(facingDir * settings.speed, 0); // Sử dụng configurable speed
                    rb2d.gravityScale = 0f; // Không gravity để bay thẳng
                }
            }

            // Setup projectile FX spawn effects tại vị trí spawn (sử dụng pos từ projectile)
            // Note: FX sẽ được spawn tại vị trí projectile đã được tính toán

            // Add auto-destroy component for both types
            var autoDestroy = proj.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = proj.AddComponent<AutoDestroyOnAnimationEnd>();
                autoDestroy.destroyOnFirstLoop = false; // Không destroy khi animation loop
                autoDestroy.fallbackLifetime = currentAttackType == "up" ? 15f : 8f; // Up attack: 15s, Normal: 8s
            }
            else
            {
                // Nếu đã có component, adjust settings
                autoDestroy.destroyOnFirstLoop = false;
                autoDestroy.fallbackLifetime = currentAttackType == "up" ? 15f : 8f;
            }
        }
    }    // ==================== UTILITY METHODS ====================

    private bool IsAtAnimationFrame(int frame)
    {
        if (!animator) return false;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Check if we're in an attack animation based on current attack type
        string expectedAnim = currentAttackType == "normal" ? "1820" : "1830";
        if (!stateInfo.IsName(expectedAnim)) return false;

        // Calculate current frame (assuming 60fps animation)
        float normalizedTime = stateInfo.normalizedTime % 1f;
        int totalFrames = 60; // Giả sử animation dài 60 frames
        int currentFrame = Mathf.FloorToInt(normalizedTime * totalFrames);

        return currentFrame == frame;
    }

    void OnDestroy()
    {
        CleanupAllEffects();
    }

    // ==================== HIT OVERRIDE ====================

    public void OnHit()
    {
        // Cleanup effects
        CleanupAllEffects();
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // if (other.CompareTag("EnemyAttack") || other.CompareTag("Enemy") || other.CompareTag("PlayerAttack"))
        if (other.CompareTag("Enemy"))
        {
            OnHit();
        }
    }

    // ==================== PROJECTILE SETUP ====================

    private void SetupProjectileComponent(GameObject projectile)
    {
        // Add Projectile component using reflection to avoid compile error
        var projComponent = projectile.GetComponent("Projectile");
        if (projComponent == null)
        {
            projComponent = projectile.AddComponent(System.Type.GetType("Projectile"));
        }

        if (projComponent != null)
        {
            // Use reflection to set properties
            var type = projComponent.GetType();

            // Setup collision settings
            type.GetField("enemyLayers")?.SetValue(projComponent, enemyLayers);
            type.GetField("damage")?.SetValue(projComponent, 50f);

            // Set attack type
            type.GetField("attackType")?.SetValue(projComponent, currentAttackType);

            // Set area damage settings  
            type.GetField("areaRadius")?.SetValue(projComponent, 3f);
            type.GetField("areaDamageMultiplier")?.SetValue(projComponent, 0.5f);

            // Ensure projectile has proper physics setup for ground collision
            var rb = projectile.GetComponent<Rigidbody2D>();
            var triggerCollider = projectile.GetComponent<Collider2D>();

            // Add a separate non-trigger collider for ground collision if needed
            if (triggerCollider != null && triggerCollider.isTrigger)
            {
                // Check if already has a non-trigger collider
                var colliders = projectile.GetComponents<Collider2D>();
                bool hasNonTrigger = false;
                foreach (var col in colliders)
                {
                    if (!col.isTrigger)
                    {
                        hasNonTrigger = true;
                        break;
                    }
                }

                if (!hasNonTrigger)
                {
                    // Add a small non-trigger collider for ground collision
                    var groundCollider = projectile.AddComponent<CircleCollider2D>();
                    groundCollider.isTrigger = false;
                    groundCollider.radius = 0.1f; // Small radius for ground detection
                }
            }

            // Only assign hit FX prefabs if Caster has them (preserve Projectile prefab settings)
            if (hitFX1560 != null)
                type.GetField("hitFX1560")?.SetValue(projComponent, hitFX1560);
            if (hitFX1570 != null)
                type.GetField("hitFX1570")?.SetValue(projComponent, hitFX1570);
            if (hitFX1360 != null)
                type.GetField("hitFX1360")?.SetValue(projComponent, hitFX1360);

            // Only assign hit sounds if Caster has them (preserve Projectile prefab settings)
            if (hitSound1 != null)
                type.GetField("hitSound1")?.SetValue(projComponent, hitSound1);
            if (hitSound2 != null)
                type.GetField("hitSound2")?.SetValue(projComponent, hitSound2);

            // Debug assigned prefabs
            var assignedFX1560 = type.GetField("hitFX1560")?.GetValue(projComponent);
            var assignedFX1570 = type.GetField("hitFX1570")?.GetValue(projComponent);
            var assignedFX1360 = type.GetField("hitFX1360")?.GetValue(projComponent);
            var assignedSound1 = type.GetField("hitSound1")?.GetValue(projComponent);
            var assignedSound2 = type.GetField("hitSound2")?.GetValue(projComponent);
            var assignedAttackType = type.GetField("attackType")?.GetValue(projComponent);

            // ProjectileLayerFixer không cần thiết vì Projectile.cs đã handle player collision
            // var fixerType = System.Type.GetType("ProjectileLayerFixer");
            // if (fixerType != null && projectile.GetComponent(fixerType) == null)
            // {
            //     var fixer = projectile.AddComponent(fixerType);
            // }
        }
        else
        {
            Debug.LogWarning("Failed to add Projectile component via reflection");
        }
    }
}
