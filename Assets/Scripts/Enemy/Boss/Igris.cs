using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Igris : MonoBehaviour, IDamageable
{
    // ...existing code...

    // Overload giống Damageable
    public void TakeHit(float dmg, Vector2 hitDirection, Transform attacker = null, float knockbackForce = 8f, float hitPullStrength = 0f)
    {
        if (!IsAlive) return;
        currentHP -= Mathf.RoundToInt(dmg);
        currentHP = Mathf.Max(currentHP, 0); // Clamp HP về 0

        if (currentHP > 0)
        {
            // Hitstop cực ngắn - chỉ 1-2 frames
            float hitstopDuration = 0.03f; // Match với PlayerCombat
            StartCoroutine(WorldHitStopCoroutine(hitstopDuration));

            // Hit pull mạnh hơn - DISABLE cho boss để tránh player nhảy lung tung
            // Chỉ giữ lại camera shake và visual effects
            /*
            if (attacker != null)
            {
                var playerRb = attacker.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // Tính hướng CHỈ THEO CHIỀU NGANG từ player đến boss
                    float horizontalDirection = Mathf.Sign(transform.position.x - attacker.position.x);
                    Vector2 pullDirection = new Vector2(horizontalDirection, 0f); // CHỈ CHIỀU NGANG
                    
                    Debug.Log($"[Hit Pull] Boss pos: {transform.position}, Player pos: {attacker.position}");
                    Debug.Log($"[Hit Pull] Horizontal direction: {horizontalDirection}, Pull direction: {pullDirection}");
                    
                    float strongPull = hitPullStrength > 0f ? hitPullStrength * 2f : 8f; // Giảm pull strength
                    Debug.Log($"[Hit Pull] Pull strength: {strongPull}");
                    StartCoroutine(StrongHitPullCoroutine(playerRb, pullDirection * strongPull, 0.15f));
                }
            }
            */

            // Knockback với weight - boss nặng nên khó đẩy
            if (rb != null && hitDirection != Vector2.zero)
            {
                float bossWeight = 0.6f; // Boss nặng, giảm knockback
                float finalKnockback = knockbackForce * bossWeight;
                Vector2 knockback = new Vector2(hitDirection.x * finalKnockback,
                    Mathf.Abs(hitDirection.y) > 0.1f ? hitDirection.y * finalKnockback * 0.5f : rb.velocity.y);
                rb.velocity = knockback;

                // Boss recoil animation
                StartCoroutine(BossRecoilCoroutine(hitDirection));
            }

            // Camera shake + zoom punch như Hades
            ShakeCamera(0.6f, 0.3f);
            StartCoroutine(CameraZoomPunchCoroutine());

            // Boss hitstun với visual feedback
            StartCoroutine(BossHitStunCoroutine(0.3f));

            // Screen flash effect như Dead Cells
            StartCoroutine(ScreenFlashCoroutine());
        }
        else
        {
            currentHP = 0;
            Die();
        }
    }

    private IEnumerator HitStopAnimatorCoroutine(Animator anim, float duration)
    {
        float prevSpeed = anim.speed;
        anim.speed = 0f;
        yield return new WaitForSecondsRealtime(duration);
        anim.speed = prevSpeed;
    }

    // World hitstop - freeze toàn bộ thế giới như Hollow Knight
    private IEnumerator WorldHitStopCoroutine(float duration)
    {
        // Freeze time scale cho effect "thế giới dừng lại"
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Wait with unscaled time
        yield return new WaitForSecondsRealtime(duration);

        // Restore time
        Time.timeScale = originalTimeScale;
    }

    // Strong hit pull như Dead Cells - player bị "hút" vào boss (CHỈ THEO CHIỀU NGANG)
    private IEnumerator StrongHitPullCoroutine(Rigidbody2D playerRb, Vector2 pullForce, float duration)
    {
        Debug.Log($"[Hit Pull] Starting pull: {pullForce}, duration: {duration}");
        float elapsed = 0f;
        Vector2 originalVelocity = playerRb.velocity;
        Debug.Log($"[Hit Pull] Original velocity: {originalVelocity}");

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            // Curve mạnh ở đầu, yếu dần
            float pullCurve = Mathf.Lerp(1f, 0f, progress * progress);

            // Apply pull CHỈ THEO CHIỀU NGANG - giữ nguyên velocity.y
            Vector2 currentPull = pullForce * pullCurve;
            Vector2 newVelocity = new Vector2(currentPull.x, playerRb.velocity.y); // GIỮ NGUYÊN Y VELOCITY
            playerRb.velocity = newVelocity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[Hit Pull] Pull complete, restoring...");

        // Smooth restore CHỈ cho X velocity
        float restoreTime = 0.1f;
        elapsed = 0f;
        Vector2 startVel = playerRb.velocity;
        while (elapsed < restoreTime)
        {
            float progress = elapsed / restoreTime;
            float targetX = originalVelocity.x * 0.3f;
            float currentX = Mathf.Lerp(startVel.x, targetX, progress);
            playerRb.velocity = new Vector2(currentX, playerRb.velocity.y); // CHỈ THAY ĐỔI X
            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[Hit Pull] Restore complete. Final velocity: {playerRb.velocity}");
    }

    // Boss recoil - boss bị "giật" khi nhận damage
    private IEnumerator BossRecoilCoroutine(Vector2 hitDirection)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 recoilScale = originalScale;

        // Compress boss sprite theo hướng hit
        if (Mathf.Abs(hitDirection.x) > Mathf.Abs(hitDirection.y))
        {
            recoilScale.x *= 0.85f; // Squeeze horizontally
            recoilScale.y *= 1.15f; // Stretch vertically
        }
        else
        {
            recoilScale.x *= 1.15f;
            recoilScale.y *= 0.85f;
        }

        // Quick squash
        float squashTime = 0.08f;
        float elapsed = 0f;
        while (elapsed < squashTime)
        {
            float progress = elapsed / squashTime;
            transform.localScale = Vector3.Lerp(originalScale, recoilScale, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Bounce back with overshoot
        Vector3 overshootScale = originalScale * 1.05f;
        float bounceTime = 0.15f;
        elapsed = 0f;
        while (elapsed < bounceTime)
        {
            float progress = elapsed / bounceTime;
            float bounce = Mathf.Sin(progress * Mathf.PI);
            transform.localScale = Vector3.Lerp(recoilScale, overshootScale, bounce);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to normal
        float returnTime = 0.1f;
        elapsed = 0f;
        while (elapsed < returnTime)
        {
            float progress = elapsed / returnTime;
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    // Camera zoom punch như Hades
    private IEnumerator CameraZoomPunchCoroutine()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;

        float originalSize = mainCam.orthographicSize;
        float zoomInSize = originalSize * 0.95f; // Zoom in slightly

        // Quick zoom in
        float zoomTime = 0.08f;
        float elapsed = 0f;
        while (elapsed < zoomTime)
        {
            float progress = elapsed / zoomTime;
            mainCam.orthographicSize = Mathf.Lerp(originalSize, zoomInSize, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Zoom back out
        float zoomBackTime = 0.2f;
        elapsed = 0f;
        while (elapsed < zoomBackTime)
        {
            float progress = elapsed / zoomBackTime;
            mainCam.orthographicSize = Mathf.Lerp(zoomInSize, originalSize, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCam.orthographicSize = originalSize;
    }

    // Boss hitstun với visual feedback
    private IEnumerator BossHitStunCoroutine(float duration)
    {
        bool wasAttacking = isAttacking;
        isAttacking = true;

        // Visual feedback - flash boss red
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            float flashDuration = 0.1f;

            // Flash to white/red
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(flashDuration);

            // Fade back to normal
            float fadeTime = duration - flashDuration;
            float elapsed = 0f;
            while (elapsed < fadeTime)
            {
                float progress = elapsed / fadeTime;
                spriteRenderer.color = Color.Lerp(Color.white, originalColor, progress);
                elapsed += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }

        isAttacking = wasAttacking;
    }

    // Screen flash như Dead Cells
    private IEnumerator ScreenFlashCoroutine()
    {
        // Tìm Canvas để tạo flash overlay
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) yield break;

        // Tạo white flash overlay NHẸ HỢN
        GameObject flashObj = new GameObject("HitFlash");
        flashObj.transform.SetParent(canvas.transform, false);

        UnityEngine.UI.Image flashImage = flashObj.AddComponent<UnityEngine.UI.Image>();
        flashImage.color = new Color(1f, 1f, 1f, 0.08f); // GIẢM MẠNH alpha từ 0.3f → 0.08f

        // Full screen
        RectTransform rect = flashImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Fade out RẤT NHANH
        float flashDuration = 0.05f; // GIẢM từ 0.15f → 0.05f
        float elapsed = 0f;
        Color startColor = flashImage.color;
        Color endColor = new Color(1f, 1f, 1f, 0f);

        while (elapsed < flashDuration)
        {
            float progress = elapsed / flashDuration;
            flashImage.color = Color.Lerp(startColor, endColor, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(flashObj);
    }

    private IEnumerator HitPullCoroutine(Rigidbody2D targetRb, Vector2 pullForce, float duration)
    {
        float elapsed = 0f;
        Vector2 originalVelocity = targetRb.velocity;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float pullStrength = Mathf.Lerp(1f, 0f, progress); // Giảm dần pull
            targetRb.velocity = Vector2.Lerp(originalVelocity, pullForce, pullStrength * 0.3f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore original velocity gradually
        targetRb.velocity = Vector2.Lerp(targetRb.velocity, originalVelocity, 0.5f);
    }

    private IEnumerator BossStunCoroutine(float duration)
    {
        bool wasAttacking = isAttacking;
        isAttacking = true; // Prevent boss from acting
        Vector2 originalVelocity = rb.velocity;
        rb.velocity = Vector2.zero; // Stop boss movement

        yield return new WaitForSeconds(duration);

        isAttacking = wasAttacking; // Restore original state
    }

    private void Die()
    {
        currentState = BossState.Dead;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;

        // Ẩn BossHealthUI
        BossHealthUI[] bossUIs = FindObjectsOfType<BossHealthUI>();
        foreach (var ui in bossUIs)
        {
            ui.ForceUpdateHealth(); // cập nhật fill về 0
            ui.HideBossUI(); // ẩn UI
        }
        // Các xử lý death khác (drop item, effect...)
        TryDropItems();
        ShowBuffSelectionUI();
        // Destroy boss sau 2s
        Destroy(gameObject, 2f);
    }
    public bool IsAlive => currentHP > 0 && currentState != BossState.Dead;
    [Header("Item Drop Prefabs")]
    public GameObject healthPotionPrefab;
    public GameObject manaPotionPrefab;
    public GameObject coinPrefab;
    [Header("Item Drop Settings")]
    public int minDropCount = 1;
    public int maxDropCount = 3;
    public float dropRadius = 1.2f;
    public float itemGravityScale = 2.5f;

    /// <summary>
    /// Call this to drop items at Igris's position. Call from death logic.
    /// </summary>
    public void TryDropItems()
    {
        int dropCount = Random.Range(minDropCount, maxDropCount + 1);
        for (int i = 0; i < dropCount; i++)
        {
            GameObject prefab = GetRandomItemPrefab();
            if (prefab == null) continue;
            Vector2 offset = Random.insideUnitCircle * dropRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, offset.y, 0);
            GameObject item = Instantiate(prefab, spawnPos, Quaternion.identity);
            Rigidbody2D rb2d = item.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.gravityScale = itemGravityScale;
                rb2d.velocity = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(2.5f, 4.5f));
            }
            Debug.Log($"Igris dropped item: {item.name} at {spawnPos}");
        }
    }

    private GameObject GetRandomItemPrefab()
    {
        int r = Random.Range(0, 3);
        switch (r)
        {
            case 0: return healthPotionPrefab;
            case 1: return manaPotionPrefab;
            case 2: return coinPrefab;
            default: return coinPrefab;
        }
    }
    public enum BossState
    {
        Intro,
        Idle,
        WalkForward,
        WalkBack,
        AttackCrossSlash,
        AttackSlam,
        AttackSpin,
        AttackSlamAttack,
        AttackWideSlash,
        Stagger,
        EnrageTransition,
        Dead
    }

    [Header("References")]
    public Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private SpriteAnimationModifier spriteModifier;

    [Header("Audio & Effects")]
    public AudioSource audioSource;

    [Header("Sound Effects")]
    // CrossSlash Sounds (MUGEN State 2230)
    public AudioClip crossSlashChargeSound;          // Warning/preparation sound
    public AudioClip crossSlashDashSound1;          // MUGEN S0,30 at time=12 (first dash)  
    public AudioClip crossSlashDashSound2;          // MUGEN S0,31 at time=32 (second dash)
    public AudioClip crossSlashHitSound;            // Impact sound on hit

    // Other Attack Sounds
    public AudioClip wideSlashSound;

    // Slam Attack Sounds (MUGEN State 2235/2240/2245)
    public AudioClip slamJumpSound;                 // MUGEN S3,8 - jump preparation sound (!time & time=5)
    public AudioClip slamImpactSound1;              // MUGEN S0,4 - main impact sound (!time)
    public AudioClip slamImpactSound2;              // MUGEN S5,35 - secondary impact sound (!time)
    public AudioClip slamImpactSound3;              // MUGEN S1,37 - tertiary impact sound (!time)

    public AudioClip slamImpactSound;               // Legacy impact sound (keep for compatibility)
    public AudioClip footstepSound;

    [Header("Visual Effects")]
    // CrossSlash Effects (MUGEN State 2230) với tuỳ chỉnh offset, scale, duration
    [Header("CrossSlash Start FX")]
    public GameObject crossSlashStartFX;            // fx40060 & fx40065 - bụi dưới chân lúc bắt đầu lướt
    public Vector3 crossSlashStartFXOffset = new Vector3(0, -0.8f, 0);
    public float crossSlashStartFXScale = 1f;
    public float crossSlashStartFXDuration = 1f;

    [Header("CrossSlash Aura FX")]
    public GameObject crossSlashAuraFX;             // Aura charge effect  
    public Vector3 crossSlashAuraFXOffset = Vector3.zero;
    public float crossSlashAuraFXScale = 1f;
    public float crossSlashAuraFXDuration = 0.8f;

    [Header("CrossSlash Slash FX")]
    public GameObject crossSlashSlashFX;            // fx6047 - bụi tại chỗ khi lướt đi mất
    public Vector3 crossSlashSlashFXOffset = new Vector3(0, -0.5f, 0);
    public float crossSlashSlashFXScale = 1f;
    public float crossSlashSlashFXDuration = 1.2f;

    [Header("CrossSlash Speed FX")]
    public GameObject crossSlashHelperFX1;          // fx6226 - hiệu ứng lướt nhanh (gió bóng)
    public Vector3 crossSlashHelperFX1Offset = Vector3.zero;
    public float crossSlashHelperFX1Scale = 1f;
    public float crossSlashHelperFX1Duration = 0.5f;

    [Header("CrossSlash Stop FX")]
    public GameObject crossSlashHelperFX2;          // fx6047 variation - bụi khi dừng lại
    public Vector3 crossSlashHelperFX2Offset = new Vector3(0, -0.5f, 0);
    public float crossSlashHelperFX2Scale = 1f;
    public float crossSlashHelperFX2Duration = 1f;

    [Header("CrossSlash Finisher FX")]
    public GameObject crossSlashFinisherFX;         // Final dust effect
    public Vector3 crossSlashFinisherFXOffset = new Vector3(0, -0.3f, 0);
    public float crossSlashFinisherFXScale = 1f;
    public float crossSlashFinisherFXDuration = 1.5f;

    [Header("CrossSlash Other FX")]
    public GameObject crossSlashWarningFX;          // Warning indicator (!time trigger)
    public GameObject crossSlashTrailFX;            // Dash trail effect
    public GameObject crossSlashHitFX;              // Blood/impact effect on hit

    // Slam Attack FX (MUGEN State 2235/2240/2245) với tuỳ chỉnh offset, scale, duration
    [Header("Slam Preparation FX (State 2235)")]
    public GameObject slamPrepFX;                   // fx6047 - dust at animelem=2
    public Vector3 slamPrepFXOffset = new Vector3(0, 0.3f, 0);
    public float slamPrepFXScale = 0.6f;
    public float slamPrepFXDuration = 1f; // Giảm từ 2f xuống 1f

    [Header("Slam Jump FX (State 2240)")]
    public GameObject slamJumpFX;                   // Jump preparation effect
    public Vector3 slamJumpFXOffset = Vector3.zero;
    public float slamJumpFXScale = 1f;
    public float slamJumpFXDuration = 0.5f; // Giảm từ 1f xuống 0.5f

    [Header("Slam Impact FX (State 2245)")]
    public GameObject slamImpactShadowFX;           // anim=1360 - shadow on ground
    public Vector3 slamImpactShadowFXOffset = new Vector3(4f, 0, 0);
    public float slamImpactShadowFXScale = 0.2f;
    public float slamImpactShadowFXDuration = 0.5f; // Giảm từ 1f xuống 0.5f - mất nhanh hơn

    public GameObject slamImpactDebrisFX;           // Helper 912 - debris particles
    public Vector3 slamImpactDebrisFXOffset = new Vector3(3f, -1f, 0);
    public float slamImpactDebrisFXScale = 1.5f;
    public float slamImpactDebrisFXDuration = 1f; // Giảm từ 2f xuống 1f

    public GameObject slamImpactBlastFX;            // anim=6211 - main blast effect
    public Vector3 slamImpactBlastFXOffset = new Vector3(3.6f, 0.1f, 0);
    public float slamImpactBlastFXScale = 0.4f;
    public float slamImpactBlastFXDuration = 0.8f; // Giảm từ -1f xuống 0.8f

    public GameObject slamImpactBeamFX;             // anim=6225 - vertical beam
    public Vector3 slamImpactBeamFXOffset = new Vector3(4.1f, -9f, 0);
    public float slamImpactBeamFXScale = 0.65f;
    public float slamImpactBeamFXDuration = 0.6f; // Giảm từ -1f xuống 0.6f

    public GameObject slamImpactFlashFX;            // anim=6055 - bright flash
    public Vector3 slamImpactFlashFXOffset = new Vector3(4f, 0, 0);
    public float slamImpactFlashFXScale = 1.5f;
    public float slamImpactFlashFXDuration = 0.3f; // Giảm từ -1f xuống 0.3f (flash should be quick)

    public GameObject slamImpactGroundFX;           // anim=6050 - ground crack
    public Vector3 slamImpactGroundFXOffset = new Vector3(4f, 0, 0);
    public float slamImpactGroundFXScale = 1f;
    public float slamImpactGroundFXDuration = 2f; // Giảm từ -1f xuống 2f (ground crack lâu hơn)

    public GameObject slamImpactSmokeFX;            // anim=6265 - smoke cloud
    public Vector3 slamImpactSmokeFXOffset = new Vector3(4f, 0, 0);
    public float slamImpactSmokeFXScale = 0.7f;
    public float slamImpactSmokeFXDuration = 1.5f; // Giảm từ -1f xuống 1.5f

    public GameObject slamImpactBlastMirrorFX;      // anim=6211 facing=-1 - mirrored blast
    public Vector3 slamImpactBlastMirrorFXOffset = new Vector3(4.3f, 0.1f, 0);
    public float slamImpactBlastMirrorFXScale = 0.4f;
    public float slamImpactBlastMirrorFXDuration = 0.8f; // Giảm từ -1f xuống 0.8f

    [Header("WideSlash/Spin FX & Sounds (MUGEN States 2250-2255)")]
    public AudioClip wideSlashLandingSound;             // MUGEN S1,38 - landing impact sound

    // Spin Phase FX (State 2250)
    public GameObject wideSlashSpinFX1;                 // anim=6207 - rotating effect 1
    public Vector3 wideSlashSpinFX1Offset = new Vector3(0, -2f, 0); // pos=0,-20 scaled down
    public float wideSlashSpinFX1Scale = 0.61f;
    public float wideSlashSpinFX1Duration = -1f; // bindtime=-1 removetime=-1 (permanent until removed)

    public GameObject wideSlashSpinFX2;                 // anim=6224 - rotating effect 2  
    public Vector3 wideSlashSpinFX2Offset = new Vector3(0, -2f, 0); // pos=0,-20 scaled down
    public float wideSlashSpinFX2Scale = 0.61f;
    public float wideSlashSpinFX2Duration = -1f; // bindtime=-1 removetime=-1 (permanent until removed)

    // Landing Phase FX (State 2255)
    public GameObject wideSlashLandingFX1;              // anim=6800 - main landing effect
    public Vector3 wideSlashLandingFX1Offset = new Vector3(1f, 0, 0); // pos=10,0 scaled down
    public float wideSlashLandingFX1Scale = 0.4f;
    public float wideSlashLandingFX1Duration = -2f; // removetime=-2 (very short)

    public GameObject wideSlashLandingFX2;              // anim=40060 - dual landing effect left
    public Vector3 wideSlashLandingFX2Offset = new Vector3(0.5f, 0, 0); // pos=5,0 scaled down  
    public float wideSlashLandingFX2Scale = 0.7f;
    public float wideSlashLandingFX2Duration = -2f; // removetime=-2 (very short)

    public GameObject wideSlashLandingFX3;              // anim=40060 facing=-1 - dual landing effect right
    public Vector3 wideSlashLandingFX3Offset = new Vector3(1.5f, 0, 0); // pos=15,0 scaled down
    public float wideSlashLandingFX3Scale = 0.7f;
    public float wideSlashLandingFX3Duration = -2f; // removetime=-2 (very short)

    // Other Attack Effects
    public GameObject wideSlashFX;
    public GameObject slamImpactFX;
    public GameObject jumpDustFX;

    [Header("Screen Effects")]
    public float cameraShakeIntensity = 0.3f;
    public float cameraShakeDuration = 0.2f;

    [Header("Settings")]
    public float walkSpeed = 3f;
    public float walkBackSpeed = 2f;
    public float dashSpeed = 6f;
    public float decisionCooldown = 0.6f;  // Giảm thời gian chờ xuống
    public float enrageDecisionCooldown = 0.3f;  // Enrage sẽ còn nhanh hơn nữa

    [Header("Attack Damage Values (MUGEN HitDef)")]
    public int crossSlashDamage = 15;               // MUGEN: Damage = 15,3
    public int crossSlashChipDamage = 3;            // MUGEN chip damage
    public int wideSlashSpinDamage = 15;            // MUGEN: Damage = 15,3 (per hit during spin)  
    public int wideSlashSpinChipDamage = 3;
    public int wideSlashLandingDamage = 15;         // MUGEN: Damage = 15,5 (landing hit)
    public int wideSlashLandingChipDamage = 5;
    public int slamDamage = 20;                     // Custom slam damage
    public int slamChipDamage = 4;

    [Header("Attack Ranges")]
    public float minAttackRange = 2.55f;
    public float closeRange = 3f;
    public float midRange = 5f;
    public float maxRange = 8f;

    [Header("Boss Stats")]
    public int maxHP = 300;
    private int currentHP;
    private bool enraged = false;

    private BossState currentState = BossState.Intro;
    private float decisionTimer;
    private float timeInState;
    private float footstepTimer = 0f;
    private const float FOOTSTEP_INTERVAL = 0.6f; // Footstep every 0.6 seconds

    // movement handled via desiredVelocity -> applied in FixedUpdate
    private Vector2 desiredVelocity = Vector2.zero;

    // --- Mugen logic mapping ---
    private float attackTimer = 0f;
    private int crossSlashHits = 0;
    private bool isSpinning = false;
    private bool isSlamming = false;
    private bool isWideSlashing = false;

    // Slam combo - simplified logic with fixed target position
    private bool slamComboInProgress = false;
    private int slamPhase = 0; // 0=not started, 1=jumping, 2=spinning, 3=slamming
    private bool slamImpactTriggered = false; // Prevent duplicate impact calls
    private Vector3 slamStartPosition;
    private Vector3 slamTargetPosition; // Fixed target position - không thay đổi
    private float slamSpinTimer = 0f; // Timer để force transition nếu stuck
    private float lastSlamTime = 0f;
    private const float SLAM_COOLDOWN = 3f;

    // Cross Slash variables
    private float crossSlashTimer = 0f;
    private bool crossSlashHasHit = false;
    private bool crossSlashTeleported = false;
    private bool crossSlashDash2Started = false; // MUGEN-style second dash tracking

    // Hit Detection & Damage System
    private bool canDamagePlayer = false;
    private string currentAttackType = "";
    private float lastHitTime = 0f;
    private const float HIT_COOLDOWN = 0.2f; // Prevent spam damage

    // track coroutine (if used later)
    private Coroutine stateRoutine = null;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteModifier = GetComponent<SpriteAnimationModifier>();
        currentHP = maxHP;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        // UI sẽ được tạo khi EnterIdle được gọi (boss thực sự active)
        // nhỏ delay trước khi bắt đầu
        Invoke(nameof(EnterIdle), 2f);
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null) return;

        timeInState += Time.deltaTime;

        // TUYỆT ĐỐI không face player khi đang trong slam combo
        bool inSlamCombo = slamComboInProgress || IsAttackState(currentState);
        if (!inSlamCombo && currentState != BossState.EnrageTransition && currentState != BossState.Stagger)
        {
            FacePlayer(); // chỉ face khi không tấn công để tránh "lắc" trong attack
        }

        // Hit detection for damage dealing
        CheckPlayerHit();

        // FORCE reset scale nếu đang trong SlamAttack phase
        if (currentState == BossState.AttackSlamAttack)
        {
            transform.localScale = Vector3.one;
        }

        switch (currentState)
        {
            case BossState.Idle:
                HandleIdle();
                break;
            case BossState.WalkForward:
                WalkTowardsPlayer();
                break;
            case BossState.WalkBack:
                WalkBackFromPlayer();
                break;
            case BossState.AttackCrossSlash:
                HandleCrossSlash();
                break;
            case BossState.AttackSlam:
                HandleSlam();
                break;
            case BossState.AttackSpin:
                HandleSpin();
                break;
            case BossState.AttackSlamAttack:
                HandleSlamAttack();
                break;
            case BossState.AttackWideSlash:
                HandleWideSlash();
                break;
        }

        UpdateAnimatorParameters();
    }

    void FixedUpdate()
    {
        // apply movement smoothly via Rigidbody only when not being driven by attacks
        if (!IsAttackState(currentState) && !isAttacking)
        {
            rb.velocity = new Vector2(desiredVelocity.x, rb.velocity.y);
        }
        // Nếu đang tấn công thì các handler (DoDashSlash, DoWideSlash, HandleSpin...) tự set rb.velocity
    }


    void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        float dist = Vector2.Distance(transform.position, player.position);

        animator.SetFloat("HP", currentHP);
        animator.SetFloat("DistanceToPlayer", dist);
        animator.SetBool("Enraged", enraged);
        animator.SetBool("IsDead", currentState == BossState.Dead);
        animator.SetBool("IsStaggered", currentState == BossState.Stagger);
        animator.SetBool("IsAttacking", IsAttackState(currentState));

        // ensure walking flags reflect actual state
        bool walkingForward = currentState == BossState.WalkForward;
        bool walkingBack = currentState == BossState.WalkBack;
        animator.SetBool("IsWalkingForward", walkingForward);
        animator.SetBool("IsWalkingBack", walkingBack);
    }

    bool IsAttackState(BossState state)
    {
        return state == BossState.AttackCrossSlash || state == BossState.AttackSlam || state == BossState.AttackSpin || state == BossState.AttackSlamAttack || state == BossState.AttackWideSlash;
    }

    void HandleIdle()
    {
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0f)
        {
            DecideNextAction();
            decisionTimer = enraged ? enrageDecisionCooldown : decisionCooldown;
        }
    }

    void DecideNextAction()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        float p2x = player.position.x - transform.position.x;

        // Face player first if needed
        if (Mathf.Abs(p2x) > 0.5f)
        {
            bool shouldFaceLeft = p2x < 0;
            if (shouldFaceLeft != isFacingLeft)
            {
                isFacingLeft = shouldFaceLeft;
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = isFacingLeft;
                }
                decisionTimer = 0.15f;
            }
        }

        // Check slam cooldown
        bool slamOnCooldown = Time.time - lastSlamTime < SLAM_COOLDOWN;

        // Xử lý theo khoảng cách với nhiều pattern khác nhau
        if (dist < minAttackRange)  // Quá gần
        {
            float r = Random.value;
            if (r < 0.4f)  // 40% lùi lại
            {
                ChangeState(BossState.WalkBack);
            }
            else if (r < 0.7f)  // 30% CrossSlash
            {
                ChangeState(BossState.AttackCrossSlash);
            }
            else if (!slamOnCooldown)  // 30% Slam Combo (nếu không cooldown)
            {
                ChangeState(BossState.AttackSlam);
            }
            else  // Fallback to CrossSlash if slam on cooldown
            {
                ChangeState(BossState.AttackCrossSlash);
            }
            return;
        }
        else if (dist <= closeRange)  // Tầm gần - ưu tiên CrossSlash
        {
            float r = Random.value;
            if (r < 0.7f)  // 70% CrossSlash - đòn chính cho tầm gần
            {
                ChangeState(BossState.AttackCrossSlash);
            }
            else if (r < 0.85f)  // 15% lùi lại để tạo khoảng cách
            {
                ChangeState(BossState.WalkBack);
            }
            else  // 15% WideSlash backup
            {
                ChangeState(BossState.AttackWideSlash);
            }
        }
        else if (dist <= midRange)  // Tầm trung - ưu tiên WideSlash
        {
            float r = Random.value;
            if (r < 0.5f)  // 50% WideSlash - đòn chính cho tầm trung
            {
                ChangeState(BossState.AttackWideSlash);
            }
            else if (r < 0.7f)  // 20% tiến vào dùng CrossSlash
            {
                ChangeState(BossState.WalkForward);
            }
            else if (r < 0.9f && !slamOnCooldown)  // 20% Slam (nếu không cooldown)
            {
                ChangeState(BossState.AttackSlam);
            }
            else  // 10% lùi để tạo khoảng cách tốt hơn
            {
                ChangeState(BossState.WalkBack);
            }
        }
        else  // Tầm xa - ưu tiên Slam
        {
            float r = Random.value;
            if (r < 0.6f && !slamOnCooldown)  // 60% Slam - đòn chính cho tầm xa
            {
                ChangeState(BossState.AttackSlam);
            }
            else if (r < 0.8f)  // 20% tiến vào
            {
                ChangeState(BossState.WalkForward);
            }
            else if (!slamOnCooldown)  // 15% Slam (nhảy vào tầm xa, nếu không cooldown)
            {
                ChangeState(BossState.AttackSlam);
            }
            else  // Fallback to WalkForward if slam on cooldown
            {
                ChangeState(BossState.WalkForward);
            }
        }
    }

    void WalkTowardsPlayer()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= closeRange)
        {
            desiredVelocity = Vector2.zero;
            footstepTimer = 0f; // Reset footstep timer when stopping
            ChangeState(BossState.Idle);
            // đặt decision timer ngắn để re-evaluate
            decisionTimer = 0.2f;
            return;
        }

        Vector2 dir = (player.position - transform.position).normalized;
        desiredVelocity = new Vector2(dir.x * walkSpeed, rb.velocity.y);

        // Play footstep sound periodically while moving
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= FOOTSTEP_INTERVAL)
        {
            PlayFootstepFX();
            footstepTimer = 0f;
        }

        // có cơ hội tấn công khi tiến tới (interrupt allowed if not currently attacking)
        if (dist <= midRange && Random.value < 0.08f)
        {
            desiredVelocity = Vector2.zero;
            footstepTimer = 0f; // Reset footstep timer when entering attack
            ChangeState(BossState.AttackCrossSlash);  // Use CrossSlash instead of WideSlash for interrupts
            return;
        }
    }

    void WalkBackFromPlayer()
    {
        Vector2 dir = (transform.position - player.position).normalized;
        desiredVelocity = new Vector2(dir.x * walkBackSpeed, rb.velocity.y);

        // Play footstep sound periodically while moving
        footstepTimer += Time.deltaTime;
        if (footstepTimer >= FOOTSTEP_INTERVAL * 1.2f) // Slower footsteps when backing away
        {
            PlayFootstepFX();
            footstepTimer = 0f;
        }

        // Lùi xa hơn và thông minh hơn - đến tầm trung hoặc xa
        float targetDistance = Random.Range(midRange * 0.9f, maxRange * 0.7f); // Lùi đến tầm trung-xa
        if (timeInState >= Random.Range(1.2f, 2.4f) || Vector2.Distance(transform.position, player.position) >= targetDistance)
        {
            desiredVelocity = Vector2.zero;
            footstepTimer = 0f; // Reset footstep timer when stopping
            ChangeState(BossState.Idle);
        }
    }

    private bool isFacingLeft = false;
    private bool isAttacking = false;

    void PreserveDirectionForAttack()
    {
        // Đảm bảo face player trước khi lock direction
        if (player != null)
        {
            bool shouldFaceLeft = player.position.x < transform.position.x;
            isFacingLeft = shouldFaceLeft;
        }

        isAttacking = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isFacingLeft;
        }
    }

    void Turn()
    {
        // Không cho phép turn khi đang tấn công
        if (isAttacking) return;

        isFacingLeft = !isFacingLeft;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = isFacingLeft;
    }

    void FacePlayer()
    {
        if (player == null || isAttacking) return;

        // Nếu player ở bên trái, flip sprite (vì sprite gốc face right)
        bool shouldFaceLeft = player.position.x < transform.position.x;
        if (shouldFaceLeft != isFacingLeft)
        {
            isFacingLeft = shouldFaceLeft;
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = isFacingLeft;
        }
    }

    void ChangeState(BossState newState)
    {
        // Nếu đang dead thì không đổi
        if (currentState == BossState.Dead) return;

        // Nếu đang attack và muốn chuyển sang attack khác thì block (giữ nguyên) — tránh xung đột
        // NHƯNG cho phép slam combo: AttackSlam -> AttackSpin -> AttackSlamAttack
        if (IsAttackState(currentState) && IsAttackState(newState))
        {
            bool allowedTransition =
                (currentState == BossState.AttackSlam && newState == BossState.AttackSpin) ||
                (currentState == BossState.AttackSpin && newState == BossState.AttackSlamAttack);

            if (!allowedTransition)
            {
                return;
            }
            else
            {
            }
        }

        // stop any running state coroutine
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        // reset variables
        desiredVelocity = Vector2.zero;
        attackTimer = 0f;
        crossSlashHits = 0;
        isSpinning = false;
        isSlamming = false;
        isWideSlashing = false;

        // Reset attack state - nhưng không reset nếu đang chuyển giữa attack states
        if (!IsAttackState(newState))
        {
            isAttacking = false;
        }

        // Reset modifiers but maintain facing direction
        spriteModifier?.ResetModifiers();
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = isFacingLeft;
        }

        // animator resets
        animator.ResetTrigger("CrossSlash");
        animator.ResetTrigger("Slam");
        animator.ResetTrigger("Spin");
        animator.ResetTrigger("SlamAttack");
        animator.ResetTrigger("WideSlash");

        BossState oldState = currentState;
        currentState = newState;
        timeInState = 0f;

        // triggers / plays
        switch (newState)
        {
            case BossState.Idle:
                animator.Play("Idle");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoIdleBreathing());
                break;
            case BossState.WalkForward:
                animator.Play("WalkForward");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoWalkModifier());
                break;
            case BossState.WalkBack:
                animator.Play("WalkBack");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoWalkModifier());
                break;
            case BossState.AttackCrossSlash:
                PreserveDirectionForAttack();
                animator.SetTrigger("CrossSlash");
                animator.Play("CrossSlash");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoCrossSlashModifier());
                // reset attack control
                attackTimer = 0f;
                crossSlashHits = 0;
                break;
            case BossState.AttackSlam:
                PreserveDirectionForAttack();
                animator.SetTrigger("Slam");
                animator.Play("Slam");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoSlamModifier());
                isSlamming = false;
                break;
            case BossState.AttackSpin:
                // Direction đã được preserve, không gọi lại
                animator.SetTrigger("Spin");
                animator.Play("Spin");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoSpinModifier());
                isSpinning = false;
                break;
            case BossState.AttackSlamAttack:
                // Direction đã được preserved, không gọi lại
                // FORCE reset scale và STOP mọi coroutine trước khi start SlamAttack
                if (spriteModifier != null)
                {
                    spriteModifier.StopAllCoroutines(); // Stop mọi animation modifier
                    spriteModifier.ResetModifiers();
                    transform.localScale = Vector3.one; // Force reset scale
                }

                animator.SetTrigger("SlamAttack");
                animator.Play("SlamAttack");
                // KHÔNG start coroutine để tránh mọi can thiệp
                // if (spriteModifier != null) StartCoroutine(spriteModifier.DoSlamAttackModifier());
                break;
            case BossState.AttackWideSlash:
                PreserveDirectionForAttack();
                attackTimer = 0f; // Reset attack timer for WideSlash
                animator.SetTrigger("WideSlash");
                animator.Play("WideSlash");
                if (spriteModifier != null) StartCoroutine(spriteModifier.DoWideSlashModifier());
                // DON'T reset isWideSlashing here - let HandleWideSlash manage it
                break;
            case BossState.Stagger:
                animator.SetTrigger("Stagger");
                animator.Play("Stagger");
                break;
            case BossState.EnrageTransition:
                animator.SetTrigger("Enrage");
                animator.Play("Enrage");
                break;
            case BossState.Dead:
                animator.SetTrigger("Death");
                animator.Play("Death");
                desiredVelocity = Vector2.zero;
                break;
        }
    }

    void EnterIdle()
    {
        ChangeState(BossState.Idle);
        if (spriteModifier != null)
        {
            StopSpriteModifiers();
            StartCoroutine(spriteModifier.DoIdleBreathing());
        }

        // Create Boss Health UI when boss becomes active
        CreateBossHealthUI();
    }

    void StopSpriteModifiers()
    {
        if (spriteModifier != null)
        {
            spriteModifier.ResetModifiers();
        }
    }

    // --- VFX & Combat Effects ---
    private void SpawnJumpEffect()
    {
        // TODO: Spawn dust particles và dash effect
    }

    private void SpawnSpinStartEffect()
    {
        // TODO: Spawn wind-up effect và energy particles
    }

    private void SpawnSlamImpactEffect()
    {
        // TODO: Spawn ground crack, shockwave và dust explosion
    }

    private void ResetSlamCombo()
    {
        isSlamming = false;
        isSpinning = false;
        slamComboInProgress = false;
        slamPhase = 0;
        animator.SetBool("IsSlamming", false);
        animator.SetBool("IsSpinning", false);
        rb.gravityScale = 1f;
    }

    // --- Attack handlers with proper effects ---
    public void DoDashSlash()
    {
        float currentSpeed = dashSpeed;

        // Tăng speed theo số hit trong combo
        if (currentState == BossState.AttackCrossSlash)
        {
            currentSpeed *= (1f + crossSlashHits * 0.3f);
        }

        // Dùng isFacingLeft làm nguồn hướng - KHÔNG DÙNG transform.localScale.x
        float moveDirection = isFacingLeft ? -1f : 1f;
        rb.velocity = new Vector2(moveDirection * currentSpeed, rb.velocity.y);

        // TODO: Spawn dash trail effect
        // TODO: Spawn slash hitbox
        // TODO: Camera shake effect
        // TODO: Dash sound effect
    }


    public void DoSlamShockwave()
    {
        // TODO: Spawn shockwave prefab expanding outward
        // TODO: Apply camera shake
        // TODO: Spawn ground crack effect
        // TODO: Play impact sound
    }

    public void DoSpinAttack()
    {
        // TODO: Spawn spinning slash effect
        // TODO: Create spinning hitbox
        // TODO: Add light trails
        // TODO: Play spin sound
    }

    public void DoWideSlash()
    {
        // Movement - sử dụng isFacingLeft để xác định hướng
        float moveDirection = isFacingLeft ? -1f : 1f;
        rb.velocity = new Vector2(moveDirection * dashSpeed * 0.7f, rb.velocity.y);

        // TODO: Spawn wide slash effect
        // TODO: Create wide hitbox
        // TODO: Screen shake effect
        // TODO: Slash sound effect
    }

    // --- Handlers that run per-frame while in that state ---
    void HandleCrossSlash()
    {
        // Cross Slash với warning system để player có thể né + MUGEN FX/Sound (LOGIC CŨ)
        crossSlashTimer += Time.deltaTime;

        if (crossSlashTimer <= 0.6f) // Phase 1: Preparation & Warning (0.6s)
        {
            // STOP movement để chuẩn bị
            rb.velocity = Vector2.zero;

            // Show warning FX at start - CHỈ SOUND THÔI, KHÔNG CÓ FX NÀO
            if (crossSlashTimer <= 0.1f && !crossSlashHasHit)
            {
                // CHỈ play sound trong phase chuẩn bị, KHÔNG có FX nào cả
                PlaySound(crossSlashChargeSound, 1f);
                // KHÔNG spawn bất kỳ FX nào ở phase 1

                crossSlashHasHit = true;
            }

            // Blink warning every 0.1s
            if (Mathf.Repeat(crossSlashTimer, 0.1f) < 0.05f)
            {
            }
        }

        else if (crossSlashTimer <= 0.8f) // Phase 2: Quick dash towards player (0.2s)
        {
            // Play dash FX at start of dash với MUGEN sounds
            if (crossSlashTimer <= 0.65f && crossSlashHasHit)
            {
                // Lưu vị trí ban đầu KHI BẮT ĐẦU LƯỚT cho fx6047
                Vector3 startDashPosition = transform.position;

                // MUGEN Sound S0,30 + dash effects
                PlaySound(crossSlashDashSound1, 1f);

                // fx40065 - aura effect KHI BẮT ĐẦU LƯỚT (chuyển từ phase 1)
                if (crossSlashAuraFX != null)
                {
                    Vector3 auraOffset = crossSlashAuraFXOffset;
                    if (isFacingLeft)
                    {
                        auraOffset.x = -auraOffset.x; // Mirror X offset
                    }
                    Vector3 auraPos = startDashPosition + auraOffset;

                    GameObject aura = Instantiate(crossSlashAuraFX, auraPos, Quaternion.identity);
                    if (crossSlashAuraFXScale != 1f)
                    {
                        // Mirror FX sprite khi boss facing left
                        float finalScaleX = isFacingLeft ? -crossSlashAuraFXScale : crossSlashAuraFXScale;
                        aura.transform.localScale = new Vector3(finalScaleX, crossSlashAuraFXScale, 1f);
                    }
                    else if (isFacingLeft)
                    {
                        // Scale = 1 nhưng vẫn cần mirror
                        aura.transform.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    // Add AutoDestroyOnAnimationEnd component
                    AutoDestroyOnAnimationEnd autoDestroy = aura.GetComponent<AutoDestroyOnAnimationEnd>();
                    if (autoDestroy == null)
                    {
                        autoDestroy = aura.AddComponent<AutoDestroyOnAnimationEnd>();
                    }
                    autoDestroy.destroyOnFirstLoop = true;
                    autoDestroy.startupGrace = 0.05f; // Faster check
                    autoDestroy.fallbackLifetime = crossSlashAuraFXDuration > 0 ? crossSlashAuraFXDuration : 2f;
                }

                // fx40060 - bụi dưới chân KHI BẮT ĐẦU LƯỚT
                if (crossSlashStartFX != null)
                {
                    Vector3 dustOffset = crossSlashStartFXOffset;
                    if (isFacingLeft)
                    {
                        dustOffset.x = -dustOffset.x; // Mirror X offset
                    }
                    Vector3 dustPos = startDashPosition + dustOffset;
                    GameObject dust = Instantiate(crossSlashStartFX, dustPos, Quaternion.identity);
                    if (crossSlashStartFXScale != 1f)
                    {
                        // Mirror FX sprite khi boss facing left
                        float finalScaleX = isFacingLeft ? -crossSlashStartFXScale : crossSlashStartFXScale;
                        dust.transform.localScale = new Vector3(finalScaleX, crossSlashStartFXScale, 1f);
                    }
                    else if (isFacingLeft)
                    {
                        // Scale = 1 nhưng vẫn cần mirror
                        dust.transform.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    // Add AutoDestroyOnAnimationEnd component
                    AutoDestroyOnAnimationEnd autoDestroy = dust.GetComponent<AutoDestroyOnAnimationEnd>();
                    if (autoDestroy == null)
                    {
                        autoDestroy = dust.AddComponent<AutoDestroyOnAnimationEnd>();
                    }
                    autoDestroy.destroyOnFirstLoop = true;
                    autoDestroy.startupGrace = 0.1f;
                    autoDestroy.fallbackLifetime = crossSlashStartFXDuration > 0 ? crossSlashStartFXDuration : 5f;
                }

                // fx6047 - bụi tại chỗ KHI BẮT ĐẦU LƯỚT (ở vị trí ban đầu, không di chuyển theo boss)
                if (crossSlashSlashFX != null)
                {
                    Vector3 stayDustOffset = crossSlashSlashFXOffset;
                    if (isFacingLeft)
                    {
                        stayDustOffset.x = -stayDustOffset.x; // Mirror X offset
                    }
                    Vector3 stayDustPos = startDashPosition + stayDustOffset;
                    GameObject stayDust = Instantiate(crossSlashSlashFX, stayDustPos, Quaternion.identity);
                    if (crossSlashSlashFXScale != 1f)
                    {
                        // Mirror FX sprite khi boss facing left
                        float finalScaleX = isFacingLeft ? -crossSlashSlashFXScale : crossSlashSlashFXScale;
                        stayDust.transform.localScale = new Vector3(finalScaleX, crossSlashSlashFXScale, 1f);
                    }
                    else if (isFacingLeft)
                    {
                        // Scale = 1 nhưng vẫn cần mirror
                        stayDust.transform.localScale = new Vector3(-1f, 1f, 1f);
                    }

                    // Add AutoDestroyOnAnimationEnd component
                    AutoDestroyOnAnimationEnd autoDestroy = stayDust.GetComponent<AutoDestroyOnAnimationEnd>();
                    if (autoDestroy == null)
                    {
                        autoDestroy = stayDust.AddComponent<AutoDestroyOnAnimationEnd>();
                    }
                    autoDestroy.destroyOnFirstLoop = true;
                    autoDestroy.startupGrace = 0.1f;
                    autoDestroy.fallbackLifetime = crossSlashSlashFXDuration > 0 ? crossSlashSlashFXDuration : 5f;

                }                // fx6226 - hiệu ứng lướt nhanh (gió bóng) - theo boss
                SpawnCustomEffect(crossSlashHelperFX1, crossSlashHelperFX1Offset, crossSlashHelperFX1Scale, crossSlashHelperFX1Duration);

                crossSlashHasHit = false;
            }

            // Dash nhanh towards player (giữ nguyên logic cũ)
            float dashSpeedValue = isFacingLeft ? -dashSpeed * 5f : dashSpeed * 5f;
            rb.velocity = new Vector2(dashSpeedValue, 0);
            rb.drag = 0.5f; // Một chút drag để không quá jerky

            // Check hit với player
            if (!crossSlashHasHit)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= 2f)
                {
                    PlayCrossSlashHitFX(player.position);
                    // MUGEN EnvShake effect
                    ShakeCamera(0.8f, 8f / 60f);
                    // MUGEN Helper effects when hitting
                    PlayCrossSlashHelperEffect1();
                    crossSlashHasHit = true;
                    // TODO: Deal damage + effects
                }
            }
        }
        else if (crossSlashTimer <= 0.9f) // Phase 3: Smooth stop (0.1s)
        {
            // Smooth deceleration để không bị giật lùi
            rb.velocity = new Vector2(rb.velocity.x * 0.5f, 0);
            rb.drag = 3f;

            // MUGEN Helper effect 2 during deceleration
            if (crossSlashTimer >= 0.85f && crossSlashTimer <= 0.86f)
            {
                PlayCrossSlashHelperEffect2();
            }
        }
        else // Phase 4: Complete
        {
            // MUGEN Finisher effect before complete
            if (crossSlashTimer >= 0.9f && crossSlashTimer <= 0.91f)
            {
                PlayCrossSlashFinisherEffect();
            }

            // Clean finish
            rb.velocity = Vector2.zero;
            rb.drag = 5f;
            crossSlashTimer = 0f;
            crossSlashHasHit = false;
            crossSlashTeleported = false;
            isAttacking = false;
            ChangeState(BossState.Idle);
        }
    }


    private IEnumerator DashCoroutine(float duration, float speed)
    {
        float elapsed = 0f;
        float direction = isFacingLeft ? -1f : 1f;

        while (elapsed < duration)
        {
            rb.velocity = new Vector2(direction * speed * (1 - elapsed / duration), rb.velocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.velocity = new Vector2(0, rb.velocity.y);
    }


    void HandleSlam()
    {
        if (!slamComboInProgress)
        {
            slamComboInProgress = true;
            slamPhase = 1;
            slamImpactTriggered = false; // Reset impact flag for new combo
            slamStartPosition = transform.position;

            // MUGEN State 2235 - Slam Preparation FX (fx6047 at animelem=2)
            PlaySlamPreparationEffects();

            // LOCK target position ngay từ đầu - không thay đổi nữa
            slamTargetPosition = player.position;

            // Face towards target and lock direction
            bool shouldFaceLeft = slamTargetPosition.x < transform.position.x;
            isFacingLeft = shouldFaceLeft;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = isFacingLeft;
            }

            // SET isAttacking để block FacePlayer()
            isAttacking = true;

            // Start MUCH higher jump để đảm bảo có thời gian cho spin phase
            animator.SetTrigger("Slam");
            rb.velocity = new Vector2(0, 22f); // Tăng từ 18f lên 22f
            SpawnJumpEffect();
        }

        // Phase 1: At peak, start approach dive - CHỈ handle bằng physics, animation event sẽ force transition
        if (slamPhase == 1 && rb.velocity.y <= 0)
        {
            // KHÔNG force transition ở đây nữa - để animation event handle
        }
    }

    void HandleSpin()
    {
        if (slamPhase != 2) return;

        slamSpinTimer += Time.deltaTime;

        // Bay về FIXED target position - không theo player nữa (giữ nguyên logic cũ)
        Vector3 currentPos = transform.position;
        Vector3 directionToTarget = (slamTargetPosition - currentPos).normalized;

        // TĂNG MẠNH tốc độ bay để lao nhanh đến target
        float horizontalSpeed = directionToTarget.x * dashSpeed * 2.5f; // Tăng từ 1.2f lên 2.5f

        // Giữ tốc độ ngang ổn định về phía target cố định
        rb.velocity = new Vector2(horizontalSpeed, rb.velocity.y);

        // MUGEN-style continuous hit detection (every 20 frames)
        if (Mathf.Repeat(slamSpinTimer * 60f, 20f) < 1f) // Convert to Unity timing
        {
            float playerDist = Vector2.Distance(transform.position, player.position);
            if (playerDist <= 3f)
            {
                // TODO: Deal spin damage
            }
        }

        // Check khoảng cách đến target position TRƯỚC KHI check ground
        float distToTarget = Vector3.Distance(transform.position, slamTargetPosition);
        float currentHeight = transform.position.y;
        float groundLevel = slamStartPosition.y;

        // PRIORITY 1: Đảm bảo đến gần target position trước (trong 2f)
        // PRIORITY 2: Hoặc timeout sau 1.5s để tránh stuck
        bool nearTarget = distToTarget <= 2.5f; // Gần target position
        bool timeOut = slamSpinTimer >= 1.5f; // Timeout safety

        if (nearTarget || timeOut)
        {
            slamPhase = 3;
            slamImpactTriggered = false; // Reset impact flag for new slam phase
            slamSpinTimer = 0f;

            // Final slam down - fast vertical drop
            rb.gravityScale = 3f;
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, -20f); // Giữ một chút horizontal để đánh đúng target

            // Force transition immediately
            animator.SetTrigger("SlamAttack");
            ChangeState(BossState.AttackSlamAttack);
            return; // Exit immediately to prevent any delay
        }

        // Only log occasionally to avoid spam
        if (Mathf.Repeat(slamSpinTimer, 0.2f) < 0.02f)
        {
        }
    }

    void HandleSlamAttack()
    {
        // MUGEN-based Slam Attack implementation (States 2235→2240→2245)
        if (slamPhase != 3) return;

        // MUGEN-style fast drop: VelAdd y=5 when pos y < 0
        if (rb.velocity.y > -15f)
        {
            // MUGEN logic: accelerate downward when in air
            rb.velocity = new Vector2(rb.velocity.x * 0.8f, -15f);
        }

        // Check if hit ground - MUGEN trigger: pos y >= -5
        float currentHeight = transform.position.y;
        float groundLevel = slamStartPosition.y;

        if (currentHeight <= groundLevel + 0.1f && !slamImpactTriggered) // MUGEN ground detection - ONLY ONCE
        {
            OnSlamImpact(); // Gọi trước
            slamImpactTriggered = true; // Set flag SAU khi gọi
        }

        // Occasional logging for debug
        if (Mathf.Repeat(Time.time, 0.5f) < 0.02f)
        {
        }
    }

    // Animation event - called during slam jump to target player với MUGEN sounds  
    public void OnSlamTargetPlayer()
    {
        if (slamPhase == 1 && slamComboInProgress)
        {
            // MUGEN State 2240 - Jump FX and Sound (S3,8)
            PlaySlamJumpEffects();

            // Force transition to approach/spin phase
            slamPhase = 2;
            slamSpinTimer = 0f; // Reset timer when starting spin phase

            // Bay về FIXED target position - không theo player hiện tại
            Vector3 directionToTarget = (slamTargetPosition - transform.position).normalized;

            // Tốc độ dive vừa phải để có thời gian spin
            float horizontalSpeed = directionToTarget.x * dashSpeed * 1.0f; // Giảm từ 1.2f xuống 1.0f
            float verticalSpeed = -6f; // Giảm từ -10f xuống -6f để có thời gian spin

            rb.velocity = new Vector2(horizontalSpeed, verticalSpeed);
            rb.gravityScale = 0.2f; // Giảm gravity để bay lâu hơn

            animator.SetTrigger("Spin");
            ChangeState(BossState.AttackSpin);
            SpawnSpinStartEffect();

        }
    }

    // Animation event - called when slam hits ground với MUGEN effects
    public void OnSlamImpact()
    {
        // Guard: Chỉ allow nếu đúng conditions và chưa trigger
        if (slamPhase != 3 || slamImpactTriggered)
        {
            return;
        }

        // FORCE reset scale ngay khi impact - chống animation curves
        transform.localScale = Vector3.one;

        // Enable slam damage for ground impact
        EnableDamage("slam");

        // MUGEN State 2245 - All Impact FX (!time trigger)
        PlaySlamImpactEffects(); // Main impact with screen shake + 3 sounds + 8 FX
        PlaySlamDebrisEffects(); // Debris particles (time=[0,10])

        DoSlamShockwave();

        // Stop movement immediately
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1f;

        // Wait for FX completion - giảm delay để responsive hơn
        float delayTime = 1.0f; // Giảm từ 1.8s xuống 1.0s cho nhanh hơn

        Invoke(nameof(CompleteSlamCombo), delayTime);
    }

    private void CompleteSlamCombo()
    {
        // Disable damage when slam combo ends
        DisableDamage();

        // FORCE reset scale cuối cùng để đảm bảo về đúng
        transform.localScale = Vector3.one;

        // Reset everything
        slamComboInProgress = false;
        slamPhase = 0;
        slamImpactTriggered = false; // Reset impact flag
        slamSpinTimer = 0f;
        isSlamming = false;
        isSpinning = false;
        isAttacking = false;

        // Set cooldown
        lastSlamTime = Time.time;

        ChangeState(BossState.Idle);
    }

    void HandleWideSlash()
    {
        attackTimer += Time.deltaTime;

        if (!isWideSlashing)
        {
            isWideSlashing = true;
            PreserveDirectionForAttack(); // Lock direction at start

            // MUGEN State 2250 - Spin start FX (rotating effects during spin phase)
            PlayWideSlashSpinStartEffects();

            // Start the slash sequence
            animator.SetTrigger("WideSlash");

            if (enraged)
            {
                StartCoroutine(WideSlashFollowup());
            }
        }

        // Phase 1: Dash towards player (0-0.4s)
        if (attackTimer <= 0.4f)
        {
            // Calculate direction to player
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float moveDirection = isFacingLeft ? -1f : 1f;

            // Lướt mạnh về phía player
            float dashSpeedToPlayer = moveDirection * dashSpeed * 1.5f; // Tăng từ 0.7f lên 1.5f
            rb.velocity = new Vector2(dashSpeedToPlayer, rb.velocity.y);

        }
        // Phase 2: Continue until close to player or timeout
        else if (attackTimer <= 0.8f)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Nếu đã gần player thì giảm tốc
            if (distanceToPlayer <= 3f)
            {
                rb.velocity = new Vector2(rb.velocity.x * 0.5f, rb.velocity.y);
            }
            else
            {
                // Tiếp tục lướt nếu còn xa
                float moveDirection = isFacingLeft ? -1f : 1f;
                float continueSpeed = moveDirection * dashSpeed * 1.2f;
                rb.velocity = new Vector2(continueSpeed, rb.velocity.y);
            }
        }
        // Phase 3: Complete
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);

            // MUGEN State 2255 - Landing FX (sound + screen shake + landing effects)
            PlayWideSlashLandingEffects();

            isWideSlashing = false;
            isAttacking = false;
            attackTimer = 0f;
            ChangeState(BossState.Idle);
        }
    }

    private IEnumerator WideSlashFollowup()
    {
        yield return new WaitForSeconds(0.28f);

        if (player != null)
        {
            // Face towards player for followup
            bool shouldFaceLeft = player.position.x < transform.position.x;
            if (shouldFaceLeft != isFacingLeft)
            {
                isFacingLeft = shouldFaceLeft;
                spriteRenderer.flipX = isFacingLeft;
            }
        }

        // Stronger followup slash
        animator.SetTrigger("WideSlash");
        float moveDirection = isFacingLeft ? -1f : 1f;
        rb.velocity = new Vector2(moveDirection * dashSpeed, rb.velocity.y);
        DoWideSlashFollowup();

        yield return new WaitForSeconds(0.3f);
        EnterIdle();
    }

    private void DoWideSlashFollowup()
    {
        PreserveDirectionForAttack(); // Lock direction during followup

        // Tạo một slash rộng hơn và mạnh hơn cho đòn follow-up
        float moveDirection = isFacingLeft ? -1f : 1f;
        rb.velocity = new Vector2(moveDirection * dashSpeed * 1.2f, rb.velocity.y);

        // TODO: Spawn enhanced wide slash effect
        // TODO: Create wider hitbox
        // TODO: Stronger screen shake
        // TODO: Enhanced slash sound
    }

    // Boss nhận damage
    public void TakeHit(int dmg)
    {
        if (currentState == BossState.Dead) return;

        currentHP -= dmg;

        // cancel movement/hit timers
        desiredVelocity = Vector2.zero;
        attackTimer = 0f;

        if (!enraged && currentHP <= maxHP / 2)
        {
            enraged = true;
            ChangeState(BossState.EnrageTransition);
            return;
        }

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die(); // Gọi Die() để ẩn UI và xử lý đúng
        }
        else
        {
            ChangeState(BossState.Stagger);
            Invoke(nameof(EnterIdle), 1f);
        }

    }

    // Call this to show buff selection UI after boss dies
    private void ShowBuffSelectionUI()
    {
        BuffSelectionUI buffUI = FindObjectOfType<BuffSelectionUI>();
        if (buffUI != null)
        {
            buffUI.ShowBuffSelection();
        }
    }


    // Gọi từ Enrage animation event
    public void FinishEnrage()
    {
        // buff
        walkSpeed *= 1.25f;
        dashSpeed *= 1.3f;
        decisionTimer = 0f;
        EnterIdle();
    }

    // Additional Animation Event Handlers
    public void OnAttackStart()
    {
        // General attack start event
    }

    public void OnAttackEnd()
    {
        // General attack end event
        isAttacking = false;
        ChangeState(BossState.Idle);
    }

    public void OnSpinStart()
    {
        if (currentState == BossState.AttackSpin)
        {
            DoSpinAttack();
        }
    }

    public void OnSpinEnd()
    {
        if (currentState == BossState.AttackSpin)
        {
            isSpinning = false;
            ChangeState(BossState.Idle);
        }
    }

    // Animation Event Handlers cho Cross Slash
    public void OnDashSlashStart()
    {
        if (currentState == BossState.AttackCrossSlash)
        {
            // Enable damage during active frames of CrossSlash
            EnableDamage("crossslash");

            float moveDirection = isFacingLeft ? -1f : 1f;
            rb.velocity = new Vector2(moveDirection * dashSpeed * (1f + crossSlashHits * 0.3f), rb.velocity.y);
            DoDashSlash();
        }
    }

    public void OnDashSlashEnd()
    {
        if (currentState == BossState.AttackCrossSlash)
        {
            // Disable damage when attack ends
            DisableDamage();

            crossSlashHits++;
            if (crossSlashHits >= 3)
            {
                ChangeState(BossState.Idle);
            }
        }
    }

    // Animation Event Handlers cho Slam Attack
    public void OnJumpStart()
    {
        if (currentState == BossState.AttackSlam && !isSlamming)
        {
            isSlamming = true;
            rb.velocity = new Vector2(0, 15f);
            SpawnJumpEffect();
        }
    }

    // Animation Event Handlers cho Wide Slash
    public void OnWideSlashStart()
    {
        if (currentState == BossState.AttackWideSlash)
        {
            // Enable damage during WideSlash spin phase (MUGEN State 2250 spinning hits)
            EnableDamage("wideslashspin");

            PreserveDirectionForAttack();
            float moveDirection = isFacingLeft ? -1f : 1f;
            rb.velocity = new Vector2(moveDirection * dashSpeed * 0.7f, rb.velocity.y);
            DoWideSlash();
        }
    }

    public void OnWideSlashEnd()
    {
        if (currentState == BossState.AttackWideSlash)
        {
            // Change to landing damage (MUGEN State 2255 landing hit)
            EnableDamage("wideslash");

            // MUGEN State 2255 - Landing effects when slash ends
            PlayWideSlashLandingEffects();

            // Disable damage after short delay to allow landing hit
            Invoke(nameof(DisableDamage), 0.3f);

            isWideSlashing = false;
            isAttacking = false; // Release direction lock
            rb.velocity = Vector2.zero;
            ChangeState(BossState.Idle);
        }
    }

    public int GetCurrentHP() => currentHP;

    public bool IsEnraged() => enraged;

    public BossState GetCurrentState() => currentState;

    // ===== IDAMAGEABLE INTERFACE IMPLEMENTATION =====


    // IDamageable interface requires TakeHit(float dmg)
    public void TakeHit(float dmg)
    {
        TakeHit(Mathf.RoundToInt(dmg)); // Convert to int and call existing method
    }

    // ===== FX AND SOUND SYSTEM =====

    #region Audio System
    private void PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void PlaySoundWithRandomPitch(AudioClip clip, float minPitch = 0.8f, float maxPitch = 1.2f, float volume = 1f)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(clip, volume);
        }
    }
    #endregion

    #region Visual Effects System
    private void SpawnEffect(GameObject effectPrefab, Vector3 position, float destroyAfter = 2f)
    {
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, position, Quaternion.identity);

            // Add AutoDestroyOnAnimationEnd component để tự hủy khi animation xong
            AutoDestroyOnAnimationEnd autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f; // Faster check
            autoDestroy.fallbackLifetime = destroyAfter > 0 ? destroyAfter : 2f; // Giảm fallback
        }
    }

    private void SpawnEffect(GameObject effectPrefab, Vector3 position, Quaternion rotation, float destroyAfter = 2f)
    {
        if (effectPrefab != null)
        {
            GameObject fx = Instantiate(effectPrefab, position, rotation);

            // Add AutoDestroyOnAnimationEnd component để tự hủy khi animation xong
            AutoDestroyOnAnimationEnd autoDestroy = fx.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f; // Faster check  
            autoDestroy.fallbackLifetime = destroyAfter > 0 ? destroyAfter : 2f; // Giảm fallback
        }
    }

    // Helper method for spawning effects with custom offset, scale, duration (with direction handling)
    private void SpawnCustomEffect(GameObject effectPrefab, Vector3 offset, float scale, float duration)
    {
        if (effectPrefab != null)
        {
            // Mirror offset theo hướng boss - Inspector offset được thiết kế cho facing RIGHT
            Vector3 finalOffset = offset;
            if (isFacingLeft)
            {
                finalOffset.x = -offset.x; // Mirror X offset relative to boss center
            }

            Vector3 spawnPosition = transform.position + finalOffset;

            GameObject effect = Instantiate(effectPrefab, spawnPosition, Quaternion.identity);

            // Apply custom scale với direction-aware scaling
            if (scale != 1f)
            {
                // Mirror FX sprite khi boss facing left  
                float finalScaleX = isFacingLeft ? -scale : scale;
                effect.transform.localScale = new Vector3(finalScaleX, scale, 1f);
            }
            else if (isFacingLeft)
            {
                // Nếu scale = 1 nhưng vẫn cần mirror
                effect.transform.localScale = new Vector3(-1f, 1f, 1f);
            }

            // Add AutoDestroyOnAnimationEnd component để tự hủy khi animation xong
            AutoDestroyOnAnimationEnd autoDestroy = effect.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = effect.AddComponent<AutoDestroyOnAnimationEnd>();
            }

            // Configure auto destroy settings - để tự detect khi animation/particle hết
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // Không set fallbackLifetime - để AutoDestroy component tự detect và hủy khi animation/particle thực sự hết
        }
    }
    #endregion

    #region Camera Effects
    private void ShakeCamera(float intensity = 0.3f, float duration = 0.2f)
    {
        // TODO: Implement camera shake
        // This would typically use Cinemachine or a custom camera shake system
    }
    #endregion

    #region Attack-Specific Effects
    // CrossSlash Effects (MUGEN State 2230 Implementation) với tuỳ chỉnh Inspector
    private void PlayCrossSlashStartEffects()
    {
        // DEPRECATED: Method này không còn được dùng
        // fx40060/40065 và fx6047 giờ spawn trực tiếp trong HandleCrossSlash phase 2
    }

    private void PlayCrossSlashSlashEffect()
    {
        // DEPRECATED: Method này không còn được dùng  
        // fx6047 và fx6226 giờ spawn trực tiếp trong HandleCrossSlash phase 2
    }

    private void PlayCrossSlashHelperEffect1()
    {
        // MUGEN Helper at AnimElemTime(3)=2 - additional speed lines khi hit

        // fx6226 - thêm hiệu ứng gió bóng khi hit player (với offset tương đối theo hướng)
        Vector3 directionOffset = new Vector3(isFacingLeft ? -0.5f : 0.5f, 0, 0);
        Vector3 finalOffset = crossSlashHelperFX1Offset + directionOffset;
        SpawnCustomEffect(crossSlashHelperFX1, finalOffset, crossSlashHelperFX1Scale, 0.3f); // Duration ngắn hơn khi hit

        // Play helper sound (S0,29 variation)
        PlaySound(crossSlashChargeSound, 0.8f);
    }

    private void PlayCrossSlashHelperEffect2()
    {
        // MUGEN Helper at AnimElemTime(5)=4 - deceleration dust

        // fx6047 - bụi khi dừng lại (tương tự fx khi lướt đi)
        SpawnCustomEffect(crossSlashHelperFX2, crossSlashHelperFX2Offset, crossSlashHelperFX2Scale, crossSlashHelperFX2Duration);

        // Play helper sound (S0,29 variation)  
        PlaySound(crossSlashChargeSound, 1.2f);
    }

    private void PlayCrossSlashFinisherEffect()
    {
        // MUGEN effects at AnimElemTime(8)=0 - final impact

        // Play finisher sound (S0,31 variation)
        PlaySound(crossSlashDashSound2, 0.9f);

        // Final dust effect khi hoàn thành
        SpawnCustomEffect(crossSlashFinisherFX, crossSlashFinisherFXOffset, crossSlashFinisherFXScale, crossSlashFinisherFXDuration);

        // Screen shake for finisher
        ShakeCamera(1.2f, 0.2f);
    }

    // ================== SLAM ATTACK FX METHODS (MUGEN State 2235/2240/2245) ==================

    private void PlaySlamPreparationEffects()
    {
        // MUGEN State 2235 - animelem=2 trigger fx6047

        // fx6047 - dust effect at pos=0,3 with scale=.6,.6
        SpawnCustomEffect(slamPrepFX, slamPrepFXOffset, slamPrepFXScale, slamPrepFXDuration);
    }

    private void PlaySlamJumpEffects()
    {
        // MUGEN State 2240 - jump preparation effects

        // Play jump sound (S3,8) - !time and time=5
        PlaySound(slamJumpSound, 1f);

        // Optional jump preparation FX
        SpawnCustomEffect(slamJumpFX, slamJumpFXOffset, slamJumpFXScale, slamJumpFXDuration);
    }

    private void PlaySlamImpactEffects()
    {
        // MUGEN State 2245 - !time trigger - all impact effects

        // Screen shake - time=30, freq=60, ampl=-11, phase=90
        ShakeCamera(2f, 0.5f); // 30/60 = 0.5s duration, strong shake

        // Play all impact sounds simultaneously
        PlaySound(slamImpactSound1, 1f); // S0,4
        PlaySound(slamImpactSound2, 1f); // S5,35  
        PlaySound(slamImpactSound3, 1f); // S1,37

        // Shadow on ground - anim=1360, pos=40,-pos y, scale=.2,.07
        SpawnCustomEffect(slamImpactShadowFX, slamImpactShadowFXOffset, slamImpactShadowFXScale, slamImpactShadowFXDuration);

        // Main blast effect - anim=6211, pos=36,1, scale=.4,.4
        SpawnCustomEffect(slamImpactBlastFX, slamImpactBlastFXOffset, slamImpactBlastFXScale, slamImpactBlastFXDuration);

        // Vertical beam - anim=6225, pos=41,-90, scale=.65,1.4, angle=-90
        if (slamImpactBeamFX != null)
        {
            Vector3 beamOffset = slamImpactBeamFXOffset;
            if (isFacingLeft)
            {
                beamOffset.x = -beamOffset.x; // Mirror X offset
            }
            Vector3 beamPos = transform.position + beamOffset;
            GameObject beam = Instantiate(slamImpactBeamFX, beamPos, Quaternion.Euler(0, 0, -90));
            beam.transform.localScale = new Vector3(slamImpactBeamFXScale, slamImpactBeamFXScale * 2.15f, 1f); // Y scale proportional to X scale (1.4f/0.65f ≈ 2.15f)

            // Add AutoDestroyOnAnimationEnd component
            AutoDestroyOnAnimationEnd autoDestroy = beam.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = beam.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // Không set fallbackLifetime - để tự detect khi beam animation hết
        }

        // Bright flash - anim=6055, pos=40,0, scale=1.5,1, angle=-90
        if (slamImpactFlashFX != null)
        {
            Vector3 flashOffset = slamImpactFlashFXOffset;
            if (isFacingLeft)
            {
                flashOffset.x = -flashOffset.x; // Mirror X offset
            }
            Vector3 flashPos = transform.position + flashOffset;
            GameObject flash = Instantiate(slamImpactFlashFX, flashPos, Quaternion.Euler(0, 0, -90));
            flash.transform.localScale = new Vector3(slamImpactFlashFXScale, slamImpactFlashFXScale, 1f); // Scale đồng đều X và Y

            // Add AutoDestroyOnAnimationEnd component
            AutoDestroyOnAnimationEnd autoDestroy = flash.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = flash.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // Flash should be very quick - add short fallback for safety
            autoDestroy.fallbackLifetime = 0.2f; // Very short fallback for flash effect
        }

        // Ground crack - anim=6050, pos=40,0, scale=1,1
        SpawnCustomEffect(slamImpactGroundFX, slamImpactGroundFXOffset, slamImpactGroundFXScale, slamImpactGroundFXDuration);

        // Smoke cloud - anim=6265, pos=40,0, scale=.7,.6
        if (slamImpactSmokeFX != null)
        {
            Vector3 smokeOffset = slamImpactSmokeFXOffset;
            if (isFacingLeft)
            {
                smokeOffset.x = -smokeOffset.x; // Mirror X offset
            }
            Vector3 smokePos = transform.position + smokeOffset;
            GameObject smoke = Instantiate(slamImpactSmokeFX, smokePos, Quaternion.identity);
            smoke.transform.localScale = new Vector3(slamImpactSmokeFXScale, slamImpactSmokeFXScale * 0.86f, 1f); // Y scale tỉ lệ với X (0.6f/0.7f ≈ 0.86f)

            // Add AutoDestroyOnAnimationEnd component
            AutoDestroyOnAnimationEnd autoDestroy = smoke.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = smoke.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // Không set fallbackLifetime - để tự detect khi smoke animation/particle hết
        }

        // Mirrored blast - anim=6211, pos=43,1, facing=-1, scale=.4,.4
        if (slamImpactBlastMirrorFX != null)
        {
            Vector3 mirrorBlastOffset = slamImpactBlastMirrorFXOffset;
            if (isFacingLeft)
            {
                mirrorBlastOffset.x = -mirrorBlastOffset.x; // Mirror X offset
            }
            Vector3 mirrorBlastPos = transform.position + mirrorBlastOffset;
            GameObject mirrorBlast = Instantiate(slamImpactBlastMirrorFX, mirrorBlastPos, Quaternion.identity);

            // Apply facing direction + mirror scale
            float scaleX = isFacingLeft ? slamImpactBlastMirrorFXScale : -slamImpactBlastMirrorFXScale;
            mirrorBlast.transform.localScale = new Vector3(scaleX, slamImpactBlastMirrorFXScale, 1f);

            // Add AutoDestroyOnAnimationEnd component
            AutoDestroyOnAnimationEnd autoDestroy = mirrorBlast.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = mirrorBlast.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // Không set fallbackLifetime - để tự detect khi mirror blast animation hết
        }
    }

    private void PlaySlamDebrisEffects()
    {
        // MUGEN Helper 912 - debris particles, trigger time=[0,10]

        // Start coroutine để spawn debris trong khoảng [0,10] frames
        StartCoroutine(SpawnDebrisOverTime());
    }

    private System.Collections.IEnumerator SpawnDebrisOverTime()
    {
        float frameTime = 1f / 60f; // MUGEN 60fps

        // Spawn debris từ frame 0 đến frame 10
        for (int frame = 0; frame <= 10; frame++)
        {
            // Spawn debris với random positions - pos=30+random%60,-10-random%15, scale=1.5,1.5
            if (slamImpactDebrisFX != null)
            {
                Vector3 baseOffset = slamImpactDebrisFXOffset;
                Vector3 randomOffset = new Vector3(
                    Random.Range(3f, 9f),     // 30+random%60 scaled down 
                    Random.Range(-2.5f, -1f), // -10-random%15 scaled down
                    0
                );

                // Mirror offsets theo hướng boss
                if (isFacingLeft)
                {
                    baseOffset.x = -baseOffset.x;
                    randomOffset.x = -randomOffset.x;
                }

                Vector3 debrisPos = transform.position + baseOffset + randomOffset;
                GameObject debris = Instantiate(slamImpactDebrisFX, debrisPos, Quaternion.identity);
                debris.transform.localScale = Vector3.one * slamImpactDebrisFXScale;

                // Add AutoDestroyOnAnimationEnd component
                AutoDestroyOnAnimationEnd autoDestroy = debris.GetComponent<AutoDestroyOnAnimationEnd>();
                if (autoDestroy == null)
                {
                    autoDestroy = debris.AddComponent<AutoDestroyOnAnimationEnd>();
                }
                autoDestroy.destroyOnFirstLoop = true;
                autoDestroy.startupGrace = 0.1f;
                autoDestroy.fallbackLifetime = slamImpactDebrisFXDuration > 0 ? slamImpactDebrisFXDuration : 5f;
            }

            // Wait 1 frame
            yield return new WaitForSeconds(frameTime);
        }
    }
    private void PlayCrossSlashWarningFX()
    {
        PlaySound(crossSlashChargeSound, 0.7f);
        if (crossSlashWarningFX != null)
        {
            SpawnEffect(crossSlashWarningFX, transform.position + Vector3.down * 0.5f, 0.6f);
        }
    }

    private void PlayCrossSlashDashFX()
    {
        PlaySoundWithRandomPitch(crossSlashDashSound1, 0.9f, 1.1f, 1f);
        if (crossSlashTrailFX != null)
        {
            SpawnEffect(crossSlashTrailFX, transform.position, 1.5f);
        }
        ShakeCamera(0.2f, 0.1f);
    }

    private void PlayCrossSlashHitFX(Vector3 hitPosition)
    {
        if (crossSlashHitFX != null)
        {
            SpawnEffect(crossSlashHitFX, hitPosition, 1f);
        }
        ShakeCamera(0.4f, 0.2f);
    }

    #region WideSlash/Spin Effects (MUGEN States 2250-2255)

    // MUGEN State 2250 - Spin Phase FX (called when entering spin)
    private void PlayWideSlashSpinStartEffects()
    {
        // anim=6207 rotating effect 1 - pos=0,-20, scale=.61,.61, bindtime=-1, removetime=-1
        SpawnCustomEffect(wideSlashSpinFX1, wideSlashSpinFX1Offset, wideSlashSpinFX1Scale, wideSlashSpinFX1Duration);

        // anim=6224 rotating effect 2 - pos=0,-20, scale=.61,.61, bindtime=-1, removetime=-1  
        SpawnCustomEffect(wideSlashSpinFX2, wideSlashSpinFX2Offset, wideSlashSpinFX2Scale, wideSlashSpinFX2Duration);
    }

    // MUGEN State 2255 - Landing Phase FX (called when spin lands)
    private void PlayWideSlashLandingEffects()
    {
        // MUGEN S1,38 landing sound (volumescale=100)  
        PlaySound(wideSlashLandingSound, 1f);

        // MUGEN EnvShake: time=15, ampl=-5, freq=12 (converted to Unity)
        ShakeCamera(0.4f, 0.25f); // 15/60 = 0.25s duration, moderate shake

        // anim=6800 main landing effect - pos=10,0, scale=.4,.4, removetime=-2
        SpawnCustomEffect(wideSlashLandingFX1, wideSlashLandingFX1Offset, wideSlashLandingFX1Scale, wideSlashLandingFX1Duration);

        // anim=40060 dual landing effects - pos=5,0 và pos=15,0, scale=.7,.7, removetime=-2
        SpawnCustomEffect(wideSlashLandingFX2, wideSlashLandingFX2Offset, wideSlashLandingFX2Scale, wideSlashLandingFX2Duration);

        // anim=40060 facing=-1 (mirrored) - pos=15,0, scale=.7,.7, removetime=-2
        if (wideSlashLandingFX3 != null)
        {
            Vector3 mirrorOffset = wideSlashLandingFX3Offset;
            if (isFacingLeft)
            {
                mirrorOffset.x = -mirrorOffset.x; // Mirror offset
            }
            Vector3 fx3Pos = transform.position + mirrorOffset;
            GameObject fx3 = Instantiate(wideSlashLandingFX3, fx3Pos, Quaternion.identity);
            fx3.transform.localScale = new Vector3(-wideSlashLandingFX3Scale, wideSlashLandingFX3Scale, 1f); // facing=-1 effect

            // Add AutoDestroy
            AutoDestroyOnAnimationEnd autoDestroy = fx3.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
            {
                autoDestroy = fx3.AddComponent<AutoDestroyOnAnimationEnd>();
            }
            autoDestroy.destroyOnFirstLoop = true;
            autoDestroy.startupGrace = 0.05f;
            // removetime=-2 means very short, use small fallback
            autoDestroy.fallbackLifetime = 0.1f;
        }
    }

    // Legacy method for compatibility - now calls MUGEN-accurate versions
    private void PlayWideSlashFX()
    {
        // For compatibility, call both spin and landing effects
        // In actual gameplay, these should be called separately based on attack phase
        PlayWideSlashSpinStartEffects();

        // Delay landing effects slightly to match MUGEN timing
        Invoke(nameof(PlayWideSlashLandingEffects), 0.3f);
    }

    #endregion

    // Slam Effects
    private void PlaySlamJumpFX()
    {
        PlaySound(slamJumpSound, 0.8f);
        if (jumpDustFX != null)
        {
            SpawnEffect(jumpDustFX, transform.position + Vector3.down * 1f, 1f);
        }
    }

    private void PlaySlamImpactFX()
    {
        PlaySound(slamImpactSound, 1f);
        if (slamImpactFX != null)
        {
            SpawnEffect(slamImpactFX, transform.position + Vector3.down * 0.5f, 2f);
        }
        ShakeCamera(0.6f, 0.3f);
    }

    // Movement Effects
    private void PlayFootstepFX()
    {
        PlaySoundWithRandomPitch(footstepSound, 0.7f, 1.3f, 0.3f);
    }
    #endregion

    #region Damage System (MUGEN HitDef Implementation)

    // Enable damage dealing for current attack
    public void EnableDamage(string attackType)
    {
        canDamagePlayer = true;
        currentAttackType = attackType;
    }

    // Disable damage dealing
    public void DisableDamage()
    {
        canDamagePlayer = false;
        currentAttackType = "";
    }

    // Check if can hit player and deal damage
    private void CheckPlayerHit()
    {
        if (!canDamagePlayer || player == null || Time.time - lastHitTime < HIT_COOLDOWN)
            return;

        // Distance check based on attack type
        float hitRange = GetAttackHitRange(currentAttackType);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= hitRange)
        {
            DealDamageToPlayer(currentAttackType);
        }
    }

    // Get hit range for different attacks
    private float GetAttackHitRange(string attackType)
    {
        switch (attackType.ToLower())
        {
            case "crossslash": return 3.5f;      // MUGEN close range
            case "wideslash": return 4f;         // MUGEN medium range
            case "wideslashspin": return 3f;     // Spinning hits during wide slash
            case "slam": return 5f;              // MUGEN wide area slam
            default: return 2.5f;
        }
    }

    // Deal damage to player based on attack type (MUGEN HitDef values)
    private void DealDamageToPlayer(string attackType)
    {
        int damage = 0;
        int chipDamage = 0;

        // Get damage values based on attack type (from MUGEN HitDef)
        switch (attackType.ToLower())
        {
            case "crossslash":
                damage = crossSlashDamage;
                chipDamage = crossSlashChipDamage;
                break;
            case "wideslash":
                damage = wideSlashLandingDamage;
                chipDamage = wideSlashLandingChipDamage;
                break;
            case "wideslashspin":
                damage = wideSlashSpinDamage;
                chipDamage = wideSlashSpinChipDamage;
                break;
            case "slam":
                damage = slamDamage;
                chipDamage = slamChipDamage;
                break;
            default:
                damage = 10; // Default damage
                chipDamage = 2;
                break;
        }

        // Try to get PlayerResources component (current health system)
        var playerResources = player.GetComponent<PlayerResources>();
        if (playerResources != null)
        {
            playerResources.TakeDamage(damage);
        }
        else
        {
            // Fallback - try legacy PlayerHealth component
            var playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                // Final fallback: Send message to player
                player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        lastHitTime = Time.time;

        // MUGEN hitstun effects could be added here
        ApplyHitEffects(attackType);
    }

    // Apply hit effects (knockback, hitstun, etc.)
    private void ApplyHitEffects(string attackType)
    {
        // Get player rigidbody for knockback
        var playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            Vector2 knockback = Vector2.zero;

            // MUGEN knockback values
            switch (attackType.ToLower())
            {
                case "crossslash":
                    // MUGEN: Ground.Velocity = -3,0 Air.Velocity = -3,-2
                    knockback = new Vector2(isFacingLeft ? 3f : -3f, 0);
                    break;
                case "wideslash":
                case "wideslashspin":
                    // MUGEN: Ground.Velocity = -8/2,0 Air.Velocity = -8/2,-4/2
                    knockback = new Vector2(isFacingLeft ? 4f : -4f, 0);
                    break;
                case "slam":
                    // Strong knockback for slam
                    knockback = new Vector2(isFacingLeft ? 5f : -5f, -2f);
                    break;
            }

            playerRb.velocity = knockback;
        }

        // Additional hit effects could be added here (screen shake, hit FX, etc.)
    }

    #endregion

    // ===== BOSS HEALTH UI AUTO-CREATION =====

    void CreateBossHealthUI()
    {
        // Check if UI already exists in scene (including inactive ones)
        BossHealthUI[] allHealthUIs = FindObjectsOfType<BossHealthUI>(true); // Include inactive
        if (allHealthUIs.Length > 0)
        {
            // Setup existing UI to use this boss
            BossHealthUI existingUI = allHealthUIs[0];
            existingUI.bossScript = this;
            existingUI.ShowBossUI(); // Make sure it's visible
            return;
        }

        // Ensure canvas exists
        Canvas canvas = CanvasEnsurer.GetOrCreateCanvas();
        if (canvas == null)
        {
            return;
        }

        // Create the UI using BossHealthUISetup
        GameObject setupObj = new GameObject("Boss Health UI Setup (Auto)");
        BossHealthUISetup setup = setupObj.AddComponent<BossHealthUISetup>();

        setup.targetCanvas = canvas;
        setup.bossReference = this;
        setup.createUIOnStart = true;

        // Create UI immediately
        setup.CreateBossHealthUI();

        // Clean up the setup object after creation
        Destroy(setupObj, 0.1f);
    }

    /// <summary>
    /// Public method to stun the boss (for Ultimate skill)
    /// </summary>
    public void StunBoss(float stunDuration = 3f)
    {
        if (currentState == BossState.Dead) return;

        // Force change to idle state and lock it there
        ChangeState(BossState.Idle);

        // Stop all movement but keep physics for ground collision
        rb.velocity = Vector2.zero;
        // DON'T make kinematic - keep physics for ground collision

        // Stop any ongoing coroutines
        if (stateRoutine != null)
        {
            StopCoroutine(stateRoutine);
            stateRoutine = null;
        }

        // Start stun coroutine
        StartCoroutine(StunCoroutine(stunDuration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        // Disable boss completely during stun
        bool originalEnabled = this.enabled;
        this.enabled = false; // Disable this script's Update/FixedUpdate

        // Keep animator in idle but don't allow state changes
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsStaggered", false);

        float timer = 0f;
        while (timer < duration && currentState != BossState.Dead)
        {
            // Keep boss completely still but maintain ground physics
            rb.velocity = new Vector2(0, rb.velocity.y); // Keep Y velocity for gravity

            // Force idle animation
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", false);

            timer += Time.deltaTime;
            yield return null;
        }

        // End stun - re-enable boss behavior
        if (currentState != BossState.Dead)
        {
            this.enabled = originalEnabled; // Re-enable script
            ChangeState(BossState.Idle); // Return to normal idle
        }
    }
}