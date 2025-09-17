
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeHit(float dmg);
    bool IsAlive { get; }
}

public class Damageable : MonoBehaviour, IDamageable
{
    [Header("HP")]
    public float MaxHP = 100f;
    public float CurrentHP = 100f;
    public bool IsAlive => CurrentHP > 0f;

    private Animator animator;
    private bool hasDied = false;

    [Header("Stun & Hit Effect")]
    public float StunDuration = 0.8f; // thời gian đứng yên sau khi bị đánh (tăng mặc định)
    public bool IsStunned { get; private set; }
    public float HitPullStrength = 5.0f; // lực kéo dính vào đòn (tăng mặc định)
    public float KnockbackForce = 10f;    // lực đẩy lùi khi bị đánh
    public float HitStopTime = 0.05f;    // thời gian hit stop

    private float stunTimer = 0f;
    private Rigidbody2D rb;
    private Vector2 lastHitDirection = Vector2.zero;

    void Awake()
    {
        if (CurrentHP <= 0f) CurrentHP = MaxHP;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        CacheAIScripts();
    }

    // Gọi hàm này khi bị đánh, truyền thêm hướng đòn đánh (nếu có)
    public void TakeHit(float dmg)
    {
        TakeHit(dmg, Vector2.zero, null);
    }

    // Overload: truyền hướng đòn đánh và vị trí attacker (nếu có)
    public void TakeHit(float dmg, Vector2 hitDirection, Transform attacker = null)
    {
        if (!IsAlive) return;

        CurrentHP -= dmg;

        if (CurrentHP > 0f)
        {
            // Gọi animation bị đánh
            if (animator != null)
                animator.SetTrigger("Hurt");

            // Gây stun
            Stun();

            // Disable AI khi bị stun/hit pull
            SetAIScriptsEnabled(false);

            // Hit pull (kéo dính về attacker) - chỉ áp dụng nếu attacker là player
            if (attacker != null && HitPullStrength > 0f && attacker.CompareTag("Player"))
            {
                StopCoroutine("HitPullCoroutine"); // tránh kéo chồng nhiều lần
                StartCoroutine(HitPullCoroutine(attacker));
            }

            // Knockback (ưu tiên dùng Rigidbody2D)
            if (rb != null && (hitDirection != Vector2.zero || attacker != null))
            {
                Vector2 dir = hitDirection;
                if (dir == Vector2.zero && attacker != null)
                    dir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
                // Knockback rõ ràng hơn: set cả trục Y nếu muốn hất lên
                Vector2 knockback = new Vector2(dir.x * KnockbackForce, Mathf.Abs(dir.y) > 0.1f ? dir.y * KnockbackForce : rb.velocity.y);
                rb.velocity = knockback;
                lastHitDirection = dir;
            }
        }
        else
        {
            CurrentHP = 0f;
            if (!hasDied)
            {
                hasDied = true;
                OnDeath();
            }
        }
    }

    // Kéo enemy về gần attacker trong thời gian ngắn (hit pull rõ ràng hơn)
    IEnumerator HitPullCoroutine(Transform attacker)
    {
        float t = 0f;
        float duration = 0.45f; // thời gian kéo dính (tăng mặc định)
        Vector3 start = transform.position;
        Vector3 target = attacker.position + (transform.position - attacker.position).normalized * 0.5f; // giữ khoảng cách nhỏ
        SetAIScriptsEnabled(false); // disable AI khi kéo
        bool wasKinematic = false;
        if (rb != null)
        {
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
        }
        while (t < duration)
        {
            t += Time.deltaTime * HitPullStrength;
            transform.position = Vector3.Lerp(start, target, t / duration);
            yield return null;
        }
        if (rb != null) rb.isKinematic = wasKinematic;
        SetAIScriptsEnabled(true); // enable lại AI sau khi kéo
    }

    // (Hit stop đã chuyển sang PlayerCombat quản lý)

    void Update()
    {
        if (IsStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                IsStunned = false;
                SetAIScriptsEnabled(true); // enable lại AI sau stun
            }
        }
    }

    void Stun()
    {
        IsStunned = true;
        stunTimer = StunDuration;
    }

    // --- AI Script Control ---
    private MonoBehaviour[] aiScripts;
    // Thêm tên các script AI/move cần disable vào đây
    private readonly string[] aiScriptNames = { "EnemyAI", "BatController" };

    void CacheAIScripts()
    {
        var list = new System.Collections.Generic.List<MonoBehaviour>();
        foreach (var name in aiScriptNames)
        {
            var script = GetComponent(name) as MonoBehaviour;
            if (script != null) list.Add(script);
        }
        aiScripts = list.ToArray();
    }

    void SetAIScriptsEnabled(bool enabled)
    {
        if (aiScripts == null) CacheAIScripts();
        foreach (var script in aiScripts)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
        // Khi disable AI thì dừng hẳn vật lý
        if (rb != null && !enabled)
        {
            rb.velocity = Vector2.zero;
        }
    }

    void OnDeath()
    {
        // Award experience if this is an enemy
        var experienceSystem = FindObjectOfType<ExperienceSystem>();
        if (experienceSystem != null)
        {
            // Check if this is a bat enemy
            var batController = GetComponent<BatController>();
            if (batController != null)
            {
                experienceSystem.GainExpFromEnemy("bat");
                Debug.Log("💀 Bat defeated via Damageable → +50 EXP awarded to player");
                batController.TryDropItems();
            }
            else
            {
                // Check if this is a skeleton enemy
                var skeletonController = GetComponent<SkeletonController>();
                if (skeletonController != null)
                {
                    experienceSystem.GainExpFromEnemy("skeleton");
                    Debug.Log("💀 Skeleton defeated via Damageable → +75 EXP awarded to player");
                    skeletonController.TryDropItems();
                }
                else
                {
                    // Generic enemy fallback
                    experienceSystem.GainExperience("Enemy (Unknown)", 10);
                    Debug.Log("💀 Enemy defeated via Damageable → +10 EXP awarded to player");
                }
            }
        }

        // Đảm bảo Animator luôn enable để play Die
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetTrigger("Die");
            animator.Update(0f); // ép update 1 frame
        }
        // Disable AI và dừng vật lý
        SetAIScriptsEnabled(false);
        if (rb != null) rb.velocity = Vector2.zero;
        // Tắt collider và scripts để không gây damage/tấn công nữa
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Hủy sau khi play animation Die (tăng thời gian nếu cần)
        Destroy(gameObject, 1.5f);
    }
}
