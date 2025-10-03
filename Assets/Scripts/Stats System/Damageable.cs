
using System.Collections;
using UnityEngine;

public interface IDamageable
{
    void TakeHit(float dmg);
    void TakeHit(float dmg, Vector2 hitDirection, Transform attacker = null, float knockbackForce = 10f, float hitPullStrength = 5f);
    bool IsAlive { get; }
}

public class Damageable : MonoBehaviour, IDamageable
{
    // Kiểm soát trạng thái AI
    private bool isAIForcedDisabled = false;
    [Header("HP")]
    public float MaxHP = 100f;
    public float CurrentHP = 100f;
    public bool IsAlive => CurrentHP > 0f;

    private Animator animator;
    private bool hasDied = false;

    [Header("Stun & Hit Effect")]
    public float StunDuration = 1.2f; // tăng thời gian stun khi bị đánh
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
        TakeHit(dmg, Vector2.zero, null, KnockbackForce, HitPullStrength);
    }

    // Overload: truyền hướng đòn đánh, vị trí attacker, lực knockback/hit pull
    public void TakeHit(float dmg, Vector2 hitDirection, Transform attacker = null, float knockbackForce = 10f, float hitPullStrength = 5f)
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
            isAIForcedDisabled = true;
            SetAIScriptsEnabled(false);

            // Hit stop cho enemy: chỉ freeze Animator, không freeze toàn bộ game
            if (animator != null)
                StartCoroutine(HitStopAnimatorCoroutine(animator, 0.12f));

            // Hit pull (kéo dính về attacker) - chỉ áp dụng nếu attacker là player
            if (attacker != null && hitPullStrength > 0f && attacker.CompareTag("Player"))
            {
                StopCoroutine("HitPullCoroutine"); // tránh kéo chồng nhiều lần
                StartCoroutine(HitPullCoroutine(attacker, hitPullStrength));
            }

            // Knockback - ưu tiên đẩy ENEMY RA KHỎI PLAYER
            if (rb != null && (hitDirection != Vector2.zero || attacker != null))
            {
                Vector2 dir = hitDirection;
                if (dir == Vector2.zero && attacker != null)
                {
                    // Tính hướng từ player ra enemy (đảm bảo đẩy ra ngoài)
                    dir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
                }

                // Force hướng đẩy ra ngoài nếu quá gần
                float distanceToPlayer = Vector3.Distance(transform.position, attacker.position);
                if (distanceToPlayer < 2f && attacker != null)
                {
                    // Đẩy mạnh ra ngoài nếu quá gần
                    dir = ((Vector2)transform.position - (Vector2)attacker.position).normalized;
                    knockbackForce *= 1.5f; // Tăng knockback khi quá gần
                }

                // Knockback chủ yếu theo chiều ngang, Y nhẹ hơn
                Vector2 knockback = new Vector2(dir.x * knockbackForce,
                    Mathf.Abs(dir.y) > 0.1f ? dir.y * knockbackForce * 0.6f : rb.velocity.y * 0.8f);
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

    // Hit stop cho enemy: chỉ freeze Animator
    IEnumerator HitStopAnimatorCoroutine(Animator anim, float duration)
    {
        float prevSpeed = anim.speed;
        anim.speed = 0f;
        yield return new WaitForSecondsRealtime(duration);
        anim.speed = prevSpeed;
    }

    // Kéo enemy vào tầm đánh NHƯNG GIỮ KHOẢNG CÁCH tối thiểu
    IEnumerator HitPullCoroutine(Transform attacker, float hitPullStrength)
    {
        float t = 0f;
        float duration = 0.2f; // Duration ngắn hơn

        isAIForcedDisabled = true;
        SetAIScriptsEnabled(false); // disable AI khi kéo

        if (rb != null)
        {
            // Backup gravity để restore sau
            float originalGravityScale = rb.gravityScale;

            Vector3 startPos = transform.position;
            float distanceToPlayer = Vector3.Distance(transform.position, attacker.position);
            float minDistance = 1.5f; // KHOẢNG CÁCH TỐI THIỂU - không kéo gần hơn thế này

            // CHỈ pull nếu ở xa hơn khoảng cách tối thiểu
            if (distanceToPlayer > minDistance)
            {
                Vector3 targetDirection = (attacker.position - transform.position).normalized;

                // GIẢM gravity trong lúc pull để tránh bị kéo xuống
                rb.gravityScale = 0.3f; // Giảm gravity mạnh

                while (t < duration)
                {
                    // Check khoảng cách realtime - dừng pull nếu đã đủ gần
                    float currentDistance = Vector3.Distance(transform.position, attacker.position);
                    if (currentDistance <= minDistance)
                        break;

                    float progress = t / duration;
                    // Curve mạnh ở đầu, yếu dần
                    float pullCurve = Mathf.Lerp(1f, 0f, progress * progress);

                    // Apply pull CHỈ THEO CHIỀU NGANG + giữ Y velocity hiện tại
                    Vector2 horizontalPull = new Vector2(targetDirection.x * hitPullStrength * pullCurve, 0f);
                    rb.velocity = new Vector2(horizontalPull.x, rb.velocity.y);

                    t += Time.deltaTime;
                    yield return null;
                }

                // Restore gravity và velocity
                rb.gravityScale = originalGravityScale;
                rb.velocity = new Vector2(0f, rb.velocity.y); // Chỉ clear X velocity
            }
        }

        // Chỉ enable lại AI nếu không còn stun
        if (!IsStunned)
        {
            isAIForcedDisabled = false;
            SetAIScriptsEnabled(true); // enable lại AI sau khi kéo
        }
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
                // Chỉ enable lại AI nếu không bị hit pull
                if (!isAIForcedDisabled)
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
