using UnityEngine;
using UnityEngine.InputSystem;

public class SummonSkill : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fxExplod1;     // hiệu ứng ánh sáng nhỏ (anim 6218)
    public GameObject fxExplod2;     // hiệu ứng lâu dài (anim 6220)
    public GameObject fxActivation;  // helper FX30910 (hiệu ứng kích hoạt skill)
    public GameObject minionPrefab;  // prefab minion caster

    [Header("Audio")]
    public AudioClip castSound;   // S950,1
    public AudioClip voiceSound;  // S0,11
    public AudioSource audioSource;

    [Header("Settings")]
    public Transform spawnPoint;

    private Animator animator;
    private bool isCasting = false;
    private float castTimer = 0f;
    private bool hasSpawnedFrame2 = false;
    private bool hasSpawnedFrame3 = false;
    private bool hasPlayedActivation = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isCasting)
        {
            castTimer += Time.deltaTime;

            // Frame 2 (khoảng 2/60 = 0.033s) - spawn minion đầu tiên
            if (!hasSpawnedFrame2 && castTimer >= 2f / 60f)
            {
                hasSpawnedFrame2 = true;
                OnSummonFrame2();
            }

            // Frame 3 (khoảng 8/60 = 0.133s) - spawn minion thứ hai (delay lâu hơn cho sống động)
            if (!hasSpawnedFrame3 && castTimer >= 8f / 60f)
            {
                hasSpawnedFrame3 = true;
                OnSummonFrame3();
            }

            // Kết thúc cast sau 2 giây
            if (castTimer >= 2f)
            {
                OnCastEnd();
            }
        }
    }

    /// <summary>
    /// ❌ DISABLED: Direct input bypass - Skills should go through UI system only!
    /// This method was causing skills to execute without mana/cooldown checks
    /// </summary>
    /*
    public void OnSummonSkill(InputValue value)
    {
        if (value.isPressed && !isCasting)
        {
            StartCasting();
        }
    }
    */

    /// <summary>
    /// Public method for UI system to trigger the skill
    /// </summary>
    public void PlaySkill()
    {
        if (!isCasting)
        {
            StartCasting();
        }
    }

    private void StartCasting()
    {
        isCasting = true;
        castTimer = 0f;
        hasSpawnedFrame2 = false;
        hasSpawnedFrame3 = false;
        hasPlayedActivation = false;

        // Ngừng animation hiện tại và chơi SummonSkill từ đầu
        if (animator)
        {
            animator.StopPlayback();
            animator.Play("SummonSkill", 0, 0f); // force từ frame 0
        }

        // Gọi trực tiếp activation effect (chỉ 1 lần)
        OnCastStart();
    }

    // ====================== Animation Events ======================

    /// <summary>
    /// Frame 0 trong MUGEN → activation effect một lần duy nhất
    /// </summary>
    public void OnCastStart()
    {
        if (hasPlayedActivation) return; // Đảm bảo chỉ chạy 1 lần
        hasPlayedActivation = true;

        // Helper FX30910 → hiệu ứng kích hoạt skill (1 lần duy nhất)
        if (fxActivation)
        {
            GameObject fxAct = Instantiate(fxActivation, spawnPoint.position, Quaternion.identity);
            fxAct.transform.SetParent(null);
            fxAct.transform.localScale = new Vector3(0.2f, 0.2f, 1); // scale 0.2,0.2

            // Tự động destroy với AutoDestroyOnAnimationEnd
            var autoDestroy = fxAct.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
                autoDestroy = fxAct.AddComponent<AutoDestroyOnAnimationEnd>();
        }

        // Âm thanh
        if (audioSource)
        {
            if (castSound) audioSource.PlayOneShot(castSound);
            if (voiceSound)
            {
                audioSource.PlayOneShot(voiceSound);
                audioSource.PlayOneShot(voiceSound);
                audioSource.PlayOneShot(voiceSound);
            }
        }
    }

    public void OnSummonFrame2()
    {
        // Spawn minion đầu tiên phía sau player (gần hơn, lệch trái)
        float facingDirection = GetFacingDirection();
        Vector3 minionOffset = new Vector3(-6f * facingDirection, 0, 0); // 5 units behind player (xa hơn)
        SpawnMinionWithFX(minionOffset, facingDirection);
    }

    public void OnSummonFrame3()
    {
        // Spawn minion thứ hai phía sau player (xa hơn, lệch phải)
        float facingDirection = GetFacingDirection();
        Vector3 minionOffset = new Vector3(-10f * facingDirection, 0, 0); // 7 units behind player (xa hơn)
        SpawnMinionWithFX(minionOffset, facingDirection);
    }

    // Lấy hướng đang quay của player (1 = phải, -1 = trái)
    private float GetFacingDirection()
    {
        return transform.localScale.x >= 0 ? 1f : -1f;
    }

    private void SpawnMinionWithFX(Vector3 offset, float facingDirection)
    {
        if (minionPrefab)
        {
            Vector3 minionPos = spawnPoint.position + offset;

            // Spawn FX khói ngay tại vị trí minion sẽ xuất hiện
            SpawnSummonFX(minionPos);

            // Spawn minion ngay sau đó (delay nhỏ để FX xuất hiện trước)
            StartCoroutine(DelayedSpawnMinion(minionPos, facingDirection, 0.05f));
        }
    }

    private System.Collections.IEnumerator DelayedSpawnMinion(Vector3 position, float facingDirection, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject minion = Instantiate(minionPrefab, position, Quaternion.identity);

        // Lật hướng minion theo player
        if (minion != null)
        {
            Vector3 scale = minion.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            minion.transform.localScale = scale;
        }
    }

    private void SpawnSummonFX(Vector3 minionPosition)
    {
        // FX khói xuất hiện tại chân minion (hạ thấp hơn)
        // Explod 6218 - ánh sáng nhỏ tại chân minion
        if (fxExplod1)
        {
            Vector3 fx1Pos = minionPosition + new Vector3(0, -0.4f, 0); // hạ xuống thấp hơn
            GameObject fx1 = Instantiate(fxExplod1, fx1Pos, Quaternion.identity);
            fx1.transform.localScale = new Vector3(0.2f, 0.2f, 1); // đúng scale MUGEN

            var autoDestroy = fx1.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
                autoDestroy = fx1.AddComponent<AutoDestroyOnAnimationEnd>();
        }

        // Explod 6220 - khói lâu dài tại chân minion  
        if (fxExplod2)
        {
            Vector3 fx2Pos = minionPosition + new Vector3(0, -0.5f, 0); // hạ xuống thấp hơn
            GameObject fx2 = Instantiate(fxExplod2, fx2Pos, Quaternion.identity);
            fx2.transform.localScale = new Vector3(0.4f, 0.3f, 1); // đúng scale MUGEN

            var autoDestroy = fx2.GetComponent<AutoDestroyOnAnimationEnd>();
            if (autoDestroy == null)
                autoDestroy = fx2.AddComponent<AutoDestroyOnAnimationEnd>();
        }
    }

    private void SpawnMinion(Vector3 offset)
    {
        if (minionPrefab)
        {
            Vector3 pos = spawnPoint.position + offset;
            Instantiate(minionPrefab, pos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Gọi khi animation kết thúc.
    /// </summary>
    public void OnCastEnd()
    {
        isCasting = false; // reset flag để cast lại lần sau
        castTimer = 0f;
        hasSpawnedFrame2 = false;
        hasSpawnedFrame3 = false;
        hasPlayedActivation = false;

        if (animator)
        {
            if (Mathf.Abs(transform.position.y) < 0.01f)
                animator.Play("Player_Idle");
            else
                animator.Play("Player_Falling");
        }
    }
}
