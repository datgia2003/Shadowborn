using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;


[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    // Khóa toàn bộ attack/skill khi intro/cutscene
    public bool isIntroLock = false;

    // ===== FX/Sound Struct & Arrays =====
    [System.Serializable]
    public class AttackFX
    {
        public GameObject fxPrefab;
        public Vector3 offset = Vector3.zero; // offset vị trí spawn (áp dụng từ pos trong mugen)
        public Vector2 scale = Vector2.one;   // scale (áp dụng từ scale trong mugen)
        public float angle = 0f;              // góc xoay (áp dụng từ angle trong mugen)
    }

    [Header("Light Attack Punch FX/Sound Array")]
    public AttackFX[] lightPunchFXs;   // FX đấm cho từng light
    public AudioClip[] lightPunchSounds;
    [Header("Light Punch Points (anchor từng đòn)")]
    public Transform[] punchPointsLight; // Anchor cho từng light punch (L1, L2...)
    [Header("Light Attack Dust FX/Sound Array")]
    public AttackFX[] lightDustFXs;    // FX bụi cho từng light
    public AudioClip[] lightDustSounds;
    [Header("Light Dust Points (anchor FX bụi từng đòn)")]
    public Transform[] dustPointsLight; // Anchor cho FX bụi từng light

    [Header("Heavy Attack Punch FX/Sound Array")]
    public AttackFX[] heavyPunchFXs;   // FX đấm cho từng heavy
    public AudioClip[] heavyPunchSounds;
    [Header("Heavy Punch Points (anchor từng đòn)")]
    public Transform[] punchPointsHeavy; // Anchor cho từng heavy punch (H1, H2...)
    [Header("Heavy Attack Dust FX/Sound Array")]
    public AttackFX[] heavyDustFXs;    // FX bụi cho từng heavy
    public AudioClip[] heavyDustSounds;
    [Header("Heavy Dust Points (anchor FX bụi từng đòn)")]
    public Transform[] dustPointsHeavy; // Anchor cho FX bụi từng heavy

    // [Header("Light Attack FX/Sound Array")]
    // public AttackFX[] lightSwingFXs;    // FX cho từng light combo step
    // public AudioClip[] lightSwingSounds;
    // [Header("Heavy Attack FX/Sound Array")]
    // public AttackFX[] heavySwingFXs;    // FX cho từng heavy combo step
    // public AudioClip[] heavySwingSounds;

    [Header("Light Hitboxes")] public Hitbox[] lightHits;
    [Header("Heavy Hitboxes")] public Hitbox[] heavyHits;

    // [Header("FX & SFX")] public GameObject[] fxLightPrefabs;
    // public AudioClip[] sfxLightClips;
    // public GameObject[] fxHeavyPrefabs;
    // public AudioClip[] sfxHeavyClips;

    [Header("Combo Settings")]
    public float comboResetTime = 0.6f;
    public float minComboDelay = 0.3f;
    public float bufferDuration = 0.2f;
    public int maxLightCombo = 6;
    public int maxHeavyCombo = 5;
    public float comboLockDuration = 0.4f;

    [Header("Attack Speed Buff")]
    [Tooltip("Multiplier applied to Animator.speed during attacks")] public float attackSpeed = 1f;

    [Header("Hit Stop")]
    public float hitStopTime = 0.03f; // Rất ngắn - chỉ 1-2 frames như game AAA

    Animator anim;
    PlayerController controller;
    AudioSource audioSource;

    bool canChain;
    bool comboLocked;
    bool isAirAttack = false; // trạng thái đang tấn công (dùng cho anim)
    float lastAttackTime;
    float lastComboStartTime;
    float nextComboAllowedTime = 0.4f;

    public enum AttackType { None = 0, Light = 1, Heavy = 2 }
    AttackType currentType = AttackType.None;
    int currentIndex = 0;
    int comboCount = 0;

    AttackType bufferedType = AttackType.None;
    float bufferedTime;


    [Header("Rock Debris FX Prefab (dùng cho đòn đặc biệt)")]
    public GameObject rockDebrisPrefab;
    [Header("Rock Debris Anchor Points (giống dust)")]
    public Transform[] rockDebrisPoints;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        audioSource = gameObject.AddComponent<AudioSource>();

        // Setup enhanced hit parameters cho meaty hit feeling
        SetupEnhancedHitParameters();
    }

    void SetupEnhancedHitParameters()
    {
        // Light attacks: hit pull vừa + knockback để giữ khoảng cách
        foreach (var hitbox in lightHits)
        {
            if (hitbox != null)
            {
                hitbox.knockbackForce = 8f;      // TĂNG knockback để đẩy enemy ra
                hitbox.hitPullStrength = 7f;     // GIẢM hit pull để không sát quá
            }
        }

        // Heavy attacks: knockback mạnh + hit pull vừa phải
        foreach (var hitbox in heavyHits)
        {
            if (hitbox != null)
            {
                hitbox.knockbackForce = 12f;     // knockback mạnh để tạo khoảng cách
                hitbox.hitPullStrength = 10f;    // GIẢM hit pull cho heavy
            }
        }

        Debug.Log("Balanced hit parameters - pull to range + push for spacing!");
    }

    void OnLight(InputValue v)
    {
        if (isIntroLock) return;
        if (!v.isPressed) return;
        // Chỉ cho phép light attack khi đang trên mặt đất
        if (!IsGrounded()) return;
        BufferAttack(AttackType.Light);
    }

    void OnHeavy(InputValue v)
    {
        if (isIntroLock) return;
        if (!v.isPressed) return;
        // Đòn heavy 5 (maxHeavyCombo) chỉ cho phép khi đang ở trên không
        // Các đòn heavy khác chỉ cho phép khi đang trên mặt đất
        if (comboCount == maxHeavyCombo - 1) // chuẩn bị đánh heavy 5
        {
            if (IsGrounded()) return; // phải ở trên không mới được đánh heavy 5
        }
        else
        {
            if (!IsGrounded()) return; // các đòn heavy khác chỉ trên mặt đất
        }
        BufferAttack(AttackType.Heavy);
    }

    void BufferAttack(AttackType type)

    {
        if (isIntroLock) return;
        if (comboLocked) return;
        if (Time.time < nextComboAllowedTime) return;

        float timeSinceLast = Time.time - lastComboStartTime;
        if (comboCount == 0 && timeSinceLast < minComboDelay) return;

        // Nếu đang không trong attack animation thì đánh luôn
        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (!state.IsTag("Attack"))
        {
            TryAttack(type);
            return;
        }

        bufferedType = type;
        bufferedTime = Time.time;
    }

    // Track previous grounded state to detect landing
    bool wasGrounded = true;
    void Update()
    {
        if (comboLocked) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        bool hasBuffer = bufferedType != AttackType.None && Time.time - bufferedTime <= bufferDuration;
        bool readyToChain = canChain && state.IsTag("Attack") && state.normalizedTime >= 0.5f && state.normalizedTime < 1f;

        if (hasBuffer && readyToChain)
        {
            TryAttack(bufferedType);
            bufferedType = AttackType.None;
        }

        if (comboCount > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            comboCount = 0;
            currentType = AttackType.None;
            currentIndex = 0;
            anim.SetInteger("ComboType", 0);
            anim.SetInteger("ComboIndex", 0);
        }

        // Reset isAirAttack when landing
        bool grounded = IsGrounded();
        if (!wasGrounded && grounded)
        {
            if (isAirAttack)
            {
                isAirAttack = false;
                anim.SetBool("IsAirAttack", false);
            }
        }
        wasGrounded = grounded;
    }

    void TryAttack(AttackType type)
    {
        if (isIntroLock) return;

        int maxCombo = (type == AttackType.Light) ? maxLightCombo : maxHeavyCombo;
        if (comboCount >= maxCombo) return;

        StartAttack(type);
    }

    void StartAttack(AttackType type)
    {
        controller.canMove = false;
        // Chỉ set isAirAttack = true nếu đang ở trên không khi bắt đầu attack
        isAirAttack = !IsGrounded();

        comboCount++;
        currentType = type;
        currentIndex = (type == AttackType.Light) ? ((comboCount - 1) % maxLightCombo) + 1 : ((comboCount - 1) % maxHeavyCombo) + 1;

        lastAttackTime = Time.time;
        lastComboStartTime = Time.time;
        canChain = false;

        //anim.ResetTrigger("AttackTrigger");
        anim.SetInteger("ComboType", (int)currentType);
        anim.SetInteger("ComboIndex", currentIndex);
        anim.SetBool("IsAirAttack", isAirAttack);
        anim.SetTrigger("AttackTrigger");

        anim.speed = attackSpeed;

        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float slide = 0.3f; // Slide nhẹ khi attack - tự nhiên hơn

        // Heavy attacks slide mạnh hơn light
        if (type == AttackType.Heavy)
            slide = 0.5f;

        // Nếu là đòn cuối của light combo thì lướt xa hơn một chút
        if (type == AttackType.Light && currentIndex == maxLightCombo)
            slide = 0.6f; // Đòn cuối lướt xa hơn một chút

        // Đòn thứ 4 của heavy attack: hất enemy lên và nhân vật nhảy lên
        if (type == AttackType.Heavy && currentIndex == 4)
        {
            LaunchEnemiesUpward();
            JumpUpward();
        }

        // Đòn cuối của heavy attack: chỉ cho phép khi ở trên không, đánh enemy bay chéo xuống
        if (type == AttackType.Heavy && currentIndex == maxHeavyCombo)
        {
            if (IsGrounded())
            {
                // Nếu đang trên mặt đất thì không cho đánh đòn này
                comboCount--;
                isAirAttack = false;
                return;
            }
            KnockdownEnemiesDiagonal();
        }

        // Slide nhẹ khi attack - natural movement
        transform.position += (Vector3)(dir * slide);

        if ((type == AttackType.Light && currentIndex == maxLightCombo) ||
            (type == AttackType.Heavy && currentIndex == maxHeavyCombo))
        {
            nextComboAllowedTime = Time.time + comboLockDuration;
            StartCoroutine(LockComboCooldown());
        }
    }

    // ====== EFFECTS FOR SPECIAL HEAVY ATTACKS ======
    void LaunchEnemiesUpward()
    {
        // TODO: Gọi hàm hất tung enemy trong vùng hitbox
        // Ví dụ: foreach (var enemy in GetEnemiesInHitbox()) enemy.LaunchUp();
    }

    void JumpUpward()
    {
        // Nếu có Rigidbody2D, set vận tốc nhảy lên
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, controller.jumpForce); // chỉnh lực nhảy nếu cần
        }
        else
        {
            // Hoặc trigger animation nhảy lên nếu không dùng Rigidbody2D
            anim.SetTrigger("JumpUp");
        }
        // Sau khi nhảy lên bằng heavy 4, set lại trạng thái attack trên không và trigger anim
        isAirAttack = true;
        anim.SetBool("IsAirAttack", true);
        anim.ResetTrigger("AttackTrigger");
        anim.SetInteger("ComboType", (int)AttackType.Heavy);
        anim.SetInteger("ComboIndex", 4);
        anim.SetTrigger("AttackTrigger");
    }

    void KnockdownEnemiesDiagonal()
    {
        // TODO: Gọi hàm đánh văng enemy chéo xuống đất
        // Ví dụ: foreach (var enemy in GetEnemiesInHitbox()) enemy.KnockdownDiagonal(transform.localScale.x);
    }

    bool IsGrounded()
    {
        // Sử dụng biến isGrounded trong PlayerController
        if (controller != null)
            return controller.isGrounded;
        return true;
    }

    IEnumerator LockComboCooldown()
    {
        comboLocked = true;
        yield return new WaitForSeconds(comboLockDuration);
        comboLocked = false;
    }

    public void ComboWindowOpen() => canChain = true;
    public void ComboWindowClose() => canChain = false;

    public void AttackEnd()
    {
        if (isIntroLock) return;
        controller.canMove = true;
        canChain = false;
        DisableAllHits();
        anim.speed = 1f;
        if (IsGrounded())
        {
            isAirAttack = false;
            anim.SetBool("IsAirAttack", false);
        }
        // Nếu vẫn ở trên không, giữ nguyên isAirAttack = true
    }

    public void EnableHitByStep()
    {
        DisableAllHits();
        var arr = (currentType == AttackType.Light) ? lightHits : heavyHits;
        int idx = currentIndex - 1;
        if (idx >= 0 && idx < arr.Length) arr[idx].gameObject.SetActive(true);
    }

    public void DisableAllHits()
    {
        foreach (var h in lightHits) h.gameObject.SetActive(false);
        foreach (var h in heavyHits) h.gameObject.SetActive(false);
    }

    // Coroutine hit stop (pause game ngắn) với SUBTLE EFFECTS
    IEnumerator HitStopCoroutine()
    {
        // WORLD HITSTOP cực ngắn
        Time.timeScale = 0f;

        // Camera shake NHẸ trong hitstop
        var virtualCam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (virtualCam != null)
        {
            var noise = virtualCam.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                float originalAmplitude = noise.m_AmplitudeGain;
                noise.m_AmplitudeGain = 1.2f; // GIẢM camera shake từ 2.5f → 1.2f

                yield return new WaitForSecondsRealtime(hitStopTime);

                // Restore camera
                noise.m_AmplitudeGain = originalAmplitude;
            }
            else
            {
                yield return new WaitForSecondsRealtime(hitStopTime);
            }
        }
        else
        {
            yield return new WaitForSecondsRealtime(hitStopTime);
        }

        Time.timeScale = 1f;
    }

    // Gọi hàm này từ Animation Event để xử lý hiệu ứng phụ (hit stop, FX, SFX...)
    public void AttackHitboxCheck()
    {
        if (isIntroLock) return;
        // Không cần gọi PlayAttackFX/Sfx nữa, đã xử lý trong Hitbox.cs

        // Tạo hit stop ngắn (pause game 1-2 frames)
        if (hitStopTime > 0f)
        {
            StartCoroutine(HitStopCoroutine());
        }

        // Thêm player recoil khi hit để tạo cảm giác phản lực
        StartCoroutine(PlayerRecoilCoroutine());
    }

    // Player recoil - phản lực nhẹ khi đánh trúng
    IEnumerator PlayerRecoilCoroutine()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Recoil ngược chiều đánh
            Vector2 recoilDirection = transform.localScale.x > 0 ? Vector2.left : Vector2.right;
            float recoilForce = (currentType == AttackType.Heavy) ? 3f : 1.5f; // Heavy mạnh hơn

            // Apply recoil ngắn
            Vector2 originalVel = rb.velocity;
            rb.velocity = new Vector2(recoilDirection.x * recoilForce, rb.velocity.y);

            // Restore sau 0.1s
            yield return new WaitForSeconds(0.1f);
            rb.velocity = new Vector2(originalVel.x * 0.8f, rb.velocity.y); // Giảm momentum nhẹ
        }
    }

    // Gọi từ Animation Event: truyền AttackType (Light/Heavy) và index (0 = L1/H1, 1 = L2/H2, ...)
    // public void PlaySwingFXByTypeAndIndex(AttackType type, int idx)
    // {
    //     AttackFX[] fxArr = (type == AttackType.Light) ? lightSwingFXs : heavySwingFXs;
    //     AudioClip[] sndArr = (type == AttackType.Light) ? lightSwingSounds : heavySwingSounds;
    //     if (fxArr != null && idx >= 0 && idx < fxArr.Length && fxArr[idx] != null && fxArr[idx].fxPrefab != null)
    //     {
    //         var fx = fxArr[idx];
    //         FXSpawner.Spawn(fx.fxPrefab, transform.position + fx.offset, fx.scale, fx.angle);
    //     }
    //     if (sndArr != null && idx >= 0 && idx < sndArr.Length && sndArr[idx] != null && audioSource != null)
    //         audioSource.PlayOneShot(sndArr[idx]);
    // }

    // ===== FX/Sound Callbacks for Animation Event =====
    // Gọi từ Animation Event: FX/sound đấm cho từng light step
    public void PlayLightPunchFX(int idx)
    {
        if (lightPunchFXs != null && idx >= 0 && idx < lightPunchFXs.Length && lightPunchFXs[idx].fxPrefab != null)
        {
            var fx = lightPunchFXs[idx];
            Vector3 spawnPos = (punchPointsLight != null && idx < punchPointsLight.Length && punchPointsLight[idx] != null)
                ? punchPointsLight[idx].position + fx.offset
                : transform.position + fx.offset;
            bool flipX = (transform.localScale.x < 0);
            FXSpawner.Spawn(fx.fxPrefab, spawnPos, fx.scale, fx.angle, flipX);
        }
        if (lightPunchSounds != null && idx >= 0 && idx < lightPunchSounds.Length && lightPunchSounds[idx] != null && audioSource != null)
            audioSource.PlayOneShot(lightPunchSounds[idx]);
    }

    // Gọi từ Animation Event: FX/sound bụi cho từng light step
    public void PlayLightDustFX(int idx)
    {
        if (isIntroLock) return;
        if (isIntroLock) return;
        if (lightDustFXs != null && idx >= 0 && idx < lightDustFXs.Length && lightDustFXs[idx].fxPrefab != null)
        {
            var fx = lightDustFXs[idx];
            bool flipX = (transform.localScale.x < 0);
            Vector3 offset = fx.offset;
            Vector2 fxScale = fx.scale;
            if (flipX)
            {
                offset.x *= -1;
                fxScale.x *= -1;
            }
            Vector3 spawnPos = (dustPointsLight != null && idx < dustPointsLight.Length && dustPointsLight[idx] != null)
                ? dustPointsLight[idx].position + offset
                : transform.position + offset;
            FXSpawner.Spawn(fx.fxPrefab, spawnPos, fxScale, fx.angle, flipX);
        }
        if (lightDustSounds != null && idx >= 0 && idx < lightDustSounds.Length && lightDustSounds[idx] != null && audioSource != null)
            audioSource.PlayOneShot(lightDustSounds[idx]);
    }

    // Gọi từ Animation Event: FX/sound đấm cho từng heavy step
    public void PlayHeavyPunchFX(int idx)
    {
        if (isIntroLock) return;
        if (heavyPunchFXs != null && idx >= 0 && idx < heavyPunchFXs.Length && heavyPunchFXs[idx].fxPrefab != null)
        {
            var fx = heavyPunchFXs[idx];
            bool flipX = (transform.localScale.x < 0);
            Vector3 offset = fx.offset;
            Vector2 fxScale = fx.scale;
            if (flipX)
            {
                offset.x *= -1;
                fxScale.x *= -1;
            }
            Vector3 spawnPos = (punchPointsHeavy != null && idx < punchPointsHeavy.Length && punchPointsHeavy[idx] != null)
                ? punchPointsHeavy[idx].position + offset
                : transform.position + offset;
            FXSpawner.Spawn(fx.fxPrefab, spawnPos, fxScale, fx.angle, flipX);
        }
        if (heavyPunchSounds != null && idx >= 0 && idx < heavyPunchSounds.Length && heavyPunchSounds[idx] != null && audioSource != null)
            audioSource.PlayOneShot(heavyPunchSounds[idx]);
    }

    // Gọi từ Animation Event: FX/sound bụi cho từng heavy step
    public void PlayHeavyDustFX(int idx)
    {
        if (isIntroLock) return;
        if (heavyDustFXs != null && idx >= 0 && idx < heavyDustFXs.Length && heavyDustFXs[idx].fxPrefab != null)
        {
            var fx = heavyDustFXs[idx];
            bool flipX = (transform.localScale.x < 0);
            Vector3 offset = fx.offset;
            Vector2 fxScale = fx.scale;
            if (flipX)
            {
                offset.x *= -1;
                fxScale.x *= -1;
            }
            Vector3 spawnPos = (dustPointsHeavy != null && idx < dustPointsHeavy.Length && dustPointsHeavy[idx] != null)
                ? dustPointsHeavy[idx].position + offset
                : transform.position + offset;
            FXSpawner.Spawn(fx.fxPrefab, spawnPos, fxScale, fx.angle, flipX);
        }
        if (heavyDustSounds != null && idx >= 0 && idx < heavyDustSounds.Length && heavyDustSounds[idx] != null && audioSource != null)
            audioSource.PlayOneShot(heavyDustSounds[idx]);
    }


    // Gọi từ Animation Event: spawn hiệu ứng đá văng (rock debris) tại anchor point riêng (rockDebrisPoints)
    // Nếu không truyền idx, mặc định là 0
    public void PlayRockDebrisFX(int idx = 0)
    {
        if (rockDebrisPrefab == null) return;
        bool flipX = (transform.localScale.x < 0);
        Vector3 basePos;
        if (rockDebrisPoints != null && idx >= 0 && idx < rockDebrisPoints.Length && rockDebrisPoints[idx] != null)
            basePos = rockDebrisPoints[idx].position;
        else
            basePos = transform.position;

        int rockCount = Random.Range(5, 9); // 5-8 rocks
        for (int i = 0; i < rockCount; i++)
        {
            // Random offset for each rock
            Vector3 offset = new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0f, 0.25f),
                0f);
            var fx = Instantiate(rockDebrisPrefab, basePos + offset, Quaternion.identity);
            fx.GetComponent<RockDebris>()?.Init(null); // scale mặc định bên RockDebris
            if (flipX)
            {
                var sr = fx.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.flipX = !sr.flipX;
                var local = fx.transform.localScale;
                local.x *= -1;
                fx.transform.localScale = local;
            }
        }
    }
}



