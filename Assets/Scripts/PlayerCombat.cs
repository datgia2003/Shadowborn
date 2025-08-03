// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(Animator))]
// public class PlayerCombat : MonoBehaviour
// {
//     [Header("Hitboxes")]
//     public Hitbox hitL1; // gán child Hit_L1
//     public Hitbox hitL2; // gán child Hit_L2
//     public Hitbox hitL3; // gán child Hit_L3

//     List<Hitbox> _allHits;

//     [Header("Combo")]
//     [Tooltip("Thời gian cho phép nối đòn tiếp (giây)")]
//     public float comboWindow = 0.25f; // khoảng người chơi có thể bấm tiếp để nối
//     [Tooltip("Reset combo nếu không bấm tiếp trong thời gian này (giây)")]
//     public float comboResetTime = 0.6f;

//     Animator _anim;
//     int _comboStep = 0; // 0 = idle, 1..3 là đang ở đòn số mấy
//     bool _canChain = false; // đang mở cửa sổ cho phép nối đòn
//     float _lastAttackTime;

//     void Awake()
//     {
//         _anim = GetComponent<Animator>();
//         // Thu thập tất cả hitbox 1 lần
//         _allHits = new List<Hitbox>();
//         if (hitL1) _allHits.Add(hitL1);
//         if (hitL2) _allHits.Add(hitL2);
//         if (hitL3) _allHits.Add(hitL3);
//         DisableAllHits();
//     }

//     // Input System (PlayerInput - Send Messages)
//     void OnLight()
//     {
//         // Nếu combo đã hết hạn, bắt đầu lại từ L1
//         if (Time.time - _lastAttackTime > comboResetTime)
//         {
//             StartAttack(1);
//             return;
//         }

//         // Đang trong combo, thử nối nếu cửa sổ mở
//         if (_canChain)
//         {
//             if (_comboStep == 1) StartAttack(2);
//             else if (_comboStep == 2) StartAttack(3);
//             // nếu đã tới 3 thì bỏ qua
//         }
//     }

//     void StartAttack(int step)
//     {
//         _comboStep = Mathf.Clamp(step, 1, 3);
//         _lastAttackTime = Time.time;
//         _canChain = false;

//         // LỚP 1: luôn tắt tất cả hitbox trước khi vào đòn mới
//         DisableAllHits();

//         _anim.ResetTrigger("LightTrigger");
//         _anim.SetInteger("ComboStep", _comboStep);
//         _anim.SetTrigger("LightTrigger");
//     }

//     // ---- Helpers ----
//     public void DisableAllHits()
//     {
//         if (_allHits == null) return;
//         for (int i = 0; i < _allHits.Count; i++)
//             if (_allHits[i]) _allHits[i].gameObject.SetActive(false);
//     }

//     // LỚP 2: Enable theo cơ chế “độc chiếm”: mỗi lần bật đòn mới → tắt hết rồi mới bật
//     public void EnableHitByStep(int step)
//     {
//         DisableAllHits();
//         switch (step)
//         {
//             case 1: if (hitL1) hitL1.gameObject.SetActive(true); break;
//             case 2: if (hitL2) hitL2.gameObject.SetActive(true); break;
//             case 3: if (hitL3) hitL3.gameObject.SetActive(true); break;
//         }
//     }

//     public void DisableHitByStep(int step)
//     {
//         switch (step)
//         {
//             case 1: if (hitL1) hitL1.gameObject.SetActive(false); break;
//             case 2: if (hitL2) hitL2.gameObject.SetActive(false); break;
//             case 3: if (hitL3) hitL3.gameObject.SetActive(false); break;
//         }
//     }

//     // ====== Animation Events (gọi từ clip) ======
//     // Mở/đóng cửa sổ nối combo
//     public void ComboWindowOpen() { _canChain = true; }
//     public void ComboWindowClose() { _canChain = false; }

//     // Kết thúc đòn đánh (cuối mỗi clip)
//     public void AttackEnd()
//     {
//         // Nếu người chơi không nối tiếp (không bấm trong cửa sổ), reset combo
//         if (Time.time - _lastAttackTime > comboWindow)
//         {
//             _comboStep = 0;
//             _anim.SetInteger("ComboStep", 0);
//         }
//     }

//     // Bật/tắt hitbox theo từng đòn
//     public void EnableHit_L1() { if (hitL1) hitL1.gameObject.SetActive(true); }
//     public void DisableHit_L1() { if (hitL1) hitL1.gameObject.SetActive(false); }
//     public void EnableHit_L2() { if (hitL2) hitL2.gameObject.SetActive(true); }
//     public void DisableHit_L2() { if (hitL2) hitL2.gameObject.SetActive(false); }
//     public void EnableHit_L3() { if (hitL3) hitL3.gameObject.SetActive(true); }
//     public void DisableHit_L3() { if (hitL3) hitL3.gameObject.SetActive(false); }
// }



// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(Animator))]
// public class PlayerCombat : MonoBehaviour
// {
//     [Header("Light Hitboxes")]
//     public Hitbox hitL1;
//     public Hitbox hitL2;
//     public Hitbox hitL3;
//     public Hitbox hitL4;

//     [Header("Heavy Hitboxes")]
//     public Hitbox hitH1;
//     public Hitbox hitH2;

//     [Header("Combo Settings")]
//     public float comboChainWindow = 0.25f;
//     public float comboResetTime = 0.6f;
//     public float slideDistance = 0.5f;

//     Animator anim;
//     PlayerController controller;
//     List<Hitbox> allHits;
//     int comboStep;
//     bool canChain;
//     float lastAttackTime;

//     void Awake()
//     {
//         anim = GetComponent<Animator>();
//         controller = GetComponent<PlayerController>();
//         allHits = new List<Hitbox> { hitL1, hitL2, hitL3, hitL4 };
//         DisableAllHits();
//     }

//     void OnLight(InputValue v)
//     {
//         if (!v.isPressed) return;
//         if (Time.time - lastAttackTime > comboResetTime)
//             StartAttack(1);
//         else if (canChain)
//             StartAttack(comboStep < 4 ? comboStep + 1 : 1);
//     }

//     void StartAttack(int step)
//     {
//         // Lock movement
//         controller.canMove = false;

//         comboStep = step;
//         lastAttackTime = Time.time;
//         canChain = false;
//         DisableAllHits();

//         anim.ResetTrigger("LightTrigger");
//         anim.SetInteger("ComboStep", comboStep);
//         anim.SetTrigger("LightTrigger");

//         // Slide move
//         Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
//         transform.position += (Vector3)(dir * slideDistance);
//     }

//     // Animation Events
//     public void ComboWindowOpen() => canChain = true;
//     public void ComboWindowClose() => canChain = false;

//     public void AttackEnd()
//     {
//         // Unlock movement
//         controller.canMove = true;

//         if (Time.time - lastAttackTime > comboChainWindow)
//         {
//             comboStep = 0;
//             anim.SetInteger("ComboStep", 0);
//         }
//     }

//     public void EnableHitByStep()
//     {
//         DisableAllHits();
//         switch (comboStep)
//         {
//             case 1: hitL1?.gameObject.SetActive(true); break;
//             case 2: hitL2?.gameObject.SetActive(true); break;
//             case 3: hitL3?.gameObject.SetActive(true); break;
//             case 4: hitL4?.gameObject.SetActive(true); break;
//         }
//     }

//     public void DisableAllHits()
//     {
//         foreach (var h in allHits)
//             h?.gameObject.SetActive(false);
//     }
// }

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Light Hitboxes")] public Hitbox[] lightHits;
    [Header("Heavy Hitboxes")] public Hitbox[] heavyHits;

    [Header("FX & SFX")] public GameObject[] fxLightPrefabs;
    public AudioClip[] sfxLightClips;
    public GameObject[] fxHeavyPrefabs;
    public AudioClip[] sfxHeavyClips;

    [Header("Combo Settings")]
    public float comboResetTime = 0.6f;
    public float minComboDelay = 0.3f;
    public float bufferDuration = 0.2f;
    public int maxLightCombo = 6;
    public int maxHeavyCombo = 5;
    public float comboLockDuration = 0.4f;

    [Header("Attack Speed Buff")]
    [Tooltip("Multiplier applied to Animator.speed during attacks")] public float attackSpeed = 1f;

    Animator anim;
    PlayerController controller;
    AudioSource audioSource;

    bool canChain;
    bool comboLocked;
    float lastAttackTime;
    float lastComboStartTime;
    float nextComboAllowedTime = 0.4f;

    enum AttackType { None = 0, Light = 1, Heavy = 2 }
    AttackType currentType = AttackType.None;
    int currentIndex = 0;
    int comboCount = 0;

    AttackType bufferedType = AttackType.None;
    float bufferedTime;

    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnLight(InputValue v)
    {
        if (!v.isPressed) return;
        BufferAttack(AttackType.Light);
    }

    void OnHeavy(InputValue v)
    {
        if (!v.isPressed) return;
        BufferAttack(AttackType.Heavy);
    }

    void BufferAttack(AttackType type)
    {
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

    void Update()
    {
        if (comboLocked) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        bool hasBuffer = bufferedType != AttackType.None && Time.time - bufferedTime <= bufferDuration;
        bool readyToChain = canChain && state.IsTag("Attack") && state.normalizedTime >= 0.65f && state.normalizedTime < 1f;

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
    }

    void TryAttack(AttackType type)
    {
        int maxCombo = (type == AttackType.Light) ? maxLightCombo : maxHeavyCombo;
        if (comboCount >= maxCombo) return;

        StartAttack(type);
    }

    void StartAttack(AttackType type)
    {
        controller.canMove = false;

        comboCount++;
        currentType = type;
        currentIndex = (type == AttackType.Light) ? ((comboCount - 1) % maxLightCombo) + 1 : ((comboCount - 1) % maxHeavyCombo) + 1;

        lastAttackTime = Time.time;
        lastComboStartTime = Time.time;
        canChain = false;

        anim.ResetTrigger("AttackTrigger");
        anim.SetInteger("ComboType", (int)currentType);
        anim.SetInteger("ComboIndex", currentIndex);
        anim.SetTrigger("AttackTrigger");

        anim.speed = attackSpeed;

        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        transform.position += (Vector3)(dir * 0.5f);

        if ((type == AttackType.Light && currentIndex == maxLightCombo) ||
            (type == AttackType.Heavy && currentIndex == maxHeavyCombo))
        {
            nextComboAllowedTime = Time.time + comboLockDuration;
            StartCoroutine(LockComboCooldown());
        }
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
        controller.canMove = true;
        canChain = false;
        DisableAllHits();
        anim.speed = 1f;

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

    public void PlayAttackFX()
    {
        var arr = (currentType == AttackType.Light) ? fxLightPrefabs : fxHeavyPrefabs;
        int idx = currentIndex - 1;
        if (idx >= 0 && idx < arr.Length) Instantiate(arr[idx], transform.position, Quaternion.identity);
    }

    public void PlayAttackSfx()
    {
        var arr = (currentType == AttackType.Light) ? sfxLightClips : sfxHeavyClips;
        int idx = currentIndex - 1;
        if (idx >= 0 && idx < arr.Length) audioSource.PlayOneShot(arr[idx]);
    }

    public void HitPauseFrame() => StartCoroutine(DoHitPause());
    IEnumerator DoHitPause()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.05f);
        Time.timeScale = 1f;
    }
}






