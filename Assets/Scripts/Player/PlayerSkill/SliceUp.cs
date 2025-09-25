using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// SliceUpSkill
/// Port từ M.U.G.E.N State 1100 -> spawn FX, play sounds, movement (VelSet), camera shake (EnvShake).
/// - Thời chuẩn: giả lập 60 ticks/s (1 tick ≈ 1/60s)
/// - Animelem -> xử lý bằng frame number (animelem n ≈ frame n)
/// </summary>
public class SliceUpSkill : MonoBehaviour
{
    // (Removed unused FX and teleport settings)

    [Header("Hit Zone")]
    public Transform SliceUpHitZone;    // Tâm vùng spawn FX loạn trảm (nếu null sẽ dùng enemy đầu tiên bị trúng)

    [Header("References")]
    public Animator animator;           // Animation controller (clip "SliceUp" recommended)
    public Rigidbody2D rb;              // Rigidbody2D của player
    public CameraShake camShake;        // CameraShake script reference
    public AudioSource audioSource;     // Audio source to play sounds

    [Header("Sound (assign)")]
    public AudioClip sndStart;          // S950,1
    public AudioClip sndSlash;          // S0,12 (dùng 3 lần cùng lúc)
    public AudioClip[] sndLoopRandom;   // S3,4 + random%2

    [Header("FX Prefabs (assign)")]
    public GameObject fxStart;          // Initial FX from Helper state 900
    public GameObject fxHit1;           // Anim 1130
    public GameObject fxHit2;           // Anim 1140
    public GameObject fxGround;         // Anim 6800
    public GameObject fxRandomA;        // Anim 30331 (0.13s duration)
    public GameObject fxRandomB;        // Anim 30332 (0.13s duration)

    [Header("Settings")]
    public float tickRate = 60f;        // ticks per sec (MUGEN ~60)
    public float animeElem6VelX = 3f;   // VelSet x at animelem 6
    public float time105VelX = -3f;     // VelSet x at time>=105
    public float time105VelY = -5f;     // VelSet y at time>=105
    [Tooltip("Offset applied when centering on enemy for loạn trảm (X affected by facing).")]
    public Vector2 loantRamCenterOffset = new Vector2(0f, -0.5f);
    [Tooltip("Match damage hitbox to the visual FX spawn region.")]
    public bool useFxAlignedHitBox = true;
    [Tooltip("Scale factor for the FX-aligned hitbox size.")]
    public float fxHitBoxScale = 1.0f;

    [Header("Animator Settings")]
    public string idleStateName = "Idle";  // Tên state Idle trong Animator
    public bool forceIdleOnCancel = true;  // Về luôn Idle nếu dash không trúng ai

    // (Removed unused Hitbox/Helper/State Management fields)
    public MonoBehaviour playerController; // Player movement controller to disable

    [Header("Ground Check")]
    public LayerMask groundLayers = ~0;     // Layers treated as ground (default: all)
    public float groundCheckRadius = 0.12f; // Circle radius for ground check
    public Vector2 groundCheckOffset = new Vector2(0f, -0.15f); // Offset from player center
    public float groundCheckDistance = 0.06f; // Ray distance as backup

    [Header("Slash Settings")]
    public LayerMask enemyLayers;           // Layers considered enemies
    public float dashAfterChargeDistance = 2.0f; // Dash distance at frame 35
    public float dashAfterChargeDuration = 0.15f; // Duration of the dash
    public float dashHitRadius = 0.6f;      // Radius to detect enemy during dash
    public float loantRamRadius = 2.5f;     // AoE radius around SliceUpHitZone during loạn trảm
    public int loantRamDamagePerTick = 10;  // Damage applied every 4 ticks during loạn trảm

    // internal
    private float timer;                // seconds since PlaySkill()
    private bool isPlaying;
    private bool helperSpawned;
    private bool trig_frame6;
    private bool trig_time105;
    private int lastProcessedFrame = -1;
    private GameObject fx1140Instance;
    private List<GameObject> activeFX = new List<GameObject>(); // Track all active FX
    // (Removed unused original/slash positions)
    private bool skipFirstUpdate;       // Bỏ qua Update đầu để không trigger animelem ngay khi start
    // (Removed startedOnGround; use IsGroundedStrict() where needed)

    // Dash/Loạn trảm runtime state
    private bool dashActive;
    private int dashStartFrame;
    private int dashEndFrame;
    private Vector3 dashStartPos;
    private Vector3 dashTargetPos;
    private bool loantRamActive;        // Đã va trúng enemy trong lúc dash
    private Vector3 dashPrevPos;        // previous position during dash for sweep detection
    private bool loantRamStarted;       // Đã bước vào pha loạn trảm (mở FX/SFX/Damage)
    private bool animatorFrozenAt35;    // Đang đóng băng animation tại frame 35
    // (Removed unused time-based dash fields)
    private float savedGravityScale;    // lưu gravity khi dash
    private bool gravitySaved;
    private Transform loantRamCenterTransform; // Tâm loạn trảm: enemy đầu tiên bị trúng
    private Vector3 lastFxSpawnPos;           // Lưu vị trí FX vừa spawn để drift camera

    // Cached PlayerController for reliable grounded state
    private PlayerController pc;
    public bool isIntroLock = false;

    // ❌ DISABLED: Direct input bypass - Skills should go through UI system only!
    // This method was causing skills to execute without mana/cooldown checks
    /*
    public void OnSliceUp(InputValue v)
    {
        if (v.isPressed)
        {
            // Tránh kích hoạt lại khi skill đang chạy (tránh phá animator/controller)
            if (isPlaying) return;
            // Chỉ cho dùng khi đang trên mặt đất
            if (!IsGroundedStrict()) return;
            PlaySkill();
        }
    }
    */

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PlayerController>();
        if (pc != null && playerController == null)
            playerController = pc;
        if (camShake == null)
            camShake = FindObjectOfType<CameraShake>();

        // Set a sensible default for enemyLayers (try layer named "Enemy")
        if (enemyLayers.value == 0)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) enemyLayers = 1 << enemyLayer;
        }
    }

    // Public API
    public void PlaySkill()
    {
        if (isIntroLock) return;
        // reset
        timer = 0f;
        isPlaying = true;
        skipFirstUpdate = true;
        helperSpawned = false;
        trig_frame6 = trig_time105 = false;
        lastProcessedFrame = -1;
        gravitySaved = false;

        // (No need to cache startedOnGround/originalPosition)
        dashActive = false;
        loantRamStarted = false;
        animatorFrozenAt35 = false;
        loantRamActive = false;
        loantRamCenterTransform = null;
        lastFxSpawnPos = transform.position;
        // Khởi động dash ngay từ lúc bắt đầu skill; mục tiêu dừng tại frame 35
        dashStartFrame = 0;
        dashEndFrame = Mathf.Max(1, Mathf.CeilToInt(dashAfterChargeDuration * tickRate));
        dashStartPos = transform.position;
        float faceStart = GetFacingSign();
        dashTargetPos = dashStartPos + new Vector3(dashAfterChargeDistance * faceStart, 0f, 0f);
        dashPrevPos = dashStartPos;

        // Disable player movement
        if (playerController != null)
            playerController.enabled = false;

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Play("SliceUp", 0, 0f);
        }

        // Set initial velocity (MUGEN: velset = 0,0)
        if (rb != null) rb.velocity = Vector2.zero;

        // Clean up previous FX instances
        if (fx1140Instance != null) Destroy(fx1140Instance);

        // Clean up all active FX from previous skill usage
        foreach (GameObject fx in activeFX)
        {
            if (fx != null) Destroy(fx);
        }
        activeFX.Clear();

        // initial sounds (S950 + S0,12 x3 at !time)
        if (audioSource != null)
        {
            if (sndStart != null) PlaySoundDelayed(sndStart, 0f);
            if (sndSlash != null)
            {
                PlaySoundDelayed(sndSlash, 0f);
                PlaySoundDelayed(sndSlash, 0.02f);
                PlaySoundDelayed(sndSlash, 0.04f);
            }
        }

        // Spawn FxStart ngay lập tức để không bị miss frame 0
        SpawnFxStart();
    }

    void Update()
    {
        if (!isPlaying) return;

        // Tránh trigger các mốc animelem ở cùng frame với lúc bắt đầu skill
        if (skipFirstUpdate)
        {
            skipFirstUpdate = false;
            return;
        }

        timer += Time.deltaTime;
        float tFrames = timer * tickRate;
        int frame = Mathf.FloorToInt(tFrames);

        if (frame == lastProcessedFrame) return;
        lastProcessedFrame = frame;

        // (FxStart đã được spawn trực tiếp trong PlaySkill)

        // ---------- animelem = 6 ----------
        if (!trig_frame6 && frame == 6)
        {
            trig_frame6 = true;
            float face = GetFacingSign();
            if (rb != null) rb.velocity = new Vector2(animeElem6VelX * face, 0f);

            // (Bỏ 1130 ở animelem 6 để 1130 chỉ xuất hiện từ frame 35 theo timemod)
            // Explod 1140: pos = 6,-18, scale = .6,.2, removetime = -2 (PPU 16)
            fx1140Instance = SpawnFXWithReturn(fxHit2, new Vector2(6f, -18f), new Vector2(0.6f, 0.2f), 0f);
        }

        // ---------- animelem = 7 ----------
        if (frame == 7)
        {
            if (rb != null) rb.velocity = Vector2.zero; // velset x = 0
        }

        // ---------- frame 35: freeze anim và bắt đầu dash ----------
        if (!dashActive && frame == 35)
        {
            dashActive = true;
            dashStartFrame = frame;
            dashEndFrame = frame + Mathf.Max(1, Mathf.CeilToInt(dashAfterChargeDuration * tickRate));
            dashStartPos = transform.position;
            dashPrevPos = dashStartPos;
            float face = GetFacingSign();
            dashTargetPos = dashStartPos + new Vector3(dashAfterChargeDistance * face, 0f, 0f);
            if (animator != null)
            {
                animator.speed = 0f; // đóng băng animation tại frame 35
                animatorFrozenAt35 = true;
            }
            if (rb != null)
            {
                if (!gravitySaved)
                {
                    savedGravityScale = rb.gravityScale;
                    gravitySaved = true;
                }
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
            }
        }

        // Thực hiện dash tuyến tính theo frame và dò enemy trong lúc dash
        if (dashActive)
        {
            float total = Mathf.Max(1, dashEndFrame - dashStartFrame);
            float p = Mathf.Clamp01((frame - dashStartFrame) / total);
            Vector3 nextPos = Vector3.Lerp(dashStartPos, dashTargetPos, p);
            // Cập nhật vị trí (ưu tiên transform để mượt; có thể thay bằng rb nếu cần)
            transform.position = nextPos;

            // Kiểm tra va chạm enemy dọc theo quãng đường dash (sweep)
            float face = GetFacingSign();
            Vector2 from = dashPrevPos;
            Vector2 delta = (Vector2)(nextPos - (Vector3)from);
            float dist = delta.magnitude;
            bool hitEnemy = false;
            Collider2D firstEnemy = null;
            Vector2? firstHitPoint = null;
            if (dist > 0.0001f)
            {
                var castHits = Physics2D.CircleCastAll(from, dashHitRadius, delta.normalized, dist, enemyLayers);
                if (castHits != null && castHits.Length > 0)
                {
                    // Chọn va chạm gần nhất dọc theo quãng đường
                    float bestFrac = float.MaxValue;
                    foreach (var h in castHits)
                    {
                        if (h.collider == null) continue;
                        if (h.fraction < bestFrac)
                        {
                            bestFrac = h.fraction;
                            firstEnemy = h.collider;
                            firstHitPoint = h.point;
                        }
                    }
                    hitEnemy = firstEnemy != null;
                }
            }
            if (!hitEnemy)
            {
                // Probe ahead as secondary check
                Vector3 probe = transform.position + new Vector3(face * 0.6f, 0f, 0f);
                var hitsOverlap = Physics2D.OverlapCircleAll(probe, dashHitRadius, enemyLayers);
                if (hitsOverlap != null && hitsOverlap.Length > 0)
                {
                    hitEnemy = true;
                    firstEnemy = hitsOverlap[0];
                    if (firstEnemy != null)
                        firstHitPoint = firstEnemy.bounds.ClosestPoint(probe);
                }
            }
            // Fallback by tag when no enemyLayers configured
            if (!hitEnemy && enemyLayers.value == 0)
            {
                Vector3 probe = transform.position + new Vector3(face * 0.6f, 0f, 0f);
                var allHits = Physics2D.OverlapCircleAll(probe, dashHitRadius, ~0);
                foreach (var h in allHits)
                {
                    if (h != null && h.CompareTag("Enemy")) { hitEnemy = true; firstEnemy = h; firstHitPoint = h.bounds.ClosestPoint(probe); break; }
                }
            }

            if (hitEnemy)
            {
                loantRamActive = true;
                // Ngay khi chạm enemy thì dừng dash và kích hoạt loạn trảm
                dashActive = false;
                // Đặt tâm loạn trảm là enemy đầu tiên bị trúng
                loantRamCenterTransform = (firstEnemy != null) ? firstEnemy.transform : null;
                // Cập nhật vị trí đứng tại điểm chạm để tránh lao quá đà
                if (firstHitPoint.HasValue)
                {
                    Vector2 hp = firstHitPoint.Value;
                    // dịch lùi nhẹ để tránh chồng hình
                    Vector2 back = (dist > 0.0001f) ? (delta.normalized * 0.05f) : Vector2.zero;
                    transform.position = new Vector3(hp.x - back.x, hp.y - back.y, transform.position.z);
                }
                // Khôi phục gravity sau dash
                if (rb != null && gravitySaved)
                {
                    rb.gravityScale = savedGravityScale;
                    gravitySaved = false;
                    rb.velocity = Vector2.zero;
                }
                // Vào loạn trảm ngay lập tức
                loantRamStarted = true;
                if (animator != null && animatorFrozenAt35)
                {
                    animator.speed = 1f;
                    animatorFrozenAt35 = false;
                }
                // Spawn ground FX at actual ground position near player
                SpawnGroundFXNear(transform.position, 20f / 16f, new Vector2(0.7f, 0.7f));
                // Camera shake focus at loạn trảm center
                if (camShake != null)
                {
                    camShake.PulseAt(GetLoantRamCenterPosition(), 0.15f, 0.25f, 0.12f);
                    // Zoom-in và giữ đến khi kết thúc skill
                    camShake.ZoomHoldStart(1.0f, 0.1f);
                }
                // Kết thúc xử lý dash trong frame này
                return;
            }

            dashPrevPos = nextPos;

            if (frame >= dashEndFrame)
            {
                dashActive = false;
                // restore gravity after dash
                if (rb != null && gravitySaved)
                {
                    rb.gravityScale = savedGravityScale;
                    gravitySaved = false;
                }
                // Quyết định: nếu trong lúc dash không trúng enemy -> kết thúc chiêu (trúng đã xử lý ngay khi va)
                if (!loantRamActive)
                {
                    // Miss: hủy chiêu và (tuỳ chọn) về Idle ngay
                    if (forceIdleOnCancel && animator != null && !string.IsNullOrEmpty(idleStateName))
                    {
                        animator.speed = 1f;
                        animator.Play(idleStateName, 0, 0f);
                    }
                    EndSkill();
                    return;
                }
            }
        }

        // ---------- Frame 35+: tiếp tục FX/SFX theo timemod ----------

        // ---------- PlaySnd S3,4+rand mỗi 7 tick từ frame 35..105 (chỉ khi đã vào loạn trảm) ----------
        if (loantRamStarted && frame >= 35 && frame <= 105 && frame % 7 == 0)
        {
            if (audioSource != null && sndLoopRandom.Length > 0)
            {
                // S3,4 + (random%2) - play twice like MUGEN
                AudioClip clip = sndLoopRandom[Random.Range(0, sndLoopRandom.Length)];
                PlaySoundDelayed(clip, 0f);
                PlaySoundDelayed(clip, 0.01f); // Slight delay for second instance
            }
        }

        // ---------- Ground FX loop: spawn liên tục khi loạn trảm và đang đứng đất ----------
        if (loantRamStarted && frame >= 35 && frame <= 105 && frame % 12 == 0 && IsGroundedStrict())
        {
            // 20 MUGEN px -> 20/16 Unity units forward
            SpawnGroundFXNear(transform.position, 20f / 16f, new Vector2(0.7f, 0.7f));
        }

        // ---------- EnvShake mỗi 4 tick từ frame 38..105 (chỉ khi đã vào loạn trảm) ----------
        if (loantRamStarted && frame >= 38 && frame <= 105 && frame % 4 == 0)
        {
            if (camShake != null)
                camShake.ShakeOnce(10f / tickRate, Mathf.Abs(-10f) * 0.05f);
        }

        // ---------- time >= 105 velocity ----------
        if (!trig_time105 && frame >= 105)
        {
            trig_time105 = true;
            if (rb != null) rb.velocity = new Vector2(time105VelX, time105VelY); // velset x = -3, y = -5
        }

        // ---------- EXPLOD RANDOM FX: chỉ khi đã vào loạn trảm ----------
        // Timemod = 3,0: Time >= 35 && Time <= 105
        if (loantRamStarted && frame >= 35 && frame <= 105 && frame % 3 == 0)
        {
            SpawnRandomFX(fxHit1, new Vector2(0.3f, 0.3f)); // anim = 1130, scale = .3,.3
            if (camShake != null)
                camShake.PulseAt(GetLoantRamCenterPosition(), 0.05f, 0.06f, 0.08f);
        }

        // Timemod = 5,0: Time >= 38 && Time <= 105
        if (loantRamStarted && frame >= 38 && frame <= 105 && frame % 5 == 0)
        {
            SpawnRandomFX(fxRandomB, new Vector2(0.3f, 0.3f)); // anim = 30332, scale = .3,.3
            if (camShake != null)
                camShake.PulseAt(GetLoantRamCenterPosition(), 0.05f, 0.05f, 0.08f);
        }

        // Timemod = 6,0: Time >= 35 && Time <= 105
        if (loantRamStarted && frame >= 35 && frame <= 105 && frame % 6 == 0)
        {
            SpawnRandomFX(fxRandomA, new Vector2(0.3f, 0.3f)); // anim = 30331, scale = .3,.3
            if (camShake != null)
                camShake.PulseAt(GetLoantRamCenterPosition(), 0.05f, 0.05f, 0.08f);
        }

        // --- Camera drift theo hướng FX mỗi 10 frame (spawn 1 FX và drift về phía nó) ---
        if (loantRamStarted && frame >= 35 && frame <= 105 && frame % 10 == 0)
        {
            // Ưu tiên spawn fxRandomA, fallback 1130 nếu null
            if (fxRandomA != null) SpawnRandomFX(fxRandomA, new Vector2(0.3f, 0.3f));
            else if (fxHit1 != null) SpawnRandomFX(fxHit1, new Vector2(0.3f, 0.3f));
            if (camShake != null)
            {
                camShake.PulseAt(lastFxSpawnPos, 2.5f, 2.5f, 1f);
            }
        }

        // ---------- HitDef every 4 ticks: TimeMod = 4,0 (chỉ khi đã vào loạn trảm) ----------
        if (loantRamStarted && frame % 4 == 0)
        {
            ApplyAoEDamage();
            if (camShake != null)
            {
                camShake.PulseAt(GetLoantRamCenterPosition(), 0.06f, 0.12f, 0.1f);
            }
        }

        // ---------- ChangeState conditions ----------
        // End skill at frame 105
        if (frame >= 105)
        {
            EndSkill();
        }
    }

    float GetFacingSign()
    {
        return transform.localScale.x >= 0f ? 1f : -1f;
    }

    // Kiểm tra đang đứng đất: ưu tiên lấy từ PlayerController nếu có property/field IsGrounded
    // (Removed unused IsOnGround; use IsGroundedStrict/PhysicsGroundCheck)

    // Ưu tiên strict theo playercontroller.cs: isGrounded (bool). Nếu không tìm thấy thì fallback sang Animator/Physics
    bool IsGroundedStrict()
    {
        // 1) Strong-typed PlayerController
        if (pc != null)
        {
            return pc.isGrounded;
        }

        // 2) Reflection trên playerController nếu có
        if (playerController != null)
        {
            var t = playerController.GetType();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var prop = t.GetProperty("isGrounded", flags) ?? t.GetProperty("IsGrounded", flags) ?? t.GetProperty("Grounded", flags);
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                try { return (bool)prop.GetValue(playerController); } catch { }
            }
            var fld = t.GetField("isGrounded", flags) ?? t.GetField("IsGrounded", flags) ?? t.GetField("Grounded", flags);
            if (fld != null && fld.FieldType == typeof(bool))
            {
                try { return (bool)fld.GetValue(playerController); } catch { }
            }
            var m = t.GetMethod("IsGrounded", flags) ?? t.GetMethod("isGrounded", flags);
            if (m != null && m.ReturnType == typeof(bool) && m.GetParameters().Length == 0)
            {
                try { return (bool)m.Invoke(playerController, null); } catch { }
            }
        }
        // 3) Animator param proxy
        if (animator != null)
        {
            try { return animator.GetBool("IsGrounded"); } catch { }
        }
        // 4) Physics fallback
        return PhysicsGroundCheck();
    }

    // Physics-based ground check for robust fallback
    bool PhysicsGroundCheck()
    {
        Vector2 origin = (Vector2)transform.position + groundCheckOffset;
        int mask = groundLayers.value;
        float radius = groundCheckRadius;

        // Prefer PlayerController config if available
        if (pc != null)
        {
            if (pc.groundCheck != null) origin = pc.groundCheck.position;
            if (pc.groundCheckRadius > 0f) radius = pc.groundCheckRadius;
            mask = pc.groundMask.value;
        }
        // Overlap circle first
        Collider2D hit = Physics2D.OverlapCircle(origin, radius, mask);
        if (hit != null && !hit.isTrigger && hit.transform != transform) return true;

        // Raycast down as backup
        RaycastHit2D ray = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, mask);
        if (ray.collider != null && !ray.collider.isTrigger && ray.collider.transform != transform) return true;

        return false;
    }

    void EndSkill()
    {
        isPlaying = false;

        // Đảm bảo animation trở về bình thường
        if (animator != null)
        {
            animator.speed = 1f; // Resume normal animation speed
            // Không force chuyển animation, để player controller tự quản lý
        }

        // Restore gravity if we modified it
        if (rb != null && gravitySaved)
        {
            rb.gravityScale = savedGravityScale;
            gravitySaved = false;
        }

        // Không ép velocity về 0 ở đây để giữ đúng velset time>=105 (-3,-5)

        // Clean up main FX instances
        if (fx1140Instance != null)
        {
            Destroy(fx1140Instance);
            fx1140Instance = null;
        }

        // Clean up ALL active FX immediately
        foreach (GameObject fx in activeFX)
        {
            if (fx != null) Destroy(fx);
        }
        activeFX.Clear();

        // Zoom trả lại camera nếu có
        if (camShake != null)
        {
            camShake.ZoomHoldEnd(0.25f);
        }

        // Re-enable player movement - để player controller tự quản lý animation
        if (playerController != null)
            playerController.enabled = true;
    }
    void PlaySoundDelayed(AudioClip clip, float delay)
    {
        if (clip == null || audioSource == null) return;
        if (delay <= 0f) audioSource.PlayOneShot(clip);
        else StartCoroutine(PlayDelayedCR(clip, delay));
    }

    // Spawn FX đầu (FxStart) theo MUGEN Helper state 900: pos -10,-30 (PPU16), scale (0.2,0.2)
    void SpawnFxStart()
    {
        if (helperSpawned) return; // đảm bảo chỉ 1 lần
        helperSpawned = true;
        if (fxStart == null) return;

        float face = GetFacingSign();
        // pos = -10,-30 (MUGEN) -> Unity: x = (-10/16)*face, y = 30/16
        Vector3 fxPos = transform.position + new Vector3((-10f * face) / 16f, 0f / 16f, 0f);
        GameObject startFx = Instantiate(fxStart, fxPos, Quaternion.identity);
        startFx.transform.localScale = new Vector3(0.2f * face, 0.2f, 1f);
        activeFX.Add(startFx);

        // Auto-destroy
        System.Type adType = System.Type.GetType("AutoDestroyOnAnimationEnd") ?? System.Type.GetType("AutoDestroyOnAnimationEnd, Assembly-CSharp");
        if (adType != null && startFx.GetComponent(adType) == null)
            startFx.AddComponent(adType);
    }

    System.Collections.IEnumerator PlayDelayedCR(AudioClip clip, float d)
    {
        yield return new WaitForSeconds(d);
        audioSource.PlayOneShot(clip);
    }

    // Spawn specific FX with removetime - PPU 16 conversion
    GameObject SpawnFXWithReturn(GameObject prefab, Vector2 offset, Vector2 scale, float angle)
    {
        if (prefab == null) return null;
        float face = GetFacingSign();
        // PPU 16: MUGEN pixel units / 16 = Unity units
        Vector3 pos = transform.position + new Vector3(
            Mathf.Abs(offset.x) * face / 16f, // luôn đẩy về phía trước theo hướng mặt
            -offset.y / 16f,                   // MUGEN Y âm = lên -> Unity Y dương
            0f);
        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, 0, angle));
        go.transform.localScale = new Vector3(scale.x * face, scale.y, 1f);
        // Gắn auto-destroy nếu có script (không tạo dependency compile-time)
        System.Type adType = System.Type.GetType("AutoDestroyOnAnimationEnd") ?? System.Type.GetType("AutoDestroyOnAnimationEnd, Assembly-CSharp");
        if (adType != null && go.GetComponent(adType) == null)
            go.AddComponent(adType);
        return go;
    }

    void SpawnFX(GameObject prefab, Vector2 offset, Vector2 scale, float angle, float removeTime = 1.0f)
    {
        if (prefab == null) return;
        float face = GetFacingSign();
        // PPU 16: MUGEN pixel units / 16 = Unity units
        Vector3 pos = transform.position + new Vector3(
            Mathf.Abs(offset.x) * face / 16f,
            -offset.y / 16f,
            0f);
        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, 0, angle));
        go.transform.localScale = new Vector3(scale.x * face, scale.y, 1f);
        // Gắn auto-destroy nếu có
        System.Type adType = System.Type.GetType("AutoDestroyOnAnimationEnd") ?? System.Type.GetType("AutoDestroyOnAnimationEnd, Assembly-CSharp");
        if (adType != null && go.GetComponent(adType) == null)
            go.AddComponent(adType);

        // Track this FX for cleanup
        activeFX.Add(go);

        // Handle removetime (-2 = permanent until hit, positive = time in seconds)
        // Để FX tự hủy khi hết animation/particle; nếu cần, vẫn có thể ép thời gian bằng removeTime
        if (removeTime > 0f) StartCoroutine(RemoveFXAfterTime(go, removeTime));
        // If removeTime = -2, FX stays until removeongethit or skill ends
    }

    // Spawn slash FX at specific center position (slash zone) with wide spread
    void SpawnSlashFX(GameObject prefab, Vector3 center, Vector2 scale)
    {
        if (prefab == null) return;

        // Create FX around the slash zone center with WIDE spread for beautiful distribution
        float randX = Random.Range(-3.0f, 3.0f); // ±3 units around center - MUCH wider
        float randY = Random.Range(-1.5f, 1.5f); // ±1.5 unit vertically - wider spread

        Vector3 pos = center + new Vector3(randX, randY, 0f);

        // Random angle for natural slash effect
        float randAngle = Random.Range(0, 360);

        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, 0, randAngle));

        // Scale without facing direction (FX is at slash zone, not player)
        go.transform.localScale = new Vector3(scale.x, scale.y, 1f);

        // Track this FX for cleanup
        activeFX.Add(go);

        // Remove after short time
        StartCoroutine(RemoveFXAfterTime(go, 1.5f));
    }

    // Spawn random FX exactly like MUGEN specs with PPU 16
    void SpawnRandomFX(GameObject prefab, Vector2 scale)
    {
        if (prefab == null) return;

        // MUGEN: pos = (-50+Random%110),(-10-Random%80) with PPU 16 conversion
        float randX = (-50f + Random.Range(0, 110)) / 16f; // giữ đúng phân bố MUGEN (cả 2 phía)
        float randY = (10f + Random.Range(0, 80)) / 16f;   // MUGEN âm (lên) -> Unity dương

        float face = GetFacingSign();
        Vector3 basePos = GetLoantRamCenterPosition();
        Vector3 pos = basePos + new Vector3(randX * face, randY, 0f);
        lastFxSpawnPos = pos;

        // angle = random%360 - exact MUGEN implementation
        float randAngle = Random.Range(0, 360); // 0 to 359

        GameObject go = Instantiate(prefab, pos, Quaternion.Euler(0, 0, randAngle));

        // scale = .3,.3 - apply facing to scaleX
        go.transform.localScale = new Vector3(scale.x * face, scale.y, 1f);

        // Track this FX for cleanup
        activeFX.Add(go);

        // Không set thời gian thủ công; FX sẽ tự hủy khi animation/particle hết clip
        System.Type adType = System.Type.GetType("AutoDestroyOnAnimationEnd") ?? System.Type.GetType("AutoDestroyOnAnimationEnd, Assembly-CSharp");
        if (adType != null && go.GetComponent(adType) == null)
            go.AddComponent(adType);
    }

    System.Collections.IEnumerator RemoveFXAfterTime(GameObject fx, float time)
    {
        yield return new WaitForSeconds(time);
        if (fx != null)
        {
            activeFX.Remove(fx);
            Destroy(fx);
        }
    }

    // Gây damage mỗi 4 tick. Hitbox khớp với vùng spawn FX khi bật useFxAlignedHitBox.
    void ApplyAoEDamage()
    {
        Vector3 center = GetLoantRamCenterPosition();
        int mask = enemyLayers.value == 0 ? ~0 : enemyLayers.value;
        HashSet<Collider2D> targets = new HashSet<Collider2D>();

        if (useFxAlignedHitBox)
        {
            // FX spawn uses: X in [-50..+60] px, Y in [10..90] px (PPU16)
            // Convert to Unity units and compute a box centered at the rectangle midpoint.
            float face = GetFacingSign();
            Vector2 boxHalf = new Vector2(110f / 32f, 80f / 32f) * 0.5f * fxHitBoxScale; // (3.4375, 2.5) * scale
            Vector2 boxCenterOffset = new Vector2(10f / 32f * face, 100f / 32f);          // (0.3125*face, 3.125)
            Vector2 boxCenter = (Vector2)center + boxCenterOffset;
            Collider2D[] boxHits = Physics2D.OverlapBoxAll(boxCenter, boxHalf * 2f, 0f, mask);
            if (boxHits != null)
            {
                foreach (var c in boxHits)
                {
                    if (c != null && (enemyLayers.value != 0 ? true : c.CompareTag("Enemy"))) targets.Add(c);
                }
            }
        }

        // Fallback: circle to catch any misses if box was empty or toggle is off
        if (!useFxAlignedHitBox || targets.Count == 0)
        {
            Collider2D[] circleHits = Physics2D.OverlapCircleAll(center, loantRamRadius, mask);
            if (circleHits != null)
            {
                foreach (var c in circleHits)
                {
                    if (c != null && (enemyLayers.value != 0 ? true : c.CompareTag("Enemy"))) targets.Add(c);
                }
            }
        }

        if (targets.Count == 0) return;

        foreach (var c in targets)
        {
            SkillDamageUtility.ApplyDamageToTarget(c.gameObject, loantRamDamagePerTick, "SliceUp Loạn Trảm");
        }
    }

    // Tính tâm loạn trảm: ưu tiên enemy đầu tiên trúng + offset, nếu không có thì dùng SliceUpHitZone hoặc player pos
    Vector3 GetLoantRamCenterPosition()
    {
        float face = GetFacingSign();
        Vector3 basePos = loantRamCenterTransform != null
            ? loantRamCenterTransform.position
            : (SliceUpHitZone != null ? SliceUpHitZone.position : transform.position);
        Vector2 off = new Vector2(loantRamCenterOffset.x * face, loantRamCenterOffset.y);
        return basePos + new Vector3(off.x, off.y, 0f);
    }

    // Spawn FX ground ở vị trí tiếp đất gần vị trí refPos, đặt theo mặt hướng
    void SpawnGroundFXNear(Vector3 refPos, float forwardOffsetUnits, Vector2 scale)
    {
        if (fxGround == null) return;
        float face = GetFacingSign();
        Vector3 start = refPos + new Vector3(forwardOffsetUnits * face, 0.5f, 0f);
        Vector3 end = refPos + new Vector3(forwardOffsetUnits * face, -2f, 0f);
        int mask = groundLayers.value;
        if (pc != null) mask = pc.groundMask.value;
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, 4f, mask);
        Vector3 place = (hit.collider != null) ? (Vector3)hit.point : (refPos + new Vector3(forwardOffsetUnits * face, 0f, 0f));
        // Chuyển sang Offset PPU16 cho SpawnFX: ta muốn pos y theo ground (0 MUGEN), nên dùng offset y = 0
        // Dùng hàm SpawnFX theo offset MUGEN -> ta thay bằng Instantiate trực tiếp tại toạ độ tuyệt đối để chính xác mặt đất
        GameObject go = Instantiate(fxGround, place, Quaternion.identity);
        go.transform.localScale = new Vector3(scale.x * face, scale.y, 1f);
        activeFX.Add(go);
        System.Type adType = System.Type.GetType("AutoDestroyOnAnimationEnd") ?? System.Type.GetType("AutoDestroyOnAnimationEnd, Assembly-CSharp");
        if (adType != null && go.GetComponent(adType) == null)
            go.AddComponent(adType);
    }

    // Gizmos hỗ trợ debug vùng loạn trảm và probe dash
    void OnDrawGizmosSelected()
    {
        Vector3 center = (SliceUpHitZone != null) ? SliceUpHitZone.position : transform.position;
        if (useFxAlignedHitBox)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            float faceG = (transform != null && transform.localScale.x >= 0f) ? 1f : -1f;
            Vector2 boxHalf = new Vector2(110f / 32f, 80f / 32f) * 0.5f * fxHitBoxScale;
            Vector2 boxCenterOffset = new Vector2(10f / 32f * faceG, 100f / 32f);
            Vector3 boxCenter = center + new Vector3(boxCenterOffset.x, boxCenterOffset.y, 0f);
            Vector3 size = new Vector3(boxHalf.x * 2f, boxHalf.y * 2f, 0f);
            Gizmos.DrawWireCube(boxCenter, size);
        }
        else
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(center, loantRamRadius);
        }
        // dash probe (approximate)
        float face = (transform != null && transform.localScale.x >= 0f) ? 1f : -1f;
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
        Vector3 probe = ((Application.isPlaying ? transform.position : center) + new Vector3(face * 0.6f, 0f, 0f));
        Gizmos.DrawWireSphere(probe, dashHitRadius);
    }
}
